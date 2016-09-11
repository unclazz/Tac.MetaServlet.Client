using NUnit.Framework;
using System;
using System.Linq;
using Tac.MetaServlet.Json.Parser;
using Tac.MetaServlet.Json;

namespace Test.Tac.MetaServlet.Json
{
	[TestFixture()]
	public class JsonObjectBuilderTest
	{
		[Test()]
		public void Build_WhenNoPropertyAppended_ReturnsEmptyObject()
		{
			// Arrange 
			var b0 = JsonObject.Builder();

			// Act
			var r0 = b0.Build();

			// Assert
			Assert.That(r0.IsObjectExactly(), Is.True);
			Assert.That(r0.ToString(), Is.EqualTo("{}"));
			Assert.That(r0.Properties.Count(), Is.EqualTo(0));
		}
		[Test()]
		public void Build_WhenOnePropertyAppended_ReturnsObjectHasOneProperty()
		{
			// Arrange 
			var b0 = JsonObject.Builder();
			var b1 = JsonObject.Builder();
			var b2 = JsonObject.Builder();
			var b3 = JsonObject.Builder();

			// Act
			var r0 = b0.Append("foo", "bar").Build();
			var r1 = b1.AppendEmptyArray("foo").Build();
			var r2 = b2.AppendNull("foo").Build();
			var r3 = b3.Append("foo", new double[] { 1, 2, 3 }).Build();

			// Assert
			Assert.That(r0.IsObjectExactly(), Is.True);
			Assert.That(r0.ToString(), Is.EqualTo("{\"foo\":\"bar\"}"));
			Assert.That(r0.Properties.Count(), Is.EqualTo(1));
			Assert.That(r1.ToString(), Is.EqualTo("{\"foo\":[]}"));
			Assert.That(r2.ToString(), Is.EqualTo("{\"foo\":null}"));
			Assert.That(r3.ToString(), Is.EqualTo("{\"foo\":[1,2,3]}"));
		}
		[Test()]
		public void Append_OverwritesOldPropertyWithNewPropertyHasSameName()
		{
			// Arrange 
			var b0 = JsonObject.Builder();

			// Act
			var r0 = b0.Append("foo", "bar").Append("foo", "baz").Build();

			// Assert
			Assert.That(r0.IsObjectExactly(), Is.True);
			Assert.That(r0.ToString(), Is.EqualTo("{\"foo\":\"baz\"}"));
			Assert.That(r0.Properties.Count(), Is.EqualTo(1));
		}
	}
}

