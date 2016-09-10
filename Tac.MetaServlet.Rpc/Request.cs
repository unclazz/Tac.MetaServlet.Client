using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tac.MetaServlet.Json;

namespace Tac.MetaServlet.Rpc
{
	/// <summary>
	/// <see cref="IRequest"/>のインスタンスを生成するためのビルダーです.
	/// </summary>
	public sealed class RequestBuilder
	{
		internal RequestBuilder()
		{
		}

		private JsonObjectBuilder builer = JsonObject.Builder();
		private string host = "localhost";
		private int port = 8080;
		private string path = "/org.talend.administrator/metaServlet";
		private int timeout = 100000;
		private Func<IRequest, IResponse> agent = (arg) => {
			var req = WebRequest.CreateHttp(arg.Uri);
			req.Method = "GET";
			req.Timeout = arg.Timeout;
			using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
			{
				IJsonObject json = JsonObject.FromStream(resp.GetResponseStream(), Encoding.UTF8);
				return Response.Builder()
					           .StatusCode(resp.StatusCode)
					           .Body(json)
					           .Build();
			}
		};

		public RequestBuilder Host(string h)
		{
			host = h;
			return this;
		}
		public RequestBuilder Port(int p)
		{
			port = p;
			return this;
		}
		public RequestBuilder Path(string p)
		{
			path = p;
			return this;
		}
		public RequestBuilder Timeout(int t)
		{
			timeout = t;
			return this;
		}
		public RequestBuilder ActionName(string an)
		{
			builer.Append("actionName", an);
			return this;
		}
		public RequestBuilder AuthUser(string au)
		{
			builer.Append("authUser", au);
			return this;
		}
		public RequestBuilder AuthPass(string ap)
		{
			builer.Append("authPass", ap);
			return this;
		}
		public RequestBuilder Parameter(string name, string value)
		{
			builer.Append(name, value);
			return this;
		}
		public RequestBuilder Parameter(string name, long value)
		{
			builer.Append(name, value);
			return this;
		}
		public RequestBuilder Parameter(string name, bool value)
		{
			builer.Append(name, value);
			return this;
		}
		public RequestBuilder Parameter(string name, IJsonObject value)
		{
			builer.Append(name, value);
			return this;
		}
		public RequestBuilder Parameter(string name, Action<JsonObjectBuilder> buildAction)
		{
			builer.Append(name, buildAction);
			return this;
		}
		public RequestBuilder Agent(Func<IRequest, IResponse> alternative)
		{
			agent = alternative;
			return this;
		}
		public IRequest Build()
		{
			return new Request(host, port, path, timeout, builer.Build(), agent);
		}
	}

	/// <summary>
	/// <see cref="IRequest"/>の実装クラスです.
	/// </summary>
	public sealed class Request : IRequest
	{
		/// <summary>
		/// ビルダー・オブジェクトを返します.
		/// </summary>
		public static RequestBuilder Builder()
		{
			return new RequestBuilder();
		}

		private Uri uriCache;
		private string actionNameCache;
		private string authUserCache;
		private string authPassCache;

		public string Host { get; }
		public int Port { get; }
		public string Path { get; }
		public int Timeout { get; }
		public IJsonObject Parameters { get; }
		public Uri Uri
		{
			get
			{
				if (uriCache == null)
				{
					byte[] bs = Encoding.UTF8.GetBytes(Parameters.ToString());
					uriCache = new Uri(new StringBuilder()
								   .Append("http://")
								   .Append(Host)
								   .Append(':')
								   .Append(Port)
								   .Append(Path)
								   .Append('?')
								   .Append(Convert.ToBase64String(bs))
								   .ToString());
				}
				return uriCache;
			}
		}

		public string ActionName
		{
			get
			{
				if (actionNameCache == null)
				{
					actionNameCache = Parameters.GetProperty("actionName").StringValue();
				}
				return actionNameCache;
			}
		}

		public string AuthUser
		{
			get
			{
				if (authUserCache == null)
				{
					authUserCache = Parameters.GetProperty("authUser").StringValue();
				}
				return authUserCache;
			}
		}

		public string AuthPass
		{
			get
			{
				if (authPassCache == null)
				{
					authPassCache = Parameters.GetProperty("authPass").StringValue();
				}
				return authPassCache;
			}
		}

		private Func<IRequest, IResponse> agent;

		internal Request(string host, int port, string path, int timeout,
					   IJsonObject json, Func<IRequest, IResponse> agent)
		{
			Assertions.MustNotBeNull("host", host);
			Assertions.MustNotBeNull("path", path);
			Assertions.MustNotBeNull("json", json);
			Assertions.MustNotBeNull("agent", agent);
			Assertions.MustNotBeEmpty("host", host);
			Assertions.MustBeGreaterThanOrEqual0("port", port);
			Assertions.MustBeGreaterThanOrEqual0("timeout", timeout);

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

