using System;

namespace Lime
{
	public class LongTapGesture : TapGesture
	{
		public LongTapGesture(float tapDuration) : this(tapDuration, 0, null)
		{
		}

		public LongTapGesture(float tapDuration, int buttonIndex)
			: this(tapDuration, buttonIndex, null)
		{
		}

		public LongTapGesture(float tapDuration, Action onRecognized)
			: this(tapDuration, 0, onRecognized)
		{
		}

		public LongTapGesture(float tapDuration, int buttonIndex, Action onRecognized)
			: base(onRecognized, GetLongTapGestureOptions(buttonIndex, tapDuration))
		{
		}

		private static TapGestureOptions GetLongTapGestureOptions(int buttonIndex, float tapDuration)
		{
			return new TapGestureOptions {
				ButtonIndex = buttonIndex,
				MinTapDuration = tapDuration,
				CanRecognizeByTapDuration = true
			};
		}
	}
}
