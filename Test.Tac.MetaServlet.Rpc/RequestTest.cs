using NUnit.Framework;
using System;
using Tac.MetaServlet.Rpc;
using System.Net;
using Tac.MetaServlet.Json;

namespace Test.Tac.MetaServlet.Rpc
{
	[TestFixture()]
	public class RequestTest
	{
		[Test()]
		public void Builder_ReturnsNewBuilderInstance()
		{
			// Arrange

			// Act
			IRequest r0 = Request.Builder().Build();

			// Assert
			Assert.That(r0.Host, Is.EqualTo("localhost"));
			Assert.That(r0.Port, Is.EqualTo(8080));
			Assert.That(r0.Path, Is.EqualTo("/org.talend.administrator/metaServlet"));
			Assert.That(r0.Timeout, Is.EqualTo(100000));
			Assert.That(r0.Uri, Is.EqualTo(new Uri("http://localhost:8080/org.talend.administrator/metaServlet?e30=")));
			Assert.That(r0.Parameters.ToString(), Is.EqualTo("{}"));
		}

		[Test()]
		public void Host_MustNotBeNull()
		{
			// Arrange
			RequestBuilder r0 = Request.Builder().Host(null);
			RequestBuilder r1 = Request.Builder().Host("foo");

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => r0.Build());
			Assert.That(r1.Build().Host, Is.EqualTo("foo"));
			Assert.That(r1.Build().Uri, Is.EqualTo(new Uri("http://foo:8080/org.talend.administrator/metaServlet?e30=")));
		}

		[Test()]
		public void Path_MustNotBeNull()
		{
			// Arrange
			RequestBuilder r0 = Request.Builder().Path(null);
			RequestBuilder r1 = Request.Builder().Path("/foo");

			// Act
			// Assert
			Assert.Throws<ArgumentNullException>(() => r0.Build());
			Assert.That(r1.Build().Path, Is.EqualTo("/foo"));
			Assert.That(r1.Build().Uri, Is.EqualTo(new Uri("http://localhost:8080/foo?e30=")));
		}

		[Test()]
		public void Send_ExecutesHttpRequestViaAgent_ThenReturnsIResponseInstance()
		{
			// Arrange
			var req0 = Request
				.Builder()
				.Agent(MakeMockAgent(JsonObject
									 .Builder()
									 .Append("foo", "bar")
									 .Build()
									))
				.Build();

			// Act
			IResponse resp0 = req0.Send();

			// Assert
			Assert.That(resp0.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(resp0.Body.GetProperty("foo").StringValue(), Is.EqualTo("bar"));
			Assert.That(resp0.Request, Is.EqualTo(req0));
		}

		[Test()]
		public void SendAsync_ExecutesHttpRequestViaAgentAsynchronously_ThenReturnsIResponseInstance()
		{
			// Arrange
			var req0 = Request
				.Builder()
				.Agent(MakeMockAgent(JsonObject
									 .Builder()
									 .Append("foo", "bar")
									 .Build()
									))
				.Build();

			// Act
			// Assert
			Assert.That(async () => {
				var r = await req0.SendAsync();
				return r.Body.GetProperty("foo").StringValue();
			}, Is.EqualTo("bar"));
		}

		Func<IRequest, IResponse> MakeMockAgent(IJsonObject json, 
			HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			return (IRequest arg) => {
				return Response.Builder().Request(arg).StatusCode(statusCode).Body(json).Build();
			};
		}
	}
}

