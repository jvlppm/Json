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
#if !dotnet40
			string json = "[        {                \"id\":\"1-q.d\",\"data\":                {                \"isViewer\":true,\"id\":\"81345167181005418762\",\"thumbnailUrl\":\"http://static1.orkut.com/img/i_nophoto64.gif\",\"name\":{        \"familyName\":\"Family Name\",\"givenName\":\"Someone\"}        }}]";

			dynamic obj = new Json.JsonValue(new Json.JsonReader(json));

			Console.WriteLine("Id: " + obj.id);
			Console.WriteLine("Name: " + obj.data.name.givenName);
			Console.WriteLine("Thumbnail: " + obj.data.thumbnailUrl);
#else
			string json = "[        {                \"id\":\"1-q.d\",\"data\":                {                \"isViewer\":true,\"id\":\"81345167181005418762\",\"thumbnailUrl\":\"http://static1.orkut.com/img/i_nophoto64.gif\",\"name\":{        \"familyName\":\"Family Name\",\"givenName\":\"Someone\"}        }}]";

			JsonValue obj = new JsonValue(new JsonReader(json));

			Console.WriteLine("Id: " + obj["id"]);
			Console.WriteLine("Name: " + obj["data"]["name"]["givenName"]);
			Console.WriteLine("Thumbnail: " + obj["data"]["thumbnailUrl"]);
#endif
		}
	}
}
