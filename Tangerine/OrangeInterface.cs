using System;
using System.Linq;
using Orange;

namespace Tangerine
{
	public class OrangeInterface : Orange.UserInterface
	{
		public override bool AskConfirmation(string text)
		{
			throw new NotImplementedException();
		}

		public override bool AskChoice(string text, out bool yes)
		{
			throw new NotImplementedException();
		}

		public override void ShowError(string message)
		{
			throw new NotImplementedException();
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
			return new OrangePluginUIBuidler();
		}

		public override void CreatePluginUI(IPluginUIBuilder builder) { }
		public override void DestroyPluginUI() { }
	}
}