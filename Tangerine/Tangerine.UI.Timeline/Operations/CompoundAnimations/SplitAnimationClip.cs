
using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations.CompoundAnimations
{
	public static class SplitAnimationClip
	{
		public static void Perform()
		{
			Document.Current.History.DoTransaction(() => {
				foreach (var track in Document.Current.SelectedAnimationTracks()) {
					int clipIndex = AnimationClipToolbox.FindClipContainingFrame(track, Timeline.Instance.Grid.CellUnderMouse().X);
					if (clipIndex >= 0) {
						AnimationClipToolbox.SplitClip(track, clipIndex, Timeline.Instance.Grid.CellUnderMouse().X);
					}
				}
			});
		}
	}
}
