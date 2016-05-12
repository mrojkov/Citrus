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
				// HandleAppTermination(input);
				yield return null;
			}
		}

		void HandleGlobalShortcuts(WidgetInput input)
		{
			if (input.IsKeyPressed(Key.LShift) && input.IsKeyPressed(Key.LWin) && input.IsKeyPressed(Key.Z)) {
				Document.Current.History.Redo();
			} else if (input.IsKeyPressed(Key.LWin) && input.IsKeyPressed(Key.Z)) {
				Document.Current.History.Undo();
			}
		}

		//void HandleAppTermination(WidgetInput input)
		//{
		//	if (input.IsKeyPressed(Key.WinLeft) && input.WasKeyPressed(Key.W)) {
		//		Application.Exit();
		//	}
		//}

		void HandleEnterExit(WidgetInput input)
		{
			var doc = Document.Current;
			if (input.WasKeyPressed(Key.Enter)) {
				var node = timeline.SelectedRows.Select(i => i.Components.Get<Components.NodeRow>()).FirstOrDefault(i => i != null).Node;
				if (node != null) {
					doc.History.Execute(new Commands.SetCurrentContainer(node));
				}
			} else if (input.WasKeyPressed(Key.BackSpace)) {
				if (timeline.Container.Parent != null) {
					doc.History.Execute(new Commands.SetCurrentContainer(timeline.Container.Parent));
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
			if (timeline.Rows.Count() == 0) {
				return;
			}
			var lastSelectedRow = timeline.SelectedRows.Count > 0 ? timeline.SelectedRows[0] : timeline.Rows[0];
			var nextRow = timeline.Rows[Mathf.Clamp(lastSelectedRow.Index + advance, 0, timeline.Rows.Count - 1)];
			if (nextRow != lastSelectedRow) {
				if (!multiselection) {
					doc.History.Add(new Commands.ClearRowSelection());
				}
				if (timeline.SelectedRows.Contains(nextRow)) {
					doc.History.Add(new Commands.SelectRow(lastSelectedRow, false));
				}
				doc.History.Add(new Commands.SelectRow(nextRow));
				doc.History.Commit();
				timeline.EnsureRowVisible(nextRow);
			}
		}

		void HorizontalScroll(WidgetInput input)
		{
			var stride = input.IsKeyPressed(Key.AltLeft) ? 10 : 1;
			if (input.WasKeyRepeated(Key.Right)) {
				Document.Current.History.Execute(new Commands.SetCurrentColumn(timeline.CurrentColumn + stride));
				timeline.EnsureColumnVisible(timeline.CurrentColumn);
			} else if (input.WasKeyRepeated(Key.Left)) {
				Document.Current.History.Execute(new Commands.SetCurrentColumn(timeline.CurrentColumn - stride));
				timeline.EnsureColumnVisible(timeline.CurrentColumn);
			}
		}		
	}	
}