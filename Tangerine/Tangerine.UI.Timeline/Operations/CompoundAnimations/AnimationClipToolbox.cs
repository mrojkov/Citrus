using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations.CompoundAnimations
{
	public static class AnimationClipToolbox
	{
		public static void InsertClip(AnimationTrack track, AnimationClip newClip)
		{
			AnimationClip clip;
			using (Document.Current.History.BeginTransaction()) {
				if (TryFindClip(track, newClip.Begin, out clip)) {
					if (newClip.Begin > clip.Begin) {
						SplitClip(track, clip, newClip.Begin);
					}
				}
				if (TryFindClip(track, newClip.End - 1, out clip)) {
					if (newClip.End < clip.End) {
						SplitClip(track, clip, newClip.End);
					}
				}
				for (int i = track.Clips.Count - 1; i >= 0; i--) {
					var c = track.Clips[i];
					if (c.Begin >= newClip.Begin && c.Begin < newClip.End) {
						RemoveClip(track, c);
					}
				}
				var index = FindClipInsertionIndex(track, newClip.Begin);
				Core.Operations.InsertIntoList<AnimationClipList, AnimationClip>.Perform(track.Clips, index, newClip);
				Document.Current.History.CommitTransaction();
			}
		}

		public static void RemoveClip(AnimationTrack track, AnimationClip clip)
		{
			Core.Operations.RemoveFromList<AnimationClipList, AnimationClip>.Perform(track.Clips, track.Clips.IndexOf(clip));
		}

		public static void SplitClip(AnimationTrack track, AnimationClip clip, int frame)
		{
			if (frame <= clip.Begin || frame > clip.End) {
				throw new InvalidOperationException();
			}
			var index = track.Clips.IndexOf(clip);
			var newClip = clip.Clone();
			newClip.Begin = frame;
			newClip.End = clip.End;
			Core.Operations.SetProperty.Perform(clip, nameof(AnimationClip.End), frame);
			Core.Operations.InsertIntoList<AnimationClipList, AnimationClip>.Perform(track.Clips, index + 1, newClip);
		}

		public static bool TryFindClip(AnimationTrack track, int frame, out AnimationClip clip)
		{
			if (TryFindClipIndex(track, frame, out int index)) {
				clip = track.Clips[index];
				return true;
			}
			clip = null;
			return false;
		}

		public static bool TryFindClipIndex(AnimationTrack track, int frame, out int clipIndex)
		{
			clipIndex = -1;
			int i = 0;
			foreach (var c in track.Clips) {
				if (c.Begin <= frame && frame < c.End) {
					clipIndex = i;
					return true;
				}
				i++;
			}
			return false;
		}

		private static int FindClipInsertionIndex(AnimationTrack track, int frame)
		{
			int i = 0;
			foreach (var c in track.Clips) {
				if (c.Begin > frame) {
					break;
				}
				i++;
			}
			return i;
		}
	}
}
