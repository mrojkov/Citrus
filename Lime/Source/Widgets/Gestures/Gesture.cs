using System;

namespace Lime
{
	public abstract partial class Gesture
	{
		private PollableEvent began;
		private PollableEvent canceled;
		private PollableEvent recognized;

		/// <summary>
		/// Occurs when gesture recognition began.
		/// If the owner has other gestures, the began event
		/// might be deferred to resolve priority of another gestures.
		/// </summary>
		public virtual event Action Began
		{
			add => began.Handler += value;
			remove => began.Handler -= value;
		}

		/// <summary>
		/// Occurs if the gesture has been canceled.
		/// </summary>
		public virtual event Action Canceled
		{
			add => canceled.Handler += value;
			remove => canceled.Handler -= value;
		}

		/// <summary>
		/// Occurs when the gesture is fully recognized.
		/// </summary>
		public virtual event Action Recognized
		{
			add => recognized.Handler += value;
			remove => recognized.Handler -= value;
		}

		public Node Owner { get; internal set; }
		public abstract bool IsActive { get; }

		protected WindowInput Input => CommonWindow.Current.Input;

		public Gesture()
		{
		}

		public Gesture(Action onRecognized)
		{
			if (onRecognized != null) {
				Recognized += onRecognized;
			}
		}

		public virtual bool WasBegan() => began.HasOccurred;
		public virtual bool WasCanceled() => canceled.HasOccurred;
		public virtual bool WasRecognized() => recognized.HasOccurred;
		public bool WasRecognizedOrCanceled() => WasCanceled() || WasRecognized();

		protected void RaiseBegan() => began.Raise();
		protected void RaiseCanceled() => canceled.Raise();
		protected void RaiseRecognized() => recognized.Raise();

		protected internal abstract bool Cancel(Gesture sender);
		protected internal abstract void Update(float delta);
	}
}
