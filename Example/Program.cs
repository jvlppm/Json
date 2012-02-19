using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Json;
using System.Dynamic;

namespace Example
{
	class Program
	{
		static void Main(string[] args)
		{
			string json = "[{\"nome\":\"João Vitor\", idade: 24}, {\"nome\":\"Teste\", idade: 10}]";

			//object obj = json.AsJson();
			//Console.WriteLine(obj.ToJson());

			foreach (var pessoa in json.AsJson())
				Console.WriteLine(pessoa.nome + ": " + pessoa.idade);
		}
	}
}
