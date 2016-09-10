using System;
using System.Net;
using Tac.MetaServlet.Json;

namespace Tac.MetaServlet.Rpc
{
	/// <summary>
	/// MetaServletに対するRPCのレスポンスを表わすインターフェースです.
	/// </summary>
	public interface IResponse
	{
		/// <summary>
		/// このレスポンスが生成される元になったリクエスト.
		/// </summary>
		/// <value>RPCリクエスト.</value>
		IRequest Request { get; }
		/// <summary>
		/// HTTPステータスコード.
		/// </summary>
		/// <value>HTTPステータスコード.</value>
		HttpStatusCode StatusCode { get; }
		/// <summary>
		/// RPCレスポンスのリターンコード.
		/// </summary>
		/// <value>リターンコード.</value>
		int ReturnCode { get; }
		/// <summary>
		/// RPCレスポンスのJSON.
		/// </summary>
		/// <value>RPCレスポンスのJSON.</value>
		IJsonObject Body { get; }
	}
}

