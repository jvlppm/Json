﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Jv.Json;
#if !NET35
using System.Dynamic;
#endif

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            string json = "[{\"nome\":\"João Vitor\", idade: 24}, {\"nome\":\"Teste\", idade: 10}]";

            //object obj = json.AsJson();
            //Console.WriteLine(obj.ToJson());

#if NET35
            foreach (IDictionary<string, object> pessoa in (IList<object>)json.AsJson())
                Console.WriteLine(pessoa["nome"] + ": " + pessoa["idade"]);
#else
            foreach (var pessoa in json.AsJson())
                Console.WriteLine(pessoa.nome + ": " + pessoa.idade);
#endif
        }
    }
}
