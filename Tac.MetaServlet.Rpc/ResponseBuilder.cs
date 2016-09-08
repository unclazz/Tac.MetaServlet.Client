using System;
using System.Net;
using Tac.MetaServlet.Json;

namespace Tac.MetaServlet.Rpc
{
	public class ResponseBuilder
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
}

