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
}

