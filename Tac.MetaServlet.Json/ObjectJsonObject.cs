using System.Collections.Generic;
using System.Text;

namespace Tac.MetaServlet.Json
{
	sealed class ObjectJsonObject : JsonObject
	{
		private readonly IEnumerable<IJsonProperty> props;
		private string literalBuff = null;

		public static IJsonObject Of(IEnumerable<IJsonProperty> props)
		{
			return new ObjectJsonObject(props);
		}

		ObjectJsonObject(IEnumerable<IJsonProperty> props) : base(JsonObjectType.Object)
		{
			this.props = props;
		}

		public override IEnumerable<IJsonProperty> Properties
		{
			get
			{
				return props;
			}
		}

		public override string ToString()
		{
			if (literalBuff == null)
			{
				StringBuilder buff = new StringBuilder().Append('{');
				foreach (IJsonProperty prop in props)
				{
					if (buff.Length > 1)
					{
						buff.Append(',');
					}
					buff.Append(Quotes(prop.Name)).Append(':')
						.Append(prop.Value.ToString());
				}
				literalBuff = buff.Append('}').ToString();
			}
			return literalBuff;
		}
	}
}

