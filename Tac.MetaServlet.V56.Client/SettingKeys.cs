using System;
namespace Tac.MetaServlet.V56.Client
{
	/// <summary>
	/// アプリケーション構成ファイルの設定キー名を定義するクラスです。
	/// </summary>
	public static class SettingKeys
	{
		private static readonly string base_ = "Tac.MetaServlet.V56.Client.";
		private static readonly string _ = ".";

		/// <summary>
		/// リモートホストに関する設定キーを定義するクラスです。
		/// </summary>
		public static class Remote
		{
			/// <summary>
			/// ホスト名
			/// </summary>
			public static readonly string Host = base_ + nameof(Remote) + _ + nameof(Host);
			/// <summary>
			/// ポート番号
			/// </summary>
			public static readonly string Port = base_ + nameof(Remote) + _ + nameof(Port);
			/// <summary>
			/// コンテキストパス
			/// </summary>
			public static readonly string Path = base_ + nameof(Remote) + _ + nameof(Path);
		}
		/// <summary>
		/// APIリクエストに関する設定キーを定義するクラスです。
		/// </summary>
		public static class Request
		{
			/// <summary>
			/// APIリクエスト間隔秒数
			/// </summary>
			public static readonly string Interval = base_ + nameof(Request) + _ + nameof(Interval);
			/// <summary>
			/// APIリクエスト・タイムアウト秒数
			/// </summary>
			public static readonly string Timeout = base_ + nameof(Request) + _ + nameof(Timeout);
			/// <summary>
			/// APIアクセス認証ユーザ名
			/// </summary>
			public static readonly string AuthUser = base_ + nameof(Request) + _ + nameof(AuthUser);
			/// <summary>
			/// APIアクセス認証パスワード
			/// </summary>
			public static readonly string AuthPass = base_ + nameof(Request) + _ + nameof(AuthPass);
			/// <summary>
			/// 実行対象タスク名
			/// </summary>
			public static readonly string TaskName = base_ + nameof(Request) + _ + nameof(TaskName);
		}
		/// <summary>
		/// コマンド実行そのものに関する設定キーを定義するクラスです。
		/// </summary>
		public static class Execution
		{
			/// <summary>
			/// ログファイル名
			/// </summary>
			public static readonly string LogFileName = base_ + nameof(Execution) + _ + nameof(LogFileName);
			/// <summary>
			/// コマンド・インスタンス名
			/// </summary>
			public static readonly string InstanceName = base_ + nameof(Execution) + _ + nameof(InstanceName);
			/// <summary>
			/// コマンド実行タイムアウト秒数
			/// </summary>
			public static readonly string Timeout = base_ + nameof(Execution) + _ + nameof(Timeout);
			/// <summary>
			/// ドライラン・モードのフラグ
			/// </summary>
			public static readonly string DryRun = base_ + nameof(Execution) + _ + nameof(DryRun);
		}
	}
}
