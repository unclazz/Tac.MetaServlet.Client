using NUnit.Framework;
using System;
using Tac.MetaServlet.Json.Parser;
using Tac.MetaServlet.Json;
using System.Linq;

namespace Test.Tac.MetaServlet.Json
{
	[TestFixture()]
	public class NumberJsonObjectTest
	{
		
		[Test()]
		public void ToString_ReturnsNumberLiteral()
		{
			// Arrange
			IJsonObject json0 = JsonObject.Of(0.5);
			IJsonObject json1 = JsonObject.Of(1.0);

			// Act
			string r0 = json0.ToString();
			string r1 = json1.ToString();

			// Assert
			Assert.That(r0, Is.EqualTo("0.5"));
			Assert.That(r1, Is.EqualTo("1"));
		}

		[Test()]
		public void IsObjectExactly_ReturnsFalse()
		{
			// Arrange
			IJsonObject json = JsonObject.Of(0);

			// Act
			var r = json.IsObjectExactly();

			// Assert
			Assert.That(r, Is.False);
		}

		[Test()]
		public void IsNull_ReturnsFalse()
		{
			// Arrange
			IJsonObject json = JsonObject.Of(0);

			// Act
			var r = json.IsNull();

			// Assert
			Assert.That(r, Is.False);
		}

		[Test()]
		public void ArrayValue_ThrowsException()
		{
			// Arrange
			IJsonObject json = JsonObject.Of(1);

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
			IJsonObject json = JsonObject.Of(1);

			// Act
			// Assert
			Assert.That(json.ArrayValue(JsonObject.Of("foo", "bar").ArrayValue()).Count, Is.EqualTo(2));
		}

		[Test()]
		public void StringValue_ThrowsException()
		{
			// Arrange
			IJsonObject json0 = JsonObject.Of(0);
			IJsonObject json1 = JsonObject.Of(1);

			// Act
			// Assert
			Assert.Throws<ApplicationException>(() =>
			{
				json0.StringValue();
			});
			Assert.Throws<ApplicationException>(() =>
			{
				json1.StringValue();
			});
		}

		[Test()]
		public void StringValue1_ReturnsFallbackValue()
		{
			// Arrange
			IJsonObject json0 = JsonObject.Of(1);
			IJsonObject json1 = JsonObject.Of(0);

			// Act
			// Assert
			Assert.That(json0.StringValue("foo"), Is.EqualTo("foo"));
			Assert.That(json1.StringValue("bar"), Is.EqualTo("bar"));
		}

		[Test()]
		public void NumberValue_ReturnsValue()
		{
			// Arrange
			IJsonObject json0 = JsonObject.Of(0.5);
			IJsonObject json1 = JsonObject.Of(-1.0);

			// Act
			// Assert
			Assert.That(json0.NumberValue(), Is.EqualTo(0.5));
			Assert.That(json1.NumberValue(), Is.EqualTo(-1));
		}

		[Test()]
		public void NumberValue_IgnoresFallbackValues()
		{
			// Arrange
			IJsonObject json0 = JsonObject.Of(0.5);
			IJsonObject json1 = JsonObject.Of(-1.0);

			// Act
			// Assert
			Assert.That(json0.NumberValue(1), Is.EqualTo(0.5));
			Assert.That(json1.NumberValue(2), Is.EqualTo(-1));
		}

		[Test()]
		public void BooleanValue_ThrowsException()
		{
			// Arrange
			IJsonObject json = JsonObject.Of(1);

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
			IJsonObject json = JsonObject.Of(0);

			// Act
			// Assert
			Assert.That(json.BooleanValue(true), Is.True);
		}

		[Test()]
		public void Properties_ReturnsEmptyEnumeralbe()
		{
			// Arrange
			IJsonObject json = JsonObject.Of(1);

			// Act
			// Assert
			Assert.That(json.Properties.Count(), Is.EqualTo(0));
		}

		[Test()]
		public void HasProperty_ReturnsFalse()
		{
			// Arrange
			IJsonObject json = JsonObject.Of(1);

			// Act
			// Assert
			Assert.That(json.HasProperty("foo"), Is.False);
		}

		[Test()]
		public void GetProperty_ThrowsException()
		{
			// Arrange
			IJsonObject json = JsonObject.Of(1);

			// Act
			// Assert
			Assert.Throws<ApplicationException>(() =>
			{
				json.GetProperty("foo");
			});
		}

		[Test()]
		public void Equals_ComparesBasedOnValueWrappedByJsonObject()
		{
			Assert.That(JsonObject.Of(1).Equals(JsonObject.Of(1.0)), Is.True);
			Assert.That(JsonObject.Of(0).Equals(JsonObject.Of(0.0)), Is.True);
			Assert.That(JsonObject.Of(0).Equals(JsonObject.Of(0.1)), Is.False);
		}
	}
}

