using System;

namespace Jv.Json
{
	public class ParseException : Exception
	{
		public int Position { get; protected set; }

		public ParseException(string message, int position)
			: base(message)
		{
			Position = position;
		}
	}

	public class LexicalException : ParseException
	{
		public LexicalException(string message, int position) : base(message, position)
		{
		}
	}

	public class SemanticException : ParseException
	{
		public override string Message
		{
			get
			{
				if(Expected.Length == 1)
					return string.Format("Expected {0} but got {1}, at position {2}.", Expected[0], Got, Position);
				return string.Format("Unexpected {0} at position {1}", Got, Position);
			}
		}

		public SemanticException(string expected, string got, int position)
			: this(new []{ expected }, got, position)
		{
		}

		public SemanticException(string[] expected, string got, int position)
			: base("Semantic Exception", position)
		{
			Expected = expected;
			Got = got;
			Position = position;
		}

		public string[] Expected { get; private set; }
		public string Got { get; private set; }
	}
}
