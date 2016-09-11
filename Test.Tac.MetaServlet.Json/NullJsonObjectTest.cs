using NUnit.Framework;
using System;
using Tac.MetaServlet.Json.Parser;
using Tac.MetaServlet.Json;
using System.Linq;

namespace Test.Tac.MetaServlet.Json
{
	[TestFixture()]
	public class NullJsonObjectTest
	{
		
		[Test()]
		public void ToString_Always_ReturnsNull()
		{
			// Arrange
			IJsonObject json = JsonObject.OfNull();

			// Act
			string r = json.ToString();

			// Assert
			Assert.That(r, Is.EqualTo("null"));
		}

		[Test()]
		public void IsObjectExactly_Always_ReturnsFalse()
		{
			// Arrange
			IJsonObject json = JsonObject.OfNull();

			// Act
			var r = json.IsObjectExactly();

			// Assert
			Assert.That(r, Is.False);
		}

		[Test()]
		public void IsNull_Always_ReturnsTrue()
		{
			// Arrange
			IJsonObject json = JsonObject.OfNull();

			// Act
			var r = json.IsNull();

			// Assert
			Assert.That(r, Is.True);
		}

		[Test()]
		public void ArrayValue_Always_ThrowsException()
		{
			// Arrange
			IJsonObject json = JsonObject.OfNull();

			// Act
			// Assert
			Assert.Throws<ApplicationException>(() =>
			{
				json.ArrayValue();
			});
		}

		[Test()]
		public void ArrayValue1_Always_ReturnsFallback()
		{
			// Arrange
			IJsonObject json = JsonObject.OfNull();

			// Act
			// Assert
			Assert.That(json.ArrayValue(JsonObject.Of("foo", "bar").ArrayValue()).Count, Is.EqualTo(2));
		}

		[Test()]
		public void StringValue_Always_ThrowsException()
		{
			// Arrange
			IJsonObject json = JsonObject.OfNull();

			// Act
			// Assert
			Assert.Throws<ApplicationException>(() =>
			{
				json.StringValue();
			});
		}

		[Test()]
		public void StringValue1_Always_ReturnsFallback()
		{
			// Arrange
			IJsonObject json = JsonObject.OfNull();

			// Act
			// Assert
			Assert.That(json.StringValue("foo"), Is.EqualTo("foo"));
		}

		[Test()]
		public void BooleanValue_Always_ThrowsException()
		{
			// Arrange
			IJsonObject json = JsonObject.OfNull();

			// Act
			// Assert
			Assert.Throws<ApplicationException>(() =>
			{
				json.BooleanValue();
			});
		}

		[Test()]
		public void BooleanValue1_Always_ReturnsFallback()
		{
			// Arrange
			IJsonObject json = JsonObject.OfNull();

			// Act
			// Assert
			Assert.That(json.BooleanValue(true), Is.True);
		}

		[Test()]
		public void Properties_Always_ReturnsEmptyEnumeralbe()
		{
			// Arrange
			IJsonObject json = JsonObject.OfNull();

			// Act
			// Assert
			Assert.That(json.Properties.Count(), Is.EqualTo(0));
		}

		[Test()]
		public void HasProperty_Always_ReturnsFalse()
		{
			// Arrange
			IJsonObject json = JsonObject.OfNull();

			// Act
			// Assert
			Assert.That(json.HasProperty("foo"), Is.False);
		}

		[Test()]
		public void GetProperty_Always_ThrowsException()
		{
			// Arrange
			IJsonObject json = JsonObject.OfNull();

			// Act
			// Assert
			Assert.Throws<ApplicationException>(() =>
			{
				json.GetProperty("foo");
			});
		}

		[Test()]
		public void NumberValue_ThrowsException()
		{
			// Arrange
			IJsonObject json0 = JsonObject.OfNull();

			// Act
			// Assert
			Assert.Throws<ApplicationException>(() => json0.NumberValue());
		}

		[Test()]
		public void NumberValue_ReturnsFallbackValue()
		{
			// Arrange
			IJsonObject json0 = JsonObject.OfNull();

			// Act
			// Assert
			Assert.That(json0.NumberValue(1), Is.EqualTo(1));
		}
	}
}

