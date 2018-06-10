using System;

namespace Tangerine.Core.Operations
{
	public class Dummy : Operation
	{
		public override bool IsChangingDocument => false;

		public static void Perform()
		{
			Document.Current.History.DoTransaction(() => Document.Current.History.Perform(new Dummy()));
		}

		private Dummy() { }
	}
}
