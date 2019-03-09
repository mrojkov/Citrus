using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.Core.Operations;
using Tangerine.UI.Timeline.Components;

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
				foreach (var clip in track.Clips.Where(i => i.IsSelected).ToList()) {
					if (removeOriginals) {
						AnimationClipToolbox.RemoveClip(track, clip);
					}
					var newClip = clip.Clone();
					newClip.Begin += offset.X;
					newClip.End += offset.X;
					AnimationClipToolbox.InsertClip(track, newClip);
				}
			}
		}
	}
}
