using System;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Tac.MetaServlet.V56.Client
{
	/// <summary>
	/// コマンド実行中のコンテキストを格納するクラスです。
	/// </summary>
	public class Context
	{
		/// <summary>
		/// タスクIDです。
		/// </summary>
		/// <value>タスクID</value>
		public int TaskId { get; set; } = 0;
		/// <summary>
		/// タスク実行リクエストIDです。
		/// </summary>
		/// <value>タスク実行リクエストID</value>
		public string ExecRequestId { get; set; } = string.Empty;
		/// <summary>
		/// タスク実行開始日時です。
		/// </summary>
		/// <value>タスク実行開始日時</value>
		public DateTime StartedOn { get; set; } = DateTime.Now;
		/// <summary>
		/// コンソールとファイルに出力を行うためのロガーです。
		/// ただし初期状態ではコンソールにのみ出力を行います。
		/// </summary>
		/// <value>ロガー</value>
		public Logger Logger { get; }
		/// <summary>
		/// コンストラクタです。
		/// </summary>
		public Context()
		{
			var conf = new LoggingConfiguration();
			var console = new ConsoleTarget("console");
			conf.AddTarget(console);
			conf.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, console));
			LogManager.Configuration = conf;
			Logger = LogManager.GetCurrentClassLogger();
		}
	}
}
