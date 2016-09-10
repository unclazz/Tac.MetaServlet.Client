using NUnit.Framework;
using System;
using Tac.MetaServlet.Rpc;

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
	}
}

