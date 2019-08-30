using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	public abstract class UserInterface
	{
		public static UserInterface Instance;

		public virtual void Initialize()
		{
			var w = new Lime.DummyWindow();
			var windowWidget = new Lime.WindowWidget(w);
		}

		public virtual void ClearLog() { }

		public virtual void ScrollLogToEnd() { }

		public virtual void RefreshMenu() { }

		public abstract bool AskConfirmation(string text);

		public abstract bool AskChoice(string text, out bool yes);

		public abstract void ShowError(string message);

		public abstract Target GetActiveTarget();

		public abstract void StopProgressBar();
		public abstract void SetupProgressBar(int maxPosition);
		public abstract void IncreaseProgressBar(int amount = 1);

		/// <summary>
		/// Reloads bundle list in BundlePicker and recreates its UI if neccessary
		/// </summary>
		public virtual void ReloadBundlePicker()
		{
			BundlePicker.Instance.Setup();
		}

		public virtual void ExitWithErrorIfPossible() { }

		public virtual void ProcessPendingEvents() { }

		public virtual void OnWorkspaceOpened() { }

		public abstract IPluginUIBuilder GetPluginUIBuilder();

		public abstract void CreatePluginUI(IPluginUIBuilder builder);

		public abstract void DestroyPluginUI();

		public virtual void SaveToWorkspaceConfig(ref WorkspaceConfig config) { }

		public virtual void LoadFromWorkspaceConfig(WorkspaceConfig config) { }
	}
}
