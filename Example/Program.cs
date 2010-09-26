using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Json;

namespace Example
{
	class Program
	{
		static void Main(string[] args)
		{
			dynamic pessoas = JsonBuilder.Build("[{\"nome\":\"João Vitor\", idade: 24}, {\"nome\":\"Teste\", idade: 10}]");

			foreach (var pessoa in pessoas)
				Console.WriteLine(pessoa.nome + ": " + pessoa.idade);
		}
	}
}
