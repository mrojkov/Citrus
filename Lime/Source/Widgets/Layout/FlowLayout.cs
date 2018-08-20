using System;
using System.Linq;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	public class FlowLayout : Layout, ILayout
	{
		public enum FlowDirection
		{
			LeftToRight,
			TopToBottom,
		}
		private List<int> splitIndices = new List<int>();

		[YuzuMember]
		public float Spacing
		{
			get => spacing;
			set
			{
				if (spacing != value) {
					spacing = value;
					InvalidateConstraintsAndArrangement();
				}
			}
		}

		private float spacing;

		// TODO: implement for any alignment other than justify or left
		[YuzuMember]
		public HAlignment RowAlignment
		{
			get => rowAlignment;
			set
			{
				if (rowAlignment != value) {
					rowAlignment = value;
					InvalidateConstraintsAndArrangement();
				}
			}
		}

		private HAlignment rowAlignment;

		[YuzuMember]
		public VAlignment ColumnAlignment
		{
			get => columnAlignment;
			set {
				if (columnAlignment != value) {
					columnAlignment = value;
					InvalidateConstraintsAndArrangement();
				}
			}
		}

		private VAlignment columnAlignment;

		protected readonly FlowDirection Direction;

		public FlowLayout(FlowDirection direction = FlowDirection.LeftToRight)
		{
			Direction = direction;
			DebugRectangles = new List<Rectangle>();
		}

		public int RowCount(int columnIndex)
		{
			if (Direction == FlowDirection.LeftToRight) {
				return splitIndices.Count - 1;
			} else if (Direction == FlowDirection.TopToBottom) {
				return splitIndices.Count > 1
					? splitIndices[columnIndex + 1] - splitIndices[columnIndex]
					: splitIndices[columnIndex];
			} else {
				throw new Lime.Exception($"Invalid FlowDirection: {Direction}");
			}
		}

		public int ColumnCount(int rowIndex)
		{
			if (Direction == FlowDirection.LeftToRight) {
				return splitIndices.Count > 1
					? splitIndices[rowIndex + 1] - splitIndices[rowIndex]
					: splitIndices[rowIndex];
			} else if (Direction == FlowDirection.TopToBottom) {
				return splitIndices.Count > 1 ?
					splitIndices[1] - splitIndices[0] :
					splitIndices[0];
			} else {
				throw new Lime.Exception($"Invalid FlowDirection: {Direction}");
			}
		}

		public override void OnSizeChanged(Widget widget, Vector2 sizeDelta)
		{
			InvalidateConstraintsAndArrangement();
		}

		public override void ArrangeChildren()
		{
			if (Direction == FlowDirection.LeftToRight) {
				ArrangementValid = true;
				var widgets = GetChildren();
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
				var availableHeight = Math.Max(0, Owner.ContentHeight - (lines.Length - 1) * Spacing);
				float dy = 0.0f;
				for (int j = 0; j < splitIndices.Count - 1; j++) {
					int i0 = splitIndices[j];
					int i1 = splitIndices[j + 1];
					var constraints = new LinearAllocator.Constraints[i1 - i0];
					var line = lines[j];
					var maxLineHeight = maxLineHeights[j];
					var availableWidth = Math.Max(0, Owner.ContentWidth - (line.Count - 1) * Spacing);
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
					var position = new Vector2(Owner.Padding.Left, Owner.Padding.Top + dy);
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
			} else if (Direction == FlowDirection.TopToBottom) {
				ArrangementValid = true;
				var widgets = GetChildren();
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
				var availableWidth = Math.Max(0, Owner.ContentWidth - (lines.Length - 1) * Spacing);
				float dx = 0.0f;
				for (int j = 0; j < splitIndices.Count - 1; j++) {
					int i0 = splitIndices[j];
					int i1 = splitIndices[j + 1];
					var constraints = new LinearAllocator.Constraints[i1 - i0];
					var line = lines[j];
					var maxLineWidth = maxLineWidths[j];
					var availableHeight = Math.Max(0, Owner.ContentHeight - (line.Count - 1) * Spacing);
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
					var position = new Vector2(Owner.Padding.Left + dx, Owner.Padding.Top);
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
			} else {
				throw new Lime.Exception($"Invalid FlowDirection: {Direction}");
			}
		}

		public override void MeasureSizeConstraints()
		{
			if (Direction == FlowDirection.LeftToRight) {
				ConstraintsValid = true;
				var widgets = GetChildren();
				float paddingH = Owner.Padding.Left + Owner.Padding.Right;
				float dx = paddingH;
				float dy = Owner.Padding.Top + Owner.Padding.Bottom - Spacing;
				float dyInRowMax = 0;
				int i = 0;
				Action<int> split = (splitIndex) => {
					splitIndices.Add(splitIndex);
					dx = paddingH;
					dy += dyInRowMax + Spacing;
					dyInRowMax = 0.0f;
				};
				splitIndices.Clear();
				splitIndices.Add(i);
				float minWidth = 0;
				while (i < widgets.Count) {
					var w = widgets[i];
					minWidth = Math.Max(minWidth, w.EffectiveMinSize.X);
					dx += w.EffectiveMinSize.X;
					if (dx <= Owner.Width) {
						dyInRowMax = Mathf.Max(dyInRowMax, w.EffectiveMinSize.Y);
					}
					if (w.EffectiveMinSize.X + paddingH > Owner.Width && splitIndices.Last() == i) {
						split(i + 1);
					} else if (dx > Owner.Width) {
						split(i);
						i--;
					} else if (i + 1 == widgets.Count) {
						split(i + 1);
					} else {
						dx += Spacing;
					}
					i++;
				}
				Owner.MeasuredMinSize = new Vector2(minWidth, dy);
				Owner.MeasuredMaxSize = new Vector2(float.PositiveInfinity, dy);
			} else if (Direction == FlowDirection.TopToBottom) {
				ConstraintsValid = true;
				var widgets = GetChildren();
				float paddingV = Owner.Padding.Top + Owner.Padding.Bottom;
				float dx = Owner.Padding.Left + Owner.Padding.Right - Spacing;
				float dy = paddingV;
				float dxInColumnMax = 0;
				int i = 0;
				Action<int> split = (splitIndex) => {
					splitIndices.Add(splitIndex);
					dx += dxInColumnMax + Spacing;
					dy = paddingV;
					dxInColumnMax = 0.0f;
				};
				splitIndices.Clear();
				splitIndices.Add(i);
				float minHeight = 0;
				while (i < widgets.Count) {
					var w = widgets[i];
					minHeight = Math.Max(minHeight, w.EffectiveMinSize.Y);
					dy += w.EffectiveMinSize.Y;
					if (dy <= Owner.Height) {
						dxInColumnMax = Mathf.Max(dxInColumnMax, w.EffectiveMinSize.X);
					}
					if (w.EffectiveMinSize.Y + paddingV > Owner.Height && splitIndices.Last() == i) {
						split(i + 1);
					} else if (dy > Owner.Height) {
						split(i);
						i--;
					} else if (i + 1 == widgets.Count) {
						split(i + 1);
					} else {
						dy += Spacing;
					}
					i++;
				}
				Owner.MeasuredMinSize = new Vector2(dx, minHeight);
				Owner.MeasuredMaxSize = new Vector2(dx, float.PositiveInfinity);
			} else {
				throw new Lime.Exception($"Invalid FlowDirection: {Direction}");
			}
		}

		public override NodeComponent Clone()
		{
			var clone = (FlowLayout)base.Clone();
			clone.splitIndices = new List<int>();
			return clone;
		}
	}
}
