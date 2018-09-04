using System;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine
{
	public class SetNextDocument : DocumentCommandHandler
	{
		public override bool GetEnabled()
		{
			// Prevent from unintentional execute by Ctrl+Tab while exposition. 
			return !Document.Current.ExpositionMode;
		}

		public override void ExecuteTransaction()
		{
			Project.Current.NextDocument();
		}
	}

	public class SetPreviousDocument : DocumentCommandHandler
	{
		public override bool GetEnabled()
		{
			// Prevent from unintentional execute by Ctrl+Tab while exposition. 
			return !Document.Current.ExpositionMode;
		}

		public override void ExecuteTransaction()
		{
			Project.Current.PreviousDocument();
		}
	}
}
