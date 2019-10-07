using System;
using System.Collections.Generic;
using System.Linq;
using Yuzu;

namespace Lime
{
	public class ColorGradient : List<GradientControlPoint>, IComparer<GradientControlPoint>
	{
		public ColorGradient() { }

		public ColorGradient(Color4 a)
		{
			Add(new GradientControlPoint(a, 0));
			Add(new GradientControlPoint(a, 1));
		}

		public ColorGradient(Color4 a, Color4 b)
		{
			Add(new GradientControlPoint(a, 0));
			Add(new GradientControlPoint(b, 1));
		}

		public GradientControlPoint GetNearestPointTo(float position)
		{
			var i = 0;
			GradientControlPoint previousPoint = null;
			foreach (var controlPoint in Ordered()) {
				if (position <= controlPoint.Position) {
					if (i <= 0) return controlPoint;
					var d1 = controlPoint.Position - position;
					var d2 = position - previousPoint.Position;
					return d1 < d2 ? controlPoint : previousPoint;
				}
				i++;
				previousPoint = controlPoint;
			}
			return previousPoint;
		}

		public IEnumerable<GradientControlPoint> Ordered()
		{
			return this.OrderBy(p => p.Position);
		}

		public void Rasterize(ref Color4[] pixels)
		{
			if (Count == 0) {
				for (int j = 0; j < pixels.Length; j++) {
					pixels[j] = Color4.White;
				}
				return;
			}

			var pixelCount = pixels.Length;
			var i = 0;
			var sorted = Ordered().ToList();
			for (var j = 0; j < Count - 1; j++) {
				var lastPixel = (j + 1 < Count) ? sorted[j + 1].Position * pixelCount : pixelCount;
				while (i < lastPixel) {
					var start = sorted[j].Position * pixelCount;
					var ratio = (i - start) / (lastPixel - start);
					ratio = double.IsInfinity(ratio) ? 0f : ratio.Clamp(0f, 1f);
					pixels[i++] = Color4.Lerp(ratio, sorted[j].Color, sorted[j + 1].Color);
				}
			}

			while (i < pixelCount - 1) {
				pixels[i++] = sorted[Count - 1].Color;
			}
		}

		public ColorGradient Clone()
		{
			var clone = new ColorGradient();
			foreach (var point in this) {
				clone.Add(point.Clone());
			}
			return clone;
		}

		public override int GetHashCode()
		{
			var hasCode = 397;
			foreach (var point in this) {
				hasCode ^= point.GetHashCode();
			}
			return hasCode;
		}

		public int Compare(GradientControlPoint x, GradientControlPoint y)
		{
			return Mathf.Sign(x.Position - y.Position);
		}
	}

	public class GradientControlPoint
	{
		[YuzuMember]
		public Color4 Color { get; set; }

		[YuzuMember]
		public float Position { get; set; }

		public GradientControlPoint(Color4 color, float position)
		{
			Color = color;
			Position = position;
		}

		public GradientControlPoint() { }

		public GradientControlPoint Clone()
		{
			return new GradientControlPoint(Color, Position);
		}

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = -1695330334;
				hashCode = hashCode * -1521134295 + EqualityComparer<Color4>.Default.GetHashCode(Color);
				hashCode = hashCode * -1521134295 + Position.GetHashCode();
				return hashCode;
			}
		}
	}
}
