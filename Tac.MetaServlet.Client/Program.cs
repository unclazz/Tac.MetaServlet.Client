using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Tac.MetaServlet.Json;
using Tac.MetaServlet.Rpc;

namespace Tac.MetaServlet.Client
{
    class MainClass
    {
        public static int Main(string[] args)
        {
            try
            {
                new MainClass().Execute(args);
            }
            catch (Exception e)
            {
                Console.Error.Write("エラー: ");
                Console.Error.WriteLine(e.ToString());
                return 1;
            }
            return 0;
        }

        private readonly IJsonFormatOptions formatOptions =
            JsonFormatOptions.Builder().Indent(true).Build();

        public void Execute(string[] args)
        {
            try
            {
                var ps = ParseParameters(args);
                var j = ParseRequestJson(ps);
                var req = BuildRequest(ps, j);
                DumpRequest(req);
                var resp = req.Send();
                DumpResponse(resp);
            }
            catch (ArgumentException e)
            {
                PrintUsage();
                throw;
            }
        }

        public void DumpRequest(IRequest req)
        {
            var b = JsonObject.Builder(req.Parameters);
            if (req.Parameters.HasProperty("authPass"))
            {
                b.Append("authPass", "*****");
            }
            Console.WriteLine("リクエストURI : {0}", req.Uri);
            Console.WriteLine("リクエストJSON: {0}", b.Build().Format(formatOptions));
        }

        public void DumpResponse(IResponse resp)
        {
            Console.WriteLine("HTTPステータス: {0}", resp.StatusCode);
            Console.WriteLine("レスポンスJSON: {0}", resp.Body.Format(formatOptions));
        }

        public void PrintUsage()
        {
            var b = new StringBuilder()
                .AppendLine("構文: TACRPC /J <json-file> [/H <host>] [/P <port>] [/Q <path>] [/T <timeout>]")
                .AppendLine("解説: /J  RPCリクエストを表わすJSONが記述されたファイルのパス.")
                .AppendLine("      /H  RPCリクエスト先のホスト名. デフォルトは\"localhost\".")
                .AppendLine("      /P  RPCリクエスト先のポート名. デフォルトは8080.")
                .AppendLine("      /Q  RPCリクエスト先のパス名. デフォルトは\"/org.talend.administrator/metaServlet\".")
                .AppendLine("      /T  RPCリクエストのタイムアウト時間. 単位はミリ秒. デフォルトは100000.");
            Console.WriteLine(b.ToString());
        }

        public IRequest BuildRequest(Parameters ps, IJsonObject j)
        {
            var b = Request.Builder()
                          .Host(ps.RemoteHost)
                          .Port(ps.RemotePort)
                          .Path(ps.RemotePath)
                          .Timeout(ps.RequestTimeout);
            j.Properties
             .ToList()
             .ForEach((obj) => b.Parameter(obj.Name, obj.Value));
            return b.Build();
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
            var result = ps.GetMetaData().Where((arg) =>
            {
                return !ResolveParameter(arg, pairs, settings);
            }).Select((arg) => arg.OptionName).ToList();

            if (result.Count == 0)
            {
                return ps;
            }
            else
            {
                throw new ArgumentException
                (string
                 .Format("argument(s) must be specified [{0}]."
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
                .FirstOrDefault((arg) => arg.Left.ToUpper()
                                .Equals(meta.OptionName));
            string settingName = null;

            if (argumentPair != null)
            {
                meta.SetterDelegate(argumentPair.Right);
            }
            else
            {
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
