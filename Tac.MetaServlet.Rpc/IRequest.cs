using System;
using System.Threading.Tasks;
using Tac.MetaServlet.Json;

namespace Tac.MetaServlet.Rpc
{
	public interface IRequest
	{
		string Host { get; }
		int Port { get; }
		string Path { get; }
		Uri Uri { get; }
		int Timeout { get; }
		string ActionName { get; }
		string AuthUser { get; }
		string AuthPass { get; }
		IJsonObject Parameters { get; }

		IResponse Send();
		Task<IResponse> SendAsync();
	}
}

