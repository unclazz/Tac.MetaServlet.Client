using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Tac.MetaServlet.Client
{
	public sealed class Parameters
	{
		private readonly List<ParameterMeta> metaList = new List<ParameterMeta>();

		public string RemoteHost { get; set; }
		public string RemotePath { get; set; }
		public int RemotePort { get; set; }
		public int RequestTimeout { get; set; }
		public string RequestJson { get; set; }
		public IList<string> Others { get; }

		public Parameters()
		{
            RemoteHost = "localhost";
            RemotePath = "/org.talend.administrator/metaServlet";
            RemotePort = 8080;
            RequestTimeout = 100000;
			Others = new List<string>();
		}

        public IEnumerable<ParameterMeta> GetMetaData()
		{
			if (metaList.Count == 0)
			{
                metaList.Add(meta("/J", "Request.Json", (obj) => RequestJson = obj, true));
                metaList.Add(meta("/H", "Remote.Host", (obj) => RemoteHost = obj));
				metaList.Add(meta("/P", "Remote.Port", (obj) => RemotePort = int.Parse(obj)));
				metaList.Add(meta("/Q", "Remote.Path", (obj) => RemotePath = obj));
				metaList.Add(meta("/T", "Request.Timeout", (obj) => RequestTimeout = int.Parse(obj)));
			}
			return metaList.AsReadOnly();
		}

		private ParameterMeta meta(string optName, string setName, Action<string> setter, bool required)
		{
			return new ParameterMeta(optName, setName, setter, required);
		}

		private ParameterMeta meta(string optName, string setName, Action<string> setter)
		{
			return meta(optName, setName, setter, false);
		}
	}

	public sealed class ParameterMeta
	{
		public string OptionName { get; }
		public string SettingName { get; }
		public Action<string> SetterDelegate { get; }
		public bool Required { get; }

		internal ParameterMeta(string optName, string setName, Action<string> setter, bool required)
		{
			if (optName == null)
			{
				throw new ArgumentNullException(nameof(optName));
			}
			if (setName == null)
			{
				throw new ArgumentNullException(nameof(setName));
			}
			if (setter == null)
			{
				throw new ArgumentNullException(nameof(setter));
			}
			OptionName = optName;
			SettingName = setName;
			SetterDelegate = setter;
			Required = required;
		}
	}
}

