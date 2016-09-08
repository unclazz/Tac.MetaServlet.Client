using System;
using System.Text;
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

	public sealed class Request : IRequest
	{
		public static RequestBuilder Builder()
		{
			return new RequestBuilder();
		}

		public string Host { get; }
		public int Port { get; }
		public string Path { get; }
		public int Timeout { get; }
		public IJsonObject Parameters { get; }

		public Uri Uri
		{
			get
			{
				byte[] bs = Encoding.UTF8.GetBytes(Parameters.ToString());
				return new Uri(new StringBuilder()
							   .Append("http://")
							   .Append(Host)
							   .Append(Port)
							   .Append(Path)
							   .Append('?')
							   .Append(System.Convert.ToBase64String(bs))
				               .ToString());
			}
		}

		public string ActionName
		{
			get
			{
				return Parameters.GetProperty("actionName").StringValue();
			}
		}

		public string AuthUser
		{
			get
			{
				return Parameters.GetProperty("authUser").StringValue();
			}
		}

		public string AuthPass
		{
			get
			{
				return Parameters.GetProperty("authPass").StringValue();
			}
		}

		private Func<IRequest, IResponse> agent;

		internal Request(string host, int port, string path, int timeout,
		               IJsonObject json, Func<IRequest, IResponse> agent)
		{
			Host = host;
			Port = port;
			Path = path;
			Timeout = timeout;
			Parameters = json;
			this.agent = agent;
		}

		public IResponse Send()
		{
			return agent.Invoke(this);
		}

		public async Task<IResponse> SendAsync()
		{
			return await Task.Run(() => agent.Invoke(this));
		}
	}
}

