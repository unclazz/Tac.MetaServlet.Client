using NUnit.Framework;
using System.Linq;
using Tac.MetaServlet.Client;

namespace Test.Tac.MetaServlet.Client
{
	[TestFixture()]
	public class ParametersTest
	{
		[Test()]
		public void ctor_InitializedPropertiesByDefaultValues()
		{
			// Arrange
			var ps = new Parameters();

			// Act
			// Assert
			Assert.That(ps.RemoteHost, Is.EqualTo("localhost"));
			Assert.That(ps.RemotePort, Is.EqualTo(8080));
			Assert.That(ps.RemotePath, Is.EqualTo("/org.talend.administrator/metaServlet"));
			Assert.That(ps.RequestTimeout, Is.EqualTo(100000));
			Assert.That(ps.RequestJson, Is.Null);
			Assert.That(ps.ShowDump, Is.False);
			Assert.That(ps.ShowHelp, Is.False);
		}

		[Test()]
		public void GetMetaData_ReturnsParameterMetaSequence()
		{
			// Arrange
			var ps = new Parameters();
			var optNames = new string[] { "/J", "/H", "/P", "/Q", "/T", "/D", "/?" };
			var setNames = new string[] {
				"Tac.MetaServlet.Client.Request.Json",
				"Tac.MetaServlet.Client.Remote.Host",
				"Tac.MetaServlet.Client.Remote.Port",
				"Tac.MetaServlet.Client.Remote.Path",
				"Tac.MetaServlet.Client.Request.Timeout",
				"Tac.MetaServlet.Client.Show.Dump",
				"Tac.MetaServlet.Client.Show.Help"
			};

			// Act
			var md = ps.GetMetaData();

			// Assert
			Assert.That(optNames
						.All((a1) => md
							 .Select((a2) => a2.OptionName)
							 .Contains(a1)),
						Is.True);
			Assert.That(setNames
				.All((a1) => md
					 .Select((a2) => a2.SettingName)
					 .Contains(a1)),
				Is.True);
		}

		[Test()]
		public void ParameterMeta_SetterDelegate_SetValueToRelatedPropertyOfParameters()
		{
			// Arrange
			var ps = new Parameters();
			var j = ps.GetMetaData().First((arg) => arg.OptionName.Equals("/J"));
			var h = ps.GetMetaData().First((arg) => arg.OptionName.Equals("/H"));
			var p = ps.GetMetaData().First((arg) => arg.OptionName.Equals("/P"));
			var q = ps.GetMetaData().First((arg) => arg.OptionName.Equals("/Q"));
			var t = ps.GetMetaData().First((arg) => arg.OptionName.Equals("/T"));
			var d = ps.GetMetaData().First((arg) => arg.OptionName.Equals("/D"));
			var qm = ps.GetMetaData().First((arg) => arg.OptionName.Equals("/?"));

			// Act
			j.SetterDelegate("foo");
			h.SetterDelegate("bar");
			p.SetterDelegate("1234");
			q.SetterDelegate("baz");
			t.SetterDelegate("5678");
			d.SetterDelegate("false");
			qm.SetterDelegate("false");

			// Assert
			Assert.That(ps.RequestJson, Is.EqualTo("foo"));
			Assert.That(ps.RemoteHost, Is.EqualTo("bar"));
			Assert.That(ps.RemotePort, Is.EqualTo(1234));
			Assert.That(ps.RemotePath, Is.EqualTo("baz"));
			Assert.That(ps.RequestTimeout, Is.EqualTo(5678));
			Assert.That(ps.ShowDump, Is.EqualTo(true));
			Assert.That(ps.ShowHelp, Is.EqualTo(true));
		}
	}
}

