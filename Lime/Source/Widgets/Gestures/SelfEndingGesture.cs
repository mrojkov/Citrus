using System;

namespace Lime
{
	public abstract class SelfEndingGesture : Gesture
	{
		private PollableEvent ended;

		/// <summary>
		/// Occurs when gesture is ended.
		/// </summary>
		public virtual event Action Ended
		{
			add => ended.Handler += value;
			remove => ended.Handler -= value;
		}

		public SelfEndingGesture()
		{
		}

		public SelfEndingGesture(Action onRecognized) : base(onRecognized)
		{
		}

		public virtual bool WasEnded() => ended.HasOccurred;
		protected virtual void RaiseEnded() => ended.Raise();
	}
}
