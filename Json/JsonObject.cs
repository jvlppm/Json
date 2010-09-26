using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace Json
{
	public enum ObjectType
	{
		Null,
		Boolean,
		String,
		Number,
		Object,
		Array
	}

	public class JsonValue : DynamicObject, IEnumerable<JsonValue>
	{
		public JsonValue this[string property]
		{
			get
			{
				if (Type == ObjectType.Array && Array.Values.Count == 1)
					return Array.Values[0][property];
				if (Type == ObjectType.Object)
					return Object.Properties[property];

				throw new Exception("Object of type " + Type + " does not have members");
			}
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (Type != ObjectType.Object)
			{
				result = this[binder.Name];
				return true;
			}

			return Object.TryGetMember(binder, out result);
		}

		public static explicit operator int(JsonValue o)
		{
			if (o.Type != ObjectType.String)
				throw new Exception("Can't convert " + o.Type + " to integer");

			return int.Parse(o.String);
		}

		public static implicit operator string(JsonValue o)
		{
			if (o.Type != ObjectType.String)
				throw new Exception("Can't convert " + o.Type + " to string");

			return o.String;
		}

		public ObjectType Type { get; set; }
		public string String { get; private set; }
		public JsonObject Object { get; private set; }
		public JsonArray Array { get; private set; }

		public JsonValue(JsonReader reader)
		{
			var token = reader.ReadToken();
			switch (token.Type)
			{
				case JsonToken.TokenType.SpecialChar:
					reader.PutBack(token);

					if (token.Value == "{")
					{
						Type = ObjectType.Object;
						Object = new JsonObject(reader);
					}
					else if (token.Value == "[")
					{
						Type = ObjectType.Array;
						Array = new JsonArray(reader);
					}
					else throw new System.Exception("Unexpected special char '" + token.Value + "'");

					break;

				case JsonToken.TokenType.Number:
					Type = ObjectType.Number;
					String = token.Value;
					break;

				case JsonToken.TokenType.String:
					Type = ObjectType.String;
					String = token.Value;
					break;

				case JsonToken.TokenType.KeyWord:
					switch (token.Value)
					{
						case "null":
							Type = ObjectType.Null;
							break;

						case "false":
						case "true":
							Type = ObjectType.Boolean;
							String = token.Value;
							break;

						default: throw new System.Exception("Unexpected keyword \"" + token.Value + "\"");
					}
					break;
			}
		}

		public IEnumerator<JsonValue> GetEnumerator()
		{
			if (Type != ObjectType.Array)
				throw new Exception("Can't iterate on a " + Type);

			return Array.Values.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			if (Type != ObjectType.Array)
				throw new Exception("Can't iterate on a " + Type);

			return Array.Values.GetEnumerator();
		}

		public override string ToString()
		{
			if (Type == ObjectType.String)
				return String;
			switch (Type)
			{
				case ObjectType.String: return String.ToString();
				case ObjectType.Array: return Array.ToString();
				case ObjectType.Object: return Object.ToString();
			}

			return base.ToString();
		}
	}

	public class JsonArray : DynamicObject, IEnumerable<JsonValue>
	{
		public static implicit operator List<JsonValue>(JsonArray o)
		{
			return o.Values;
		}

		public List<JsonValue> Values { get; private set; }

		public JsonArray(JsonReader reader)
		{
			Values = new List<JsonValue>();

			var nextToken = reader.ReadToken();
			if (nextToken.Type != JsonToken.TokenType.SpecialChar || nextToken.Value != "[")
				throw new Exception("Invalid Json, '[' expected");

			nextToken = reader.ReadToken();

			if (nextToken.Type != JsonToken.TokenType.SpecialChar || nextToken.Value != "]")
			{
				reader.PutBack(nextToken);

				do
				{
					Values.Add(new JsonValue(reader));
					nextToken = reader.ReadToken();
				} while (nextToken.Type == JsonToken.TokenType.SpecialChar && nextToken.Value == ",");

				if (nextToken.Type != JsonToken.TokenType.SpecialChar || nextToken.Value != "]")
					throw new Exception("Invalid Json, ']' expected");
			}
		}

		public IEnumerator<JsonValue> GetEnumerator()
		{
			return Values.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return Values.GetEnumerator();
		}
	}

	public class JsonObject : DynamicObject
	{
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (Properties.ContainsKey(binder.Name))
				result = Properties[binder.Name];
			else
			{
				result = null;
				return false;
			}

			return true;
		}

		public Dictionary<string, JsonValue> Properties { get; private set; }

		public JsonObject(JsonReader reader)
		{
			var nextToken = reader.ReadToken();

			if (nextToken.Type != JsonToken.TokenType.SpecialChar || nextToken.Value != "{")
				throw new Exception("Invalid Json, '{' expected");

			Properties = new Dictionary<string, JsonValue>();

			nextToken = reader.ReadToken();

			if (nextToken.Type != JsonToken.TokenType.SpecialChar || nextToken.Value != "}")
			{
				reader.PutBack(nextToken);

				do
				{
					var property = reader.ReadToken();
					if (property.Type != JsonToken.TokenType.String)
						throw new Exception("Invalid Json, field-name expected, got \"" + property.Type + "\" instead");

					nextToken = reader.ReadToken();
					if (nextToken.Type != JsonToken.TokenType.SpecialChar || nextToken.Value != ":")
						throw new Exception("Invalid Json, ':' expected");

					JsonValue value = new JsonValue(reader);
					Properties.Add(property.Value, value);

					nextToken = reader.ReadToken();

				} while (nextToken.Type == JsonToken.TokenType.SpecialChar && nextToken.Value == ",");

				if (nextToken.Type != JsonToken.TokenType.SpecialChar || nextToken.Value != "}")
					throw new Exception("Invalid Json, '}' expected");
			}
		}
	}
}
