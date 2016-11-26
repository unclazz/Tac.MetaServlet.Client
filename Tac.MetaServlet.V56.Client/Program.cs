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
	class MainClass
	{
		private static readonly int exitCodeOnEndedNormally = 0;
		private static readonly int exitCodeOnEndedAbnormally = 1;

		private readonly IJsonFormatOptions formatOptions =
			JsonFormatOptions.Builder().Indent(true).Build();
		private Logger log;

		public static int Main(string[] args)
		{
			return new MainClass().Execute(args);
		}

		private void ConfigureNLog(Parameters ps)
		{
			var now = DateTime.Now;
			var conf = new LoggingConfiguration();
			conf.Variables.Add("instanceName", ps.Execution.InstanceName);
			conf.Variables.Add("executionName", MakeExecutionId());
			conf.Variables.Add("yyyyMMdd", now.ToString("yyyyMMdd"));
			conf.Variables.Add("hhmmssfff", now.ToString("hhmmssfff"));

			var file = new FileTarget("file")
			{
				FileName = ps.Execution.LogFileName,
				Encoding = Encoding.UTF8
			};

			var console = new ConsoleTarget("console");

			conf.AddTarget(file);
			conf.AddTarget(console);
			conf.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, file));
			conf.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, console));
			LogManager.Configuration = conf;
			log = LogManager.GetCurrentClassLogger();
		}

		public int Execute(string[] args)
		{
			// パラメータ・オブジェクトを初期化
			var ps = new Parameters();
			// コマンドラインの定義を作成
			var cl = MakeCommandLine(ps);

			try
			{
				// コマンドライン引数とアプリケーション構成ファイルからパラメータを読み取り
				cl.Parse(args);
				// NLogロガーを構成
				ConfigureNLog(ps);
				// 実行コンテキストを初期化
				var ctx = new Context();
				// APIリクエスト：タスク名をキーにしてタスクIDを取得
				var resp0 = RequestGetTaskIdByName(ps);
				// 取得したIDを実行コンテキストに設定
				ctx.TaskId = (int)resp0.GetProperty("taskId").NumberValue();
				// APIリクエスト：タスクのステータスを取得
				var resp1 = RequestGetTaskStatus(ps, ctx);
				// ステータスをチェック
				if (!resp1.GetProperty("status").StringValue().Equals("READY_TO_RUN"))
				{
					// "Ready to run"以外のステータスの場合はエラー
					throw MakeException(exitCodeOnEndedAbnormally,
										"Task is already running.");
				}
				// APIリクエスト：タスクを非同期モードで起動する
				var resp2 = RequestRunTask(ps, ctx);
				// 返された実行リクエストIDを実行コンテキストに設定
				ctx.ExecRequestId = resp2.GetProperty("execRequestId").StringValue();
				// APIリクエスト：所定の時間内タスクの完了を
				var resp3 = RequestGetTaskExecutionStatusWithThreadSleep(ps, ctx);
				var jobExitCode = (int)resp3.GetProperty("jobExitCode").NumberValue();
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
					Console.Error.WriteLine(ex);
				}
				// エラーとともに処理が中断
				return exitCodeOnEndedAbnormally;
			}
			catch (ClientException ex)
			{
				// APIリクエスト時にエラーが発生した場合
				// 例外オブジェクトの内容をロギング
				log.Error(ex);
				// エラーとともに処理が中断
				return ex.ExitCode;
			}
			catch (Exception ex)
			{
				// それ以外のエラーが発生した場合
				// 例外オブジェクトの内容をロギング
				if (log == null)
				{
					Console.Error.WriteLine(ex);
				}
				else {
					log.Error(ex);
				}
				// エラーとともに処理が中断
				return exitCodeOnEndedAbnormally;
			}
		}

		public ICommandLine MakeCommandLine(Parameters ps)
		{
			return CommandLine
				.Builder("tacrpc.exe")
				.Description("")// TODO
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
						   .SettingName(SettingKeys.Execution.Timeout)
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
												ps.Execution.Timeout))
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
						   .SettingName(SettingKeys.Execution.Timeout)
						   .Required(false)
						   .HasArgument()
						   .ArgumentName("filename")
				           .Description(string
				                        .Format("Name of log file. Default is \"{0}\".",
				                                ps.Execution.LogFileName))
				           .SetterDelegate(s => ps.Execution.LogFileName = s))
				.AddOption(Option
						   .Builder("/j")
						   .SettingName(SettingKeys.Execution.Timeout)
						   .Required(false)
						   .HasArgument()
						   .ArgumentName("instance")
				           .Description(string
				                        .Format("Name of command instance. Default is \"{0}\".",
				                                ps.Execution.InstanceName))
				           .SetterDelegate(s => ps.Execution.InstanceName = s))
				.AddOption(Option
						   .Builder("/dryrun")
						   .Required(false)
						   .HasArgument(false)
						   .Description("Use mock for a simulation. Request is NOT sent for anything.")
				           .SetterDelegate(() => ps.Execution.DryRun = true))
				.Build();
		}

		public string MakeExecutionId()
		{
			return new System.Random().Next(65535).ToString("x4");
		}

		public IJsonObject RequestGetTaskIdByName(Parameters ps)
		{
			var req = MakeRequest(ps, JsonObject.Builder()
						.Append("actionName", "getTaskIdByName")
						.Append("taskName", ps.Request.TaskName)
						.Build());
			return SendRequest(ps, req);
		}

		public IJsonObject RequestGetTaskStatus(Parameters ps, Context ctx)
		{
			var req = MakeRequest(ps, JsonObject.Builder()
						.Append("actionName", "getTaskStatus")
			                      .Append("taskId", ctx.TaskId)
						.Build());
			return SendRequest(ps, req);
		}

		public IJsonObject RequestRunTask(Parameters ps, Context ctx)
		{
			var req = MakeRequest(ps, JsonObject.Builder()
						.Append("actionName", "runTask")
			                      .Append("taskId", ctx.TaskId)
			                      .Append("mode", "asynchronous")
						.Build());
			return SendRequest(ps, req);
		}

		public IJsonObject RequestGetTaskExecutionStatus(Parameters ps, Context ctx)
		{
			var req = MakeRequest(ps, JsonObject.Builder()
						.Append("actionName", "getTaskExecutionStatus")
								  .Append("taskId", ctx.TaskId)
			                      .Append("execRequestId", ctx.ExecRequestId)
						.Build());
			return SendRequest(ps, req);
		}

		public IJsonObject RequestGetTaskExecutionStatusWithThreadSleep(Parameters ps, Context ctx)
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

		public IJsonObject RequestTaskLog(Parameters ps, Context ctx)
		{
			var req = MakeRequest(ps, JsonObject.Builder()
						.Append("actionName", "taskLog")
								  .Append("taskId", ctx.TaskId)
								  .Append("lastExecution", true)
						.Build());
			return SendRequest(ps, req);
		}

		public bool WithinTimeLimit(Parameters ps, DateTime startedOn)
		{
			var delta = DateTime.Now.Subtract(startedOn);
			return delta.TotalSeconds > ps.Execution.Timeout;
		}

		public string SerializeSecurely(IJsonObject j)
		{
			var b = JsonObject.Builder(j);
			if (j.HasProperty("authPass"))
			{
				b.Append("authPass", "*****");
			}
			return b.Build().Format(formatOptions);
		}

		public IJsonObject SendRequest(Parameters ps, IRequest req)
		{
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
					log.Error("ResponseBody = {0}",  SerializeSecurely(resp.Body));
					throw MakeException(exitCode, "Bad API response.");
				}
			}
			catch (Exception ex)
			{
				throw MakeException(exitCodeOnEndedAbnormally, "API request failed.", ex);
			}
		}

		public IRequest MakeRequest(Parameters ps, IJsonObject reqParams)
		{
			var b = Request.Builder()
						   .Host(ps.Remote.Host)
						   .Port(ps.Remote.Port)
						   .Path(ps.Remote.Path)
						   .AuthUser(ps.Request.AuthUser)
						   .AuthPass(ps.Request.AuthPass)
						   .Timeout(ps.Request.Timeout);
			if (ps.Execution.DryRun)
			{
				b.Agent(DelegateAgent);
			}
			foreach (var reqParam in reqParams.Properties)
			{
				b.Parameter(reqParam.Name, reqParam.Value);
			}
			return b.Build();
		}

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

		public ClientException MakeException(int exitCode, string message)
		{
			return MakeException(exitCode, message, null);
		}

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
