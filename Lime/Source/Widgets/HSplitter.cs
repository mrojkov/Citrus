using System;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	public enum SplitterLineState
	{
		Default,
		Highlight,
		Drag
	}

	[YuzuDontGenerateDeserializer]
	[TangerineAllowedChildrenTypes(typeof(Node))]
	public abstract class Splitter : Widget
	{
		public float SeparatorActiveAreaWidth;
		public Color4 SeparatorColor;
		public Color4 SeparatorHighlightColor;
		public Color4 SeparatorDragColor;

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

		protected abstract class SeparatorsRenderPresenterBase : IPresenter
		{
			public IPresenter Clone() => (IPresenter)MemberwiseClone();

			public Lime.RenderObject GetRenderObject(Node node)
			{
				var splitter = (Splitter)node;
				var ro = RenderObjectPool<RenderObject>.Acquire();
				ro.CaptureRenderState(splitter);
				ro.SeparatorWidth = splitter.SeparatorWidth;
				ro.SeparatorColor = splitter.SeparatorColor;
				ro.SeparatorHighlightColor = splitter.SeparatorHighlightColor;
				ro.SeparatorDragColor = splitter.SeparatorDragColor;
				GetLines(splitter, ro.Lines);
				return ro;
			}

			protected abstract void GetLines(Splitter splitter, List<SplitterLine> lines);

			public bool PartialHitTest(Node node, ref HitTestArgs args) => false;

			private class RenderObject : WidgetRenderObject
			{
				public float SeparatorWidth;
				public Color4 SeparatorColor;
				public Color4 SeparatorHighlightColor;
				public Color4 SeparatorDragColor;
				public readonly List<SplitterLine> Lines = new List<SplitterLine>();

				public override void Render()
				{
					PrepareRenderState();
					for (var i = 0; i < Lines.Count; i ++) {
						Renderer.DrawLine(Lines[i].Start, Lines[i].End, GetColorForState(Lines[i].State), thickness: SeparatorWidth);
					}
				}

				private Color4 GetColorForState(SplitterLineState state)
				{
					switch (state) {
						case SplitterLineState.Highlight:
							return SeparatorHighlightColor;
						case SplitterLineState.Drag:
							return SeparatorDragColor;
						default:
							return SeparatorColor;
					}
				}

				protected override void OnRelease()
				{
					Lines.Clear();
				}
			}

			public struct SplitterLine
			{
				public Vector2 Start;
				public Vector2 End;
				public SplitterLineState State;
			}
		}
	}

	[YuzuDontGenerateDeserializer]
	public class HSplitter : Splitter
	{
		private SeparatorsRenderPresenter separartorRenderer;

		public HSplitter()
		{
			Tasks.Add(MainTask());
			PostPresenter = separartorRenderer = new SeparatorsRenderPresenter();
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
				if (IsMouseOver() && separartorRenderer.SeparatorUnderMouse >= 0) {
					WidgetContext.Current.MouseCursor = MouseCursor.SizeWE;
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
				Layout.InvalidateConstraintsAndArrangement();
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

		class SeparatorsRenderPresenter : SeparatorsRenderPresenterBase
		{
			public int SeparatorUnderMouse = -1;
			public bool isSeparatorUnderMouseDrag;

			protected override void GetLines(Splitter splitter, List<SplitterLine> lines)
			{
				for (int i = 0; i < splitter.Nodes.Count - 1; i++) {
					var w = splitter.Nodes[i + 1].AsWidget;
					var x = w.X - splitter.SeparatorWidth * 0.5f;
					lines.Add(new SplitterLine {
						Start = new Vector2(x, w.Y),
						End = new Vector2(x, w.Y + w.Height),
						State = i == SeparatorUnderMouse ? (isSeparatorUnderMouseDrag ?
							SplitterLineState.Drag : SplitterLineState.Highlight) : SplitterLineState.Default
					});
				}
			}
		}

		class SeparatorsHitTestPresenter : IPresenter
		{
			public int SeparatorUnderMouse { get; private set; } = -1;

			public IPresenter Clone() => (IPresenter)MemberwiseClone();

			public RenderObject GetRenderObject(Node node) => null;

			public bool PartialHitTest(Node node, ref HitTestArgs args)
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

		[YuzuDontGenerateDeserializer]
		class HSplitterLayout : HBoxLayout
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
					cell.StretchX = i < splitter.Stretches.Count ? splitter.Stretches[i] : 1;
				}
			}
		}
	}
}
