using System;

namespace Lime
{
	public class AnimationClip
	{
		public int Frame { get; set; }
		public string AnimationId { get; set; }
		public AnimationTrack Owner { get; internal set; }
		public AnimationClip Clone()
		{
			var clone = (AnimationClip)MemberwiseClone();
			clone.Owner = null;
			return clone;
		}
	}
}
