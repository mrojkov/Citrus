using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using System.Collections.Generic;

namespace Tangerine.UI.Timeline
{
	public class KeyboardShortcutsProcessor : Core.IProcessor
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Loop()
		{
			while (true) {
				var input = timeline.PanelWidget.Input;
				HandleShortcuts(input);
				HorizontalScroll(input);
				VerticalScroll(input);
				HandleEnterExit(input);
				yield return Task.WaitForInput();
			}
		}

		void HandleShortcuts(WidgetInput input)
		{
			var hasSelection = Document.Current.SelectedNodes.Count() > 0;
			input.EnableKey(Key.Commands.Undo, Document.Current.History.UndoEnabled);
			input.EnableKey(Key.Commands.Redo, Document.Current.History.RedoEnabled);
			input.EnableKey(Key.Commands.SelectAll, hasSelection);
			input.EnableKey(Key.Commands.Cut, hasSelection);
			input.EnableKey(Key.Commands.Copy, hasSelection);
			input.EnableKey(Key.Commands.Paste, Timeline.Clipboard.Nodes.Count > 0);
			input.EnableKey(Key.Commands.Delete, hasSelection);
			if (input.WasKeyRepeated(Key.Commands.Redo)) {
				Document.Current.History.Redo();
			} else if (input.WasKeyRepeated(Key.Commands.Undo)) {
				Document.Current.History.Undo();
			} else if (input.WasKeyRepeated(Key.Commands.SelectAll)) {
				foreach (var row in timeline.Rows) {
					Operations.SelectRow.Perform(row, true);
				}
			} else if (input.WasKeyRepeated(Key.Commands.Cut)) {
				Operations.Cut.Perform();
			} else if (input.WasKeyRepeated(Key.Commands.Copy)) {
				Operations.Copy.Perform();
			} else if (input.WasKeyRepeated(Key.Commands.Paste)) {
				Operations.Paste.Perform();
			} else if (input.WasKeyRepeated(Key.Commands.Delete)) {
				Operations.Delete.Perform();
			}
		}

		void HandleEnterExit(WidgetInput input)
		{
			var doc = Document.Current;
			if (input.WasKeyPressed(KeyBindings.Timeline.EnterNode)) {
				var node = timeline.SelectedRows.Select(i => i.Components.Get<Components.NodeRow>()).FirstOrDefault(i => i != null)?.Node;
				if (node != null) {
					Operations.SetCurrentContainer.Perform(node);
				}
			} else if (input.WasKeyPressed(KeyBindings.Timeline.ExitNode)) {
				if (timeline.Container.Parent != null) {
					Operations.SetCurrentContainer.Perform(timeline.Container.Parent);
				}
			}
		}
		
		void VerticalScroll(WidgetInput input)
		{
			if (input.WasKeyRepeated(KeyBindings.Timeline.ScrollUp)) {
				SelectRow(-1, false);
			} else if (input.WasKeyRepeated(KeyBindings.Timeline.ScrollDown)) {
				SelectRow(1, false);
			} else if (input.WasKeyRepeated(KeyBindings.Timeline.SelectUp)) {
				SelectRow(-1, true);
			} else if (input.WasKeyRepeated(KeyBindings.Timeline.SelectDown)) {
				SelectRow(1, true);
			}
		}

		void SelectRow(int advance, bool multiselection)
		{
			var doc = Document.Current;
			if (timeline.Rows.Count == 0) {
				return;
			}
			var lastSelectedRow = timeline.SelectedRows.Count > 0 ? timeline.SelectedRows[0] : timeline.Rows[0];
			var nextRow = timeline.Rows[Mathf.Clamp(lastSelectedRow.Index + advance, 0, timeline.Rows.Count - 1)];
			if (nextRow != lastSelectedRow) {
				if (!multiselection) {
					Operations.ClearRowSelection.Perform();
				}
				if (timeline.SelectedRows.Contains(nextRow)) {
					Operations.SelectRow.Perform(lastSelectedRow, false);
				}
				Operations.SelectRow.Perform(nextRow);
				timeline.EnsureRowVisible(nextRow);
			}
		}

		void HorizontalScroll(WidgetInput input)
		{
			int stride = 0;
			if (input.WasKeyRepeated(KeyBindings.Timeline.FastScrollLeft)) {
				stride = -10;
			} else if (input.WasKeyRepeated(KeyBindings.Timeline.FastScrollRight)) {
				stride = 10;
			} else if (input.WasKeyRepeated(KeyBindings.Timeline.ScrollRight)) {
				stride = 1;
			} else if (input.WasKeyRepeated(KeyBindings.Timeline.ScrollLeft)) {
				stride = -1;
			}
			if (stride != 0) {
				Operations.SetCurrentColumn.Perform(Math.Max(0, timeline.CurrentColumn + stride));
			}
		}		
	}	
}