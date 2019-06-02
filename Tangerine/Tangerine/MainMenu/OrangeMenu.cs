using Lime;
using Tangerine.Core.Commands;

namespace Tangerine.MainMenu
{
	public class OrangePluginOptionsCommand : CommandHandler
	{
		public override void Execute() => new OrangePluginOptionsDialog();

		public override void RefreshCommand(ICommand command) => command.Enabled = !OrangeCommand.AnyExecuting;
	}
}
