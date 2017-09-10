using NUnit.Framework;
using System.Linq;
using Tac.MetaServlet.Client;
using System;
using System.Reflection;
using System.IO;

namespace Test.Tac.MetaServlet.Client
{
	[TestFixture()]
	public class ParametersTest
	{
        static string GetSolutionDirectoryPath()
        {
            var dllFullPath = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
            var releaseOfDebug = Path.GetDirectoryName(dllFullPath);
            var bin = Path.GetDirectoryName(releaseOfDebug);
            var project = Path.GetDirectoryName(bin);
            return Path.GetDirectoryName(project);
        }

        static string GetSampleJsonPath(string filename)
        {
            return Path.Combine(GetSolutionDirectoryPath(), 
                "Tac.MetaServlet.Client", "sample", filename);
        }

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

		[Test()]
		public void Validate_WhenJsonFileNotFount_ThrowsException()
		{
			// Arrange
			var ps = new Parameters();
			var j = ps.GetMetaData().First((arg) => arg.OptionName.Equals("/J"));

			// Act
			// Assert
			j.SetterDelegate(null);
			Assert.Throws<ArgumentException>(ps.Validate);
			j.SetterDelegate(string.Empty);
			Assert.Throws<ArgumentException>(ps.Validate);
			j.SetterDelegate("foo");
			Assert.Throws<ArgumentException>(ps.Validate);
			j.SetterDelegate(GetSampleJsonPath("sample_getTaskIdByName.json"));
			Assert.DoesNotThrow(ps.Validate);
		}

		[Test()]
		public void Validate_WhenHostNameIsEmpty_ThrowsException()
		{
			// Arrange
			var ps = new Parameters();
			ps.GetMetaData().First((arg) => arg.OptionName.Equals("/J")).SetterDelegate(GetSampleJsonPath("sample_getTaskIdByName.json"));
			var m = ps.GetMetaData().First((arg) => arg.OptionName.Equals("/H"));

			// Act
			// Assert
			m.SetterDelegate(null);
			Assert.Throws<ArgumentException>(ps.Validate);
			m.SetterDelegate(string.Empty);
			Assert.Throws<ArgumentException>(ps.Validate);
			m.SetterDelegate("abc");
			Assert.DoesNotThrow(ps.Validate);
		}

		[Test()]
		public void Validate_WhenPortNumberIsLessThan1_ThrowsException()
		{
			// Arrange
			var ps = new Parameters();
			ps.GetMetaData().First((arg) => arg.OptionName.Equals("/J")).SetterDelegate(GetSampleJsonPath("sample_getTaskIdByName.json"));
			var m = ps.GetMetaData().First((arg) => arg.OptionName.Equals("/P"));

			// Act
			// Assert
			m.SetterDelegate("-1");
			Assert.Throws<ArgumentException>(ps.Validate);
			m.SetterDelegate("0");
			Assert.Throws<ArgumentException>(ps.Validate);
			m.SetterDelegate("1");
			Assert.DoesNotThrow(ps.Validate);
		}

		[Test()]
		public void Validate_WhenTimeoutMillisIsLessThan1_ThrowsException()
		{
			// Arrange
			var ps = new Parameters();
			ps.GetMetaData().First((arg) => arg.OptionName.Equals("/J")).SetterDelegate(GetSampleJsonPath("sample_getTaskIdByName.json"));
			var m = ps.GetMetaData().First((arg) => arg.OptionName.Equals("/T"));

			// Act
			// Assert
			m.SetterDelegate("-2");
			Assert.Throws<ArgumentException>(ps.Validate);
			m.SetterDelegate("-1");
			Assert.Throws<ArgumentException>(ps.Validate);
			m.SetterDelegate("0");
			Assert.DoesNotThrow(ps.Validate);
		}
	}
}

