using System;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine
{
	public class SetNextDocument : DocumentCommandHandler
	{
		public override void Execute()
		{
			Project.Current.NextDocument();
		}
	}

	public class SetPreviousDocument : DocumentCommandHandler
	{
		public override void Execute()
		{
			Project.Current.PreviousDocument();
		}
	}
}
