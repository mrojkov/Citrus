using System;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations.CompoundAnimations
{
	public static class DeleteSelectedAnimationClips
	{
		public static bool IsEnabled()
		{
			foreach (var track in Document.Current.Animation.Tracks) {
				foreach (var clip in track.Clips) {
					if (clip.IsSelected) {
						return true;
					}
				}
			}
			return false;
		}

		public static void Perform()
		{
			Document.Current.History.DoTransaction(() => {
				foreach (var track in Document.Current.Animation.Tracks) {
					foreach (var clip in track.Clips.ToList()) {
						if (clip.IsSelected) {
							AnimationClipToolbox.RemoveClip(track, clip);
						}
					}
				}
			});
		}
	}
}
