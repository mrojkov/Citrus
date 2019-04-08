using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;

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
					var clips = track.Clips.Where(i => i.IsSelected).ToList();
					var keys = new List<IKeyframe>();
					if (track.Animators.TryFind(nameof(AnimationTrack.Weight), out var weightAnimator, Document.Current.AnimationId)) {
						keys = weightAnimator.ReadonlyKeys.Where(k => clips.Any(c => c.BeginFrame <= k.Frame && k.Frame <= c.EndFrame)).ToList();
					}
					foreach (var key in keys) {
						RemoveKeyframe.Perform(weightAnimator, key.Frame);
					}
					foreach (var clip in clips) {
						AnimationClipToolbox.RemoveClip(track, clip);
					}
				}
			});
		}
	}
}
