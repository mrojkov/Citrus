using System.Collections.Generic;
using Lime;
using Yuzu;

namespace Tangerine.Core
{
	public class Ruler
	{
		[YuzuRequired]
		public List<RulerLine> Lines { get; } = new List<RulerLine>();

		[YuzuRequired]
		public string Name { get; set; }

		[YuzuMember]
		public bool AnchorToRoot { get; set; }

		public readonly ComponentCollection<Component> Components = new ComponentCollection<Component>();
	}

	public class RulerLine
	{
		[YuzuRequired]
		public float Value { get; set; }

		[YuzuRequired]
		public RulerOrientation RulerOrientation { get; set; }

		public RulerLine() { }

		public RulerLine(float value = 0, RulerOrientation rulerOrientation = RulerOrientation.Horizontal)
		{
			RulerOrientation = rulerOrientation;
			Value = value;
		}

		public RulerLine(Vector2 value, RulerOrientation rulerOrientation = RulerOrientation.Horizontal)
		{
			RulerOrientation = rulerOrientation;
			MakePassingThroughPoint(value);
		}

		public Vector2 GetClosestPointToOrigin()
		{
			return RulerOrientation == RulerOrientation.Vertical ? new Vector2(Value, 0) : new Vector2(0, Value);
		}

		public void MakePassingThroughPoint(Vector2 vector)
		{
			Value = RulerOrientation == RulerOrientation.Vertical ? vector.X : vector.Y;
		}
	}

	public enum RulerOrientation
	{
		Horizontal,
		Vertical
	}

	public static class RulerOrientationExtensions
	{
		public static Vector2 GetDirection(this RulerOrientation orientation)
		{
			return orientation == RulerOrientation.Horizontal ? Vector2.Down : Vector2.Right;
		}

		public static float GetComponentFor(this RulerOrientation orientation, Vector2 value)
		{
			return orientation == RulerOrientation.Horizontal ? value.Y : value.X;
		}
	}
}
