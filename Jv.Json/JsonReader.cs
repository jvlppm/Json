using System.Collections.Generic;
using System.Globalization;

namespace Jv.Json
{
	public class JsonToken
	{
		public enum TokenType
		{
			Unidentified,
			KeyWord,
			SpecialChar,
			Number,
			String
		}

		public JsonToken(TokenType type, string value)
		{
			Type = type;
			Value = value;
		}

		public TokenType Type { get; private set; }
		public string Value { get; private set; }

		public override string ToString()
		{
			return string.Format("{0}: \"{1}\"", Type, Value);
		}
	}

	public class JsonReader
	{
		public string Text { get; private set; }
		public int Position { get; private set; }

		Stack<JsonToken> BackToken { get; set; }

		public JsonReader(string json)
		{
			Text = json;
			Position = 0;
			BackToken = new Stack<JsonToken>();
		}

		public JsonToken ReadToken()
		{
			if (BackToken.Count > 0)
				return BackToken.Pop();

			JsonToken.TokenType tokenType = JsonToken.TokenType.Unidentified;
			string token = string.Empty;
			bool endToken = false;
			for (; !endToken && Position < Text.Length; Position++)
			{
				switch (Text[Position])
				{
					case '\t':
					case '\r':
					case '\n':
					case ' ':
						if (token != string.Empty)
							endToken = true;
						break;

					case ',':
					case ':':
					case '{':
					case '}':
					case '[':
					case ']':
						if (token != string.Empty)
						{
							endToken = true;
							Position--;
							break;
						}
						return new JsonToken(JsonToken.TokenType.SpecialChar, Text[Position++].ToString());

					case '\"':
						if (token != string.Empty)
							throw new LexicalException("Unexpected char '\"' while reading \"" + token + "\"", Position);

						int startQuote = Position;
						for (Position++; Position < Text.Length; Position++)
						{
							if (Text[Position] != '\"')
							{
								if (Text[Position] != '\\')
									token += Text[Position];
								else
								{
									Position++;
									switch (Text[Position])
									{
										case 'b': token += "\b"; break;
										case 'f': token += "\f"; break;
										case 'n': token += "\n"; break;
										case 'r': token += "\r"; break;
										case 't': token += "\t"; break;
										case 'v': token += "\v"; break;
										case '\"': token += "\""; break;
										case '\\': token += "\\"; break;
										case '/': token += "/"; break;

										case 'u':
											string code = Text.Substring(Position + 1, 4);
											Position += 4;
											token += (char)int.Parse(code, NumberStyles.HexNumber);
											break;

										default:
											throw new LexicalException("Bad escape sequence: \'\\" + Text[Position] + "\'", Position);
									}
								}
							}
							else
							{
								Position++;
								return new JsonToken(JsonToken.TokenType.String, token);
							}
						}
						throw new LexicalException("End quote not found for quote at " + startQuote, Position);

					default:
						if (token == string.Empty)
						{
							if (char.IsNumber(Text[Position]) || Text[Position] == '+' || Text[Position] == '-')
								tokenType = JsonToken.TokenType.Number;
						}
						else if (tokenType == JsonToken.TokenType.Number && !char.IsNumber(Text[Position]) && (Text[Position] != '.' || token.Contains(".")))
							tokenType = JsonToken.TokenType.String;
						token += Text[Position];
						break;
				}
			}

			if (tokenType == JsonToken.TokenType.Unidentified)
			{
				switch (token)
				{
					case "true":
					case "false":
					case "null":
						tokenType = JsonToken.TokenType.KeyWord;
						break;
					default:
						tokenType = JsonToken.TokenType.String;
						break;
				}
			}

			return new JsonToken(tokenType, token);
		}

		public void PutBack(JsonToken token)
		{
			BackToken.Push(token);
		}
	}
}
