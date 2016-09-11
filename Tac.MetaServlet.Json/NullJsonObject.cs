namespace Tac.MetaServlet.Json
{
	sealed class NullJsonObject : JsonObject
	{
		public static readonly IJsonObject Instance = new NullJsonObject();

		private NullJsonObject() : base(JsonObjectType.Null)
		{

		}
		public override string ToString()
		{
			return "null";
		}
		public override int GetHashCode()
		{
			return 0;
		}
		public override bool Equals(object obj)
		{
			return obj is NullJsonObject;
		}
	}
}

