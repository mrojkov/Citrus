using System;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	public class Splitter : Widget
	{
		public float SeparatorActiveAreaWidth;
		public float SeparatorWidth;
		public Color4 SeparatorColor;

		public event Action DragStarted;
		public event Action DragEnded;

		protected void RaiseDragStarted()
		{
			if (DragStarted != null) {
				DragStarted();
			}
		}

		protected void RaiseDragEnded()
		{
			if (DragEnded != null) {
				DragEnded();
			}
		}
	}

	public class HSplitter : Splitter
	{
		public HSplitter()
		{
			Tasks.Add(MainTask());
			PostPresenter = new DelegatePresenter<Widget>(RenderSeparator);
			Theme.Current.Apply(this);
			Layout = new HBoxLayout { Spacing = SeparatorWidth };
		}

		void RenderSeparator(Widget widget)
		{
			widget.PrepareRendererState();
			for (int i = 0; i < Nodes.Count - 1; i++) {
				var w = Nodes[i + 1].AsWidget;
				var x = w.X - SeparatorWidth / 2;
				Renderer.DrawLine(x, w.Y, x, w.Height + w.Y, SeparatorColor, thickness: SeparatorWidth);
			}
		}

		private IEnumerator<object> MainTask()
		{
			while (true) {
				if (Input.WasMousePressed()) {
					int index;
					if (FindSeparatorUnderMouse(out index)) {
						yield return DragSeparatorTask(index);
					}
				}
				yield return null;
			}
		}

		private IEnumerator<object> DragSeparatorTask(int index)
		{
			RaiseDragStarted();
			var initialMousePosition = Input.MousePosition;
			var initialWidths = Nodes.Select(i => i.AsWidget.Width).ToList();
			Input.CaptureMouse();
			while (Input.IsMousePressed()) {
				var dragDelta = Input.MousePosition.X - initialMousePosition.X;
				AdjustStretchDelta(initialWidths[index], Nodes[index].AsWidget, ref dragDelta);
				dragDelta = -dragDelta;
				AdjustStretchDelta(initialWidths[index + 1], Nodes[index + 1].AsWidget, ref dragDelta);
				dragDelta = -dragDelta;
				for (int i = 0; i < Nodes.Count; i++) {
					var d = (i == index) ? dragDelta : ((i == index + 1) ? -dragDelta : 0);
					Nodes[i].AsWidget.LayoutCell = new LayoutCell { StretchX = initialWidths[i] + d };
				}
				Layout.InvalidateConstraintsAndArrangement(this);
				yield return null;
			}
			Input.ReleaseMouse();
			RaiseDragEnded();
		}

		private void AdjustStretchDelta(float initialWidth, Widget widget, ref float delta)
		{
			if (initialWidth + delta <= widget.MinWidth) {
				delta = widget.MinWidth - initialWidth;
			}
			if (initialWidth + delta >= widget.MaxWidth) {
				delta = widget.MaxWidth - initialWidth;
			}
		}

		private bool FindSeparatorUnderMouse(out int index)
		{
			for (int i = 0; i < Nodes.Count - 1; i++) {
				var widget = Nodes[i + 1].AsWidget;
				var p = widget.GlobalPosition;
				if (Mathf.Abs(Input.MousePosition.X - (p.X - SeparatorWidth * 0.5f)) < SeparatorActiveAreaWidth * 0.5f) {
					if (Input.MousePosition.X > p.Y && Input.MousePosition.Y < p.Y + widget.Height) {
						index = i;
						return true;
					}
				}
			}
			index = -1;
			return false;
		}
	}
}