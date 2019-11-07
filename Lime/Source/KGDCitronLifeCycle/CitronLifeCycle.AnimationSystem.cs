using System;

namespace Lime.KGDCitronLifeCycle
{
	public static partial class CitronLifeCycle
	{
		private class AnimationSystem
		{
			private Action pendingStoppedActions;

			internal void OnAnimationStoppedRisen(Action onStopped)
			{
				pendingStoppedActions += onStopped;
			}

			public void ConsumePendingActions()
			{
				pendingStoppedActions?.Invoke();
				pendingStoppedActions = null;
			}
		}
	}
}
