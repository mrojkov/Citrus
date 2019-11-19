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
			ConnectCommand(TimelineCommands.ExpandRecursively, ExpandRecursively, Document.HasCurrent);
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
			ConnectCommand(TimelineCommands.DeleteMarkersInRange, Rulerbar.DeleteMarkersInRange);
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
			InternalExpand(recursive: false);
		}

		private static void ExpandRecursively()
		{
			InternalExpand(recursive: true);
		}

		private static void InternalExpand(bool recursive = false)
		{
			void ExpandOrCollapseRow(Row row, object component, string property, bool expanded, int level)
			{
				if (!recursive) {
					SetProperty.Perform(component, property, expanded, isChangingDocument: false);
				} else if (expanded) {
					SetProperty.Perform(component, property, expanded, isChangingDocument: false);
					foreach (var child in row.Rows.ToList()) {
						SetExpanded(child, expanded, level);
					}
				} else {
					foreach (var child in row.Rows.ToList()) {
						SetExpanded(child, expanded, level);
					}
					SetProperty.Perform(component, property, expanded, isChangingDocument: false);
				}
			}

			var processedRows = new HashSet<Row>();
			void SetExpanded(Row row, bool expanded = false, int level = 0)
			{
				if (processedRows.Contains(row)) {
					return;
				}

				processedRows.Add(row);
				foreach (var component in row.Components) {
					switch (component) {
						case NodeRow nodeRow:
							ExpandOrCollapseRow(row, nodeRow, nameof(NodeRow.Expanded), expanded, level + 1);
							if (nodeRow.Expanded && row.Rows.Count > 0) {
								Timeline.Instance.EnsureRowChildsVisible(row);
							}
							break;

						case BoneRow boneRow:
							if (boneRow.HaveChildren) {
								ExpandOrCollapseRow(row, boneRow, nameof(BoneRow.ChildrenExpanded), expanded, level + 1);
								if (boneRow.ChildrenExpanded) {
									Timeline.Instance.EnsureRowChildsVisible(row);
								}
							} else if (row.Parent.Parent != null) {
								SetExpanded(row.Parent, expanded, level);
								if (level == 0) {
									Core.Operations.SelectRow.Perform(row, select: false);
									Core.Operations.SelectRow.Perform(row.Parent, select: true);
								}
							}
							return;

						case FolderRow folderRow:
							var folder = folderRow.Folder;
							if (folder.Items.Count > 0) {
								ExpandOrCollapseRow(row, folder, nameof(Folder.Expanded), expanded, level + 1);
								if (folder.Expanded) {
									Timeline.Instance.EnsureRowChildsVisible(row);
								}
							}
							return;

						case PropertyRow propertyRow:
							SetExpanded(row.Parent, expanded, level);
							if (level == 0) {
								Core.Operations.SelectRow.Perform(row, select: false);
								Core.Operations.SelectRow.Perform(row.Parent, select: true);
							}
							return;
					}
				}
			}

			var topMostRows = new HashSet<Row>(Document.Current.SelectedRows());
			foreach (var row in Document.Current.SelectedRows()) {
				for (var p = row.Parent; p != null; p = p.Parent) {
					if (topMostRows.Contains(p)) {
						topMostRows.Remove(row);
						break;
					}
				}
			}

			ClearRowSelection.Perform();
			foreach (var row in topMostRows) {
				Core.Operations.SelectRow.Perform(row);
				SetExpanded(row, expanded: !IsRowExpanded(row));
			}
		}

		private static bool IsRowExpanded(Row row)
		{
			foreach (var component in row.Components) {
				switch (component) {
					case NodeRow nodeRow:
						return nodeRow.Expanded;

					case BoneRow boneRow:
						return boneRow.ChildrenExpanded;

					case FolderRow folderRow:
						return folderRow.Folder.Expanded;

					case PropertyRow propertyRow:
						return true;
				}
			}
			return false;
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
					if (node == null || node.EditorState().Locked) {
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
