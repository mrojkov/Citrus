using Lime;

namespace Tangerine.Core
{
	public enum AnimationTrackVisibility
	{
		Default = 0,
		Hidden = 1,
		Shown = 2,
	}

	public class AnimationTrackEditorState
	{
		readonly AnimationTrack track;

		public AnimationTrackVisibility Visibility
		{
			get
			{
				if (track.GetTangerineFlag(TangerineFlags.Shown)) {
					return AnimationTrackVisibility.Shown;
				} else if (track.GetTangerineFlag(TangerineFlags.Hidden)) {
					return AnimationTrackVisibility.Hidden;
				} else {
					return AnimationTrackVisibility.Default;
				}
			}
			set
			{
				track.SetTangerineFlag(TangerineFlags.Shown, value == AnimationTrackVisibility.Shown);
				track.SetTangerineFlag(TangerineFlags.Hidden, value == AnimationTrackVisibility.Hidden);
			}
		}

		public bool Locked { get { return track.GetTangerineFlag(TangerineFlags.Locked); } set { track.SetTangerineFlag(TangerineFlags.Locked, value); } }

		public AnimationTrackEditorState(AnimationTrack track)
		{
			this.track = track;
		}
	}

	public static class AnimationTrackExtensions
	{
		public static AnimationTrackEditorState EditorState(this AnimationTrack track)
		{
			if (track.UserData == null) {
				track.UserData = new AnimationTrackEditorState(track);
			}
			return (AnimationTrackEditorState)track.UserData;
		}
	}
}
