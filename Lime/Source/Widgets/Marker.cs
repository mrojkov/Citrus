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
		private int frame;
		private BezierEasing bezierEasing;

		public Animation Owner { get; internal set; }

		[YuzuMember]
		public string Id { get; set; }

		[YuzuMember]
		public int Frame
		{
			get => frame;
			set {
				if (frame != value) {
					frame = value;
					Owner?.InvalidateCache();
				}
			}
		}

		public double Time { get { return AnimationUtils.FramesToSeconds(Frame); } }

		[YuzuMember]
		public MarkerAction Action { get; set; }

		[YuzuMember]
		public string JumpTo { get; set; }

		[YuzuMember]
		public BezierEasing BezierEasing
		{
			get => bezierEasing;
			set {
				if (!bezierEasing.Equals(value)) {
					bezierEasing = value;
					Owner?.InvalidateCache();
				}
			}
		}

		public Action CustomAction { get; set; }

		public Marker()
		{
			BezierEasing = BezierEasing.Default;
		}

		public Marker(string id, int frame, MarkerAction action, string jumpTo = null)
		{
			this.Id = id;
			this.Frame = frame;
			this.Action = action;
			this.JumpTo = jumpTo;
			this.BezierEasing = BezierEasing.Default;
		}

		public Marker Clone()
		{
			var clone = (Marker)MemberwiseClone();
			clone.Owner = null;
			return clone;
		}

		public override string ToString()
		{
			return string.Format("{1} '{0}'", Id, Action);
		}
	}
}
