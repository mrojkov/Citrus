using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.Core.Operations;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline
{
	public static class CommandBindings
	{
		public static void Bind()
		{
			Action enter = () => {
				var node = Document.Current.SelectedNodes().FirstOrDefault();
				if (node != null) {
					EnterNode.Perform(node);
				}
			};
			ConnectCommand(TimelineCommands.EnterNode, enter, Document.HasCurrent);
			ConnectCommand(TimelineCommands.EnterNodeAlias, enter, Document.HasCurrent);
			ConnectCommand(TimelineCommands.EnterNodeMouse, enter, Document.HasCurrent);
			ConnectCommand(TimelineCommands.Expand, Expand, Document.HasCurrent);
			ConnectCommand(TimelineCommands.RenameRow, RenameCurrentRow);
			ConnectCommand(TimelineCommands.ExitNode, LeaveNode.Perform);
			ConnectCommand(TimelineCommands.ExitNodeAlias, LeaveNode.Perform);
			ConnectCommand(TimelineCommands.ExitNodeMouse, LeaveNode.Perform);
			ConnectCommand(TimelineCommands.ScrollUp, () => SelectRow(-1, false));
			ConnectCommand(TimelineCommands.ScrollDown, () => SelectRow(1, false));
			ConnectCommand(TimelineCommands.SelectNodeUp, () => SelectRow(-1, true));
			ConnectCommand(TimelineCommands.SelectNodeDown, () => SelectRow(1, true));
			ConnectCommand(TimelineCommands.ScrollLeft, () => AdvanceCurrentColumn(-1));
			ConnectCommand(TimelineCommands.ScrollRight, () => AdvanceCurrentColumn(1));
			ConnectCommand(TimelineCommands.FastScrollLeft, () => AdvanceCurrentColumn(-10));
			ConnectCommand(TimelineCommands.FastScrollRight, () => AdvanceCurrentColumn(10));
			ConnectCommand(TimelineCommands.DeleteKeyframes, RemoveKeyframes);
			ConnectCommand(TimelineCommands.CreateMarkerPlay, () => CreateMarker(MarkerAction.Play));
			ConnectCommand(TimelineCommands.CreateMarkerStop, () => CreateMarker(MarkerAction.Stop));
			ConnectCommand(TimelineCommands.CreateMarkerJump, () => CreateMarker(MarkerAction.Jump));
			ConnectCommand(TimelineCommands.DeleteMarker, DeleteMarker);
			ConnectCommand(TimelineCommands.CopyMarkers, Rulerbar.CopyMarkers);
			ConnectCommand(TimelineCommands.PasteMarkers, Rulerbar.PasteMarkers);
			ConnectCommand(TimelineCommands.DeleteMarkers, Rulerbar.DeleteMarkers);
			ConnectCommand(TimelineCommands.MoveDown, MoveNodesDown.Perform);
			ConnectCommand(TimelineCommands.MoveUp, MoveNodesUp.Perform);
			ConnectCommand(TimelineCommands.SelectAllRowKeyframes, SelectAllRowKeyframes);
			ConnectCommand(TimelineCommands.SelectAllKeyframes, SelectAllKeyframes);
		}

		private static void SelectAllKeyframes()
		{
			SelectKeyframes(Document.Current.Rows);
		}

		private static void SelectAllRowKeyframes()
		{
			SelectKeyframes(Document.Current.SelectedRows());
		}

		private static void SelectKeyframes(IEnumerable<Row> rows)
		{
			Operations.ClearGridSelection.Perform();
			foreach (var row in rows) {
				if (row.Components.Get<NodeRow>() is NodeRow nodeRow) {
					foreach (var animator in nodeRow.Node.Animators) {
						foreach (var key in animator.ReadonlyKeys) {
							Operations.SelectGridSpan.Perform(row.Index, key.Frame, key.Frame + 1);
						}
					}
				}
			}
		}

		private static void ConnectCommand(ICommand command, Action action, Func<bool> enableChecker = null)
		{
			CommandHandlerList.Global.Connect(command, new DocumentDelegateCommandHandler(action, enableChecker));
		}

		private static void Expand()
		{
			foreach (var row in Document.Current.SelectedRows().ToList()) {
				if (row.Components.Get<NodeRow>() is NodeRow nodeRow) {
					SetProperty.Perform(nodeRow, nameof(NodeRow.Expanded), !nodeRow.Expanded, isChangingDocument: false);
					if (nodeRow.Expanded && row.Rows.Count > 0) {
						Timeline.Instance.EnsureRowChildsVisible(row);
					}
				}
			}
		}

		private static void RenameCurrentRow()
		{
			var doc = Document.Current;
			if (doc.SelectedRows().Count() != 1) {
				return;
			}
			var row = doc.SelectedRows().First();
			row.Components.Get<RowView>().RollRow.Rename();
		}

		private static void SelectRow(int advance, bool multiselection)
		{
			var doc = Document.Current;
			if (doc.Rows.Count == 0) {
				return;
			}
			if (!doc.SelectedRows().Any()) {
				Core.Operations.SelectRow.Perform(doc.Rows[0]);
				return;
			}
			var lastSelectedRow = doc.SelectedRows().OrderByDescending(i => i.SelectCounter).First();
			var nextRow = doc.Rows[Mathf.Clamp(lastSelectedRow.Index + advance, 0, doc.Rows.Count - 1)];
			if (nextRow != lastSelectedRow) {
				if (!multiselection) {
					Core.Operations.ClearRowSelection.Perform();
				}
				if (nextRow.Selected) {
					Core.Operations.SelectRow.Perform(lastSelectedRow, false);
				}
				Core.Operations.SelectRow.Perform(nextRow);
			}
		}

		private static void RemoveKeyframes()
		{
			foreach (var row in Document.Current.Rows.ToList()) {
				var spans = row.Components.GetOrAdd<GridSpanListComponent>().Spans;
				foreach (var span in spans.GetNonOverlappedSpans()) {
					var node = row.Components.Get<NodeRow>()?.Node ?? row.Components.Get<PropertyRow>()?.Node;
					if (node == null) {
						continue;
					}
					var property = row.Components.Get<PropertyRow>()?.Animator.TargetPropertyPath;
					foreach (var a in node.Animators.ToList()) {
						if (a.AnimationId != Document.Current.AnimationId) {
							continue;
						}
						if (property != null && a.TargetPropertyPath != property) {
							continue;
						}
						foreach (var k in a.Keys.Where(k => k.Frame >= span.A && k.Frame < span.B).ToList()) {
							Core.Operations.RemoveKeyframe.Perform(a, k.Frame);
						}
					}
				}
			}
		}

		private static void AdvanceCurrentColumn(int stride)
		{
			Operations.SetCurrentColumn.Perform(Math.Max(0, Timeline.Instance.CurrentColumn + stride));
		}

		private static void CreateMarker(MarkerAction action)
		{
			var timeline = Timeline.Instance;
			var nearestMarker = Document.Current.Animation.Markers.LastOrDefault(
				m => m.Frame < timeline.CurrentColumn && m.Action == MarkerAction.Play);
			string markerId = (action == MarkerAction.Play) ? GenerateMarkerId(Document.Current.Animation.Markers, "Start") : "";
			var newMarker = new Marker(
				markerId,
				timeline.CurrentColumn,
				action,
				action == MarkerAction.Jump && nearestMarker != null ? nearestMarker.Id : ""
			);
			SetMarker.Perform(newMarker, true);
		}

		private static string GenerateMarkerId(MarkerList markers, string markerId)
		{
			int c = 1;
			string id = markerId;
			while (markers.Any(i => i.Id == id)) {
				id = markerId + c;
				c++;
			}
			return id;
		}

		private static void DeleteMarker()
		{
			var timeline = Timeline.Instance;
			var marker = Document.Current.Animation.Markers.GetByFrame(timeline.CurrentColumn);
			if (marker != null) {
				Core.Operations.DeleteMarker.Perform(marker, true);
			}
		}
	}
}
