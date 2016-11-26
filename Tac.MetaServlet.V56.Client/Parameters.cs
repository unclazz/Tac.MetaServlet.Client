using System;
namespace Tac.MetaServlet.V56.Client
{
	/// <summary>
	/// コマンドのパラメータを格納するクラスです。
	/// </summary>
	public class Parameters
	{
		/// <summary>
		/// コマンドのパラメータのうちリモートホストに関するものを格納するクラスです。
		/// </summary>
		public class RemoteParameters
		{
			/// <summary>
			/// ホスト名です。
			/// </summary>
			/// <value>ホスト名</value>
			public string Host { get; set; } = "localhost";
			/// <summary>
			/// ポート番号です。
			/// </summary>
			/// <value>ポート番号</value>
			public int Port { get; set; } = 8080;
			/// <summary>
			/// コンテキストパスです。
			/// </summary>
			/// <value>コンテキストパス</value>
			public string Path { get; set; } = "/org.talend.administrator/metaServlet";
		}
		/// <summary>
		/// コマンドのパラメータのうちAPIリクエストに関するものを格納するクラスです。
		/// </summary>
		public class RequestParameters
		{
			/// <summary>
			/// タスク実行リクエエストの間隔秒数です。
			/// </summary>
			/// <value>間隔秒数</value>
			public int Interval { get; set; } = 60;
			/// <summary>
			/// タスク実行リクエストのタイムアウト秒数です。
			/// </summary>
			/// <value>タイムアウト秒数</value>
			public int Timeout { get; set; } = 30;
			/// <summary>
			/// APIアクセス時の認証に使用されるユーザ名です。
			/// </summary>
			/// <value>ユーザ名</value>
			public string AuthUser { get; set; }
			/// <summary>
			/// APIアクセス時の認証に使用されるパスワードです。
			/// </summary>
			/// <value>パスワード</value>
			public string AuthPass { get; set; }
			/// <summary>
			/// タスク実行リクエストのターゲットとなるタスク名です。
			/// </summary>
			/// <value>タスク名</value>
			public string TaskName { get; set; }
		}
		/// <summary>
		/// コマンドのパラメータのうちコマンド実行そのものに関するものを格納するクラスです。
		/// </summary>
		public class ExecutionParameters
		{
			/// <summary>
			/// コマンド実行のタイムアウト秒数です。
			/// </summary>
			/// <value>タイムアウト秒数</value>
			public int Timeout { get; set; } = 3600;
			/// <summary>
			/// ドライラン・モードのフラグです。
			/// </summary>
			/// <value>フラグ</value>
			public bool DryRun { get; set; } = false;
			/// <summary>
			/// コマンドのインスタンス名です。
			/// </summary>
			/// <value>インスタンス名</value>
			public string InstanceName { get; set; } = "tacrpc";
			/// <summary>
			/// ログファイルの名前です。
			/// </summary>
			/// <value>ログファイル名</value>
			public string LogFileName { get; set; } = 
				"tacrpc_${var:instanceName}_${var:executionName}_" +
				"${var:yyyyMMdd}_${var:hhmmssfff}.log";
		}
		/// <summary>
		/// リモートホストに関するパラメータです。
		/// </summary>
		/// <value>パラメータ</value>
		public RemoteParameters Remote { get; set; }
		/// <summary>
		/// APIリクエストに関するパラメータです。
		/// </summary>
		/// <value>パラメータ</value>
		public RequestParameters Request { get; set; }
		/// <summary>
		/// コマンド実行そのものに関するパラメータです。
		/// </summary>
		/// <value>パラメータ</value>
		public ExecutionParameters Execution { get; set; }

		/// <summary>
		/// コンストラクタです。
		/// </summary>
		public Parameters()
		{
			Remote = new RemoteParameters();
			Request = new RequestParameters();
			Execution = new ExecutionParameters();
		}
	}
}
