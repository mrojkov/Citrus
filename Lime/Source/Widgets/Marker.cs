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
		private EasingParams easing;

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
					Owner?.OnMarkersChanged();
				}
			}
		}

		public double Time { get { return AnimationUtils.FramesToSeconds(Frame); } }

		[YuzuMember]
		public MarkerAction Action { get; set; }

		[YuzuMember]
		public string JumpTo { get; set; }

		[YuzuMember]
		public EasingParams Easing
		{
			get => easing;
			set {
				if (!easing.Equals(value)) {
					easing = value;
					Owner?.OnMarkersChanged();
				}
			}
		}

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
