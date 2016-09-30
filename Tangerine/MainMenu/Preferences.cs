using System;
using System.Linq;
using Lime;
using Tangerine.UI;

namespace Tangerine
{
	public class PreferencesCommand : Command
	{
		public PreferencesCommand()
		{
			Text = "Preferences...";
			Shortcut = KeyBindings.GenericKeys.PreferencesDialog;
		}

		public override void Execute()
		{
			new PreferencesDialog();
		}
	}	
}