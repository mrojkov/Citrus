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

		public virtual void ExitWithErrorIfPossible() { }

		public virtual void ProcessPendingEvents() { }

		public virtual void OnWorkspaceOpened() { }

		public abstract bool DoesNeedSvnUpdate();

		public abstract IPluginUIBuilder GetPluginUIBuilder();

		public abstract void CreatePluginUI(IPluginUIBuilder builder);

		public abstract void DestroyPluginUI();

		public virtual void SaveToWorkspaceConfig(ref WorkspaceConfig config) { }

		public virtual void LoadFromWorkspaceConfig(WorkspaceConfig config) { }
	}
}
