using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tac.MetaServlet.Client
{
	/// <summary>
	/// アプリケーションのパラメータを格納するオブジェクトです.
	/// </summary>
	public sealed class Parameters
	{
		private readonly List<ParameterMeta> metaList = new List<ParameterMeta>();

		/// <summary>
		/// リクエスト先のホスト名を取得・設定します.
		/// </summary>
		/// <value>ホスト名（もしくはIPアドレス）.</value>
		public string RemoteHost { get; set; }
		/// <summary>
		/// リクエスト先のパス名を取得・設定します.
		/// </summary>
		/// <value>パス名.</value>
		public string RemotePath { get; set; }
		/// <summary>
		/// リクエスト先のポート番号を取得・設定します.
		/// </summary>
		/// <value>リクエスト先のポート番号.</value>
		public int RemotePort { get; set; }
		/// <summary>
		/// リクエストのタイムアウト時間を取得・設定します.
		/// </summary>
		/// <value>タイムアウト時間（ミリ秒単位）.</value>
		public int RequestTimeout { get; set; }
		/// <summary>
		/// リクエスト内容となるJSONのパスを取得・設定します.
		/// </summary>
		/// <value>JSONのパス.</value>
		public string RequestJson { get; set; }
		/// <summary>
		/// ヘルプ表示フラグを取得・設定します.
		/// </summary>
		/// <value>ヘルプ表示フラグ.</value>
		public bool ShowHelp { get; set; }
		/// <summary>
		/// ダンプ出力フラグを取得・設定します.
		/// </summary>
		/// <value>ダンプ出力フラグ.</value>
		public bool ShowDump { get; set; }

		/// <summary>
		/// インスタンスを生成し各プロパティをデフォルト値とともに初期化します.
		/// </summary>
		public Parameters()
		{
            RemoteHost = "localhost";
            RemotePath = "/org.talend.administrator/metaServlet";
            RemotePort = 8080;
            RequestTimeout = 100000;
			ShowDump = false;
			ShowHelp = false;
		}

		/// <summary>
		/// 構文解説テキストを生成して返します.
		/// </summary>
		/// <returns>構文解説テキスト.</returns>
		public static string GetSyntax()
		{
			var b = new StringBuilder()
				.AppendLine("TACRPC {/J <json-file> | /?} [/H <host>] [/P <port>] [/Q <path>] [/T <timeout>] [/D]")
				.AppendLine("/J  RPCリクエストを表わすJSONが記述されたファイルのパス.")
				.AppendLine("/H  RPCリクエスト先のホスト名. デフォルトは\"localhost\".")
				.AppendLine("/P  RPCリクエスト先のポート名. デフォルトは8080.")
				.AppendLine("/Q  RPCリクエスト先のパス名. デフォルトは\"/org.talend.administrator/metaServlet\".")
				.AppendLine("/T  RPCリクエストのタイムアウト時間. 単位はミリ秒. デフォルトは100000.")
				.AppendLine("/D  リクエストとレスポンスのダンプ出力を行う.")
				.AppendLine("/?  このヘルプを表示する.");

			return b.ToString();
		}

		/// <summary>
		/// パラメータの妥当性をチェックします.
		/// </summary>
		/// <exception cref="ArgumentException">必須パラメータの欠如や妥当でないパラメータの存在が検知された場合</exception>
		public void Validate()
		{
			if (ShowHelp) return;
			if (string.IsNullOrEmpty(RequestJson))
			{
				throw new ArgumentException("コマンドライン引数 /J の指定が必要です.");
			}
			if (!File.Exists(RequestJson))
			{
				throw new ArgumentException("コマンドライン引数 /J に指定されたパスは存在しません.");
			}
			if (string.IsNullOrEmpty(RemoteHost))
			{
				throw new ArgumentException("コマンドライン引数 /H の指定が必要です.");
			}
			if (RequestTimeout < 0)
			{
				throw new ArgumentException("コマンドライン引数 /T には正の整数を指定してください.");
			}
			if (RemotePort <= 0)
			{
				throw new ArgumentException("コマンドライン引数 /P には正の整数を指定してください.");
			}
		}

		/// <summary>
		/// パラメータのメタ情報のシーケンスを取得します.
		/// </summary>
		/// <returns>メタデータのシーケンス.</returns>
        public IEnumerable<ParameterMeta> GetMetaData()
		{
			if (metaList.Count == 0)
			{
                metaList.Add(meta("/J", "Tac.MetaServlet.Client.Request.Json", (obj) => RequestJson = obj));
                metaList.Add(meta("/H", "Tac.MetaServlet.Client.Remote.Host", (obj) => RemoteHost = obj));
				metaList.Add(meta("/P", "Tac.MetaServlet.Client.Remote.Port", (obj) => RemotePort = int.Parse(obj)));
				metaList.Add(meta("/Q", "Tac.MetaServlet.Client.Remote.Path", (obj) => RemotePath = obj));
				metaList.Add(meta("/T", "Tac.MetaServlet.Client.Request.Timeout", (obj) => RequestTimeout = int.Parse(obj)));
				metaList.Add(meta("/D", "Tac.MetaServlet.Client.Show.Dump", (obj) => ShowDump = true));
				metaList.Add(meta("/?", "Tac.MetaServlet.Client.Show.Help", (obj) => ShowHelp = true));
			}
			return metaList.AsReadOnly();
		}

		private ParameterMeta meta(string optName, string setName, Action<string> setter)
		{
			return new ParameterMeta(optName, setName, setter);
		}
	}

	/// <summary>
	/// パラメータのメタ情報を表わすオブジェクトです.
	/// </summary>
	public sealed class ParameterMeta
	{
		/// <summary>
		/// コマンドラインオプションとしての名前を取得します.
		/// </summary>
		/// <value>オプション名.</value>
		public string OptionName { get; }
		/// <summary>
		/// アプリケーション構成ファイルの設定情報としての名前を取得します.
		/// </summary>
		/// <value>設定情報名.</value>
		public string SettingName { get; }
		/// <summary>
		/// パラメータを格納するオブジェクトに値を設定するためのデリゲートを取得します.
		/// </summary>
		/// <value>デリゲート.</value>
		public Action<string> SetterDelegate { get; }

		/// <summary>
		/// インスタンスを生成し引数の妥当性を検証します.
		/// </summary>
		/// <param name="optName">オプション名.</param>
		/// <param name="setName">設定情報名.</param>
		/// <param name="setter">setterデリゲート.</param>
		internal ParameterMeta(string optName, string setName, Action<string> setter)
		{
			if (optName == null)
			{
				throw new ArgumentNullException(nameof(optName));
			}
			if (setName == null)
			{
				throw new ArgumentNullException(nameof(setName));
			}
			if (setter == null)
			{
				throw new ArgumentNullException(nameof(setter));
			}
			OptionName = optName;
			SettingName = setName;
			SetterDelegate = setter;
		}
	}
}

