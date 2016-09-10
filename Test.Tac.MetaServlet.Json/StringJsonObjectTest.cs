using NUnit.Framework;
using System;
using Tac.MetaServlet.Json.Parser;
using Tac.MetaServlet.Json;
using System.Linq;

namespace Test.Tac.MetaServlet.Json
{
	[TestFixture()]
	public class StringJsonObjectTest
	{
		
		[Test()]
		public void ToString_ReturnsStringLiteral()
		{
			// Arrange
			IJsonObject json0 = JsonObject.Of("");
			IJsonObject json1 = JsonObject.Of("abc");

			// Act
			string r0 = json0.ToString();
			string r1 = json1.ToString();

			// Assert
			Assert.That(r0, Is.EqualTo("\"\""));
			Assert.That(r1, Is.EqualTo("\"abc\""));
		}

		[Test()]
		public void IsObjectExactly_ReturnsFalse()
		{
			// Arrange
			IJsonObject json = JsonObject.Of("");

			// Act
			var r = json.IsObjectExactly();

			// Assert
			Assert.That(r, Is.False);
		}

		[Test()]
		public void IsNull_ReturnsFalse()
		{
			// Arrange
			IJsonObject json = JsonObject.Of("");

			// Act
			var r = json.IsNull();

			// Assert
			Assert.That(r, Is.False);
		}

		[Test()]
		public void ArrayValue_ThrowsException()
		{
			// Arrange
			IJsonObject json = JsonObject.Of("");

			// Act
			// Assert
			Assert.Throws<ApplicationException>(() =>
			{
				json.ArrayValue();
			});
		}

		[Test()]
		public void ArrayValue1_ReturnsFallback()
		{
			// Arrange
			IJsonObject json = JsonObject.Of("");

			// Act
			// Assert
			Assert.That(json.ArrayValue(JsonObject.Of("foo", "bar").ArrayValue()).Count, Is.EqualTo(2));
		}

		[Test()]
		public void StringValue_ReturnsValue()
		{
			// Arrange
			IJsonObject json0 = JsonObject.Of(string.Empty);
			IJsonObject json1 = JsonObject.Of("abc");

			// Act
			// Assert
			Assert.That(json0.StringValue(), Is.EqualTo(string.Empty));
			Assert.That(json1.StringValue(), Is.EqualTo("abc"));
		}

		[Test()]
		public void StringValue1_IgnoresFallbackValues()
		{
			// Arrange
			IJsonObject json0 = JsonObject.Of(string.Empty);
			IJsonObject json1 = JsonObject.Of("abc");

			// Act
			// Assert
			Assert.That(json0.StringValue("foo"), Is.EqualTo(string.Empty));
			Assert.That(json1.StringValue("bar"), Is.EqualTo("abc"));
		}

		[Test()]
		public void BooleanValue_ThrowsException()
		{
			// Arrange
			IJsonObject json = JsonObject.Of(string.Empty);

			// Act
			// Assert
			Assert.Throws<ApplicationException>(() =>
			{
				json.BooleanValue();
			});
		}

		[Test()]
		public void BooleanValue1_ReturnsFallback()
		{
			// Arrange
			IJsonObject json = JsonObject.Of(string.Empty);

			// Act
			// Assert
			Assert.That(json.BooleanValue(true), Is.True);
		}

		[Test()]
		public void Properties_ReturnsEmptyEnumeralbe()
		{
			// Arrange
			IJsonObject json = JsonObject.Of(string.Empty);

			// Act
			// Assert
			Assert.That(json.Properties.Count(), Is.EqualTo(0));
		}

		[Test()]
		public void HasProperty_ReturnsFalse()
		{
			// Arrange
			IJsonObject json = JsonObject.Of(string.Empty);

			// Act
			// Assert
			Assert.That(json.HasProperty("foo"), Is.False);
		}

		[Test()]
		public void GetProperty_ThrowsException()
		{
			// Arrange
			IJsonObject json = JsonObject.Of(string.Empty);

			// Act
			// Assert
			Assert.Throws<ApplicationException>(() =>
			{
				json.GetProperty("foo");
			});
		}
	}
}

