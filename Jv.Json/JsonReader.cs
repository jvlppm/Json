﻿//
// JsonReader.cs
//
// Author:
//   João Vitor Pietsiaki Moraes <jvlppm@gmail.com>
//
// Copyright (c) 2010 João Vitor Pietsiaki Moraes
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections.Generic;
using System.Globalization;

namespace Jv.Json
{
	class JsonToken
	{
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

	class JsonReader
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

			TokenType tokenType = TokenType.Unidentified;
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
						return new JsonToken(TokenType.SpecialChar, Text[Position++].ToString());

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
								return new JsonToken(TokenType.String, token);
							}
						}
						throw new LexicalException("End quote not found for quote at " + startQuote, Position);

					default:
						if (token == string.Empty)
						{
							if (char.IsNumber(Text[Position]) || Text[Position] == '+' || Text[Position] == '-')
								tokenType = TokenType.Number;
						}
						else if (tokenType == TokenType.Number && !char.IsNumber(Text[Position]) && (Text[Position] != '.' || token.Contains(".")))
							tokenType = TokenType.String;
						token += Text[Position];
						break;
				}
			}

			if (tokenType == TokenType.Unidentified)
			{
				switch (token)
				{
					case "true":
					case "false":
					case "null":
						tokenType = TokenType.KeyWord;
						break;
					default:
						tokenType = TokenType.String;
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
