using System;
using System.Linq;
using System.Threading;
using Lime;
using Orange;
using Tangerine.UI;

namespace Tangerine
{
	public class OrangeInterface : Orange.UserInterface
	{
		public OrangePluginUIBuidler PluginUIBuilder;

		public override bool AskConfirmation(string text)
		{
			bool? result = null;
			Application.InvokeOnMainThread(() => result = AlertDialog.Show(text, new string[] { "Yes", "No" }) == 0);
			while (result == null) {
				Thread.Sleep(1);
			}
			return result.Value;
		}

		public override bool AskChoice(string text, out bool yes)
		{
			throw new NotImplementedException();
		}

		public override void ShowError(string message)
		{
			Application.InvokeOnMainThread(() => AlertDialog.Show(message));
		}

		public override Target GetActiveTarget()
		{
			return The.Workspace.Targets.First();
		}

		public override bool DoesNeedSvnUpdate()
		{
			return false;
		}

		public override IPluginUIBuilder GetPluginUIBuilder()
		{
			PluginUIBuilder = new OrangePluginUIBuidler();
			return PluginUIBuilder;
		}

		public override void CreatePluginUI(IPluginUIBuilder builder) { }
		public override void DestroyPluginUI() { }
	}
}