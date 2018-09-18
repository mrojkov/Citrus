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

		public Action CustomAction { get; set; }

		public Marker() { }

		public Marker(string id, int frame, MarkerAction action, string jumpTo = null)
		{
			this.Id = id;
			this.Frame = frame;
			this.Action = action;
			this.JumpTo = jumpTo;
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
}