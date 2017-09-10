using System;
using System.Net;
using Unclazz.Commons.Json;

namespace Tac.MetaServlet.Rpc
{
	/// <summary>
	/// <see cref="IResponse"/>のインスタンスを構築するためのビルダーです.
	/// </summary>
	public sealed class ResponseBuilder
	{
		internal ResponseBuilder()
		{
		}
		HttpStatusCode statusCode = HttpStatusCode.OK;
		IJsonObject json;
		IRequest req;
        /// <summary>
        /// レスポンスの元になるリクエストを設定します.
        /// </summary>
        /// <returns></returns>
        /// <param name="r">リクエスト</param>
		public ResponseBuilder Request(IRequest r)
		{
			req = r;
			return this;
		}
        /// <summary>
        /// レスポンスのHTTPステータスコードを設定します.
        /// </summary>
        /// <returns></returns>
        /// <param name="c">HTTPステータスコード</param>
		public ResponseBuilder StatusCode(HttpStatusCode c)
		{
			statusCode = c;
			return this;
		}
        /// <summary>
        /// レスポンスの本文のJSONオブジェクトを設定します.
        /// </summary>
        /// <returns></returns>
        /// <param name="b">JSONオブジェクト</param>
		public ResponseBuilder Body(IJsonObject b)
		{
			json = b;
			return this;
		}
        /// <summary>
        /// レスポンスの本文の文字列を設定します.
        /// </summary>
        /// <returns></returns>
        /// <param name="b">文字列</param>
		public ResponseBuilder Body(string b)
		{
			json = JsonObject.FromString(b);
			return this;
		}
        /// <summary>
        /// レスポンス・オブジェクトを構築します.
        /// </summary>
        /// <returns>The build.</returns>
		public IResponse Build()
		{
			return new Response(req, statusCode, json);
		}

	}

	/// <summary>
	/// <see cref="IResponse"/>の実装クラスです.
	/// </summary>
	public sealed class Response : IResponse
	{
		/// <summary>
		/// ビルダー・オブジェクトを返します.
		/// </summary>
		public static ResponseBuilder Builder()
		{
			return new ResponseBuilder();
		}
        /// <summary>
        /// このレスポンスの元になったリクエストです.
        /// </summary>
        /// <value></value>
		public IRequest Request { get; }
        /// <summary>
        /// レスポンス本文のJSONオブジェクトです.
        /// </summary>
        /// <value></value>
		public IJsonObject Body { get; }
        /// <summary>
        /// HTTPステータスコードです.
        /// </summary>
        /// <value></value>
		public HttpStatusCode StatusCode { get; }
        /// <summary>
        /// リモート実行したアクションのリターンコードです.
        /// </summary>
        /// <value></value>
		public int ReturnCode
		{
			get
			{
				double d = Body.GetProperty("returnCode").AsNumber(-1);
				return (int)d;
			}
		}

		internal Response(IRequest req, HttpStatusCode statusCode, IJsonObject body)
		{
			Request = req;
			StatusCode = statusCode;
			Body = body;
		}
	}
}

