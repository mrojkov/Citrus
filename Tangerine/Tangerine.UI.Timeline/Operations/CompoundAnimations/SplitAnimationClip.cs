using System;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations.CompoundAnimations
{
	public static class SplitAnimationClip
	{
		public static bool IsEnabled(IntVector2 cell)
		{
			return TryFindClip(cell, out var track, out var clip);
		}

		public static void Perform(IntVector2 cell)
		{
			if (TryFindClip(cell, out var track, out var clip)) {
				Document.Current.History.DoTransaction(() => {
					AnimationClipToolbox.SplitClip(track, clip, cell.X);
				});
			}
		}

		public static bool TryFindClip(IntVector2 cell, out AnimationTrack track, out AnimationClip clip)
		{
			track = null;
			clip = null;
			if (cell.Y >= Document.Current.Animation.Tracks.Count) {
				return false;
			}
			track = Document.Current.Animation.Tracks[cell.Y];
			return AnimationClipToolbox.TryFindClip(track, cell.X, out clip) && cell.X > clip.BeginFrame;
		}
	}
}
