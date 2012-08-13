using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jv.Json
{
    public static class Extensions
    {
#if !NET35
        public static dynamic AsJson(this string json)
#else
        public static object AsJson(this string json)
#endif
        {
            return JsonBuilder.Build(json);
        }

        public static string ToJson(this object obj)
        {
            return JsonBuilder.Extract(obj, false);
        }

        public static string ToJson(this object obj, bool ident)
        {
            return JsonBuilder.Extract(obj, ident);
        }
    }
}
