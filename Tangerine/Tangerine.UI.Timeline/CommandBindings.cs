using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline
{
	public static class CommandBindings
	{
		public static void Bind()
		{
			var h = CommandHandlerList.Global;
			h.Connect(TimelineCommands.EnterNode, () => {
				var node = Document.Current.SelectedNodes().FirstOrDefault();
				if (node != null) {
					Core.Operations.EnterNode.Perform(node);
				}
			}, Document.HasCurrent);
			h.Connect(TimelineCommands.RenameRow, () => RenameCurrentRow(), Document.HasCurrent);
			h.Connect(TimelineCommands.ExitNode, Core.Operations.LeaveNode.Perform, Document.HasCurrent);
			h.Connect(TimelineCommands.ScrollUp, () => SelectRow(-1, false), Document.HasCurrent);
			h.Connect(TimelineCommands.ScrollDown, () => SelectRow(1, false), Document.HasCurrent);
			h.Connect(TimelineCommands.SelectUp, () => SelectRow(-1, true), Document.HasCurrent);
			h.Connect(TimelineCommands.SelectDown, () => SelectRow(1, true), Document.HasCurrent);
			h.Connect(TimelineCommands.ScrollLeft, () => AdvanceCurrentColumn(-1), Document.HasCurrent);
			h.Connect(TimelineCommands.ScrollRight, () => AdvanceCurrentColumn(1), Document.HasCurrent);
			h.Connect(TimelineCommands.FastScrollLeft, () => AdvanceCurrentColumn(-10), Document.HasCurrent);
			h.Connect(TimelineCommands.FastScrollRight, () => AdvanceCurrentColumn(10), Document.HasCurrent);
			h.Connect(TimelineCommands.DeleteKeyframes, RemoveKeyframes, Document.HasCurrent);
			h.Connect(TimelineCommands.InsertFrame, InsertFrame, Document.HasCurrent);
			h.Connect(TimelineCommands.DeleteFrame, DeleteFrame, Document.HasCurrent);
			h.Connect(TimelineCommands.CreateMarkerPlay, () => CreateMarker(MarkerAction.Play), Document.HasCurrent);
			h.Connect(TimelineCommands.CreateMarkerStop, () => CreateMarker(MarkerAction.Stop), Document.HasCurrent);
			h.Connect(TimelineCommands.CreateMarkerJump, () => CreateMarker(MarkerAction.Jump), Document.HasCurrent);
			h.Connect(TimelineCommands.DeleteMarker, DeleteMarker, Document.HasCurrent);
			h.Connect(TimelineCommands.CopyMarkers, CopyMarkers, Document.HasCurrent);
			h.Connect(TimelineCommands.PasteMarkers, PasteMarkers, Document.HasCurrent);
			h.Connect(TimelineCommands.DeleteMarkers, DeleteMarkers, Document.HasCurrent);
		}

		static void RenameCurrentRow()
		{
			var doc = Document.Current;
			if (doc.SelectedRows().Count() != 1) {
				return;
			}
			var row = doc.SelectedRows().First();
			row.Components.Get<RowView>().RollRow.Rename();
		}

		static void SelectRow(int advance, bool multiselection)
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

		static void RemoveKeyframes()
		{
			foreach (var row in Document.Current.Rows) {
				var spans = row.Components.GetOrAdd<GridSpanListComponent>().Spans;
				foreach (var span in spans.GetNonOverlappedSpans()) {
					var node = row.Components.Get<NodeRow>()?.Node ?? row.Components.Get<PropertyRow>()?.Node;
					if (node == null) {
						continue;
					}
					var property = row.Components.Get<PropertyRow>()?.Animator.TargetProperty;
					foreach (var a in node.Animators) {
						if (property != null && a.TargetProperty != property) {
							continue;
						}
						foreach (var k in a.Keys.Where(k => k.Frame >= span.A && k.Frame < span.B).ToList()) {
							Core.Operations.RemoveKeyframe.Perform(a, k.Frame);
						}
					}
				}
			}
		}

		private static void WithKeyframes(Action<IKeyframe, IAnimator> action)
		{
			foreach (var row in Document.Current.Rows) {
				var node = row.Components.Get<NodeRow>()?.Node ?? row.Components.Get<PropertyRow>()?.Node;
				if (node == null) {
					continue;
				}
				var property = row.Components.Get<PropertyRow>()?.Animator.TargetProperty;
				foreach (var a in node.Animators) {
					if (property != null && a.TargetProperty != property) {
						continue;
					}
					var keys = a.Keys.Where(k => k.Frame >= Document.Current.AnimationFrame).ToList();
					foreach (var k in keys) {
						action(k, a);
					}
				}
			}
		}

		private static void WithMarkers(Action<Marker> action)
		{
			var markers = Document.Current.Container.Markers.Where(m => m.Frame >= Document.Current.AnimationFrame).ToList();
			foreach (var marker in markers) {
				action(marker);
			}
		}

		private static void InsertFrame()
		{
			Document.Current.History.BeginTransaction();
			WithKeyframes((k, a) => Core.Operations.SetProperty.Perform(k, nameof(IKeyframe.Frame), k.Frame + 1));
			WithMarkers(m => Core.Operations.SetProperty.Perform(m, nameof(Marker.Frame), m.Frame + 1));
			Document.Current.History.EndTransaction();
		}

		private static void DeleteFrame()
		{
			Document.Current.History.BeginTransaction();
			WithKeyframes((k, a) => {
				if (k.Frame != Document.Current.AnimationFrame) {
					Core.Operations.SetProperty.Perform(k, nameof(IKeyframe.Frame), k.Frame - 1);
				} else {
					Core.Operations.RemoveKeyframe.Perform(a, k.Frame);
				}
			});
			WithMarkers(m => {
				if (m.Frame != Document.Current.AnimationFrame) {
					Core.Operations.SetProperty.Perform(m, nameof(Marker.Frame), m.Frame - 1);
				} else {
					Core.Operations.DeleteMarker.Perform(Document.Current.Container.Markers, m);
				}
			});
			Document.Current.History.EndTransaction();
		}

		static void AdvanceCurrentColumn(int stride)
		{
			Operations.SetCurrentColumn.Perform(Math.Max(0, Timeline.Instance.CurrentColumn + stride));
		}

		static void CreateMarker(MarkerAction action)
		{
			var timeline = Timeline.Instance;
			var nearestMarker = Document.Current.Container.Markers.LastOrDefault(
				m => m.Frame < timeline.CurrentColumn && m.Action == MarkerAction.Play);
			var newMarker = new Marker(
				action == MarkerAction.Play ? "Start" : "",
				timeline.CurrentColumn,
				action,
				action == MarkerAction.Jump && nearestMarker != null ? nearestMarker.Id : ""
			);
			Core.Operations.SetMarker.Perform(Document.Current.Container.DefaultAnimation.Markers, newMarker);
		}

		static void DeleteMarker()
		{
			var timeline = Timeline.Instance;
			var marker = Document.Current.Container.Markers.FirstOrDefault(i => i.Frame == timeline.CurrentColumn);
			if (marker != null) {
				Core.Operations.DeleteMarker.Perform(Document.Current.Container.DefaultAnimation.Markers, marker);
			}
		}

		static void CopyMarkers()
		{
			var frame = new Frame();
			foreach (var marker in Document.Current.Container.Markers) {
				frame.Markers.Add(marker);
			}
			var stream = new System.IO.MemoryStream();
			Serialization.WriteObject(Document.Current.Path, stream, frame, Serialization.Format.JSON);
			var text = System.Text.Encoding.UTF8.GetString(stream.ToArray());
			Clipboard.Text = text;
		}

		static void PasteMarkers()
		{
			try {
				var text = Clipboard.Text;
				var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));
				var frame = Serialization.ReadObject<Frame>(Document.Current.Path, stream);
				foreach (var marker in frame.Markers) {
					Core.Operations.SetMarker.Perform(Document.Current.Container.DefaultAnimation.Markers, marker);
				}
			} catch (System.Exception e) {
				Debug.Write(e);
			}
		}

		static void DeleteMarkers()
		{
			var timeline = Timeline.Instance;
			foreach (var marker in Document.Current.Container.Markers.ToList()) {
				Core.Operations.DeleteMarker.Perform(Document.Current.Container.DefaultAnimation.Markers, marker);
			}
		}
	}
}
