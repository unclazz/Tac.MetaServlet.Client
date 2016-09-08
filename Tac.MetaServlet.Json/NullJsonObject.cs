namespace Tac.MetaServlet.Json
{
	sealed class NullJsonObject : JsonObject
	{
		public static readonly IJsonObject Instance = new NullJsonObject();

		NullJsonObject() : base(JsonObjectType.Null)
		{

		}
		public override string ToString()
		{
			return "null";
		}
	}
}

