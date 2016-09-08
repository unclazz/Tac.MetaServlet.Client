using System;
using System.Net;
using System.Text;
using Tac.MetaServlet.Json;

namespace Tac.MetaServlet.Rpc
{
	public class RequestBuilder
	{
		internal RequestBuilder()
		{
		}

		private JsonObjectBuilder builer = JsonObject.Builder();
		private string host = "localhost";
		private int port = 8080;
		private string path = "org.talend.administrator/metaServlet";
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

		RequestBuilder Host(string h)
		{
			host = h;
			return this;
		}
		RequestBuilder Port(int p)
		{
			port = p;
			return this;
		}
		RequestBuilder Path(string p)
		{
			path = p;
			return this;
		}
		RequestBuilder Timeout(int t)
		{
			timeout = t;
			return this;
		}
		RequestBuilder ActionName(string an)
		{
			builer.Append("actionName", an);
			return this;
		}
		RequestBuilder AuthUser(string au)
		{
			builer.Append("authUser", au);
			return this;
		}
		RequestBuilder AuthPass(string ap)
		{
			builer.Append("authPass", ap);
			return this;
		}
		RequestBuilder Parameter(string name, string value)
		{
			builer.Append(name, value);
			return this;
		}
		RequestBuilder Parameter(string name, long value)
		{
			builer.Append(name, value);
			return this;
		}
		RequestBuilder Parameter(string name, bool value)
		{
			builer.Append(name, value);
			return this;
		}
		RequestBuilder Parameter(string name, IJsonObject value)
		{
			builer.Append(name, value);
			return this;
		}
		RequestBuilder Parameter(string name, Action<JsonObjectBuilder> buildAction)
		{
			builer.Append(name, buildAction);
			return this;
		}
		RequestBuilder Agent(Func<IRequest, IResponse> alternative)
		{
			agent = alternative;
			return this;
		}
		IRequest Build()
		{
			return new Request(host, port, path, timeout, builer.Build(), agent);
		}
	}
}

