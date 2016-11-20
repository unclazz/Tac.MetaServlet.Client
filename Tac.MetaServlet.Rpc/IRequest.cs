using System;
using System.Threading.Tasks;
using Unclazz.Commons.Json;

namespace Tac.MetaServlet.Rpc
{
	/// <summary>
	/// MetaServletに対するRPCのリクエストを表わすインターフェースです.
	/// </summary>
	public interface IRequest
	{
		/// <summary>
		/// リクエスト先のホスト名.
		/// </summary>
		/// <value>リクエスト先のホスト名.</value>
		string Host { get; }
		/// <summary>
		/// リクエスト先のポート番号.
		/// </summary>
		/// <value>リクエスト先のポート番号.</value>
		int Port { get; }
		/// <summary>
		/// リクエスト先のパス名.
		/// </summary>
		/// <value>リクエスト先のパス名.</value>
		string Path { get; }
		/// <summary>
		/// エンコード済みのリクエスト内容を含むリクエストURI.
		/// </summary>
		/// <value>リクエストURI.</value>
		Uri Uri { get; }
		/// <summary>
		/// リクエストがタイムアウトするまでのミリ秒.
		/// </summary>
		/// <value>タイムアウトするまでのミリ秒.</value>
		int Timeout { get; }
		/// <summary>
		/// RPCリクエストするアクション名.
		/// </summary>
		/// <value>アクション名.</value>
		string ActionName { get; }
		/// <summary>
		/// RPCリクエストする認証ユーザ名.
		/// </summary>
		/// <value>認証ユーザ名.</value>
		string AuthUser { get; }
		/// <summary>
		/// RPCリクエストする認証パスワード.
		/// </summary>
		/// <value>認証パスワード.</value>
		string AuthPass { get; }
		/// <summary>
		/// PRCリクエストのパラメータを表わすJSON.
		/// </summary>
		/// <value>パラメータを表わすJSON.</value>
		IJsonObject Parameters { get; }

		/// <summary>
		/// RPCリクエストを送信します.
		/// </summary>
		IResponse Send();
		/// <summary>
		/// RPCリクエストを非同期に送信します.
		/// </summary>
		/// <returns>RPCレスポンスを生成するタスク.</returns>
		Task<IResponse> SendAsync();
	}
}

