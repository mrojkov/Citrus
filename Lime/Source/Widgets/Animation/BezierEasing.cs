using System;
using Yuzu;

namespace Lime
{
	[YuzuCompact]
	public struct BezierEasing : IEquatable<BezierEasing>
	{
		[YuzuMember]
		public float P1X { get; set; }

		[YuzuMember]
		public float P1Y { get; set; }

		[YuzuMember]
		public float P2X { get; set; }

		[YuzuMember]
		public float P2Y { get; set; }

		public Vector2 P1 { get => new Vector2(P1X, P1Y); set { P1X = value.X; P1Y = value.Y; } }
		public Vector2 P2 { get => new Vector2(P2X, P2Y); set { P2X = value.X; P2Y = value.Y; } }

		public bool IsDefault() => P1X == 0 && P1Y == 0 && P2X == 1 && P2Y == 1;

		public BezierEasing Clone()
		{
			return (BezierEasing)MemberwiseClone();
		}

		public bool Equals(BezierEasing other)
		{
			return P1X == other.P1X && P2X == other.P2X && P1Y == other.P1Y && P2Y == other.P2Y;
		}

		public static readonly BezierEasing Default = new BezierEasing { P1X = 0, P1Y = 0, P2X = 1, P2Y = 1 };

		public float this[int component]
		{
			get {
				if (component == 0) {
					return P1X;
				} else if (component == 1) {
					return P1Y;
				} else if (component == 2) {
					return P2X;
				} else if (component == 3) {
					return P2Y;
				} else {
					throw new IndexOutOfRangeException();
				}
			}
			set {
				if (component == 0) {
					P1X = value;
				} else if (component == 1) {
					P1Y = value;
				} else if (component == 2) {
					P2X = value;
				} else if (component == 3) {
					P2Y = value;
				} else {
					throw new IndexOutOfRangeException();
				}
			}
		}
	}
}
