using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public class ChoosePrevNode : ContextualAction
	{
		public ChoosePrevNode(QWidget context)
			: base(context, "Choose previous node")
		{
			Shortcut = "Up";
		}

		protected override void OnTriggered()
		{
			var lines = The.Document.SelectedRows;
			int line = 0;
			if (lines.Count > 0) {
				line = Math.Max(0, lines[0] - 1);
			}
			The.Document.History.Add(new Commands.SelectRows(line));
			The.Document.History.Commit(Text);
		}
	}

	public class ChooseNextNode : ContextualAction
	{
		public ChooseNextNode(QWidget context)
			: base(context, "Choose next node")
		{
			Shortcut = "Down";
		}

		protected override void OnTriggered()
		{
			var lines = The.Document.SelectedRows;
			int line = 0;
			if (lines.Count > 0 && The.Document.Rows.Count > 0) {
				line = lines[lines.Count - 1] + 1;
				line = Math.Min(The.Document.Rows.Count - 1, line);
			}
			The.Document.History.Add(new Commands.SelectRows(line));
			The.Document.History.Commit(Text);
		}
	}

	public class EnterContainer : ContextualAction
	{
		public EnterContainer(QWidget context)
			: base(context, "Enter container")
		{
			Shortcut = "Return";
		}

		protected override void OnTriggered()
		{
			var lines = The.Document.SelectedRows;
			if (lines.Count > 0) {
				var container = The.Document.Container.Nodes[lines[0]];
				The.Document.History.Add(new Commands.ChangeContainer(container));
				The.Document.History.Commit(Text);
			}
		}
	}

	public class ExitContainer : ContextualAction
	{
		public ExitContainer(QWidget context)
			: base(context, "Exit container")
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

	public class MoveCursorRight : ContextualAction
	{
		public MoveCursorRight(QWidget context)
			: base(context, "Move cursor right")
		{
			Shortcut = "Right";
		}

		protected override void OnTriggered()
		{
			The.Document.CurrentColumn++;
			The.Timeline.EnsureColumnVisible(The.Document.CurrentColumn);
			The.Document.OnChanged();
		}
	}

	public class MoveCursorLeft : ContextualAction
	{
		public MoveCursorLeft(QWidget context)
			: base(context, "Move cursor left")
		{
			Shortcut = "Left";
		}

		protected override void OnTriggered()
		{
			if (The.Document.CurrentColumn > 0) {
				The.Document.CurrentColumn--;
				The.Timeline.EnsureColumnVisible(The.Document.CurrentColumn);
			}
			The.Document.OnChanged();
		}
	}

	public class MoveCursorRightFast : ContextualAction
	{
		public MoveCursorRightFast(QWidget context)
			: base(context, "Move cursor right fast")
		{
			Shortcut = "Ctrl+Right";
		}

		protected override void OnTriggered()
		{
			The.Document.CurrentColumn += 10;
			The.Timeline.EnsureColumnVisible(The.Document.CurrentColumn);
			The.Document.OnChanged();
		}
	}

	public class MoveCursorLeftFast : ContextualAction
	{
		public MoveCursorLeftFast(QWidget context)
			: base(context, "Move cursor left fast")
		{
			Shortcut = "Ctrl+Left";
		}

		protected override void OnTriggered()
		{
			if (The.Document.CurrentColumn > 0) {
				The.Document.CurrentColumn = Math.Max(0, The.Document.CurrentColumn - 10);
				The.Timeline.EnsureColumnVisible(The.Document.CurrentColumn);
			}
			The.Document.OnChanged();
		}
	}
}
