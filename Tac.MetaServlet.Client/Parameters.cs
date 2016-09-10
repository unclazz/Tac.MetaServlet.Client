using System;
using System.Linq;
using System.Collections.Generic;

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
			Others = new List<string>();
		}

		public IEnumerable<ParameterMeta> GetMetaData()
		{
			if (metaList.Count == 0)
			{
				metaList.Add(meta("/h", "Remote.Host", (obj) => RemoteHost = obj));
				metaList.Add(meta("/p", "Remote.Port", (obj) => RemotePort = int.Parse(obj)));
				metaList.Add(meta("/q", "Remote.Path", (obj) => RemotePath = obj));
				metaList.Add(meta("/t", "Request.Timeout", (obj) => RequestTimeout = int.Parse(obj)));
				metaList.Add(meta("/j", "Request.Json", (obj) => RequestJson = obj, true));
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

