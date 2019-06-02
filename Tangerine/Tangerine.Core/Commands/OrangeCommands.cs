using System;
using Lime;

namespace Tangerine.Core.Commands
{
	public class OrangeCommand : CommandHandler
	{
		private static volatile bool isExecuting;

		public static bool AnyExecuting => isExecuting;

		protected readonly Action Action;

		public Action Executing { get; set; }

		public OrangeCommand(Action action)
		{
			Action = action;
		}

		public override void Execute()
		{
			ExecuteAsync(Action);
		}

		protected void ExecuteAsync(Action action)
		{
			object WrappedAction()
			{
				action();
				return null;
			}
			_ = ExecuteAsync(WrappedAction);
		}

		protected async System.Threading.Tasks.Task<T> ExecuteAsync<T>(Func<T> function)
		{
			var result = default(T);
			try {
				if (isExecuting) {
					Console.WriteLine("Orange is busy with a previous request.");
					return result;
				}

				isExecuting = true;
				Executing?.Invoke();
				Orange.The.Workspace?.AssetFiles?.Rescan();
				result = await System.Threading.Tasks.Task<T>.Factory.StartNew(function);
				await System.Threading.Tasks.Task.Delay(500);
			} catch (System.Exception e) {
				Console.WriteLine(e);
			} finally {
				isExecuting = false;
			}
			Console.WriteLine("Done");
			return result;
		}

		public override void RefreshCommand(ICommand command) => command.Enabled = !isExecuting;
	}

	public class OrangeBuildCommand : OrangeCommand
	{
		public static OrangeBuildCommand Instance { get; set; }

		protected readonly Func<string> Function;

		public OrangeBuildCommand(Func<string> action) : base(() => action())
		{
			Function = action;
		}

		public override void Execute()
		{
			_ = ExecuteAsync(Function);
		}

		public static async System.Threading.Tasks.Task<(bool successful, string errorText)> ExecuteAsync()
		{
			if (Instance == null) {
				return (false, "Orange build command wasn't found.");
			}
			var errorText = await Instance.ExecuteAsync(Instance.Function);
			return (errorText == null, errorText);
		}
	}
}
