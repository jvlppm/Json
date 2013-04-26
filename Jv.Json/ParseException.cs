//
// ParseException.cs
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

using System;

namespace Jv.Json
{
#if !NETFX_CORE
	public
#endif
	class ParseException : FormatException
	{
		public int Position { get; protected set; }

		public ParseException(string message, int position)
			: base(message)
		{
			Position = position;
		}
	}

#if !NETFX_CORE
	public
#endif
	class LexicalException : ParseException
	{
		public LexicalException(string message, int position) : base(message, position)
		{
		}
	}

#if !NETFX_CORE
	public
#endif
    class SemanticException : ParseException
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
