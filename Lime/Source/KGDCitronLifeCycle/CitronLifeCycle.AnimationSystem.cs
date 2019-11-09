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

			public bool ConsumePendingActions()
			{
				bool hasActions = pendingStoppedActions != null;
				pendingStoppedActions?.Invoke();
				pendingStoppedActions = null;
				return hasActions;
			}
		}
	}
}
