using System;
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

		public override TargetPlatform GetActivePlatform()
		{
#if WIN
			return TargetPlatform.Win;
#elif MAC
			return TargetPlatform.Mac;
#endif
		}

		public override SubTarget GetActiveSubTarget()
		{
			return null;
		}

		public override bool DoesNeedSvnUpdate()
		{
			return false;
		}

		public override IPluginUIBuilder GetPluginUIBuilder()
		{
			return null;
		}

		public override void CreatePluginUI(IPluginUIBuilder builder) { }
		public override void DestroyPluginUI() { }
	}
}