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
			using (Document.Current.History.BeginTransaction()) {
				var index = FindClipContainingFrame(track, newClip.Begin);
				if (index >= 0 && newClip.Begin > track.Clips[index].Begin) {
					SplitClip(track, index, newClip.Begin);
				}
				index = FindClipContainingFrame(track, newClip.End - 1);
				if (index >= 0 && newClip.End < track.Clips[index].End) {
					SplitClip(track, index, newClip.End);
				}
				for (int i = track.Clips.Count - 1; i >= 0; i--) {
					var c = track.Clips[i];
					if (c.Begin >= newClip.Begin && c.Begin < newClip.End) {
						RemoveClip(track, i);
					}
				}
				index = FindClipInsertionIndex(track, newClip.Begin);
				Core.Operations.InsertIntoList<AnimationClipList, AnimationClip>.Perform(track.Clips, index, newClip);
				Document.Current.History.CommitTransaction();
			}
		}

		public static void RemoveClip(AnimationTrack track, AnimationClip clip)
		{
			RemoveClip(track, track.Clips.IndexOf(clip));
		}

		public static void RemoveClip(AnimationTrack track, int index)
		{
			Core.Operations.RemoveFromList<AnimationClipList, AnimationClip>.Perform(track.Clips, index);
		}

		public static void SplitClip(AnimationTrack track, AnimationClip clip, int frame)
		{
			SplitClip(track, track.Clips.IndexOf(clip), frame);
		}

		public static void SplitClip(AnimationTrack track, int index, int frame)
		{
			var clip = track.Clips[index];
			if (frame <= clip.Begin || frame > clip.End) {
				throw new InvalidOperationException();
			}
			var newClip = clip.Clone();
			newClip.Begin = frame;
			newClip.End = clip.End;
			Core.Operations.SetProperty.Perform(clip, nameof(AnimationClip.End), frame);
			Core.Operations.InsertIntoList<AnimationClipList, AnimationClip>.Perform(track.Clips, index + 1, newClip);
		}

		public static int FindClipContainingFrame(AnimationTrack track, int frame)
		{
			int i = 0;
			foreach (var c in track.Clips) {
				if (c.Begin <= frame && frame < c.End) {
					return i;
				}
				i++;
			}
			return -1;
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
