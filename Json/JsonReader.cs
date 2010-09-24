using System.Collections.Generic;
namespace Json
{
	public class JsonReader
	{
		public string Text { get; private set; }
		int Position { get; set; }

		Stack<string> BackToken { get; set; }

		public JsonReader(string json)
		{
			Text = json;
			Position = 0;
			BackToken = new Stack<string>();
		}

		public string ReadToken()
		{
			if (BackToken.Count > 0)
				return BackToken.Pop();

			string token = string.Empty;
			for (; Position < Text.Length; Position++)
			{
				switch (Text[Position])
				{
					case '\t':
					case '\r':
					case '\n':
					case ' ':
						if (token != string.Empty)
							return token;
						break;

					case ',':
					case ':':
					case '{':
					case '}':
					case '[':
					case ']':
						if (token != string.Empty)
							return token;
						return Text[Position++].ToString();

					case '\"':
						if (token != string.Empty)
							throw new System.Exception("Unexpected char: '\"'");
						for (Position++; Position < Text.Length; Position++)
						{
							if (Text[Position] != '\"')
								token += Text[Position];
							else
							{
								Position++;
								return token;
							}
						}
						break;

					default:
						token += Text[Position];
						break;
				}
			}

			return token;
		}

		public void PutBack(string token)
		{
			BackToken.Push(token);
		}
	}
}
