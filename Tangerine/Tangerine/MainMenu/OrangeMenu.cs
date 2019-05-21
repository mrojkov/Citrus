using System;
using Lime;

namespace Tangerine.MainMenu
{
	public class OrangeCommand : CommandHandler
	{
		private static volatile bool isExecuting;
		private readonly Action action;

		public static bool AnyExecuting => isExecuting;

		public OrangeCommand(Action action)
		{
			this.action = action;
		}

		public override void Execute()
		{
			UI.Console.Instance.Show();
			if (isExecuting) {
				Console.WriteLine("Orange is busy with a previous request.");
				return;
			}

			Orange.The.Workspace?.AssetFiles?.Rescan();
			ExecuteAsync();

			async void ExecuteAsync()
			{
				try {
					isExecuting = true;
					await System.Threading.Tasks.Task.Factory.StartNew(action);
					await System.Threading.Tasks.Task.Delay(500);
				} catch (System.Exception e) {
					Console.WriteLine(e);
				} finally {
					isExecuting = false;
				}
				Console.WriteLine("Done");
			}
		}

		public override void RefreshCommand(ICommand command) => command.Enabled = !isExecuting;
	}

	public class OrangePluginOptionsCommand : CommandHandler
	{
		public override void Execute() => new OrangePluginOptionsDialog();

		public override void RefreshCommand(ICommand command) => command.Enabled = !OrangeCommand.AnyExecuting;
	}
}
