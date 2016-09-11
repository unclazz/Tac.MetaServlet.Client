namespace Tac.MetaServlet.Json
{
	sealed class StringJsonObject : JsonObject
	{
		private readonly string val;
		private string literalBuff;

		internal StringJsonObject(string val) : base(JsonObjectType.String)
		{
			this.val = val;
		}
		public override string StringValue()
		{
			return val;
		}
		public override string StringValue(string fallback)
		{
			return val;
		}
		public override string ToString()
		{
			if (literalBuff == null)
			{
				literalBuff = Quotes(val);
			}
			return literalBuff;
		}
		public override int GetHashCode()
		{
			return val.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			var other = obj as StringJsonObject;
			return other != null && this.val.Equals(other.val);
		}
	}
}

