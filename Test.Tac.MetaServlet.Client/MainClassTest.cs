using NUnit.Framework;
using System;
using Tac.MetaServlet.Client;

namespace Test.Tac.MetaServlet.Client
{
	[TestFixture()]
	public class MainClassTest
	{
		[Test()]
		public void Execute_ApplyToQuestionMarkFlag_AlwaysReturns0()
		{
			// Arrange
			var m = new MainClass();

			// Act
			var ec0 = m.Execute("/?");
			var ec1 = m.Execute("/?", "/J", "foo.json");

			// Assert
			Assert.That(ec0, Is.EqualTo(0));
			Assert.That(ec1, Is.EqualTo(0));
		}

		[Test()]
		public void Execute_WhenRequiedParameterNotSpecified_Returns1()
		{
			// Arrange
			var m = new MainClass();

			// Act
			var ec0 = m.Execute();
			var ec1 = m.Execute("/H", "example.com");

			// Assert
			Assert.That(ec0, Is.EqualTo(1));
			Assert.That(ec1, Is.EqualTo(1));
		}

		[Test()]
		public void Execute_WhenJsonFileNotFound_Returns1()
		{
			// Arrange
			var m = new MainClass();

			// Act
			var ec0 = m.Execute("/J", "");
			var ec1 = m.Execute("/J", "foo.json");

			// Assert
			Assert.That(ec0, Is.EqualTo(1));
			Assert.That(ec1, Is.EqualTo(1));
		}

		[Test()]
		public void Execute_WhenConnectionRefused_Returns1()
		{
			// Arrange
			var m = new MainClass();

			// Act
			var ec0 = m.Execute("/J", "../../../Tac.MetaServlet.Client/sample/sample_getTaskIdByName.json");

			// Assert
			Assert.That(ec0, Is.EqualTo(1));
		}

		[Test()]
		public void FormatMessage_WhenLabelIsEmptyOrNull_ReturnsNotIndentedString()
		{
			// Arrange
			var m = new MainClass();

			// Act
			var ec0 = m.FormatMessage(string.Empty, "foo\nbar\nbaz");
			var ec1 = m.FormatMessage(null, "foo\nbar\nbaz");

			// Assert
			Assert.That(ec0, Is.EqualTo("foo\nbar\nbaz"));
			Assert.That(ec1, Is.EqualTo("foo\nbar\nbaz"));
		}

		[Test()]
		public void FormatMessage_WhenLabelIsNotEmpty_ReturnsIndentedString()
		{
			// Arrange
			var m = new MainClass();

			// Act
			var ec0 = m.FormatMessage("らべる", "foo\nbar\nbaz");
			var ec1 = m.FormatMessage("label", "foo\nbar\nbaz");
			var ec2 = m.FormatMessage("label", "foo\r\nbar\n\nbaz\r");

			// Assert
			Assert.That(ec0, Is.EqualTo("らべる: foo\n        bar\n        baz"));
			Assert.That(ec1, Is.EqualTo("label: foo\n       bar\n       baz"));
			Assert.That(ec2, Is.EqualTo("label: foo\n       bar\n       baz"));
		}
	}
}

