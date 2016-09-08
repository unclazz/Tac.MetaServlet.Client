namespace Tac.MetaServlet.Json
{
	sealed class BooleanJsonObject : JsonObject
	{
		private readonly bool val;

		internal BooleanJsonObject(bool val) : base(JsonObjectType.Boolean)
		{
			this.val = val;
		}
		public override bool BooleanValue()
		{
			return val;
		}
		public override bool BooleanValue(bool fallback)
		{
			return val;
		}
		public override string ToString()
		{
			return val ? "true" : "false";
		}
	}
}

