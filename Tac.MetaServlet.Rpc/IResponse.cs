using System;
using System.Net;
using Tac.MetaServlet.Json;

namespace Tac.MetaServlet.Rpc
{
	public interface IResponse
	{
		HttpStatusCode StatusCode { get; }
		int ReturnCode { get; }
		IJsonObject Body { get; }
	}

	sealed class Response : IResponse
	{
		public static ResponseBuilder Builder()
		{
			return new ResponseBuilder();
		}

		public IJsonObject Body { get; }
		public HttpStatusCode StatusCode { get; }
		public int ReturnCode
		{
			get
			{
				double d = Body.GetProperty("returnCode").NumberValue(-1);
				return (int)d;
			}
		}

		internal Response(HttpStatusCode statusCode, IJsonObject body)
		{
			StatusCode = statusCode;
			Body = body;
		}

	}
}

