using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Tac.MetaServlet.Json;
using Tac.MetaServlet.Rpc;

namespace Tac.MetaServlet.Client
{
	/// <summary>
	/// アプリケーションのメインモジュールです.
	/// </summary>
    class MainClass
    {
		private static readonly int ExitCodeNormal = 0;
		private static readonly int ExitCodeAbnormal = 1;
		private static readonly IJsonFormatOptions formatOptions =
			JsonFormatOptions.Builder().Indent(true).Build();

		/// <summary>
		/// アプリケーションのエントリーポイントです.
		/// </summary>
		/// <param name="args">コマンドライン引数の文字列.</param>
		/// <returns>アプリケーションの終了コード（正常終了であれば<c>0</c>、異常終了であれば<c>1</c>）.</returns>
		public static int Main(string[] args)
        {
            return new MainClass().Execute(args);
        }

        public int Execute(string[] args)
        {
            try
            {
				// コマンドライン引数からパラメータを読み取る
                var ps = ParseParameters(args);
				// ヘルプ表示のフラグが指定されていた場合はヘルプを表示し終了
				if (ps.ShowHelp)
				{
					PrintMessage("構文", Parameters.GetSyntax());
					return ExitCodeNormal;
				}

				// パラメータ情報を元にリクエスト内容となるJSONを読み取る
				var json = ParseRequestJson(ps);
				// パラメータとJSONからリクエストを構築する
                var req = BuildRequest(ps, json);
				// ダンプフラグが指定されていた場合はリクエスト内容をダンプ
				if (ps.ShowDump)  DumpRequest(req);
				// リクエストを同期的に送信してレスポンスを得る
                var resp = req.Send();
				// ダンプフラグをチェック
				if (ps.ShowDump)
				{
					// 指定されていた場合はレスポンス内容をダンプ
					DumpResponse(resp);
				}
				else {
					// 指定されていなかった場合はレスポンス本文のみをダンプ
					PrintMessage(string.Empty, resp.Body);
				}
				return ExitCodeNormal;
            }
            catch (Exception e)
            {
				if (e is ArgumentException)
				{
					// 引数例外の場合はヘルプを表示する
					PrintErrorMessage("構文", Parameters.GetSyntax());
				}
				// エラー内容を表示する
				PrintErrorMessage("エラー", e);
				return ExitCodeAbnormal;
            }
        }

		/// <summary>
		/// RPCリクエストを標準出力にダンプします.
		/// </summary>
		/// <param name="req">レスポンス.</param>
		public void DumpRequest(IRequest req)
        {
            var b = JsonObject.Builder(req.Parameters);
            if (req.Parameters.HasProperty("authPass"))
            {
                b.Append("authPass", "*****");
            }
			PrintMessage("リクエストURI", req.Uri);
			PrintMessage("リクエストJSON", b.Build().Format(formatOptions));
        }

		/// <summary>
		/// RPCレスポンスを標準出力にダンプします.
		/// </summary>
		/// <param name="resp">レスポンス.</param>
        public void DumpResponse(IResponse resp)
        {
            PrintMessage("HTTPステータス", resp.StatusCode);
            PrintMessage("レスポンスJSON", resp.Body.Format(formatOptions));
        }

		/// <summary>
		/// 標準出力にメッセージを出力します.
		/// </summary>
		/// <param name="label">ラベル.</param>
		/// <param name="target">メッセージに使用されるオブジェクト.</param>
		public void PrintMessage(string label, object target)
		{
			Console.WriteLine(FormatMessage(label, target));
		}

		/// <summary>
		/// 標準エラー出力にメッセージを出力します.
		/// </summary>
		/// <param name="label">ラベル.</param>
		/// <param name="target">メッセージに使用されるオブジェクト.</param>
		public void PrintErrorMessage(string label, object target)
		{
			Console.Error.WriteLine(FormatMessage(label, target));
		}

		/// <summary>
		/// コンソール出力するためにメッセージを整形します.
		/// ラベルとして空文字列もしくは<c>null</c>が指定された場合は、
		/// ターゲットとなるオブジェクトの文字列表現を返すだけでとくに整形を行いません。
		/// </summary>
		/// <returns>整形済みのメッセージ.</returns>
		/// <param name="label">ラベル.</param>
		/// <param name="target">メッセージに使用されるオブジェクト.</param>
		public string FormatMessage(string label, object target)
		{
			if (label == null || label.Length == 0)
			{
				return target.ToString();
			}

			// ラベルの幅を計算し（日本語環境を前提としている）
			// その幅でもって出力の2行目以降インデントするための空白文字シーケンスを準備する
			var shiftJis = Encoding.GetEncoding("Shift_JIS");
			var labelWidth = shiftJis.GetByteCount(label);
			var labelColon = new StringBuilder(label).Append(": ");
			var indent = Enumerable.Range(0, labelWidth)
					  .Aggregate(new StringBuilder(), (b, s) => b.Append(' '));

			// 出力対象のオブジェクトを一度文字列化
			// その結果を改行文字で行ごとに分解し（空文字列は排除する）
			// 1行目の先頭にはラベルを付加し、2行目以降の先頭にはインデントを付加する
			var buff = target
				.ToString()
				.Split(new char[] {'\n', '\r'},
				       StringSplitOptions.RemoveEmptyEntries)
				.Aggregate(new StringBuilder(),
				           (b, s) => b
				           .Append(b.Length == 0 ? labelColon : indent)
				           .AppendLine(s));

			// 最終行に余計な改行文字列が入るのでこれを除去してから呼び出し元に返す
			return buff.ToString().TrimEnd();
		}

		/// <summary>
		/// RPCリクエストを構築します.
		/// </summary>
		/// <returns>構築した結果.</returns>
		/// <param name="ps">パラメータが格納されたオブジェクト.</param>
		/// <param name="j">リクエスト内容となるJSON.</param>
        public IRequest BuildRequest(Parameters ps, IJsonObject j)
        {
            var b = Request.Builder()
                          .Host(ps.RemoteHost)
                          .Port(ps.RemotePort)
                          .Path(ps.RemotePath)
                          .Timeout(ps.RequestTimeout);
            j.Properties
             .ToList()
             .ForEach((obj) => b.Parameter(obj.Name, obj.Value));
            return b.Build();
        }

		/// <summary>
		/// RPCリクエストの内容となるJSONを読み取ります.
		/// </summary>
		/// <returns>パース結果.</returns>
		/// <param name="ps">パラメータが格納されたオブジェクト.</param>
        public IJsonObject ParseRequestJson(Parameters ps)
        {
            return JsonObject.FromFile(ps.RequestJson, Encoding.UTF8);
        }

		/// <summary>
		/// パラメータを読み取ります.
		/// </summary>
		/// <returns>読み取った結果.</returns>
		/// <param name="args">コマンドライン引数の文字列配列.</param>
        public Parameters ParseParameters(string[] args)
        {
            Parameters ps = new Parameters();

            var settings = System.Configuration.ConfigurationManager.AppSettings;
			ps.GetMetaData().ToList().ForEach((meta) => ResolveParameter(meta, args, settings));

			ps.Validate();

			return ps;
        }

		/// <summary>
		/// パラメータを解決します.
		/// このメソッドは<see cref="ParameterMeta"/>インスタンスと
		/// コマンドライン引数の配列、そしてアプリケーション構成ファイルで定義された設定情報をもとに、
		/// アプリケーションのパラメータを解決して、パラメータ値を<see cref="Parameters"/>に設定します。
		/// </summary>
		/// <returns>パラメータの解決に成功した場合<c>true</c>.</returns>
		/// <param name="meta">パラメータのメタ情報.</param>
		/// <param name="args">コマンドライン引数の文字列配列.</param>
		/// <param name="settings">アプリケーション構成ファイルの設定情報.</param>
		public bool ResolveParameter(ParameterMeta meta,
				string[] args, NameValueCollection settings)
		{
			// コマンドライン引数のペアを作るための一時変数
			// 要素位置を1だけずらした2つのシーケンスを用意する
			// 例：
			// argsLeft  ["/J", "foo/bar/baz.json", "/H", "127.0.0.1"]
			// argsRight ["foo/bar/baz.json", "/H", "127.0.0.1", ""]
			// この2つをzip処理すると [("/J", "foo/bar/baz.json"), ...] というペアのシーケンスが得られる
			IEnumerable<string> argsLeft = args;
			IEnumerable<string> argsRight = args.Skip(1).Concat(new string[1]{ "" });

			// アプリケーション構成ファイルで定義された設定情報のうち
			// 特定の名称を持つものを抽出してパラメータを表わす匿名型のインスタンスでラップする
			var sets = settings.AllKeys
				.Where((a) => a.Equals(meta.SettingName))
				.Select((a) => new { Name = a, Value = settings[a], Priority = 2 });

			// Zip    : コマンドライン引数のペアを作成しパラメータを表わす匿名型のインスタンスでラップする
			// Where  : その中で特定の名称を持つものだけを抽出して
			// Concat : アプリケーション構成ファイル由来のパラメータの結果セットと集合和を構成する
			// OrderBy: 優先度にしたがい結果セットをソート
			// FirstOrDefault: 結果セットの先頭の（最初の）要素を取得（シーケンスが空ならnullを取得）
			var param = argsLeft
				.Zip(argsRight, (a1, a2) => new { Name = a1, Value = a2, Priority = 1 })
				.Where((a) => a.Name.ToUpper().Equals(meta.OptionName))
				.Concat(sets)
				.OrderBy((a) => a.Priority)
				.FirstOrDefault();

			// パラメータ（匿名型インスタンスで表される）の解決に成功したかどうかチェック
			if (param != null)
			{
				// 解決に成功した場合はそのパラメータ値をバリューオブジェクトに格納
				meta.SetterDelegate(param.Value);
				// 呼び出し元にパラメータ解決に成功した旨通知
				return true;
			}
			else
			{
				// パラメータ解決に失敗した旨通知
				return false;
			}
		}
    }
}
