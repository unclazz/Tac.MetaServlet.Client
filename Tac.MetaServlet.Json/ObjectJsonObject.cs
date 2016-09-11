using System.Collections.Generic;
using System.Text;
using System.Linq;

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

		private ObjectJsonObject(IEnumerable<IJsonProperty> props) : base(JsonObjectType.Object)
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

		public override int GetHashCode()
		{
			return props.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var other = obj as ObjectJsonObject;
			return other != null && AreSame(this, other);
		}

		private bool AreSame(ObjectJsonObject l, ObjectJsonObject r)
		{
			return l.Properties.Count() == r.Properties.Count() 
			 && l
				.Properties
				.OrderBy((p) => p.Name)
				.Zip(r
				     .Properties
				     .OrderBy((p) => p.Name), (lp, rp) => 
				     lp.Name.Equals(rp.Name) && lp.Value.Equals(rp.Value))
				.All((arg) => arg);
		}
	}
}

