using System.Collections.Generic;
using System;
using System.Dynamic;
using System.Text;
using System.Globalization;
using System.Reflection;

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
					((IDictionary<string, object>) obj).Add(property.Value, value);

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

		public static string Extract(dynamic obj)
		{
			if (obj == null)
				return "null";

			if (obj is IDictionary<string, object>)
			{
				StringBuilder json = new StringBuilder();
				json.Append("{");

				bool first = true;

				foreach (var key in (obj as IDictionary<string, object>).Keys)
				{
					if (first) first = false;
					else json.Append(',');

					json.AppendFormat("\"{0}\":{1}", key, Extract((obj as IDictionary<string, object>)[key]));
				}

				json.Append("}");

				return json.ToString();
			}

			else if (obj is Array)
			{
				StringBuilder json = new StringBuilder();
				json.Append("[");

				bool first = true;

				foreach (var value in obj)
				{
					if (first) first = false;
					else json.Append(',');

					json.Append(Extract(value));
				}

				json.Append("]");

				return json.ToString();
			}

			if (obj is bool)
				return ((bool)obj).ToString().ToLower();

			if (obj is int || obj is Int64 || obj is decimal || obj is double || obj is float)
				return obj.ToString(CultureInfo.InvariantCulture);

			if(obj is string)
				return "\"" + obj.ToString() + "\"";

			Dictionary<string, object> extractedInfo = new Dictionary<string,object>();
			foreach (PropertyInfo prop in obj.GetType().GetProperties())
				extractedInfo.Add(prop.Name, prop.GetValue(obj, null));
			return Extract(extractedInfo);
		}
	}
}
