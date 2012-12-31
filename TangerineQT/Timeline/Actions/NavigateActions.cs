using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public class ChoosePrevNodeAction : ContextualAction
	{
		public ChoosePrevNodeAction()
			: base(Timeline.Instance.DockWidget, "Choose previous node")
		{
			Shortcut = "Up";
		}

		protected override void OnTriggered()
		{
			var lines = The.Document.SelectedLines;
			int line = 0;
			if (lines.Count > 0) {
				line = Math.Max(0, lines[0] - 1);
			}
			The.Document.History.Add(new Commands.SelectLines(line));
			The.Document.History.Commit(Text);
		}
	}

	public class ChooseNextNodeAction : ContextualAction
	{
		public ChooseNextNodeAction()
			: base(Timeline.Instance.DockWidget, "Choose next node")
		{
			Shortcut = "Down";
		}

		protected override void OnTriggered()
		{
			var lines = The.Document.SelectedLines;
			int line = 0;
			if (lines.Count > 0 && The.Timeline.Lines.Count > 0) {
				line = lines[lines.Count - 1] + 1;
				line = Math.Min(The.Timeline.Lines.Count - 1, line);
			}
			The.Document.History.Add(new Commands.SelectLines(line));
			The.Document.History.Commit(Text);
		}
	}

	public class EnterContainerAction : ContextualAction
	{
		public EnterContainerAction()
			: base(Timeline.Instance.DockWidget, "Enter container")
		{
			Shortcut = "Return";
		}

		protected override void OnTriggered()
		{
			var lines = The.Document.SelectedLines;
			if (lines.Count > 0) {
				var container = The.Document.Container.Nodes[lines[0]];
				The.Document.History.Add(new Commands.ChangeContainer(container));
				The.Document.History.Commit(Text);
			}
		}
	}

	public class ExitContainerAction : ContextualAction
	{
		public ExitContainerAction()
			: base(Timeline.Instance.DockWidget, "Exit container")
		{
			Shortcut = "Backspace";
		}

		protected override void OnTriggered()
		{
			if (The.Document.Container.Parent != null) {
				var container = The.Document.Container.Parent;
				The.Document.History.Add(new Commands.ChangeContainer(container));
				The.Document.History.Commit(Text);
			}
		}
	}
}
