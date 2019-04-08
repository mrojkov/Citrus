using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.Core.Operations;

namespace Tangerine.UI.Timeline.Operations.CompoundAnimations
{
	public static class DragAnimationClips
	{
		public static void Perform(IntVector2 offset, bool removeOriginals)
		{
			var processedKeys = new HashSet<IKeyframe>();
			var operations = new List<Action>();
			foreach (var row in Document.Current.Rows) {
				var track = row.Components.Get<AnimationTrackRow>()?.Track;
				if (track?.EditorState().Locked != false) {
					continue;
				}
				var clips = track.Clips.Where(i => i.IsSelected).ToList();
				foreach (var clip in clips) {
					if (removeOriginals) {
						AnimationClipToolbox.RemoveClip(track, clip);
					} else {
						SetProperty.Perform(clip, nameof(AnimationClip.IsSelected), false);
					}
				}
				foreach (var clip in clips) {
					var newClip = clip.Clone();
					newClip.BeginFrame += offset.X;
					newClip.EndFrame += offset.X;
					newClip.IsSelected = true;
					AnimationClipToolbox.InsertClip(track, newClip);
				}
			}
		}
	}
}
