using System;

namespace Lime
{
	public abstract partial class Gesture
	{
		protected struct PollableEvent
		{
			private int? occurredOnIteration;

			public event Action Handler;

			private int CurrentIteration => WidgetContext.Current.GestureManager.CurrentIteration;
			public bool HasOccurred => occurredOnIteration == CurrentIteration;

			public void Raise()
			{
				CommonWindow.Current.Invalidate();
				occurredOnIteration = CurrentIteration;
				Handler?.Invoke();
			}
		}
	}
}
