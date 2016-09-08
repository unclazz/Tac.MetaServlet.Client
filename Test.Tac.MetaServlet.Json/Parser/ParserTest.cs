using NUnit.Framework;
using System.Linq;
using Tac.MetaServlet.Json.Parser;
using Tac.MetaServlet.Json;

namespace Test.Tac.MetaServlet.Json
{
	[TestFixture]
	public class ParserTest
	{

		[Test]
		public void Parse_WhenApplyToEmptyString_ThrowsException()
		{
			// Arrange
			var p = new JsonParser();

			// Act
			// Assert
			Assert.Throws<ParseException>(() =>
			{
				p.Parse(Input.FromString(""));
			});
			Assert.Throws<ParseException>(() =>
			{
				p.Parse(Input.FromString(" "));
			});
		}

		[Test]
		public void Parse_WhenApplyToBooleanLiteral_ReturnsBooleanJsonObject()
		{
			// Arrange
			var p = new JsonParser();

			// Act
			var r0 = p.Parse(Input.FromString("true"));
			var r1 = p.Parse(Input.FromString(" true "));
			var r2 = p.Parse(Input.FromString("false"));
			var r3 = p.Parse(Input.FromString(" false "));

			// Assert
			Assert.That(r0.BooleanValue(), Is.True);
			Assert.That(r1.BooleanValue(), Is.True);
			Assert.That(r2.BooleanValue(), Is.False);
			Assert.That(r3.BooleanValue(), Is.False);
		}

		[Test]
		public void Parse_WhenApplyToNullLiteral_ReturnsNullJsonObject()
		{
			// Arrange
			var p = new JsonParser();

			// Act
			var r0 = p.Parse(Input.FromString("null"));
			var r1 = p.Parse(Input.FromString(" null "));

			// Assert
			Assert.That(r0.IsNull, Is.True);
			Assert.That(r1.IsNull, Is.True);
		}

		[Test]
		public void Parse_WhenApplyToNumberLiteral_ReturnsNumberJsonObject()
		{
			// Arrange
			var p = new JsonParser();

			// Act
			var r0 = p.Parse(Input.FromString("0.0"));
			var r1 = p.Parse(Input.FromString(" 0.0 "));
			var r2 = p.Parse(Input.FromString(" -0.1 "));
			var r3 = p.Parse(Input.FromString(" -0.1e+1 "));

			// Assert
			Assert.That(r0.NumberValue(), Is.EqualTo(0.0));
			Assert.That(r1.NumberValue(), Is.EqualTo(0.0));
			Assert.That(r2.NumberValue(), Is.EqualTo(-0.1));
			Assert.That(r3.NumberValue(), Is.EqualTo(-0.1e+1));
		}

		[Test]
		public void Parse_WhenApplyToArrayLiteral_ReturnsArrayJsonObject()
		{
			// Arrange
			var p = new JsonParser();

			// Act
			IJsonObject r0 = p.Parse(Input.FromString("[0.0, true]"));
			IJsonObject r1 = p.Parse(Input.FromString(" [0.0 ,true] "));
			IJsonObject r2 = p.Parse(Input.FromString("[ null,'foo' ]"));
			IJsonObject r3 = p.Parse(Input.FromString(" [{} , \"foo\"] "));

			// Assert
			Assert.That(r0.ArrayValue().Count, Is.EqualTo(2));
			Assert.That(r0.ArrayValue()[0].NumberValue(), Is.EqualTo(0.0));
			Assert.That(r0.ArrayValue()[1].BooleanValue(), Is.True);
			Assert.That(r1.ArrayValue().Count, Is.EqualTo(2));
			Assert.That(r2.ArrayValue().Count, Is.EqualTo(2));
			Assert.That(r3.ArrayValue().Count, Is.EqualTo(2));
			Assert.That(r3.ArrayValue()[0].IsObjectExactly(), Is.True);
			Assert.That(r3.ArrayValue()[1].StringValue(), Is.EqualTo("foo"));
		}

		[Test]
		public void Parse_WhenApplyToObjectLiteral_ReturnsObjectJsonObject()
		{
			// Arrange
			var p = new JsonParser();

			// Act
			IJsonObject r0 = p.Parse(Input.FromString("{}"));
			IJsonObject r1 = p.Parse(Input.FromString(" {} "));
			IJsonObject r2 = p.Parse(Input.FromString("{ foo:\"bar\" }"));
			IJsonObject r3 = p.Parse(Input.FromString("{'foo' : \"bar\", baz: true}"));
			IJsonObject r4 = p.Parse(Input.FromString(" {\"foo\" : 'bar' ,baz:true} "));
			IJsonObject r5 = p.Parse(Input.FromString(" {\"foo\": {\"bar\": {\"baz\": null}}} "));

			// Assert
			Assert.That(r0.Properties.Count(), Is.EqualTo(0));
			Assert.That(r1.Properties.Count(), Is.EqualTo(0));
			Assert.That(r2.Properties.Count(), Is.EqualTo(1));
			Assert.That(r3.Properties.Count(), Is.EqualTo(2));
			Assert.That(r4.Properties.Count(), Is.EqualTo(2));
			Assert.That(r4.GetProperty("foo").StringValue(), Is.EqualTo("bar"));
			Assert.That(r4.GetProperty("baz").BooleanValue(), Is.True);
			Assert.That(r5.Properties.Count(), Is.EqualTo(1));
			Assert.That(r5.GetProperty("foo").GetProperty("bar").GetProperty("baz").IsNull(), Is.True);
		}
	}
}