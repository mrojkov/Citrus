using System;
using System.Linq;
using System.Collections.Generic;

namespace Lime
{
	public class FlowLayout : CommonLayout, ILayout
	{
		private readonly List<int> splitIndices = new List<int>();
		public float Spacing { get; set; }
		// TODO: implement for any alignment other than justify or left
		public HAlignment RowAlignment { get; set; }
		public VAlignment ColumnAlignment { get; set; }
		public int direction = 0;

		public FlowLayout()
		{
			DebugRectangles = new List<Rectangle>();
		}

		public int RowCount(int columnIndex)
		{
			if (direction == 0) {
				return splitIndices.Count - 1;
			} else {
				return splitIndices[columnIndex + 1] - splitIndices[columnIndex];
			}
		}

		public int ColumnCount(int rowIndex)
		{
			if (direction == 0) {
				return splitIndices[rowIndex + 1] - splitIndices[rowIndex];
			} else {
				return splitIndices.Count - 1;
			}
		}

		public override void OnSizeChanged(Widget widget, Vector2 sizeDelta)
		{
			InvalidateConstraintsAndArrangement(widget);
		}

		public override void ArrangeChildren(Widget widget)
		{
			if (direction == 0) {
				ArrangementValid = true;
				var widgets = GetChildren(widget);
				if (widgets.Count == 0) {
					return;
				}
				DebugRectangles.Clear();

				List<Widget>[] lines = new List<Widget>[splitIndices.Count - 1];
				float[] maxLineHeights = new float[splitIndices.Count - 1];
				for (int j = 0; j < splitIndices.Count - 1; j++) {
					int i0 = splitIndices[j];
					int i1 = splitIndices[j + 1];
					lines[j] = widgets.GetRange(i0, i1 - i0);
					maxLineHeights[j] = lines[j].Max((w) => w.EffectiveMinSize.Y);
				}
				var availableHeight = Math.Max(0, widget.ContentHeight - (lines.Length - 1) * Spacing);
				float dy = 0.0f;
				for (int j = 0; j < splitIndices.Count - 1; j++) {
					int i0 = splitIndices[j];
					int i1 = splitIndices[j + 1];
					var constraints = new LinearAllocator.Constraints[i1 - i0];
					var line = lines[j];
					var maxLineHeight = maxLineHeights[j];
					var availableWidth = Math.Max(0, widget.ContentWidth - (line.Count - 1) * Spacing);
					int i = 0;
					foreach (var w in line) {
						constraints[i++] = new LinearAllocator.Constraints {
							MinSize = w.EffectiveMinSize.X,
							MaxSize = w.EffectiveMaxSize.X,
							Stretch = (w.LayoutCell ?? LayoutCell.Default).StretchX
						};
					}
					var sizes = LinearAllocator.Allocate(availableWidth, constraints, roundSizes: true);
					i = 0;
					float justifyDx = 0.0f;
					if (RowAlignment == HAlignment.Justify) {
						justifyDx = (availableWidth - sizes.Sum(size => size)) / (line.Count + 1);
					}
					if (ColumnAlignment == VAlignment.Justify) {
						var justifyDy = (availableHeight - maxLineHeights.Sum(h => h)) / (lines.Length + 1);
						dy += justifyDy;
					}
					var position = new Vector2(widget.Padding.Left, widget.Padding.Top + dy);
					foreach (var w in line) {
						position.X += justifyDx;
						var height = (w.LayoutCell ?? LayoutCell.Default).Stretch.Y == 0.0f
							? w.EffectiveMinSize.Y
							: maxLineHeight;
						var size = new Vector2(sizes[i], height);
						var align = (w.LayoutCell ?? LayoutCell.Default).Alignment;
						LayoutWidgetWithinCell(w, position, size, align, DebugRectangles);
						position.X += size.X + Spacing;
						i++;
					}
					dy += maxLineHeight + Spacing;
				}
			} else {
				ArrangementValid = true;
				var widgets = GetChildren(widget);
				if (widgets.Count == 0) {
					return;
				}
				DebugRectangles.Clear();

				List<Widget>[] lines = new List<Widget>[splitIndices.Count - 1];
				float[] maxLineWidths = new float[splitIndices.Count - 1];
				for (int j = 0; j < splitIndices.Count - 1; j++) {
					int i0 = splitIndices[j];
					int i1 = splitIndices[j + 1];
					lines[j] = widgets.GetRange(i0, i1 - i0);
					maxLineWidths[j] = lines[j].Max((w) => w.EffectiveMinSize.X);
				}
				var availableWidth = Math.Max(0, widget.ContentWidth - (lines.Length - 1) * Spacing);
				float dx = 0.0f;
				for (int j = 0; j < splitIndices.Count - 1; j++) {
					int i0 = splitIndices[j];
					int i1 = splitIndices[j + 1];
					var constraints = new LinearAllocator.Constraints[i1 - i0];
					var line = lines[j];
					var maxLineWidth = maxLineWidths[j];
					var availableHeight = Math.Max(0, widget.ContentHeight - (line.Count - 1) * Spacing);
					int i = 0;
					foreach (var w in line) {
						constraints[i++] = new LinearAllocator.Constraints {
							MinSize = w.EffectiveMinSize.Y,
							MaxSize = w.EffectiveMaxSize.Y,
							Stretch = (w.LayoutCell ?? LayoutCell.Default).StretchY
						};
					}
					var sizes = LinearAllocator.Allocate(availableHeight, constraints, roundSizes: true);
					i = 0;
					float justifyDy = 0.0f;
					if (ColumnAlignment == VAlignment.Justify) {
						justifyDy = (availableHeight - sizes.Sum(size => size)) / (line.Count + 1);
					}
					if (RowAlignment == HAlignment.Justify) {
						var justifyDx = (availableWidth - maxLineWidths.Sum(w => w)) / (lines.Length + 1);
						dx += justifyDx;
					}
					var position = new Vector2(widget.Padding.Left + dx, widget.Padding.Top);
					foreach (var w in line) {
						position.Y += justifyDy;
						var width = (w.LayoutCell ?? LayoutCell.Default).Stretch.X == 0.0f
							? w.EffectiveMinSize.X
							: maxLineWidth;
						var size = new Vector2(width, sizes[i]);
						var align = (w.LayoutCell ?? LayoutCell.Default).Alignment;
						LayoutWidgetWithinCell(w, position, size, align, DebugRectangles);
						position.Y += size.Y + Spacing;
						i++;
					}
					dx += maxLineWidth + Spacing;
				}
			}
		}

		public override void MeasureSizeConstraints(Widget widget)
		{
			if (direction == 0) {
				ConstraintsValid = true;
				var widgets = GetChildren(widget);
				float paddingH = widget.Padding.Left + widget.Padding.Right;
				float dx = paddingH;
				float dy = widget.Padding.Top + widget.Padding.Bottom - Spacing;
				float maxrowdy = 0;
				int i = 0;
				Action<int> split = (splitIndex) => {
					splitIndices.Add(splitIndex);
					dx = paddingH;
					dy += maxrowdy + Spacing;
					maxrowdy = 0.0f;
				};
				splitIndices.Clear();
				splitIndices.Add(i);
				float minWidth = 0;
				while (i < widgets.Count) {
					var w = widgets[i];
					minWidth = Math.Max(minWidth, w.EffectiveMinSize.X);
					dx += w.EffectiveMinSize.X;
					if (dx <= widget.Width) {
						maxrowdy = Mathf.Max(maxrowdy, w.EffectiveMinSize.Y);
					}
					if (w.EffectiveMinSize.X + paddingH > widget.Width && splitIndices.Last() == i) {
						split(i + 1);
					} else if (dx > widget.Width) {
						split(i);
						i--;
					} else if (i + 1 == widgets.Count) {
						split(i + 1);
					} else {
						dx += Spacing;
					}
					i++;
				}
				widget.MeasuredMinSize = new Vector2(minWidth, dy);
				widget.MeasuredMaxSize = new Vector2(float.PositiveInfinity, dy);
			} else {
				ConstraintsValid = true;
				var widgets = GetChildren(widget);
				float paddingV = widget.Padding.Top + widget.Padding.Bottom;
				float dx = widget.Padding.Left + widget.Padding.Right - Spacing;
				float dy = paddingV;
				float maxColumnDx = 0;
				int i = 0;
				Action<int> split = (splitIndex) => {
					splitIndices.Add(splitIndex);
					dx += maxColumnDx + Spacing;
					dy = paddingV;
					maxColumnDx = 0.0f;
				};
				splitIndices.Clear();
				splitIndices.Add(i);
				float minHeight = 0;
				while (i < widgets.Count) {
					var w = widgets[i];
					minHeight = Math.Max(minHeight, w.EffectiveMinSize.Y);
					dy += w.EffectiveMinSize.Y;
					if (dy <= widget.Height) {
						maxColumnDx = Mathf.Max(maxColumnDx, w.EffectiveMinSize.X);
					}
					if (w.EffectiveMinSize.Y + paddingV > widget.Height && splitIndices.Last() == i) {
						split(i + 1);
					} else if (dy > widget.Height) {
						split(i);
						i--;
					} else if (i + 1 == widgets.Count) {
						split(i + 1);
					} else {
						dy += Spacing;
					}
					i++;
				}
				widget.MeasuredMinSize = new Vector2(dx, minHeight);
				widget.MeasuredMaxSize = new Vector2(dx, float.PositiveInfinity);
			}
		}
	}
}
