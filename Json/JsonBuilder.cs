using System.Collections.Generic;
using System;
using System.Dynamic;

namespace Json
{
	public static class JsonBuilder
	{
		public static dynamic Build(string json)
		{
			return Build(new JsonReader(json));
		}

		public static dynamic Build(JsonReader reader)
		{
			var token = reader.ReadToken();
			switch (token.Type)
			{
				case JsonToken.TokenType.SpecialChar:
					reader.PutBack(token);

					if (token.Value == "{")
						return BuildObject(reader);

					else if (token.Value == "[")
						return BuildArray(reader);

					throw new SemanticException(new [] {"'{'", "'['"}, token.Value, reader.Position);

				case JsonToken.TokenType.Number:
					if(token.Value.Contains(".") || token.Value.Length > 18)
						return decimal.Parse(token.Value, System.Globalization.CultureInfo.InvariantCulture);
					if (token.Value.Length > 10)
						return Int64.Parse(token.Value);
					return int.Parse(token.Value);

				case JsonToken.TokenType.KeyWord:
					switch (token.Value)
					{
						case "null":
							return null;

						case "false":
						case "true":
							return token.Value == "true";
					}

					throw new SemanticException(new[] { "Boolean", "Number", "String", "Array", "Object" }, token.Value, reader.Position);

				default:
					return token.Value;
			}
		}

		public static dynamic BuildObject(JsonReader reader)
		{
			dynamic obj = new ExpandoObject();

			var nextToken = reader.ReadToken();

			if (nextToken.Type != JsonToken.TokenType.SpecialChar || nextToken.Value != "{")
				throw new SemanticException("{", nextToken.Value, reader.Position);

			nextToken = reader.ReadToken();

			if (nextToken.Type != JsonToken.TokenType.SpecialChar || nextToken.Value != "}")
			{
				reader.PutBack(nextToken);

				do
				{
					var property = reader.ReadToken();
					if (property.Type != JsonToken.TokenType.String)
						throw new SemanticException("String", property.Type.ToString(), reader.Position);

					nextToken = reader.ReadToken();
					if (nextToken.Type != JsonToken.TokenType.SpecialChar || nextToken.Value != ":")
						throw new SemanticException("':'", nextToken.Value, reader.Position);

					dynamic value = JsonBuilder.Build(reader);
					((IDictionary<String, Object>) obj).Add(property.Value, value);

					nextToken = reader.ReadToken();

				} while (nextToken.Type == JsonToken.TokenType.SpecialChar && nextToken.Value == ",");

				if (nextToken.Type != JsonToken.TokenType.SpecialChar || nextToken.Value != "}")
					throw new SemanticException(new []{"'}'", "','"}, nextToken.Value, reader.Position);
			}

			return obj;
		}

		public static dynamic[] BuildArray(JsonReader reader)
		{
			var values = new List<dynamic>();

			var nextToken = reader.ReadToken();
			if (nextToken.Type != JsonToken.TokenType.SpecialChar || nextToken.Value != "[")
				throw new SemanticException("'['", nextToken.Value, reader.Position);

			nextToken = reader.ReadToken();

			if (nextToken.Type != JsonToken.TokenType.SpecialChar || nextToken.Value != "]")
			{
				reader.PutBack(nextToken);

				do
				{
					values.Add(Build(reader));
					nextToken = reader.ReadToken();
				} while (nextToken.Type == JsonToken.TokenType.SpecialChar && nextToken.Value == ",");

				if (nextToken.Type != JsonToken.TokenType.SpecialChar || nextToken.Value != "]")
					throw new SemanticException("']'", nextToken.Value, reader.Position);
			}

			return values.ToArray();
		}
	}
}
