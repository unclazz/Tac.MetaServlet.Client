using System;
namespace Tac.MetaServlet.Rpc
{
	static class Assertions
	{
		internal static void MustNotBeNull(string targetLabel, params object[] targets)
		{
			foreach (object target in targets)
			{
				if (target == null)
				{
					throw new ArgumentNullException(targetLabel);
				}
			}
		}
		internal static void MustNotBeEmpty(string targetLabel, params string[] targets)
		{
			foreach (string target in targets)
			{
				if (target.Length == 0)
				{
					throw new ArgumentException(string.Format("{0} must not be empty.", targetLabel));
				}
			}
		}
		internal static void MustBeGreaterThanOrEqual0(string targetLabel, params int[] targets)
		{
			foreach (int target in targets)
			{
				if (target < 0)
				{
					throw new ArgumentException(string.Format("{0} must be greater than or equal 0.", targetLabel));
				}
			}
		}
	}
}

