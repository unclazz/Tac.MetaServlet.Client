using System;
using System.Collections.Generic;
using System.Linq;

namespace Tac.MetaServlet.Json
{
	/// <summary>
	/// <c>Object</c>型のJSONノードを構築するためのビルダー・オブジェクトです.
	/// </summary>
	public sealed class JsonObjectBuilder
	{
		/// <summary>
		/// ビルダーのインスタンスを返します.
		/// </summary>
		/// <returns>ビルダー.</returns>
		public static JsonObjectBuilder GetInstance()
		{
			return new JsonObjectBuilder();
		}
		/// <summary>
		/// ビルダーのインスタンスを返します.
		/// 引数で指定指定されたJSONノードのプロパティは
		/// ビルダーにより生成されるJSONノードにコピーされます。
		/// </summary>
		/// <returns>ビルダー.</returns>
		/// <param name="prototype">Prototype.</param>
		public static JsonObjectBuilder GetInstance(IJsonObject prototype)
		{
			return prototype == null ? GetInstance() : new JsonObjectBuilder(prototype);
		}

		private readonly IDictionary<string, IJsonObject> propsDict 
			= new Dictionary<string, IJsonObject>();

		private JsonObjectBuilder()
		{
		}

		private JsonObjectBuilder(IJsonObject prototype)
		{
			foreach (IJsonProperty prop in prototype.Properties)
			{
				propsDict[prop.Name] = prop.Value;
			}
		}

		/// <summary>
		/// プロパティを追加します.
		/// </summary>
		/// <param name="propName">プロパティ名.</param>
		/// <param name="propValue">プロパティ値.</param>
		public JsonObjectBuilder Append(string propName, IJsonObject propValue)
		{
			propsDict[propName] = propValue;
			return this;
		}
		/// <summary>
		/// プロパティを追加します.
		/// </summary>
		/// <param name="propName">プロパティ名.</param>
		/// <param name="propValue">プロパティ値.</param>
		public JsonObjectBuilder Append(string propName, string propValue)
		{
			return Append(propName, JsonObject.Of(propValue));
		}
		/// <summary>
		/// <c>Null</c>型のプロパティを追加します.
		/// </summary>
		/// <param name="propName">プロパティ名.</param>
		public JsonObjectBuilder AppendNull(string propName)
		{
			return Append(propName, JsonObject.OfNull());
		}
		/// <summary>
		/// プロパティを追加します.
		/// </summary>
		/// <param name="propName">プロパティ名.</param>
		/// <param name="propValue">プロパティ値.</param>
		public JsonObjectBuilder Append(string propName, double propValue)
		{
			return Append(propName, JsonObject.Of(propValue));
		}
		/// <summary>
		/// プロパティを追加します.
		/// </summary>
		/// <param name="propName">プロパティ名.</param>
		/// <param name="propValue">プロパティ値.</param>
		public JsonObjectBuilder Append(string propName, bool propValue)
		{
			return Append(propName, JsonObject.Of(propValue));
		}
		/// <summary>
		/// プロパティを追加します.
		/// 第2引数のアクションにはJSONプロパティ値となる<c>Object</c>型JSONノードを構築するためのビルダーが渡されます。
		/// ビルダーを通じて当該JSONノードに対してプロパティを追加することができます。
		/// </summary>
		/// <param name="propName">プロパティ名.</param>
		/// <param name="buildAction">プロパティ値を構築するアクション.</param>
		public JsonObjectBuilder Append(string propName, Action<JsonObjectBuilder> buildAction)
		{
			if (buildAction == null)
			{
				throw new ArgumentNullException(nameof(buildAction));
			}
			var b = new JsonObjectBuilder();
			buildAction(b);
			return Append(propName, b.Build());
		}
		/// <summary>
		/// プロパティを追加します.
		/// </summary>
		/// <param name="propName">プロパティ名.</param>
		/// <param name="arrayItems">プロパティ値.</param>
		public JsonObjectBuilder Append(string propName, IEnumerable<IJsonObject> arrayItems)
		{
			return Append(propName, JsonObject.Of(arrayItems));
		}
		/// <summary>
		/// プロパティを追加します.
		/// </summary>
		/// <param name="propName">プロパティ名.</param>
		/// <param name="arrayItems">プロパティ値.</param>
		public JsonObjectBuilder Append(string propName, params IJsonObject[] arrayItems)
		{
			return Append(propName, JsonObject.Of(arrayItems));
		}
		/// <summary>
		/// プロパティを追加します.
		/// </summary>
		/// <param name="propName">プロパティ名.</param>
		/// <param name="arrayItems">プロパティ値.</param>
		public JsonObjectBuilder Append(string propName, IEnumerable<string> arrayItems)
		{
			return Append(propName, JsonObject.Of(arrayItems.Select(JsonObject.Of)));
		}
		/// <summary>
		/// プロパティを追加します.
		/// </summary>
		/// <param name="propName">プロパティ名.</param>
		/// <param name="arrayItems">プロパティ値.</param>
		public JsonObjectBuilder Append(string propName, params string[] arrayItems)
		{
			return Append(propName, arrayItems as IEnumerable<string>);
		}
		/// <summary>
		/// プロパティを追加します.
		/// </summary>
		/// <param name="propName">プロパティ名.</param>
		/// <param name="arrayItems">プロパティ値.</param>
		public JsonObjectBuilder Append(string propName, IEnumerable<bool> arrayItems)
		{
			return Append(propName, JsonObject.Of(arrayItems.Select(JsonObject.Of)));
		}
		/// <summary>
		/// プロパティを追加します.
		/// </summary>
		/// <param name="propName">プロパティ名.</param>
		/// <param name="arrayItems">プロパティ値.</param>
		public JsonObjectBuilder Append(string propName, params bool[] arrayItems)
		{
			return Append(propName, arrayItems as IEnumerable<bool>);
		}
		/// <summary>
		/// プロパティを追加します.
		/// </summary>
		/// <param name="propName">プロパティ名.</param>
		/// <param name="arrayItems">プロパティ値.</param>
		public JsonObjectBuilder Append(string propName, IEnumerable<double> arrayItems)
		{
			return Append(propName, JsonObject.Of(arrayItems.Select(JsonObject.Of)));
		}
		/// <summary>
		/// プロパティを追加します.
		/// </summary>
		/// <param name="propName">プロパティ名.</param>
		/// <param name="arrayItems">プロパティ値.</param>
		public JsonObjectBuilder Append(string propName, params double[] arrayItems)
		{
			return Append(propName, arrayItems as IEnumerable<double>);
		}
		/// <summary>
		/// 空の<c>Array</c>型のプロパティを追加します.
		/// </summary>
		/// <param name="propName">プロパティ名.</param>
		public JsonObjectBuilder AppendEmptyArray(string propName)
		{
			return Append(propName, JsonObject.Of(JsonObject.Of(new IJsonObject[0])));
		}
		/// <summary>
		/// JSONノードを構築します.
		/// </summary>
		public IJsonObject Build()
		{
			return ObjectJsonObject.Of(propsDict.Select((e) => new JsonProperty(e.Key, e.Value)));
		}
	}

	sealed class JsonProperty : IJsonProperty
	{
		internal JsonProperty(string name, IJsonObject value)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}
			Name = name;
			Value = value;
		}

		public string Name { get; }
		public IJsonObject Value { get; }
	}
}

