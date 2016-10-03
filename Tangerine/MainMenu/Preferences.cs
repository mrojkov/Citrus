using System;
using System.Linq;
using Lime;
using Tangerine.UI;

namespace Tangerine
{
	public class PreferencesCommand : Command
	{
		public override string Text => "Preferences...";
		public override Shortcut Shortcut => KeyBindings.GenericKeys.PreferencesDialog;

		public override void Execute()
		{
			new PreferencesDialog();
		}
	}	
}