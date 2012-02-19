using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jv.Json
{
	public static class Extensions
	{
		public static dynamic AsJson(this string json)
		{
			return JsonBuilder.Build(json);
		}

		public static string ToJson(this object obj, bool ident=false)
		{
			return JsonBuilder.Extract(obj, ident);
		}
	}
}
