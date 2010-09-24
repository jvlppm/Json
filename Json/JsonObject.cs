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
		String,
		Object,
		Array
	}

	public class JsonValue : DynamicObject, IEnumerable<JsonValue>
	{
		public JsonValue this[string property]
		{
			get
			{
				if(Type == ObjectType.Array && Array.Values.Count == 1)
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
			string token = reader.ReadToken();
			switch (token)
			{
				case "{":
					Type = ObjectType.Object;
					reader.PutBack(token);
					Object = new JsonObject(reader);
					break;

				case "[":
					Type = ObjectType.Array;
					reader.PutBack(token);
					Array = new JsonArray(reader);
					break;

				default:
					Type = ObjectType.String;
					String = token;
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

			if (reader.ReadToken() != "[")
				throw new Exception("Invalid Json, '[' expected");

			string nextToken = reader.ReadToken();
			reader.PutBack(nextToken);

			do
			{
				Values.Add(new JsonValue(reader));
				nextToken = reader.ReadToken();
				if(nextToken != ",")
					reader.PutBack(nextToken);
			} while (nextToken == ",");

			if (reader.ReadToken() != "]")
				throw new Exception("Invalid Json, ']' expected");
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
			if (reader.ReadToken() != "{")
				throw new Exception("Invalid Json, '{' expected");

			Properties = new Dictionary<string, JsonValue>();

			string nextToken = reader.ReadToken();

			if (nextToken == "}")
				return;

			reader.PutBack(nextToken);

			do
			{
				string property = reader.ReadToken();

				if (reader.ReadToken() != ":")
					throw new Exception("Invalid Json, ':' expected");

				JsonValue value = new JsonValue(reader);
				nextToken = reader.ReadToken();

				Properties.Add(property, value);

			} while (nextToken == ",");

			if (nextToken != "}")
				throw new Exception("Invalid Json, '}' expected");
		}
	}
}
