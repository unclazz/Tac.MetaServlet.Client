﻿using System;
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
		IRequest req;
		public ResponseBuilder Request(IRequest r)
		{
			req = r;
			return this;
		}
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
			return new Response(req, statusCode, json);
		}

	}

	public sealed class Response : IResponse
	{
		public static ResponseBuilder Builder()
		{
			return new ResponseBuilder();
		}
		public IRequest Request { get; }
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

		internal Response(IRequest req, HttpStatusCode statusCode, IJsonObject body)
		{
			Request = req;
			StatusCode = statusCode;
			Body = body;
		}

	}
}
