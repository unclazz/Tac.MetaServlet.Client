using System;
using System.Collections.Generic;
using System.Linq;

namespace Tac.MetaServlet.Json
{
	public sealed class JsonObjectBuilder
	{
		public static JsonObjectBuilder GetInstance()
		{
			return new JsonObjectBuilder();
		}
		public static JsonObjectBuilder GetInstance(IJsonObject prototype)
		{
			return prototype == null ? GetInstance() : new JsonObjectBuilder(prototype);
		}

		private readonly IDictionary<string, IJsonObject> propsDict 
			= new Dictionary<string, IJsonObject>();

		JsonObjectBuilder()
		{
		}

		JsonObjectBuilder(IJsonObject prototype)
		{
			foreach (IJsonProperty prop in prototype.Properties)
			{
				propsDict[prop.Name] = prop.Value;
			}
		}

		public JsonObjectBuilder Append(string propName, IJsonObject propValue)
		{
			propsDict[propName] = propValue;
			return this;
		}
		public JsonObjectBuilder Append(string propName, string propValue)
		{
			return Append(propName, JsonObject.Of(propValue));
		}
		public JsonObjectBuilder AppendNull(string propName)
		{
			return Append(propName, JsonObject.OfNull());
		}
		public JsonObjectBuilder Append(string propName, double propValue)
		{
			return Append(propName, JsonObject.Of(propValue));
		}
		public JsonObjectBuilder Append(string propName, bool propValue)
		{
			return Append(propName, JsonObject.Of(propValue));
		}
		public JsonObjectBuilder Append(string propName, Action<JsonObjectBuilder> buildAction)
		{
			var b = new JsonObjectBuilder();
			buildAction(b);
			return Append(propName, b.Build());
		}
		public JsonObjectBuilder Append(string propName, IEnumerable<IJsonObject> arrayItems)
		{
			return Append(propName, JsonObject.Of(arrayItems));
		}
		public JsonObjectBuilder Append(string propName, params IJsonObject[] arrayItems)
		{
			return Append(propName, JsonObject.Of(arrayItems));
		}
		public JsonObjectBuilder Append(string propName, IEnumerable<string> arrayItems)
		{
			return Append(propName, JsonObject.Of(arrayItems.Select(JsonObject.Of)));
		}
		public JsonObjectBuilder Append(string propName, params string[] arrayItems)
		{
			return Append(propName, arrayItems as IEnumerable<string>);
		}
		public JsonObjectBuilder Append(string propName, IEnumerable<bool> arrayItems)
		{
			return Append(propName, JsonObject.Of(arrayItems.Select(JsonObject.Of)));
		}
		public JsonObjectBuilder Append(string propName, params bool[] arrayItems)
		{
			return Append(propName, arrayItems as IEnumerable<bool>);
		}
		public JsonObjectBuilder Append(string propName, IEnumerable<double> arrayItems)
		{
			return Append(propName, JsonObject.Of(arrayItems.Select(JsonObject.Of)));
		}
		public JsonObjectBuilder Append(string propName, params double[] arrayItems)
		{
			return Append(propName, arrayItems as IEnumerable<double>);
		}
		public JsonObjectBuilder AppendEmptyArray(string propName)
		{
			return Append(propName, JsonObject.Of(JsonObject.Of(new IJsonObject[0])));
		}

		public IJsonObject Build()
		{
			return ObjectJsonObject.Of(propsDict.Select((e) => new JsonProperty(e.Key, e.Value)));
		}
	}

	sealed class JsonProperty : IJsonProperty
	{
		internal JsonProperty(string name, IJsonObject value)
		{
			if (name == null || value == null)
			{
				throw new NullReferenceException();
			}
			Name = name;
			Value = value;
		}

		public string Name { get; }
		public IJsonObject Value { get; }
	}
}

