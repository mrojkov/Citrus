using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.Timeline
{
	public class ChoosePrevNode : XwtPlus.Command
	{
		public ChoosePrevNode()
			: base("Choose previous node", "Up")
		{
		}

		public override void OnTriggered()
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

	public class ChooseNextNode : XwtPlus.Command
	{
		public ChooseNextNode()
			: base("Choose next node", "Down")
		{
		}

		public override void OnTriggered()
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

	public class EnterContainer : XwtPlus.Command
	{
		public EnterContainer()
			: base("Enter container", "Return")
		{
		}

		public override void OnTriggered()
		{
			var lines = The.Document.SelectedRows;
			if (lines.Count > 0) {
				var container = The.Document.Container.Nodes[lines[0]];
				The.Document.History.Add(new Commands.ChangeContainer(container));
				The.Document.History.Commit(Text);
			}
		}
	}

	public class ExitContainer : XwtPlus.Command
	{
		public ExitContainer()
			: base("Exit container", "Backspace")
		{
		}

		public override void OnTriggered()
		{
			if (The.Document.Container.Parent != null) {
				var container = The.Document.Container.Parent;
				The.Document.History.Add(new Commands.ChangeContainer(container));
				The.Document.History.Commit(Text);
			}
		}
	}

	public class MoveCursorRight : XwtPlus.Command
	{
		public MoveCursorRight()
			: base("Move cursor right", "Right")
		{
		}

		public override void OnTriggered()
		{
			The.Document.CurrentColumn++;
			The.Timeline.EnsureColumnVisible(The.Document.CurrentColumn);
			The.Document.UpdateViews();
		}
	}

	public class MoveCursorLeft : XwtPlus.Command
	{
		public MoveCursorLeft()
			: base("Move cursor left", "Left")
		{
		}

		public override void OnTriggered()
		{
			if (The.Document.CurrentColumn > 0) {
				The.Document.CurrentColumn--;
				The.Timeline.EnsureColumnVisible(The.Document.CurrentColumn);
			}
			The.Document.UpdateViews();
		}
	}

	public class MoveCursorRightFast : XwtPlus.Command
	{
		public MoveCursorRightFast()
			: base("Move cursor right fast", "Ctrl+Right")
		{
		}

		public override void OnTriggered()
		{
			The.Document.CurrentColumn += 10;
			The.Timeline.EnsureColumnVisible(The.Document.CurrentColumn);
			The.Document.UpdateViews();
		}
	}

	public class MoveCursorLeftFast : XwtPlus.Command
	{
		public MoveCursorLeftFast()
			: base("Move cursor left fast", "Ctrl+Left")
		{
		}

		public override void OnTriggered()
		{
			if (The.Document.CurrentColumn > 0) {
				The.Document.CurrentColumn = Math.Max(0, The.Document.CurrentColumn - 10);
				The.Timeline.EnsureColumnVisible(The.Document.CurrentColumn);
			}
			The.Document.UpdateViews();
		}
	}
}
