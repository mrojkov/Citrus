using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using System.Collections.Generic;

namespace Tangerine.UI.Timeline
{
	public class KeyboardShortcutsTask
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Main()
		{
			while (true) {
				var input = timeline.RootWidget.Input;
				HandleGlobalShortcuts(input);
				HorizontalScroll(input);
				VerticalScroll(input);
				HandleEnterExit(input);
				yield return null;
			}
		}

		void HandleGlobalShortcuts(WidgetInput input)
		{
			input.EnableKey(Key.Undo, Document.Current.History.UndoEnabled);
			input.EnableKey(Key.Redo, Document.Current.History.RedoEnabled);
			if (input.WasKeyRepeated(Key.Redo)) {
				Document.Current.History.Redo();
			} else if (input.WasKeyRepeated(Key.Undo)) {
				Document.Current.History.Undo();
			}
		}

		void HandleEnterExit(WidgetInput input)
		{
			var doc = Document.Current;
			if (input.WasKeyPressed(Key.Enter)) {
				var node = timeline.SelectedRows.Select(i => i.Components.Get<Components.NodeRow>()).FirstOrDefault(i => i != null).Node;
				if (node != null) {
					doc.History.Execute(new Operations.SetCurrentContainer(node));
				}
			} else if (input.WasKeyPressed(Key.BackSpace)) {
				if (timeline.Container.Parent != null) {
					doc.History.Execute(new Operations.SetCurrentContainer(timeline.Container.Parent));
				}
			}
		}
		
		void VerticalScroll(WidgetInput input)
		{
			if (input.WasKeyRepeated(Key.Down)) {
				SelectRow(1, input.IsKeyPressed(Key.ShiftLeft));
			}
			if (input.WasKeyRepeated(Key.Up)) {
				SelectRow(-1, input.IsKeyPressed(Key.ShiftLeft));
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
					doc.History.Add(new Operations.ClearRowSelection());
				}
				if (timeline.SelectedRows.Contains(nextRow)) {
					doc.History.Add(new Operations.SelectRow(lastSelectedRow, false));
				}
				doc.History.Add(new Operations.SelectRow(nextRow));
				doc.History.Commit();
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
				Document.Current.History.Execute(new Operations.SetCurrentColumn(timeline.CurrentColumn + stride));
				timeline.EnsureColumnVisible(timeline.CurrentColumn);
			}
		}		
	}	
}