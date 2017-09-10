using System;
using System.Text;
using System.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;
using Tac.MetaServlet.Rpc;
using Unclazz.Commons.CLI;
using Unclazz.Commons.Json;

namespace Tac.MetaServlet.V56.Client
{
	/// <summary>
	/// コマンドのエントリーポイントを提供するクラスです。
	/// </summary>
	public class MainClass
	{
		private static readonly int exitCodeOnEndedNormally = 0;
		private static readonly int exitCodeOnEndedAbnormally = 1;
		private static readonly IJsonFormatOptions formatOptions =
			JsonFormatOptions.Builder().Indent(true).Build();

		/// <summary>
		/// エントリーポイントです。
		/// </summary>
		/// <param name="args">コマンドライン引数</param>
		/// <returns>終了コード</returns>
		public static int Main(string[] args)
		{
			return new MainClass().Execute(args);
		}

		private readonly Func<IRequest, IResponse> agent;

		/// <summary>
		/// コンストラクタです。
		/// 引数でデリゲートを指定した場合APIリクエストはそれを通じて行われます。
		/// このコンストラクタはAPIとの間で行われるやり取りの詳細をカスタマイズしたり、
		/// 単体テストのためのモックを注入するための手段を提供します。
		/// </summary>
		/// <param name="agent">Agent.</param>
		public MainClass(Func<IRequest, IResponse> agent)
		{
			this.agent = agent;
		}
		/// <summary>
		/// コンストラクタです。
		/// </summary>
		public MainClass() : this(null) { }

		/// <summary>
		/// コマンドの主処理を実行します。
		/// </summary>
		/// <param name="args">コマンドライン引数</param>
		public int Execute(string[] args)
		{
			// 1. コマンド実行中に使用するオブジェクトの初期化

			// パラメータ・オブジェクトを初期化
			var ps = new Parameters();
			// コマンドラインの定義を作成
			var cl = MakeCommandLine(ps);
			// 実行コンテキストを初期化
			var ctx = new Context();

			try
			{
				// 2. コマンドライン引数のパースとロガーの再構成

				// コマンドライン引数とアプリケーション構成ファイルからパラメータを読み取り
				cl.Parse(args);
				// NLogロガーを構成し実行コンテキストに設定
				ReConfigureNLog(ps);

				// 3. タスク起動前の状態チェック

				// APIリクエスト：タスク名をキーにしてタスクIDを取得
				var resp0 = RequestGetTaskIdByName(ps, ctx);
				// 取得したIDを実行コンテキストに設定
				ctx.TaskId = (int)resp0.GetProperty("taskId").AsNumber();
				// APIリクエスト：タスクのステータスを取得
				var resp1 = RequestGetTaskStatus(ps, ctx);
				// ステータスをチェック
				if (!resp1.GetProperty("status").AsString().Equals("READY_TO_RUN"))
				{
					// "Ready to run"以外のステータスの場合はエラー
					throw MakeException(exitCodeOnEndedAbnormally,
										"Task is already running.");
				}

				// 4. タスクの起動とその完了の待機

				// APIリクエスト：タスクを非同期モードで起動する
				var resp2 = RequestRunTask(ps, ctx);
				// 返された実行リクエストIDを実行コンテキストに設定
				ctx.ExecRequestId = resp2.GetProperty("execRequestId").AsString();
				// APIリクエスト：所定の時間内タスクの完了を
				var resp3 = RequestGetTaskExecutionStatusRepeatedly(ps, ctx);

				// 5. リターンコードのログの処理

				var jobExitCode = (int)resp3.GetProperty("jobExitCode").AsNumber();
				// APIリクエスト：今回の実行時のログを取得（と同時にクライアント側でもロギング）
				RequestTaskLog(ps, ctx);
				// タスクの終了コードをチェック
				if (jobExitCode > 0)
				{
					// 0より大きい数字の場合はエラー
					throw MakeException(jobExitCode, "Task has ended abnormally.");
				}
				// 正常に処理が完了
				return exitCodeOnEndedNormally;
			}
			catch (ParseException ex)
			{
				// コマンドラインの読み取り時にエラーが発生した場合
				// ヘルプを表示
				Console.WriteLine(new HelpFormatter().Format(cl));
				// コマンドライン引数が指定されていた場合はエラー内容も表示
				if (args.Length > 0)
				{
					ctx.Logger.Error(ex);
				}
				// エラーとともに処理が中断
				return exitCodeOnEndedAbnormally;
			}
			catch (ClientException ex)
			{
				// APIリクエスト時にエラーが発生した場合
				// 例外オブジェクトの内容をロギング
				ctx.Logger.Error(ex);
				// エラーとともに処理が中断
				return ex.ExitCode;
			}
			catch (Exception ex)
			{
				// それ以外のエラーが発生した場合
				// 例外オブジェクトの内容をロギング
				if (ctx == null && ctx.Logger == null)
				{
					Console.Error.WriteLine(ex);
				}
				else {
					ctx.Logger.Error(ex);
				}
				// エラーとともに処理が中断
				return exitCodeOnEndedAbnormally;
			}
		}
		/// <summary>
		/// コマンドラインの定義情報を生成します。
		/// 生成された定義情報に基づきユーザが指定したコマンドライン引数がパースされたとき、
		/// このメソッドの引数として渡されたオブジェクトに読み取られたパラメータが設定されます。
		/// </summary>
		/// <returns>コマンドライン定義情報</returns>
		/// <param name="ps">パラメータ</param>
		public ICommandLine MakeCommandLine(Parameters ps)
		{
			return CommandLine
				.Builder("tacrpc.v56.exe")
				.Description("A RPC client command to execute task on " +
				             "TAC(Talend Administration Center).")
				.CaseSensitive(false)
				.AddOption(Option
						   .Builder("/h")
				           .SettingName(SettingKeys.Remote.Host)
						   .Required(false)
						   .HasArgument()
						   .ArgumentName("hostname")
						   .Description(string
										.Format("Hostname of API. Default is \"{0}\".",
												ps.Remote.Path))
						   .SetterDelegate(s => ps.Remote.Host = s))
				.AddOption(Option
						   .Builder("/p")
				           .SettingName(SettingKeys.Remote.Port)
						   .Required(false)
						   .HasArgument()
						   .ArgumentName("port")
						   .Description(string
										.Format("Port number of API. Default is {0}.",
												ps.Remote.Port))
				           .SetterDelegate(n => ps.Remote.Port = n))
				.AddOption(Option
						   .Builder("/q")
				           .AlternativeName("/path")
				           .SettingName(SettingKeys.Remote.Path)
						   .Required(false)
						   .HasArgument()
						   .ArgumentName("path")
						   .Description(string
										.Format("Context path of API. Default is \"{0}\".",
				                                ps.Remote.Path))
						   .SetterDelegate(s => ps.Remote.Path = s))
				.AddOption(Option
						   .Builder("/n")
				           .SettingName(SettingKeys.Request.TaskName)
						   .Required()
						   .HasArgument()
						   .ArgumentName("task")
						   .Description("Task name to execute.")
						   .SetterDelegate(s => ps.Request.TaskName = s))
				.AddOption(Option
						   .Builder("/a")
				           .AlternativeName("/authuser")
				           .SettingName(SettingKeys.Request.AuthUser)
						   .Required()
						   .HasArgument()
						   .ArgumentName("user")
						   .Description("Username for authentication of API access.")
				           .SetterDelegate(s => ps.Request.AuthUser = s))
				.AddOption(Option
						   .Builder("/b")
						   .AlternativeName("/authpass")
						   .SettingName(SettingKeys.Request.AuthPass)
						   .Required()
						   .HasArgument()
						   .ArgumentName("password")
						   .Description("Password for authentication of API access.")
				           .SetterDelegate(s => ps.Request.AuthPass = s))
				.AddOption(Option
						   .Builder("/i")
				           .SettingName(SettingKeys.Request.Interval)
						   .Required(false)
						   .HasArgument()
						   .ArgumentName("interval")
						   .Description(string
										.Format("Interval for executing API request. " +
				                                "Specify value by seconds. Default is {0}.",
				                                ps.Request.Interval))
				           .SetterDelegate(n => ps.Request.Interval = n))
				.AddOption(Option
						   .Builder("/t")
				           .SettingName(SettingKeys.Request.Timeout)
						   .Required(false)
						   .HasArgument()
						   .ArgumentName("timeout")
						   .Description(string
										.Format("Timeout for executing API request. " +
				                                "Specify value by seconds. Default is {0}.",
				                                ps.Request.Timeout))
				           .SetterDelegate(n => ps.Request.Timeout = n))
				.AddOption(Option
						   .Builder("/u")
				           .SettingName(SettingKeys.Execution.Timeout)
						   .Required(false)
						   .HasArgument()
						   .ArgumentName("timeout")
				           .Description(string
				                        .Format("Timeout for executing THIS command." +
				                                "Specify value by seconds. Default is {0}.",
				                                ps.Execution.Timeout))
				           .SetterDelegate(n => ps.Execution.Timeout = n))
				.AddOption(Option
						   .Builder("/l")
				           .SettingName(SettingKeys.Execution.LogFileName)
						   .Required(false)
						   .HasArgument()
						   .ArgumentName("filename")
				           .Description(string
				                        .Format("Name of log file. Default is \"{0}\".",
				                                ps.Execution.LogFileName))
				           .SetterDelegate(s => ps.Execution.LogFileName = s))
				.AddOption(Option
						   .Builder("/j")
				           .SettingName(SettingKeys.Execution.InstanceName)
						   .Required(false)
						   .HasArgument()
						   .ArgumentName("instance")
				           .Description(string
				                        .Format("Name of command instance. Default is \"{0}\".",
				                                ps.Execution.InstanceName))
				           .SetterDelegate(s => ps.Execution.InstanceName = s))
				.AddOption(Option
						   .Builder("/dryrun")
				           .SettingName(SettingKeys.Execution.DryRun)
						   .Required(false)
						   .HasArgument(false)
						   .Description("Use mock for a simulation. Request is NOT sent for anything.")
				           .SetterDelegate((bool b) => ps.Execution.DryRun = b))
				.Build();
		}
		/// <summary>
		/// NLogのロガーを構成し直します。
		/// </summary>
		/// <param name="ps">パラメータ</param>
		public void ReConfigureNLog(Parameters ps)
		{
			// 現在日付
			var now = DateTime.Now;
			// ランダムな数値を16進数表記文字列化
			var octed4 = new Random().Next(65535).ToString("x4");

			// NLogの構成情報を取得
			var conf = LogManager.Configuration;
			// 独自のレイアウト変数を追加
			conf.Variables.Add("instanceName", ps.Execution.InstanceName);
			conf.Variables.Add("executionName", octed4);
			conf.Variables.Add("yyyyMMdd", now.ToString("yyyyMMdd"));
			conf.Variables.Add("hhmmssfff", now.ToString("hhmmssfff"));

			// 新しいターゲットを初期化
			var file = new FileTarget("file")
			{
				FileName = ps.Execution.LogFileName,
				Encoding = Encoding.UTF8
			};
			// 既存の構成情報に追加
			conf.AddTarget(file);
			conf.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, file));

			// 構成情報をリロードさせる
			LogManager.Configuration = conf;
		}
		/// <summary>
		/// APIリクエスト"getTaskIdByName"を実行します。
		/// </summary>
		/// <returns>APIから返却されたJSON</returns>
		/// <param name="ps">パラメータ</param>
		/// <param name="ctx">実行コンテキスト</param>
		public IJsonObject RequestGetTaskIdByName(Parameters ps, Context ctx)
		{
			var req = MakeRequest(ps, JsonObject.Builder()
						.Append("actionName", "getTaskIdByName")
						.Append("taskName", ps.Request.TaskName)
						.Build());
			var resp = SendRequest(ps, ctx, req);
			if (resp.HasProperty("taskId"))
			{
				return resp;
			}
			throw MakeException("Unexpected result of API calling. " +
			                    "\"getTaskIdByName\" does not return \"taskId\".");
		}
		/// <summary>
		/// APIリクエスト"getTaskStatus"を実行します。
		/// </summary>
		/// <returns>APIから返却されたJSON</returns>
		/// <param name="ps">パラメータ</param>
		/// <param name="ctx">実行コンテキスト</param>
		public IJsonObject RequestGetTaskStatus(Parameters ps, Context ctx)
		{
			var req = MakeRequest(ps, JsonObject.Builder()
						.Append("actionName", "getTaskStatus")
			                      .Append("taskId", ctx.TaskId)
						.Build());
			var resp = SendRequest(ps, ctx, req);
			if (resp.HasProperty("status"))
			{
				return resp;
			}
			throw MakeException("Unexpected result of API calling. " +
								"\"getTaskStatus\" does not return \"status\".");
		}
		/// <summary>
		/// APIリクエスト"runTask"を実行します。
		/// </summary>
		/// <returns>APIから返却されたJSON</returns>
		/// <param name="ps">パラメータ</param>
		/// <param name="ctx">実行コンテキスト</param>
		public IJsonObject RequestRunTask(Parameters ps, Context ctx)
		{
			var req = MakeRequest(ps, JsonObject.Builder()
						.Append("actionName", "runTask")
			                      .Append("taskId", ctx.TaskId)
			                      .Append("mode", "asynchronous")
						.Build());
			var resp = SendRequest(ps, ctx, req);
			if (resp.HasProperty("execRequestId"))
			{
				return resp;
			}
			throw MakeException("Unexpected result of API calling. " +
								"\"runTask\" does not return \"execRequestId\".");
		}
		/// <summary>
		/// APIリクエスト"getTaskExecutionStatus"を実行します。
		/// このメソッドはパラメータ・オブジェクトに格納された設定情報に基づき
		/// タイムアウトを迎えるまで繰り返しタスクの状態を確認します。
		/// </summary>
		/// <returns>APIから返却されたJSON</returns>
		/// <param name="ps">パラメータ</param>
		/// <param name="ctx">実行コンテキスト</param>
		public IJsonObject RequestGetTaskExecutionStatusRepeatedly(Parameters ps, Context ctx)
		{
			// do...whileループで所定の時間内繰り返しステータス確認
			do
			{
				try
				{
					// スレッドを一時停止して待機
					Thread.Sleep(ps.Request.Interval * 1000);
				}
				catch (ThreadInterruptedException ex)
				{
					// 何らかの理由で待機が中断された場合は例外スロー
					throw MakeException(exitCodeOnEndedAbnormally, 
					                    "Sleep was interrupted.", ex);
				}
				// "getTaskExecutionStatus"リクエストを実行
				var resp = RequestGetTaskExecutionStatus(ps, ctx);
				// レスポンスのJSONをチェック
				if (resp.HasProperty("jobExitCode"))
				{
					// タスクが終わったことを示す値があればループを抜ける
					return resp;
				}
				// さもなくばタイムリミットの確認のうえで次回再度確認する
			} while (WithinTimeLimit(ps, ctx.StartedOn));
			// タイムリミット到達でループを抜けた場合はエラーにする
			throw MakeException(exitCodeOnEndedAbnormally,
			                    "Execution timed out.");
		}
		/// <summary>
		/// APIリクエスト"getTaskExecutionStatus"を実行します。
		/// </summary>
		/// <returns>APIから返却されたJSON</returns>
		/// <param name="ps">パラメータ</param>
		/// <param name="ctx">実行コンテキスト</param>
		public IJsonObject RequestGetTaskExecutionStatus(Parameters ps, Context ctx)
		{
			var req = MakeRequest(ps, JsonObject.Builder()
						.Append("actionName", "getTaskExecutionStatus")
								  .Append("taskId", ctx.TaskId)
								  .Append("execRequestId", ctx.ExecRequestId)
						.Build());
			return SendRequest(ps, ctx, req);
		}
		/// <summary>
		/// コマンド実行の所定の時間内に収まっているかどうかをチェックします。
		/// </summary>
		/// <returns>所定時間内の場合は<c>true</c></returns>
		/// <param name="ps">パラメータ</param>
		/// <param name="startedOn">開始日時</param>
		public bool WithinTimeLimit(Parameters ps, DateTime startedOn)
		{
			var delta = DateTime.Now.Subtract(startedOn);
			return delta.TotalSeconds <= ps.Execution.Timeout;
		}
		/// <summary>
		/// APIリクエスト"taskLog"を実行します。
		/// </summary>
		/// <returns>APIから返却されたJSON</returns>
		/// <param name="ps">パラメータ</param>
		/// <param name="ctx">実行コンテキスト</param>
		public IJsonObject RequestTaskLog(Parameters ps, Context ctx)
		{
			var req = MakeRequest(ps, JsonObject.Builder()
						.Append("actionName", "taskLog")
								  .Append("taskId", ctx.TaskId)
								  .Append("lastExecution", true)
						.Build());
			return SendRequest(ps, ctx, req);
		}
		/// <summary>
		/// APIリクエストを行います。
		/// このメソッドはAPIから返されたレスポンスの内容をチェックし、
		/// タスクがエラーとともに終了した場合やAPIリクエストのリターンコードが<c>0</c>でない場合、
		/// さらにまたAPIリクエストそのものが何らかの理由で失敗した場合には、例外をスローします。
		/// </summary>
		/// <returns>APIから返却されたJSON</returns>
		/// <param name="ps">パラメータ</param>
		/// <param name="ctx">実行コンテキスト</param>
		/// <param name="req">APIリクエスト</param>
		public IJsonObject SendRequest(Parameters ps, Context ctx, IRequest req)
		{
			var log = ctx.Logger;
			try
			{
				log.Info("RequestContent = {0}", SerializeSecurely(req.Parameters));
				var resp = req.Send();
				var exitCode = resp.StatusCode == System.Net.HttpStatusCode.OK
								   ? resp.ReturnCode : exitCodeOnEndedAbnormally;
				if (exitCode == 0)
				{
					log.Info("HttpStatus = {0}", resp.StatusCode);
					log.Info("ReturnCode = {0}", TranslateReturnCode(resp.ReturnCode));
					log.Info("ResponseBody = {0}", SerializeSecurely(resp.Body));
					return resp.Body;
				}
				else {
					log.Error("HttpStatus = {0}", resp.StatusCode);
					log.Error("ReturnCode = {0}", TranslateReturnCode(resp.ReturnCode));
					log.Error("ResponseBody = {0}", SerializeSecurely(resp.Body));
					throw MakeException(exitCode, "Bad API response.");
				}
			}
			catch (ClientException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw MakeException(exitCodeOnEndedAbnormally, "API request failed.", ex);
			}
		}
		/// <summary>
		/// JSONデータをシリアライズします。
		/// ただし認証情報が含まれている場合はそれをマスキングします。
		/// </summary>
		/// <returns>シリアライズされたJSONデータ</returns>
		/// <param name="j">デシリアライズされたJSONデータ</param>
		public string SerializeSecurely(IJsonObject j)
		{
			var b = JsonObject.Builder(j);
			if (j.HasProperty("authPass"))
			{
				b.Append("authPass", "*****");
			}
			return b.Build().Format(formatOptions);
		}
		/// <summary>
		/// APIリクエストのためのオブジェクトを生成します。
		/// </summary>
		/// <returns>APIリクエスト・オブジェクト</returns>
		/// <param name="ps">コマンド実行パラメータ</param>
		/// <param name="reqParams">APIリクエストに付加されるパラメータ</param>
		public IRequest MakeRequest(Parameters ps, IJsonObject reqParams)
		{
			var b = Request.Builder()
						   .Host(ps.Remote.Host)
						   .Port(ps.Remote.Port)
						   .Path(ps.Remote.Path)
						   .AuthUser(ps.Request.AuthUser)
						   .AuthPass(ps.Request.AuthPass)
						   .Timeout(ps.Request.Timeout * 1000);
			if (agent != null)
			{
				b.Agent(agent);
			}
			else if (ps.Execution.DryRun)
			{
				b.Agent(DelegateAgent);
			}
			foreach (var reqParam in reqParams.Properties)
			{
				b.Parameter(reqParam.Name, reqParam.Value);
			}
			return b.Build();
		}
		/// <summary>
		/// APIレスポンスに含まれるリターンコードをそれが意味する文字列に変換します。
		/// </summary>
		/// <returns>文字列</returns>
		/// <param name="code">リターンコード</param>
		public string TranslateReturnCode(int code)
		{
			switch (code)
			{
				case 0: return "Success";
				case 1: return "Unknown error";
				case 2: return "Invalid request";
				case 3: return "Authentication error";
				case 4: return "License problem";
				case 5: return "Invalid parameter";
				case 6: return "Error formatting response";
				case 7: return "Insufficient right";
				case 30: return "Error while launching task";
				case 31: return "Thread interupted while running";
				case 32: return "No right to run this task";
				case 33: return "The parameter 'mode' must have the value 'synchronous' or 'asynchronous'";
				case 220: return "Error happened when reading logs.";
				case 225: return "Error happened when date range is illegal.";
				case 226: return "Error appears when parsing the date.";
				default:return "Return code not found in API reference...";
			}
		}
		/// <summary>
		/// 例外オブジェクトを生成します。
		/// </summary>
		/// <returns>例外オブジェクト</returns>
		/// <param name="exitCode">終了コード</param>
		/// <param name="message">メッセージ</param>
		/// <param name="cause">原因となった例外</param>
		public ClientException MakeException(int exitCode, string message, Exception cause)
		{
			if (cause == null)
			{
				return new ClientException(exitCode, message);
			}
			else {
				return new ClientException(exitCode, message, cause);
			}
		}
		/// <summary>
		/// 例外オブジェクトを生成します。
		/// </summary>
		/// <returns>例外オブジェクト</returns>
		/// <param name="exitCode">終了コード</param>
		/// <param name="message">メッセージ</param>
		public ClientException MakeException(int exitCode, string message)
		{
			return MakeException(exitCode, message, null);
		}
		/// <summary>
		/// 例外オブジェクトを生成します。
		/// </summary>
		/// <returns>例外オブジェクト</returns>
		/// <param name="message">メッセージ</param>
		public ClientException MakeException(string message)
		{
			return MakeException(exitCodeOnEndedAbnormally, message, null);
		}
		/// <summary>
		/// DryRunモードでHTTPクライアントのモックとして使用されるメソッドです。
		/// </summary>
		/// <returns>APIレスポンス</returns>
		/// <param name="req">APIリクエスト</param>
		public IResponse DelegateAgent(IRequest req)
		{
			var b = Response
			   .Builder()
			   .Request(req)
			   .StatusCode(System.Net.HttpStatusCode.OK);
			var b2 = JsonObject
				.Builder()
				.Append("returnCode", 0)
				.Append("taskId", 123);

			if (req.ActionName.Equals("getTaskIdByName"))
			{
				return b.Body(b2.Build())
						.Build();
			}
			else if (req.ActionName.Equals("getTaskStatus"))
			{
				return b.Body(b2.Append("status", "READY_TO_RUN")
				              .Build())
						.Build();
			}
			else if (req.ActionName.Equals("runTask"))
			{
				return b.Body(b2.Append("execRequestId", "123_abc")
							  .Build())
						.Build();
			}
			else if (req.ActionName.Equals("getTaskExecutionStatus"))
			{
				return b.Body(b2.Append("jobExitCode", 0)
							  .Build())
						.Build();
			}
			else if (req.ActionName.Equals("taskLog"))
			{
				return b.Body(b2.Build())
						.Build();
			}
			else {
				return null;
			}
		}
	}
}
