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
				} else if (cell.StripCount == 2) {
					var numberOfDrawnStrips = 0;
					int colorIndex = 0;
					for (colorIndex = 0; colorIndex < 32; colorIndex++) {
						if (cell.Strips[colorIndex]) {
							var b = a + new Vector2(TimelineMetrics.ColWidth - 1, d);
							DrawFigure(a, b, cell.Func1, KeyframePalette.Colors[colorIndex]);
							numberOfDrawnStrips++;
							a.Y += d;
							break;
						}
					}
					for (colorIndex = colorIndex + 1; colorIndex < 32; colorIndex++) {
						if (cell.Strips[colorIndex]) {
							var b = a + new Vector2(TimelineMetrics.ColWidth - 1, d);
							DrawFigure(a, b, cell.Func2, KeyframePalette.Colors[colorIndex]);
							numberOfDrawnStrips++;
							a.Y += d;
							break;
						}
					}
					if(numberOfDrawnStrips < cell.StripCount) {
						for (colorIndex = 0; colorIndex < 32; colorIndex++) {
							if (cell.Strips[colorIndex]) {
								var b = a + new Vector2(TimelineMetrics.ColWidth - 1, d);
								DrawFigure(a, b, cell.Func2, KeyframePalette.Colors[colorIndex]);
								numberOfDrawnStrips++;
								a.Y += d;
								break;
							}
						}
					}
				} else {
					var numberOfDrawnStrips = 0;
					for (int colorIndex = 0; colorIndex < 32; colorIndex++) {
						if (cell.Strips[colorIndex]) {
							var b = a + new Vector2(TimelineMetrics.ColWidth - 1, d);
							Renderer.DrawRect(a, b, KeyframePalette.Colors[colorIndex]);
							a.Y += d;
							numberOfDrawnStrips++;
							if (numberOfDrawnStrips == cell.StripCount) break;
						}
					}
					if(numberOfDrawnStrips < cell.StripCount) {
						int colorIndex;
						for (colorIndex = 0; colorIndex < 32; colorIndex++) {
							if (cell.Strips[colorIndex]) break;
						}
						for (numberOfDrawnStrips = numberOfDrawnStrips; numberOfDrawnStrips != cell.StripCount; numberOfDrawnStrips++) {
							var b = a + new Vector2(TimelineMetrics.ColWidth - 1, d);
							Renderer.DrawRect(a, b, KeyframePalette.Colors[colorIndex]);
							a.Y += d;
						}
					}
				}
			}
		}

		static Vertex[] vertices;

		protected void DrawFigure(Vector2 a, Vector2 b, KeyFunction func, Color4 color)
		{
			var segmentWidth = b.X - a.X;
			var segmentHeight = b.Y - a.Y;
			switch (func) {
				case KeyFunction.Linear:
					vertices = new Vertex[3];
					vertices[0].Pos = new Vector2(a.X, b.Y - 0.5f);
					vertices[1].Pos = new Vector2(b.X, a.Y);
					vertices[2].Pos = new Vector2(b.X, b.Y - 0.5f);
					for(int i = 0; i < vertices.Count(); i++) {
						vertices[i].Color = color;
					}
					Renderer.DrawTriangleFan(vertices, vertices.Count());
					break;
				case KeyFunction.Steep:
					var rightBigVertexA = new Vector2(a.X + segmentWidth / 2, a.Y + 0.5f);
					var rightBigVertexB = new Vector2(b.X, b.Y - 0.5f);
					Renderer.DrawRect(rightBigVertexA, rightBigVertexB, color);
					var leftSmallVertexA = new Vector2(a.X + 0.5f, a.Y + segmentHeight / 2);
					var leftSmallVertexB = new Vector2(a.X + segmentWidth / 2, b.Y - 0.5f);
					Renderer.DrawRect(leftSmallVertexA, leftSmallVertexB, color);
					break;
				case KeyFunction.Spline:
					var numSegments = 10;
					var center = b;
					var radius = 0f;
					if (segmentWidth < segmentHeight) {
						radius = segmentWidth;
					} else {
						radius = segmentHeight;
					}
					vertices = new Vertex[numSegments + 1];
					vertices[0] = new Vertex { Pos = center, Color = color };
					for (int i = 0; i < numSegments; i++) {
						vertices[i + 1].Pos = Vector2.CosSin(i * Mathf.HalfPi / (numSegments - 1)) * (-1)* radius + center;
						vertices[i + 1].Color = color;
					}
					Renderer.DrawTriangleFan(vertices, numSegments + 1);
					break;
				case KeyFunction.ClosedSpline:
					var circleCenter = new Vector2(a.X + segmentWidth / 2, a.Y + segmentHeight / 2);
					var circleRadius = 0f;
					if (segmentWidth < segmentHeight) {
						circleRadius = circleCenter.X - a.X - 0.5f;
					}
					else {
						circleRadius = circleCenter.Y - a.Y - 0.5f;
					}
					Renderer.DrawRound(circleCenter, circleRadius, 16, color);
					break;
			}
		}
	}
}
