using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Tac.MetaServlet.Json
{
	sealed class ArrayJsonObject : JsonObject
	{
		private readonly IList<IJsonObject> items;
		private string literalBuff = null;

		internal ArrayJsonObject(IList<IJsonObject> items) : base(JsonObjectType.Array)
		{
			this.items = items;
		}
		public override IList<IJsonObject> ArrayValue()
		{
			return items;
		}
		public override IList<IJsonObject> ArrayValue(IList<IJsonObject> fallback)
		{
			return items;
		}
		public override string ToString()
		{
			if (literalBuff == null)
			{
				StringBuilder buff = new StringBuilder().Append('[');
				foreach (IJsonObject item in items)
				{
					if (buff.Length > 1)
					{
						buff.Append(',');
					}
					buff.Append(item.ToString());
				}
				literalBuff = buff.Append(']').ToString();
			}
			return literalBuff;
		}
		public override int GetHashCode()
		{
			return items.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			var other = obj as ArrayJsonObject;
			return other != null && this.items.SequenceEqual(other.items);
		}
	}
}

