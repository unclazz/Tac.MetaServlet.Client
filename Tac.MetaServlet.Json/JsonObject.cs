using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tac.MetaServlet.Json.Parser;

namespace Tac.MetaServlet.Json
{
	/// <summary>
	/// <see cref="IJsonObject"/>の抽象実装クラスでありユーティリティ・クラスです.
	/// <para>このクラスはJSONのノードを表わす実装クラスの親クラスとして各種の共通処理を実装しています.
	/// ライブラリの利用者はこのクラスが提供する静的メソッドを通じてJSONのパースや、
	/// JSONノードを表わす実装クラスのインスタンスの生成を行うことができます.</para>
	/// </summary>
	public abstract class JsonObject : IJsonObject
	{
		private static readonly JsonParser parser = new JsonParser();
		private static readonly Regex re = new Regex("\"|\\\\|/|\b|\f|\n|\r|\t");
		private static readonly StringJsonObject emptyString = new StringJsonObject(string.Empty);
		private static readonly IJsonObject trueValue = new BooleanJsonObject(true);
		private static readonly IJsonObject falseValue = new BooleanJsonObject(false);
		private static readonly IJsonObject zero = new NumberJsonObject(0);
		private static readonly IJsonObject emptyArray = new ArrayJsonObject(new List<IJsonObject>());

		internal static string Quotes(string val)
		{
			return new StringBuilder()
				.Append('"')
				.Append(re.Replace(val, ReplaceControls))
				.Append('"').ToString();
		}

		static string ReplaceControls(Match m)
		{
			char ch = m.Value[0];
			switch (ch)
			{
				case '\a': return "\\a";
				case '\b': return "\\b";
				case '\t': return "\\t";
				case '\n': return "\\n";
				case '\v': return "\\v";
				case '\f': return "\\f";
				case '\r': return "\\r";
				default: return "\\" + ch;
			}
		}

		/// <summary>
		/// 文字列からJSONを読み取ります.
		/// </summary>
		/// <returns>読み取り結果.</returns>
		/// <param name="json">文字列.</param>
		public static IJsonObject FromString(string json)
		{
			return parser.Parse(Input.FromString(json));
		}
		/// <summary>
		/// ファイルからJSONを読み取ります.
		/// </summary>
		/// <returns>読み取り結果.</returns>
		/// <param name="path">ファイルのパス.</param>
		/// <param name="enc">ファイルのエンコーディング.</param>
		public static IJsonObject FromFile(string path, Encoding enc)
		{
			return parser.Parse(Input.FromFile(path, enc));
		}
		/// <summary>
		/// ファイルからJSONを読み取ります.
		/// </summary>
		/// <returns>読み取り結果.</returns>
		/// <param name="path">ファイルのパス.</param>
		public static IJsonObject FromFile(string path)
		{
			return parser.Parse(Input.FromFile(path));
		}
		public static IJsonObject FromStream(Stream stream, Encoding enc)
		{
			return parser.Parse(Input.FromStream(stream, enc));
		}
		/// <summary>
		/// <code>Object</code>を表わす<see cref="IJsonObject"/>を組み立てるためのビルダーを生成します.
		/// </summary>
		public static JsonObjectBuilder Builder()
		{
			return JsonObjectBuilder.GetInstance();
		}
		/// <summary>
		/// <code>Object</code>型のJSONノードを組み立てるためのビルダーを生成します.
		/// </summary>
		/// <param name="proto">プロパティの初期値を提供する<see cref="IJsonObject"/>.</param>
		public static JsonObjectBuilder Builder(IJsonObject proto)
		{
			return JsonObjectBuilder.GetInstance(proto);
		}
		/// <summary>
		/// <code>String</code>型のJSONノードを生成します.
		/// </summary>
		/// <param name="val">文字列.</param>
		/// <exception cref="NullReferenceException">引数に<code>null</code>が指定された場合</exception>
		public static IJsonObject Of(string val)
		{
			if (val == null)
			{
				throw new NullReferenceException();
			}
			return val.Length == 0 ? emptyString : new StringJsonObject(val);
		}
		/// <summary>
		/// <code>Number</code>型のJSONノードを生成します.
		/// </summary>
		/// <param name="val">数値.</param>
		public static IJsonObject Of(double val)
		{
			return val.CompareTo(0) == 0 ? zero : new NumberJsonObject(val);
		}
		/// <summary>
		/// <code>Boolean</code>型のJSONノードを生成します.
		/// </summary>
		/// <param name="val">ブール値.</param>
		public static IJsonObject Of(bool val)
		{
			return val ? trueValue : falseValue;
		}
		/// <summary>
		/// <code>null</code>型のJSONノードを生成します.
		/// </summary>
		/// <returns>JSONノード.</returns>
		public static IJsonObject OfNull()
		{
			return NullJsonObject.Instance;
		}
		/// <summary>
		/// <code>Array</code>型のJSONノードを生成します.
		/// </summary>
		/// <param name="items">配列の要素となるJSONノード.</param>
		public static IJsonObject Of(IEnumerable<IJsonObject> items)
		{
			IList<IJsonObject> l = items.ToList().AsReadOnly();
			return l.Count == 0 ? emptyArray : new ArrayJsonObject(l);
		}
		/// <summary>
		/// <code>Array</code>型のJSONノードを生成します.
		/// </summary>
		/// <param name="items">配列の要素となるJSONノード.</param>
		public static IJsonObject Of(params IJsonObject[] items)
		{
			IList<IJsonObject> l = items.ToList().AsReadOnly();
			return l.Count == 0 ? emptyArray : new ArrayJsonObject(l);
		}
		/// <summary>
		/// <code>Array</code>型のJSONノードを生成します.
		/// </summary>
		/// <param name="items">配列の要素となる文字列.</param>
		public static IJsonObject Of(IEnumerable<string> items)
		{
			return Of(items.Select(Of));
		}
		/// <summary>
		/// <code>Array</code>型のJSONノードを生成します.
		/// </summary>
		/// <param name="items">配列の要素となる文字列.</param>
		public static IJsonObject Of(params string[] items)
		{
			return Of(items.Select(Of));
		}
		/// <summary>
		/// <code>Array</code>型のJSONノードを生成します.
		/// </summary>
		/// <param name="items">配列の要素となるブール値.</param>
		public static IJsonObject Of(IEnumerable<bool> items)
		{
			return Of(items.Select(Of));
		}
		/// <summary>
		/// <code>Array</code>型のJSONノードを生成します.
		/// </summary>
		/// <param name="items">配列の要素となるブール値.</param>
		public static IJsonObject Of(params bool[] items)
		{
			return Of(items.Select(Of));
		}
		/// <summary>
		/// <code>Array</code>型のJSONノードを生成します.
		/// </summary>
		/// <param name="items">配列の要素となる数値.</param>
		public static IJsonObject Of(IEnumerable<double> items)
		{
			return Of(items.Select(Of));
		}
		/// <summary>
		/// <code>Array</code>型のJSONノードを生成します.
		/// </summary>
		/// <param name="items">配列の要素となる数値.</param>
		public static IJsonObject Of(params double[] items)
		{
			return Of(items.Select(Of));
		}

		public virtual double NumberValue()
		{
			throw new ApplicationException("json node does not represent Number value.");
		}
		public virtual double NumberValue(double fallback)
		{
			return fallback;
		}
		public virtual string StringValue()
		{
			throw new ApplicationException("json node does not represent String value.");
		}

		public virtual string StringValue(string fallback)
		{
			return fallback;
		}

		public virtual bool BooleanValue()
		{
			throw new ApplicationException("json node does not represent Boolean value.");
		}

		public virtual IList<IJsonObject> ArrayValue()
		{
			throw new ApplicationException("json node does not represent Array value.");
		}

		public virtual IList<IJsonObject> ArrayValue(IList<IJsonObject> fallback)
		{
			return fallback;
		}

		public bool IsNull()
		{
			return Type == JsonObjectType.Null;
		}

		public bool IsObjectExactly()
		{
			return Type == JsonObjectType.Object;
		}

		public bool TypeIs(JsonObjectType type)
		{
			return Type == type;
		}

		public bool HasProperty(string name)
		{
			return IsObjectExactly() && PropertyNames.Any((arg) => arg.Equals(name));
		}

		public IJsonObject GetProperty(string name)
		{
			IJsonProperty r = Properties.FirstOrDefault((arg) => arg.Name.Equals(name));
			if (r == null)
			{
				throw new ApplicationException(
					string.Format("json node has not property \"{0}\".", name));
			}
			return r.Value;
		}

		public IJsonObject GetProperty(string name, IJsonObject fallback)
		{
			return HasProperty(name) ? GetProperty(name) : fallback;
		}

		public virtual bool BooleanValue(bool fallback)
		{
			return fallback;
		}

		public JsonObjectType Type { get; }

		public virtual IEnumerable<IJsonProperty> Properties
		{
			get
			{
				return Enumerable.Empty<IJsonProperty>();
			}
		}

		public IEnumerable<string> PropertyNames
		{
			get
			{
				return Properties.Select((arg) => arg.Name);
			}
		}

		public IEnumerable<IJsonObject> PropertyValues
		{
			get
			{
				return Properties.Select((arg) => arg.Value);
			}
		}

		public IJsonObject this[string propertyName]
		{
			get
			{
				return GetProperty(propertyName);
			}
		}

		internal JsonObject(JsonObjectType type)
		{
			Type = type;
		}
	}
}

