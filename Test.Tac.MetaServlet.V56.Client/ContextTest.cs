using NUnit.Framework;
using System;
using Tac.MetaServlet.V56.Client;
using NLog;

namespace Test.Tac.MetaServlet.V56.Client
{
	[TestFixture()]
	public class ContextTest
	{
		[Test()]
		public void Constructor_InisializeEachProperty()
		{
			// Arrange

			// Act
			var ctx = new Context();

			// Assert
			Assert.That(ctx.Logger, Is.Not.Null);
			Assert.DoesNotThrow(() => {
				var logger = LogManager.GetCurrentClassLogger();
				logger.Info("foo");
			});
			Assert.That(ctx.StartedOn
			            .Subtract(DateTime.Now)
			            .TotalSeconds, Is.LessThanOrEqualTo(15));
			Assert.That(ctx.TaskId, Is.EqualTo(0));
		}
	}
}
