using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Jv.Json;

namespace Example
{
	using JsonObject = System.Collections.Generic.IDictionary<string, object>;

	class Program
	{
		static void Main(string[] args)
		{
			string json = "[{\"nome\":\"João Vitor\", idade: 24}, {\"nome\":\"Teste\", idade: 10}]";

			//object obj = json.AsJson();
			//Console.WriteLine(obj.ToJson());

			foreach (JsonObject pessoa in json.AsJson<IEnumerable>())
				Console.WriteLine(pessoa["nome"] + ": " + pessoa["idade"]);
		}
	}
}
