using System;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	[AllowedChildrenTypes(typeof(Node))]
	public abstract class Splitter : Widget
	{
		public float SeparatorActiveAreaWidth;
		public Color4 SeparatorColor;

		public abstract float SeparatorWidth { get; set; }

		public List<float> Stretches { get; set; } = new List<float>();

		public event Action DragStarted;
		public event Action DragEnded;

		protected void RaiseDragStarted() => DragStarted?.Invoke();
		protected void RaiseDragEnded() => DragEnded?.Invoke();

		public static List<float> GetStretchesList(List<float> stretches, params float[] defaults)
		{
			if (stretches == null) {
				throw new InvalidOperationException("stretches shouldn't be null");
			}
			if (stretches.Count < defaults.Length) {
				stretches.Clear();
				stretches.AddRange(defaults);
			}
			return stretches;
		}
	}

	public class HSplitter : Splitter
	{
		public HSplitter()
		{
			Tasks.Add(MainTask());
			PostPresenter = new DelegatePresenter<Widget>(RenderSeparator);
			Layout = new HSplitterLayout();
		}

		public override float SeparatorWidth
		{
			get
			{
				return (Layout as HSplitterLayout).Spacing;
			}
			set
			{
				var l = (Layout as HSplitterLayout);
				if (l.Spacing != value) {
					l.Spacing = value;
					l.InvalidateArrangement(this);
				}
			}
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
			var p = new SeparatorsHitTestPresenter();
			CompoundPostPresenter.Add(p);
			while (true) {
				if (IsMouseOver() && p.SeparatorUnderMouse >= 0) {
					WidgetContext.Current.MouseCursor = MouseCursor.SizeWE;
					if (Input.WasMousePressed()) {
						yield return DragSeparatorTask(p.SeparatorUnderMouse);
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
			while (Input.IsMousePressed()) {
				WidgetContext.Current.MouseCursor = MouseCursor.SizeWE;
				var dragDelta = Input.MousePosition.X - initialMousePosition.X;
				AdjustStretchDelta(initialWidths[index], Nodes[index].AsWidget, ref dragDelta);
				dragDelta = -dragDelta;
				AdjustStretchDelta(initialWidths[index + 1], Nodes[index + 1].AsWidget, ref dragDelta);
				dragDelta = -dragDelta;
				for (int i = 0; i < Nodes.Count; i++) {
					var d = (i == index) ? dragDelta : ((i == index + 1) ? -dragDelta : 0);
					if (i == Stretches.Count) {
						Stretches.Add(0);
					}
					Stretches[i] = initialWidths[i] + d;
				}
				Layout.InvalidateConstraintsAndArrangement(this);
				yield return null;
			}
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

		class SeparatorsHitTestPresenter : CustomPresenter
		{
			public int SeparatorUnderMouse { get; private set; }

			public override bool PartialHitTest(Node node, ref HitTestArgs args)
			{
				var splitter = (Splitter)node;
				for (int i = 0; i < splitter.Nodes.Count - 1; i++) {
					var widget = splitter.Nodes[i + 1].AsWidget;
					var widgetPos = widget.GlobalPosition;
					var mousePos = Window.Current.Input.MousePosition;
					if (Mathf.Abs(mousePos.X - (widgetPos.X - splitter.SeparatorWidth * 0.5f)) < splitter.SeparatorActiveAreaWidth * 0.5f) {
						if (mousePos.Y > widgetPos.Y && mousePos.Y < widgetPos.Y + widget.Height) {
							SeparatorUnderMouse = i;
							args.Node = splitter;
							return true;
						}
					}
				}
				SeparatorUnderMouse = -1;
				return false;
			}
		}

		[TangerineIgnore]
		class HSplitterLayout : HBoxLayout
		{
			public override void MeasureSizeConstraints(Widget widget)
			{
				UpdateLayoutCells((Splitter)widget);
				base.MeasureSizeConstraints(widget);
			}

			void UpdateLayoutCells(Splitter splitter)
			{
				for (int i = 0; i < splitter.Nodes.Count; i++) {
					var widget = (Widget)splitter.Nodes[i];
					var cell = widget.LayoutCell ?? (widget.LayoutCell = new LayoutCell());
					cell.StretchX = i < splitter.Stretches.Count ? splitter.Stretches[i] : 1;
				}
			}
		}
	}
}
