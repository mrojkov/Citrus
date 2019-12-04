using System;
using System.Threading;

namespace Lime
{
	public abstract class CommonWindow
	{
		public event Action Activated;
		public event Action Deactivated;
		public event ClosingDelegate Closing;
		public event Action Closed;
		public event Action Moved;
		public event ResizeDelegate Resized;
		public event UpdatingDelegate Updating;
		public event Action Rendering;
		public event Action Sync;
		public event SafeAreaInsetsChangedDelegate SafeAreaInsetsChanged;
		public event VisibleChangingDelegate VisibleChanging;
		public object Tag { get; set; }

		public static IWindow Current { get; private set; }
		public IContext Context { get; set; }

		public Rectangle SafeAreaInsets { get; protected set; }

		/// <summary>
		/// Keeps refresh rate the same as monitor's refresh rate.
		/// Setting to false allows to render as much frames as possible.
		/// Works only on Windows with disabled Timer.
		/// </summary>
		public virtual bool VSync { get; set; }

		public event Action<System.Exception> UnhandledExceptionOnUpdate;

		private static readonly object PendingActionsOnRenderingLock = new object();
		private static Action pendingActionsOnRendering;

		[ThreadStatic]
		private static bool isRenderingPhase;

		public bool IsRenderingPhase
		{
			get { return isRenderingPhase; }
		}

		protected CommonWindow()
		{
			if (Current == null) {
				Current = (IWindow)this;
			}
			Context = new Context(Property.Create(() => Current, (v) => Current = v), this);
		}

		protected void RaiseActivated()
		{
			using (Context.Activate().Scoped()) {
				Activated?.Invoke();
			}
		}

		protected void RaiseDeactivated()
		{
			using (Context.Activate().Scoped()) {
				Deactivated?.Invoke();
			}
		}

		protected void RaiseClosed()
		{
			using (Context.Activate().Scoped()) {
				Closed?.Invoke();
			}
		}

		protected void RaiseRendering()
		{
			using (Context.Activate().Scoped()) {
				isRenderingPhase = true;
				try {
					NofityPendingActionsOnRendering();
					Rendering?.Invoke();
				} finally {
					isRenderingPhase = false;
				}
			}
		}

		protected void RaiseUpdating(float delta)
		{
			using (Context.Activate().Scoped()) {
				if (UnhandledExceptionOnUpdate != null) {
					try {
						RaiseUpdatingHelper(delta);
					} catch (System.Exception e) {
						UnhandledExceptionOnUpdate(e);
					}
				} else {
					RaiseUpdatingHelper(delta);
				}
			}
		}

		protected void RaiseSync()
		{
			using (Context.Activate().Scoped()) {
				Sync?.Invoke();
			}
		}

		protected void RaiseSafeAreaInsetsChanged()
		{
			using (Context.Activate().Scoped()) {
				SafeAreaInsetsChanged?.Invoke(SafeAreaInsets);
			}
		}

		private void RaiseUpdatingHelper(float delta)
		{
			if (Current.Active) {
				Command.ResetConsumedCommands();
				CommandQueue.Instance.IssueCommands();
				try {
					Updating?.Invoke(delta);
					CommandHandlerList.Global.ProcessCommands();
				} finally {
					Application.MainMenu?.Refresh();
				}
			} else {
				Updating?.Invoke(delta);
			}
		}

		protected bool RaiseClosing(CloseReason reason)
		{
			using (Context.Activate().Scoped()) {
				if (Closing != null) {
					return Closing(reason);
				}
			}
			return true;
		}

		protected void RaiseMoved()
		{
			using (Context.Activate().Scoped()) {
				Moved?.Invoke();
			}
		}

		protected void RaiseResized(bool deviceRotated)
		{
			using (Context.Activate().Scoped()) {
				Resized?.Invoke(deviceRotated);
			}
		}

		protected void RaiseVisibleChanging(bool value, bool modal)
		{
			using (Context.Activate().Scoped()) {
				VisibleChanging?.Invoke(value, modal);
			}
		}

		public void InvokeOnRendering(Action action)
		{
			if (isRenderingPhase) {
				action?.Invoke();
			} else {
				lock (PendingActionsOnRenderingLock) {
					pendingActionsOnRendering += action;
				}
			}
		}

		private void NofityPendingActionsOnRendering()
		{
			Action usePendingActionsOnRendering;
			lock (PendingActionsOnRenderingLock) {
				usePendingActionsOnRendering = pendingActionsOnRendering;
				pendingActionsOnRendering = null;
			}
			usePendingActionsOnRendering?.Invoke();
		}

	}
}
