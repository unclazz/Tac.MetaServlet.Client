using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Tac.MetaServlet.Json.Parser
{
	public sealed class JsonParser
	{
		private static readonly char WhiteSpace = ' ';
		private static readonly Regex NumberPattern = 
			new Regex("^-?(0|[1-9][0-9]*)(\\.[0-9]+)?([eE][+-]?[0-9]+)?");
		private static readonly Regex BooleanPattern = new Regex("^(true|false)");

		public IJsonObject Parse(Input input)
		{
			using (input)
			{
				input.SkipWhitespace();
				return ParseNode(input);
			}
		}

		IJsonObject ParseNode(Input input)
		{
			char curr = input.Current;
			if (curr == '{')
			{
				return ParseObjectNode(input);
			}
			else if (curr == '[')
			{
				return ParseArrayNode(input);
			}
			else if (curr == '"' || curr == '\'')
			{
				return ParseStringNode(input);
			}
			else if (curr == 't' || curr == 'f')
			{
				return ParseBooleanNode(input);
			}
			else if (curr == 'n')
			{
				return ParseNullNode(input);
			}
			else if (curr == '-' || ('0' <= curr && curr <= '9'))
			{
				return ParseNumberNode(input);
			}
			throw new ParseException(input, "unknown token.");
		}

		string ParseQuotedString(Input input)
		{
			StringBuilder buff = new StringBuilder();
			char quote = input.Current;

			while (!input.EndOfFile)
			{
				char c1 = input.GoNext();
				if (c1 == quote)
				{
					input.GoNext();
					return buff.ToString();
				}
				buff.Append(c1 != '\\' ? c1 : input.GoNext());
			}
			throw new ParseException(input, "syntax error. unclosed quoted string.");
		}
		string ParseIdentifierString(Input input)
		{
			StringBuilder buff = new StringBuilder();
			while (!input.EndOfFile)
			{
				if (input.Current <= WhiteSpace || input.Current == ':')
				{
					break;
				}
				buff.Append(input.Current);
				input.GoNext();
			}
			return buff.ToString();
		}
		IJsonObject ParseStringNode(Input input)
		{
			return JsonObject.Of(ParseQuotedString(input));
		}
		IJsonObject ParseBooleanNode(Input input)
		{
			string r = input.GoNext(BooleanPattern);
			return JsonObject.Of(r.Equals("true"));
		}
		IJsonObject ParseNullNode(Input input)
		{
			input.GoNext("null");
			return JsonObject.OfNull();
		}
		IJsonObject ParseNumberNode(Input input)
		{
			string r = input.GoNext(NumberPattern);
			return JsonObject.Of(double.Parse(r));
		}
		IJsonObject ParseArrayNode(Input input)
		{
			input.Check('[');
			input.GoNext();
			IList<IJsonObject> buff = new List<IJsonObject>();

			input.SkipWhitespace();
			if (input.Current == ']')
			{
				input.GoNext();
				return JsonObject.Of(buff);
			}

			while (!input.EndOfFile)
			{
				buff.Add(ParseNode(input));

				input.SkipWhitespace();
				if (input.Current == ']')
				{
					input.GoNext();
					return JsonObject.Of(buff);
				}
				input.Check(',');
				input.GoNext();
				input.SkipWhitespace();
			}
			throw new ParseException(input, "unclosed Array literal.");
		}
		IJsonObject ParseObjectNode(Input input)
		{
			input.Check('{');
			input.GoNext();
			var b = JsonObject.Builder();

			input.SkipWhitespace();
			if (input.Current == '}')
			{
				input.GoNext();
				return b.Build();
			}

			while (!input.EndOfFile)
			{
				string propName;
				if (input.Current == '"' || input.Current == '\'')
				{
					propName = ParseQuotedString(input);
				}
				else 
				{
					propName = ParseIdentifierString(input);
				}

				input.SkipWhitespace();
				input.Check(':');
				input.GoNext();

				input.SkipWhitespace();
				b.Append(propName, ParseNode(input));

				input.SkipWhitespace();
				if (input.Current == '}')
				{
					input.GoNext();
					return b.Build();
				}
				input.Check(',');
				input.GoNext();
				input.SkipWhitespace();
			}
			throw new ParseException(input, "unclosed Object literal.");
		}
	}
}

