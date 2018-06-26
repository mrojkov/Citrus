using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	public class GridNodeView : IGridRowView
	{
		readonly Node node;

		public Widget GridWidget { get; private set; }
		public Widget OverviewWidget { get; private set; }

		public GridNodeView(Node node)
		{
			this.node = node;
			GridWidget = new Widget { LayoutCell = new LayoutCell { StretchY = 0 }, MinHeight = TimelineMetrics.DefaultRowHeight };
			GridWidget.Presenter = new DelegatePresenter<Widget>(Render);
			OverviewWidget = new Widget { LayoutCell = new LayoutCell { StretchY = 0 }, MinHeight = TimelineMetrics.DefaultRowHeight };
			OverviewWidget.Presenter = new DelegatePresenter<Widget>(Render);
		}

		struct Cell
		{
			public BitSet32 Strips;
			public int StripCount;
			public KeyFunction Func1;
			public KeyFunction Func2;
		}

		static Cell[] cells = new Cell[0];

		protected virtual void Render(Widget widget)
		{
			var maxCol = Timeline.Instance.ColumnCount;
			widget.PrepareRendererState();
			if (maxCol > cells.Length) {
				cells = new Cell[maxCol];
			}
			for (int i = 0; i < maxCol; i++) {
				cells[i] = new Cell();
			}
			foreach (var a in node.Animators) {
				for (int j = 0; j < a.ReadonlyKeys.Count; j++) {
					var key = a.ReadonlyKeys[j];
					var colorIndex = PropertyAttributes<TangerineKeyframeColorAttribute>.Get(node.GetType(), a.TargetProperty)?.ColorIndex ?? 0;
					var c = cells[key.Frame];
					c.Strips[colorIndex] = true;
					if (c.StripCount == 0) {
						c.Func1 = key.Function;
					}
					else if (c.StripCount == 1) {
						c.Func2 = key.Function;
					}
					c.StripCount++;
					cells[key.Frame] = c;
				}
			}
			for (int i = 0; i < maxCol; i++) {
				var cell = cells[i];
				if (cell.StripCount == 0) {
					continue;
				}
				var a = new Vector2(i * TimelineMetrics.ColWidth + 1, 0);
				var d = widget.Height / cell.StripCount;
				if (cell.StripCount == 1) {
					var b = a + new Vector2(TimelineMetrics.ColWidth - 1, d);
					int colorIndex;
					for (colorIndex = 0; colorIndex < 32; colorIndex++) {
						if (cell.Strips[colorIndex]) break;
					}
					DrawFigure(a, b, cell.Func1, KeyframePalette.Colors[colorIndex]);
				}
				else if (cell.StripCount == 2) {
					bool flag = true;
					for (int colorIndex = 0; colorIndex < 32; colorIndex++) {
						if (cell.Strips[colorIndex]) {
							var b = a + new Vector2(TimelineMetrics.ColWidth - 1, d / 2);
							if (flag) {
								DrawFigure(a, b, cell.Func1, KeyframePalette.Colors[colorIndex]);
								flag = false;
							} else {
								DrawFigure(a, b, cell.Func2, KeyframePalette.Colors[colorIndex]);
								break;
							}
							a.Y += d;
						}
					}
				}
				else {
					for (int colorIndex = 0; colorIndex < 32; colorIndex++) {
						if (cell.Strips[colorIndex]) {
							var b = a + new Vector2(TimelineMetrics.ColWidth - 1, d);
							Renderer.DrawRect(a, b, KeyframePalette.Colors[colorIndex]);
							a.Y += d;
						}
					}
				}
			}
		}

		protected void DrawFigure(Vector2 a, Vector2 b, KeyFunction func, Color4 color)
		{
			switch (func) {
				case KeyFunction.Linear:
					var quadrangle = new Quadrangle {
						V1 = new Vector2(a.X + (b.X - a.X) / 2, a.Y),
						V2 = new Vector2(b.X, a.Y + (b.Y - a.Y) / 2),
						V3 = new Vector2(a.X + (b.X - a.X) / 2, b.Y),
						V4 = new Vector2(a.X, a.Y + (b.Y - a.Y) / 2)
					};
					Renderer.DrawQuadrangle(quadrangle, color);
					break;
				case KeyFunction.Steep:
					var rectSize = (b.X - a.X) / 2;
					var horizontalOffset = rectSize / 2;
					var verticalOffset = (b.Y - a.Y - rectSize) / 2;
					var rectVertexA = new Vector2(a.X + horizontalOffset, a.Y + verticalOffset);
					var rectVertexB = new Vector2(b.X - horizontalOffset, b.Y - verticalOffset);
					Renderer.DrawRect(rectVertexA, rectVertexB, color);
					break;
				case KeyFunction.Spline:
					var circleCenter = new Vector2(a.X + (b.X - a.X) / 2, a.Y + (b.Y - a.Y) / 2);
					var circleRadius = (b.X - a.X) / 3;
					Renderer.DrawRound(circleCenter, circleRadius, 16, color);
					break;
				case KeyFunction.ClosedSpline:
					var roundCenter = new Vector2(a.X + (b.X - a.X) / 2, a.Y + (b.Y - a.Y) / 2);
					var roundRadius = (b.X - a.X) / 3;
					Renderer.DrawRound(roundCenter, roundRadius, 16, Color4.Transparent, color);
					break;
			}
		}
	}
}
