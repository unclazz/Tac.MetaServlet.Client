using System;
using Unclazz.Commons.Json;

namespace Tac.MetaServlet.V56.Client
{
	/// <summary>
	/// コマンド実行中に発生した例外を表わすクラスです。
	/// </summary>
	public class ClientException : Exception
	{
		/// <summary>
		/// コマンドの終了コードとして使用される値です。
		/// </summary>
		/// <value>終了コード</value>
		public int ExitCode { get; }
		/// <summary>
		/// コンストラクタです。
		/// </summary>
		/// <param name="exitCode">コマンドの終了コードとして使用される値</param>
		/// <param name="message">メッセージ</param>
		/// <param name="cause">原因となった例外</param>
		public ClientException(int exitCode, string message, Exception cause) : base(message, cause)
		{
			ExitCode = exitCode;
		}
		/// <summary>
		/// コンストラクタです。
		/// </summary>
		/// <param name="exitCode">コマンドの終了コードとして使用される値</param>
		/// <param name="message">メッセージ</param>
		public ClientException(int exitCode, string message) : base(message)
		{
			ExitCode = exitCode;
		}
	}
}
