using System;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine
{
	public class NextDocumentCommand : Command
	{
		public NextDocumentCommand()
		{
			Text = "Next Document";
			Shortcut = KeyBindings.GenericKeys.NextDocument;
		}

		public override void Execute()
		{
			Project.Current.NextDocument();
		}
	}

	public class PreviousDocumentCommand : Command
	{
		public PreviousDocumentCommand()
		{
			Text = "Previous Document";
			Shortcut = KeyBindings.GenericKeys.PreviousDocument;
		}

		public override void Execute()
		{
			Project.Current.PreviousDocument();
		}
	}
}
