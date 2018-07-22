using System;
using Lime;

namespace Tangerine.UI
{
	public class OrangeCommandHandler : CommandHandler
	{
		private readonly Action action;

		public OrangeCommandHandler(Action action) {
			this.action = action;
		}

		public override void Execute() {
			action();
		}

		public override void RefreshCommand(ICommand command) {
			command.Enabled = Core.Project.Current != Core.Project.Null;
		}
	}
}
