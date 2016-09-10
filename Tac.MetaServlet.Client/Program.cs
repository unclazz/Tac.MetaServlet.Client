using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Tac.MetaServlet.Json;
using Tac.MetaServlet.Rpc;

namespace Tac.MetaServlet.Client
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			try
			{
				new MainClass().Execute(args);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.ToString());
			}
		}

		public void Execute(string[] args)
		{
			if (args.Length == 0)
			{
				PrintUsage();
				return;
			}

			var ps = ParseParameters(args);
		}

		public void PrintUsage()
		{
			var b = new StringBuilder()
				.AppendLine("構文: tacrpc /j <json-file> [/h <host>] [/p <port>] [/q <path>] [/t <timeout>]")
				.AppendLine("解説: /j  RPCリクエストを表わすJSONが記述されたファイルのパス.")
				.AppendLine("      /h  RPCリクエスト先のホスト名. デフォルトは\"localhost\".")
				.AppendLine("      /p  RPCリクエスト先のポート名. デフォルトは8080.")
				.AppendLine("      /q  RPCリクエスト先のパス名. デフォルトは\"/org.talend.administrator/metaServlet\".")
				.AppendLine("      /t  RPCリクエストのタイムアウト時間. 単位はミリ秒. デフォルトは100000.");
			Console.WriteLine(b.ToString());
		}

		public IResponse SendRequest(Parameters ps, IJsonObject j)
		{
			var b = Request.Builder()
						  .Host(ps.RemoteHost)
						  .Port(ps.RemotePort)
						  .Path(ps.RemotePath)
						  .Timeout(ps.RequestTimeout);
			j.Properties
			 .ToList()
			 .ForEach((obj) => b.Parameter(obj.Name, obj.Value));
			return b.Build().Send();
		}

		public IJsonObject ParseRequestJson(Parameters ps)
		{
			return JsonObject.FromFile(ps.RequestJson, Encoding.UTF8);
		}

		public Parameters ParseParameters(string[] args)
		{
			Parameters ps = new Parameters();
		
			var pairs = MakeArgumentPairs(args);
			var settings = System.Configuration.ConfigurationManager.AppSettings;
			var result = ps.GetMetaData().Where((arg) => {
				return !ResolveParameter(arg, pairs, settings);
			}).Select((arg) => arg.OptionName).ToList();

			if (result.Count == 0)
			{
				return ps;
			}
			else {
				throw new ArgumentException
				(string
				 .Format("argument(s) muet be specified [{0}]."
				         , result.Aggregate((arg1, arg2) => arg1 + ", " + arg2)));
			}
		}

		private IEnumerable<Pair> MakeArgumentPairs(string[] args)
		{
			for (int i = 0; i + 1 < args.Length; i++)
			{
				yield return new Pair(args[i], args[i + 1]);
			}
		}

		private bool ResolveParameter(ParameterMeta meta, 
		                              IEnumerable<Pair> argPairs, 
		                              NameValueCollection settings)
		{
			Pair argumentPair = argPairs
				.FirstOrDefault((arg) => arg.Left.ToLower()
				                .Equals(meta.OptionName));
			string settingName = null;

			if (argumentPair != null)
			{
				meta.SetterDelegate(argumentPair.Right);
			}
			else {
				settingName = settings
					.AllKeys
					.FirstOrDefault((arg) => arg
					                .Equals(meta.SettingName));
				if (settingName != null)
				{
					meta.SetterDelegate(settings[settingName]);
				}
			}
			return !meta.Required || argumentPair != null || settingName != null;
		}

		public sealed class Pair
		{
			public string Left { get; }
			public string Right { get; }
			internal Pair(string left, string right)
			{
				Left = left; Right = right;
			}
		}
	}
}
