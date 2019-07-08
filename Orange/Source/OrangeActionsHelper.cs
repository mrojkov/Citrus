using System;
using System.Collections.Generic;
using System.Threading;

namespace Orange.Source
{
	public static class OrangeActionsHelper
	{

		public static void ExecuteOrangeActionInstantly(Target target, Action action, Action onBegin, Action onEnd,
			Func<Boolean> onDoesNeedSvnUpdate, Func<Action, IEnumerator<object>> onCreateOrNotAsynchTask)
		{
			IEnumerator<object> enumerator = ExecuteOrangeAction(target, action, onBegin, onEnd, onDoesNeedSvnUpdate, onCreateOrNotAsynchTask);
			while (enumerator.MoveNext()) {
				Thread.Yield();
			}
		}

		public static void ExecuteOrangeActionInstantly(Target target, Func<string> action, Action onBegin, Action onEnd,
			Func<Boolean> onDoesNeedSvnUpdate, Func<Action, IEnumerator<object>> onCreateOrNotAsynchTask)
		{
			IEnumerator<object> enumerator = ExecuteOrangeAction(target, action, onBegin, onEnd, onDoesNeedSvnUpdate, onCreateOrNotAsynchTask);
			while (enumerator.MoveNext()) {
				Thread.Yield();
			}
		}

		public static IEnumerator<object> ExecuteOrangeAction(Target target, Action action, Action onBegin, Action onEnd,
			Func<Boolean> onDoesNeedSvnUpdate, Func<Action, IEnumerator<object>> onCreateOrNotAsynchTask)
		{
			return ExecuteOrangeAction(target, () => {
				action();
				return null;
			}, onBegin, onEnd, onDoesNeedSvnUpdate, onCreateOrNotAsynchTask);
		}

		public static IEnumerator<object> ExecuteOrangeAction(Target target, Func<string> action, Action onBegin, Action onEnd,
			Func<Boolean> onDoesNeedSvnUpdate, Func<Action, IEnumerator<object>> onCreateOrNotAsynchTask)
		{
			var startTime = DateTime.Now;
			onBegin();
			string executonResultReadable = "Build Failed! Unknown Error.";
			try {
				var updateCompleted = true;
				if (onDoesNeedSvnUpdate()) {
					var builder = new SolutionBuilder(target.Platform, target.ProjectPath);
					Action svnUpdateAction = () => {
						updateCompleted = SafeExecute(() => builder.SvnUpdate(target));
						if (!updateCompleted) executonResultReadable = "Build Failed! Can not update a repository.";
					};

					if (onCreateOrNotAsynchTask != null) {
						yield return onCreateOrNotAsynchTask(svnUpdateAction);
					} else {
						svnUpdateAction();
					}
				}

				if (!updateCompleted) yield break;

				The.Workspace?.AssetFiles?.Rescan();

				executonResultReadable = "Done.";

				Action mainAction = () => {
					string errorDetails = SafeExecuteWithErrorDetails(action);
					if (errorDetails != null) {
						if (errorDetails.Length > 0) {
							Console.WriteLine(errorDetails);
						}
						executonResultReadable = "Build Failed!";
					}
				};
				if (onCreateOrNotAsynchTask != null) {
					yield return onCreateOrNotAsynchTask(mainAction);
				} else {
					mainAction();
				}
			} finally {
				Console.WriteLine(executonResultReadable);
				Console.WriteLine(@"Elapsed time {0:hh\:mm\:ss}", DateTime.Now - startTime);
				onEnd();
			}
		}

		private static bool SafeExecute(Action action)
		{
			try {
				action();
			} catch (Exception ex) {
				Console.WriteLine(ex);
				return false;
			}
			return true;
		}

		private static string SafeExecuteWithErrorDetails(Func<string> action)
		{
			try {
				return action();
			} catch (MSBuildNotFound e) {
				bool dialogResult = The.UI.AskConfirmation("You need to download and install MSBuild 15.0. Download?");
				if (dialogResult) {
					System.Diagnostics.Process.Start(e.DownloadUrl);
				}
				return e.ToString();
			} catch (Exception ex) {
				return ex.ToString();
			}
		}

	}
}
