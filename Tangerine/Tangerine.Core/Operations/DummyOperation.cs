using System;

namespace Tangerine.Core.Operations
{
	public class Dummy : Operation
	{
		public override bool IsChangingDocument => false;

		public static void Perform(DocumentHistory history)
		{
			history.DoTransaction(() => Document.Current.History.Perform(new Dummy()));
		}

		private Dummy() { }
	}
}
