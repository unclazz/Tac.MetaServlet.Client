using NUnit.Framework;
using System;
using Tac.MetaServlet.Json.Parser;
using Tac.MetaServlet.Json;

namespace Test.Tac.MetaServlet.Json
{
	[TestFixture()]
	public class JsonObjectTest
	{
		[Test()]
		public void FromString_WhenApplyToValidJsonLiteral_ReturnsSuccessfuly()
		{
			// Arrange 
			// Act
			IJsonObject r0 = JsonObject.FromString("{}");
			IJsonObject r1 = JsonObject.FromString("[]");
			IJsonObject r2 = JsonObject.FromString("true");
			IJsonObject r3 = JsonObject.FromString("null");
			IJsonObject r4 = JsonObject.FromString("''");
			IJsonObject r5 = JsonObject.FromString("\"\"");
			IJsonObject r6 = JsonObject.FromString("0.0");

			// Assert
			Assert.That(r0.TypeIs(JsonObjectType.Object), Is.True);
			Assert.That(r1.TypeIs(JsonObjectType.Array), Is.True);
			Assert.That(r2.TypeIs(JsonObjectType.Boolean), Is.True);
			Assert.That(r3.TypeIs(JsonObjectType.Null), Is.True);
			Assert.That(r4.TypeIs(JsonObjectType.String), Is.True);
			Assert.That(r5.TypeIs(JsonObjectType.String), Is.True);
			Assert.That(r6.TypeIs(JsonObjectType.Number), Is.True);
		}
	}
}

