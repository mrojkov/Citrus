using System;
using Yuzu;

namespace Lime
{
	public enum MarkerAction
	{
		Play,
		Stop,
		Jump
	}

	public class Marker
	{
		[YuzuMember]
		public string Id { get; set; }

		[YuzuMember]
		public int Frame { get; set; }

		public double Time { get { return AnimationUtils.FramesToSeconds(Frame); } }

		[YuzuMember]
		public MarkerAction Action { get; set; }

		[YuzuMember]
		public string JumpTo { get; set; }

		[YuzuMember]
		public EasingParams Easing { get; set; }

		public Action CustomAction { get; set; }

		public Marker()
		{
			Easing = EasingParams.Default;
		}

		public Marker(string id, int frame, MarkerAction action, string jumpTo = null)
		{
			this.Id = id;
			this.Frame = frame;
			this.Action = action;
			this.JumpTo = jumpTo;
			this.Easing = EasingParams.Default;
		}

		public Marker Clone()
		{
			return (Marker)MemberwiseClone();
		}

		public override string ToString()
		{
			return string.Format("{1} '{0}'", Id, Action);
		}
	}

	[YuzuCompact]
	public struct EasingParams
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

		public EasingParams Clone()
		{
			return (EasingParams)MemberwiseClone();
		}

		public static readonly EasingParams Default = new EasingParams { P1X = 0, P1Y = 0, P2X = 1, P2Y = 1 };

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
