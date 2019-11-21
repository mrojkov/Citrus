using System;

namespace Lime
{
	public class ClickGesture : TapGesture
	{
		public ClickGesture() : this(0, null)
		{
		}

		public ClickGesture(int buttonIndex) : this(buttonIndex, null)
		{
		}

		public ClickGesture(Action onRecognized) : this(0, onRecognized)
		{
		}

		public ClickGesture(int buttonIndex, Action onRecognized)
			: base(onRecognized, GetClickGestureOptions(buttonIndex))
		{
		}

		private static TapGestureOptions GetClickGestureOptions(int buttonIndex)
		{
			return new TapGestureOptions {
				ButtonIndex = buttonIndex,
				MinTapDuration = 0.0f,
				CanRecognizeByTapDuration = false
			};
		}
	}
}
