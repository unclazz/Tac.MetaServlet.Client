using System;
using System.Net;
using Tac.MetaServlet.Json;

namespace Tac.MetaServlet.Rpc
{
	public sealed class ResponseBuilder
	{
		internal ResponseBuilder()
		{
		}
		HttpStatusCode statusCode = HttpStatusCode.OK;
		IJsonObject json;
		public ResponseBuilder StatusCode(HttpStatusCode c)
		{
			statusCode = c;
			return this;
		}
		public ResponseBuilder Body(IJsonObject b)
		{
			json = b;
			return this;
		}
		public ResponseBuilder Body(string b)
		{
			json = JsonObject.FromString(b);
			return this;
		}
		public IResponse Build()
		{
			return new Response(statusCode, json);
		}

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

