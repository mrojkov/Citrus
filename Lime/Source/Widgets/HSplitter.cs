using System;
using System.Collections.Generic;
using System.Linq;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	[TangerineAllowedChildrenTypes(typeof(Node))]
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
				ro.Lines.Clear();
				GetLines(splitter, ro.Lines);
				return ro;
			}

			protected abstract void GetLines(Splitter splitter, List<Vector2> lines);

			public bool PartialHitTest(Node node, ref HitTestArgs args) => false;

			private class RenderObject : WidgetRenderObject
			{
				public float SeparatorWidth;
				public Color4 SeparatorColor;
				public List<Vector2> Lines = new List<Vector2>();

				public override void Render()
				{
					PrepareRenderState();
					for (var i = 0; i < Lines.Count - 1; i += 2) {
						var p1 = Lines[i];
						var p2 = Lines[i + 1];
						Renderer.DrawLine(p1, p2, SeparatorColor, thickness: SeparatorWidth);
					}
				}
			}
		}
	}

	[YuzuDontGenerateDeserializer]
	public class HSplitter : Splitter
	{
		public HSplitter()
		{
			Tasks.Add(MainTask());
			PostPresenter = new SeparatorsRenderPresenter();
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
			protected override void GetLines(Splitter splitter, List<Vector2> lines)
			{
				for (int i = 0; i < splitter.Nodes.Count - 1; i++) {
					var w = splitter.Nodes[i + 1].AsWidget;
					var x = w.X - splitter.SeparatorWidth * 0.5f;
					lines.Add(new Vector2(x, w.Y));
					lines.Add(new Vector2(x, w.Y + w.Height));
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
