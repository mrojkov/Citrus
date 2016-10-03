using System;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine
{
	public class NextDocumentCommand : Command
	{
		public override string Text => "Next Document";
		public override Shortcut Shortcut => KeyBindings.GenericKeys.NextDocument;

		public override void Execute()
		{
			Project.Current.NextDocument();
		}
	}

	public class PreviousDocumentCommand : Command
	{
		public override string Text => "Previous Document";
		public override Shortcut Shortcut => KeyBindings.GenericKeys.PreviousDocument;

		public override void Execute()
		{
			Project.Current.PreviousDocument();
		}
	}
}
