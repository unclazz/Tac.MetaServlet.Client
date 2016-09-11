using NUnit.Framework;
using System;
using Tac.MetaServlet.Json.Parser;
using Tac.MetaServlet.Json;
using System.Linq;

namespace Test.Tac.MetaServlet.Json
{
	[TestFixture()]
	public class ArrayJsonObjectTest
	{
		
		[Test()]
		public void ToString_ReturnsStringLiteral()
		{
			// Arrange
			IJsonObject json0 = JsonObject.Of(new string[0]);
			IJsonObject json1 = JsonObject.Of(1,2,3);

			// Act
			string r0 = json0.ToString();
			string r1 = json1.ToString();

			// Assert
			Assert.That(r0, Is.EqualTo("[]"));
			Assert.That(r1, Is.EqualTo("[1,2,3]"));
		}

		[Test()]
		public void IsObjectExactly_ReturnsFalse()
		{
			// Arrange
			IJsonObject json = JsonObject.Of(new string[0]);

			// Act
			var r = json.IsObjectExactly();

			// Assert
			Assert.That(r, Is.False);
		}

		[Test()]
		public void IsNull_ReturnsFalse()
		{
			// Arrange
			IJsonObject json = JsonObject.Of(new string[0]);

			// Act
			var r = json.IsNull();

			// Assert
			Assert.That(r, Is.False);
		}

		[Test()]
		public void ArrayValue_ReturnsValue()
		{
			// Arrange
			IJsonObject json = JsonObject.Of(2,3,4);

			// Act
			// Assert
			Assert.That(json
			            .ArrayValue()
			            .Select((arg) => arg.NumberValue())
			            .ToArray(),
			            Is.EqualTo(new long[] { 2, 3, 4 }));
		}

		[Test()]
		public void ArrayValue1_IgnoresFallback()
		{
			// Arrange
			IJsonObject json = JsonObject.Of(2, 3, 4);

			// Act
			// Assert
			Assert.That(json
						.ArrayValue()
						.Select((arg) => arg.NumberValue())
						.ToArray(),
						Is.EqualTo(new long[] { 2, 3, 4 }));
		}

		[Test()]
		public void StringValue_ThrowsException()
		{
			// Arrange
			IJsonObject json0 = JsonObject.Of(new bool[0]);
			IJsonObject json1 = JsonObject.Of("abc", "def");

			// Act
			// Assert
			Assert.Throws<ApplicationException>(() => json0.StringValue());
			Assert.Throws<ApplicationException>(() => json1.StringValue());
		}

		[Test()]
		public void StringValue1_ReturnsFallback()
		{
			// Arrange
			IJsonObject json0 = JsonObject.Of(new bool[0]);
			IJsonObject json1 = JsonObject.Of("abc", "def");

			// Act
			// Assert
			Assert.That(json0.StringValue("foo"), Is.EqualTo("foo"));
			Assert.That(json1.StringValue("bar"), Is.EqualTo("bar"));
		}

		[Test()]
		public void BooleanValue_ThrowsException()
		{
			// Arrange
			IJsonObject json = JsonObject.Of("abc", "def");

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
			IJsonObject json = JsonObject.Of("abc", "def");

			// Act
			// Assert
			Assert.That(json.BooleanValue(true), Is.True);
		}

		[Test()]
		public void Properties_ReturnsEmptyEnumeralbe()
		{
			// Arrange
			IJsonObject json = JsonObject.Of("abc", "def");

			// Act
			// Assert
			Assert.That(json.Properties.Count(), Is.EqualTo(0));
		}

		[Test()]
		public void HasProperty_ReturnsFalse()
		{
			// Arrange
			IJsonObject json = JsonObject.Of("abc", "def");

			// Act
			// Assert
			Assert.That(json.HasProperty("foo"), Is.False);
		}

		[Test()]
		public void GetProperty_ThrowsException()
		{
			// Arrange
			IJsonObject json = JsonObject.Of("abc", "def");

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
			IJsonObject json0 = JsonObject.Of(new bool[0]);
			IJsonObject json1 = JsonObject.Of("abc", "def");

			// Act
			// Assert
			Assert.Throws<ApplicationException>(() => json0.NumberValue());
			Assert.Throws<ApplicationException>(() => json1.NumberValue());
		}

		[Test()]
		public void NumberValue_ReturnsFallbackValue()
		{
			// Arrange
			IJsonObject json0 = JsonObject.Of(new bool[0]);
			IJsonObject json1 = JsonObject.Of("abc", "def");

			// Act
			// Assert
			Assert.That(json0.NumberValue(1), Is.EqualTo(1));
			Assert.That(json1.NumberValue(2), Is.EqualTo(2));
		}
	}
}

