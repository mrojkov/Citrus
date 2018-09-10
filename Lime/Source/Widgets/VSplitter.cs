using System;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	public class VSplitter : Splitter
	{
		private SeparatorsRenderPresenter separartorRenderer;

		public VSplitter()
		{
			Tasks.Add(MainTask());
			PostPresenter = separartorRenderer = new SeparatorsRenderPresenter();
			Layout = new VSplitterLayout();
		}

		public override float SeparatorWidth
		{
			get
			{
				return (Layout as VSplitterLayout).Spacing;
			}
			set
			{
				var l = (Layout as VSplitterLayout);
				if (l.Spacing != value) {
					l.Spacing = value;
					l.InvalidateArrangement();
				}
			}
		}

		private IEnumerator<object> MainTask()
		{
			var p = new SeparatorsHitTestPresenter();
			CompoundPostPresenter.Add(p);
			while (true) {
				bool isNeedToInvalidate = false;
				if (separartorRenderer.SeparatorUnderMouse != p.SeparatorUnderMouse) {
					separartorRenderer.SeparatorUnderMouse = p.SeparatorUnderMouse;
					isNeedToInvalidate = true;
				}
				if (IsMouseOver() && p.SeparatorUnderMouse >= 0) {
					WidgetContext.Current.MouseCursor = MouseCursor.SizeNS;
					if (Input.WasMousePressed()) {
						separartorRenderer.isSeparatorUnderMouseDrag = true;
						Window.Current.Invalidate();
						yield return DragSeparatorTask(p.SeparatorUnderMouse);
						separartorRenderer.isSeparatorUnderMouseDrag = false;
						isNeedToInvalidate = true;
					}
				}
				if (isNeedToInvalidate) {
					Window.Current.Invalidate();
				}
				yield return null;
			}
		}

		private IEnumerator<object> DragSeparatorTask(int index)
		{
			RaiseDragStarted();
			var initialMousePosition = Input.MousePosition;
			var initialHeights = Nodes.Select(i => i.AsWidget.Height).ToList();
			while (Input.IsMousePressed()) {
				WidgetContext.Current.MouseCursor = MouseCursor.SizeNS;
				var dragDelta = Input.MousePosition.Y - initialMousePosition.Y;
				AdjustStretchDelta(initialHeights[index], Nodes[index].AsWidget, ref dragDelta);
				dragDelta = -dragDelta;
				AdjustStretchDelta(initialHeights[index + 1], Nodes[index + 1].AsWidget, ref dragDelta);
				dragDelta = -dragDelta;
				for (int i = 0; i < Nodes.Count; i++) {
					var d = (i == index) ? dragDelta : ((i == index + 1) ? -dragDelta : 0);
					if (i == Stretches.Count) {
						Stretches.Add(0);
					}
					Stretches[i] = initialHeights[i] + d;
				}
				Layout.InvalidateConstraintsAndArrangement();
				yield return null;
			}
			RaiseDragEnded();
		}

		private void AdjustStretchDelta(float initialHeight, Widget widget, ref float delta)
		{
			if (initialHeight + delta <= widget.MinHeight) {
				delta = widget.MinHeight - initialHeight;
			}
			if (initialHeight + delta >= widget.MaxHeight) {
				delta = widget.MaxHeight - initialHeight;
			}
		}

		class SeparatorsRenderPresenter : SeparatorsRenderPresenterBase
		{
			public int SeparatorUnderMouse = -1;
			public bool isSeparatorUnderMouseDrag;

			protected override void GetLines(Splitter splitter, List<SplitterLine> lines)
			{
				for (int i = 0; i < splitter.Nodes.Count - 1; i++) {
					var w = splitter.Nodes[i + 1].AsWidget;
					var y = w.Y - splitter.SeparatorWidth * 0.5f;
					lines.Add(new SplitterLine {
						Start = new Vector2(w.X, y),
						End = new Vector2(w.X + w.Width, y),
						State = i == SeparatorUnderMouse ? (isSeparatorUnderMouseDrag ?
							SplitterLineState.Drag : SplitterLineState.Highlight) : SplitterLineState.Default
					});
				}
			}
		}

		class SeparatorsHitTestPresenter : IPresenter
		{
			public int SeparatorUnderMouse { get; private set; }

			public IPresenter Clone() => (IPresenter)MemberwiseClone();

			public RenderObject GetRenderObject(Node node) => null;

			public bool PartialHitTest(Node node, ref HitTestArgs args)
			{
				var splitter = (Splitter)node;
				for (int i = 0; i < splitter.Nodes.Count - 1; i++) {
					var widget = splitter.Nodes[i + 1].AsWidget;
					var widgetPos = widget.GlobalPosition;
					var mousePos = Window.Current.Input.MousePosition;
					if (Mathf.Abs(mousePos.Y - (widgetPos.Y - splitter.SeparatorWidth * 0.5f)) < splitter.SeparatorActiveAreaWidth * 0.5f) {
						if (mousePos.X > widgetPos.X && mousePos.X < widgetPos.X + widget.Width) {
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

		[YuzuDontGenerateDeserializer]
		class VSplitterLayout : VBoxLayout
		{
			public override void MeasureSizeConstraints()
			{
				UpdateLayoutCells((Splitter)Owner);
				base.MeasureSizeConstraints();
			}

			void UpdateLayoutCells(Splitter splitter)
			{
				for (int i = 0; i < splitter.Nodes.Count; i++) {
					var widget = (Widget)splitter.Nodes[i];
					var cell = widget.LayoutCell ?? (widget.LayoutCell = new LayoutCell());
					cell.StretchY = i < splitter.Stretches.Count ? splitter.Stretches[i] : 1;
				}
			}
		}
	}
}
