using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;
using Tangerine.Core.Components;

namespace Tangerine.UI.Timeline
{
	public class SelectAndDragRowsProcessor : ITaskProvider
	{
		public SelectAndDragRowsProcessor()
		{
			Probers.Add(new BoneRowProber());
			Probers.Add(new FolderRowProber());
			Probers.Add(new NodeRowProber());
		}

		public IEnumerator<object> Task()
		{
			var roll = Timeline.Instance.Roll;
			var input = roll.RootWidget.Input;
			input.AcceptMouseThroughDescendants = true;
			while (true) {
				yield return null;
				if (!input.WasMousePressed() || input.WasKeyPressed(Key.Mouse0DoubleClick)) {
					continue;
				}
				var initialMousePosition = input.MousePosition;
				var row = RowUnderMouse(initialMousePosition);
				if (row == null) {
					continue;
				}
				if (input.IsKeyPressed(Key.Shift)) {
					if (Document.Current.SelectedRows().Any()) {
						var firstRow = Document.Current.SelectedRows().FirstOrDefault();
						ClearRowSelection.Perform();
						SelectRowRange.Perform(firstRow, row);
					} else {
						ClearRowSelection.Perform();
						SelectRow.Perform(row);
					}
				} else if (input.IsKeyPressed(Key.Control)) {
					SelectRow.Perform(row, !row.Selected);
				} else {
					if (!row.Selected) {
						ClearRowSelection.Perform();
						SelectRow.Perform(row);
					}
					while (
						input.IsMousePressed() &&
						Math.Abs(initialMousePosition.Y - input.MousePosition.Y) < TimelineMetrics.DefaultRowHeight / 4
					) {
						yield return null;
					}
					if (input.IsMousePressed()) {
						yield return DragTask();
					} else {
						ClearRowSelection.Perform();
						SelectRow.Perform(row);
					}
				}
			}
		}

		static IEnumerator<object> DragTask()
		{
			var roll = Timeline.Instance.Roll;
			RowLocation? dragLocation = new RowLocation(Document.Current.RowTree, 0);
			Action<Widget> a = _ => {
				if (dragLocation != null) {
					RenderDragCursor(dragLocation.Value);
				}
			};
			roll.OnRenderOverlay += a;
			var input = roll.RootWidget.Input;
			while (input.IsMousePressed()) {
				dragLocation = MouseToRowLocation(input.MousePosition);
				CommonWindow.Current.Invalidate();
				yield return null;
			}
			roll.OnRenderOverlay -= a;
			CommonWindow.Current.Invalidate();
			if (dragLocation != null) {
				DragRows(dragLocation.Value);
			}
		}

		static void DragRows(RowLocation dragLocation)
		{
			List<Row> parentRowRows = dragLocation.ParentRow.Rows;

			IEnumerable<KeyValuePair<Row, int>> enumeratedSelectedRows = Document.Current.TopLevelSelectedRows()
				.Select(row => new KeyValuePair<Row, int>(row, parentRowRows.IndexOf(row)));
			
			var rows = enumeratedSelectedRows.ToList();
			foreach (var elem in rows) {
				Probers.Any(p => p.Probe(elem.Key, dragLocation));
				if (elem.Value >= dragLocation.Index) dragLocation.Index++;
			}
		}

		static void RenderDragCursor(RowLocation rowLocation)
		{
			float y = 1;
			var pr = rowLocation.ParentRow;
			if (rowLocation.Index < pr.Rows.Count) {
				y = pr.Rows[rowLocation.Index].GridWidget().Top();
			} else if (pr.Rows.Count > 0) {
				var lastRow = pr.Rows[rowLocation.Index - 1];
				y = lastRow.GridWidget().Bottom() + CalcSubtreeHeight(lastRow) + TimelineMetrics.RowSpacing;
			} else if (pr != Document.Current.RowTree) {
				y = pr.GridWidget().Bottom() + TimelineMetrics.RowSpacing;
			}
			Timeline.Instance.Roll.ContentWidget.PrepareRendererState();
			Renderer.DrawRect(
				new Vector2(TimelineMetrics.RollIndentation * CalcIndentation(pr), y - 1),
				new Vector2(Timeline.Instance.Roll.ContentWidget.Width, y + 1), ColorTheme.Current.TimelineRoll.DragCursor);

			for (var p = pr; p != null; p = p.Parent) {
				var parentWidget = p.GridWidget();
				if (parentWidget == null) {
					continue;
				}

				Renderer.DrawRect(
					parentWidget.Left(),
					parentWidget.Top(),
					parentWidget.Right(),
					parentWidget.Bottom(),
					ColorTheme.Current.TimelineRoll.DragTarget
				);
			}
		}

		static int CalcIndentation(Row row)
		{
			int i = 0;
			for (var r = row.Parent; r != null; r = r.Parent) {
				i++;
			}
			return i;
		}

		static float CalcSubtreeHeight(Row row)
		{
			float r = 0;
			foreach (var i in row.Rows) {
				r += i.GridWidget().Height + TimelineMetrics.RowSpacing + CalcSubtreeHeight(i);
			}
			return r;
		}

		static Row RowUnderMouse(Vector2 position)
		{
			var doc = Document.Current;
			if (doc.Rows.Count == 0) {
				return null;
			}
			position -= Timeline.Instance.Roll.ContentWidget.GlobalPosition;
			if (position.Y < 0) {
				return doc.Rows[0];
			}
			foreach (var row in doc.Rows) {
				var gw = row.GridWidget();
				if (position.Y >= gw.Top() && position.Y < gw.Bottom() + TimelineMetrics.RowSpacing) {
					return doc.Rows[row.Index];
				}
			}
			return doc.Rows[doc.Rows.Count - 1];
		}

		static RowLocation? MouseToRowLocation(Vector2 position)
		{
			position -= Timeline.Instance.Roll.ContentWidget.GlobalPosition;
			if (position.Y <= 0) {
				return new RowLocation(Document.Current.RowTree, 0);
			}
			for (int i = 0; i < Document.Current.Rows.Count; i++) {
				var row = Document.Current.Rows[i];
				var gw = row.GridWidget();
				if (position.Y >= gw.Top() && position.Y < gw.Bottom() + TimelineMetrics.RowSpacing) {
					int index = row.Parent.Rows.IndexOf(row);

					var rowNext = i < Document.Current.Rows.Count ? Document.Current.Rows[i + 1] : null;
					int rowNextIndex = rowNext?.Parent.Rows.IndexOf(rowNext) ?? -1;

					bool allowedDropPrev = row.Parent.CanHaveChildren;
					bool allowedDropNext = row.Parent.CanHaveChildren && (row.Parent == rowNext || rowNext == null);
					bool allowedDropPrevForNext = !allowedDropNext && rowNext != null && rowNext.Parent.CanHaveChildren;
					bool allowedDropIn = row.CanHaveChildren;

					if (allowedDropPrev && position.Y < gw.Y + gw.Height * (
						(allowedDropNext || allowedDropPrevForNext) && allowedDropIn
							? 0.25f
							: allowedDropNext || allowedDropPrevForNext || allowedDropIn
								? 0.5f
								: 1f
					)) {
						return new RowLocation(row.Parent, index);
					} else if (allowedDropIn && position.Y < gw.Y + gw.Height * (allowedDropNext || allowedDropPrevForNext ? 0.75f : 1f)) {
						return new RowLocation(row, 0);
					} else if (allowedDropNext) {
						return new RowLocation(row.Parent, index + 1);
					} else if (allowedDropPrevForNext) {
						return new RowLocation(rowNext.Parent, rowNextIndex);
					} else {
						return null;
					}
				}
			}
			return new RowLocation(Document.Current.RowTree, Document.Current.RowTree.Rows.Count);
		}

		public static readonly List<IRowProber> Probers = new List<IRowProber>();

		public interface IRowProber
		{
			bool Probe(Row node, RowLocation location);
		}

		public abstract class Prober<T> : IRowProber where T : Component
		{
			public bool Probe(Row row, RowLocation location) => row.Components.Contains<T>() && ProbeInternal(row.Components.Get<T>(), row, location);

			protected abstract bool ProbeInternal(T component, Row row, RowLocation location);

			protected void MoveFolderItemTo(IFolderItem item, RowLocation newLocation)
			{
				FolderItemLocation targetLoc;
				var folder = newLocation.ParentRow.Components.Get<FolderRow>().Folder;
				if (newLocation.ParentRow.Rows.Count <= newLocation.Index) {
					targetLoc = new FolderItemLocation(folder, folder.Items.Count);
				} else {
					targetLoc = Row.GetFolderItemLocation(newLocation.ParentRow.Rows[newLocation.Index]);
				}
				MoveNodes.Perform(item, targetLoc);
			}
		}

		public class NodeRowProber : Prober<NodeRow>
		{
			protected override bool ProbeInternal(NodeRow component, Row row, RowLocation location)
			{
				if (!location.ParentRow.Components.Contains<FolderRow>() || row == location.ParentRow || row.Rows.Contains(location.ParentRow))
					return false;
				try {
					MoveFolderItemTo(component.Node, location);
				} catch (InvalidOperationException e) {
					AlertDialog.Show(e.Message);
					return false;
				}
				return true;
			}
		}

		public class FolderRowProber : Prober<FolderRow>
		{
			protected override bool ProbeInternal(FolderRow component, Row row, RowLocation location)
			{
				if (!location.ParentRow.Components.Contains<FolderRow>() || row == location.ParentRow || row.Rows.Contains(location.ParentRow))
					return false;
				try {
					MoveFolderItemTo(component.Folder, location);
				} catch (InvalidOperationException e) {
					AlertDialog.Show(e.Message);
					return false;
				}
				return true;
			}
		}

		public class BoneRowProber : Prober<BoneRow>
		{
			protected override bool ProbeInternal(BoneRow node, Row row, RowLocation location)
			{
				if (!(location.ParentRow.Components.Contains<BoneRow>() || location.ParentRow.Components.Contains<FolderRow>())) {
					return false;
				}
				var targetParent = location.ParentRow.Components.Get<BoneRow>()?.Bone;
				try {
					var bone = row.Components.Get<BoneRow>().Bone;
					// Check if bone target parent is bone descendant
					if (IsDescendant(bone, targetParent)) return false;
					if (bone.BaseIndex == 0 && !location.ParentRow.Components.Contains<BoneRow>()) {
						MoveFolderItemTo(bone, location);
					} else {
						SetProperty.Perform(bone, nameof(Bone.Position), Vector2.Zero);
					}

					SetProperty.Perform(bone, nameof(Bone.BaseIndex), targetParent?.Index ?? 0);
					SortBonesInChain.Perform(bone);
					var nodes = Document.Current.Container.Nodes;
					var parentBone = nodes.GetBone(bone.BaseIndex);
					while (parentBone != null && !parentBone.EditorState().ChildrenExpanded) {
						SetProperty.Perform(parentBone.EditorState(), nameof(NodeEditorState.ChildrenExpanded), true);
						bone = nodes.GetBone(bone.BaseIndex);
					}
				} catch (InvalidOperationException e) {
					AlertDialog.Show(e.Message);
					return false;
				}
				return true;
			}

			private bool IsDescendant(Bone bone, Bone targetParent)
			{
				while (targetParent != null) {
					if (targetParent == bone) {
						return true;
					} else {
						targetParent = Document.Current.Container.Nodes.GetBone(targetParent.BaseIndex);
					}
				}
				return false;
			}
		}
	}
}
