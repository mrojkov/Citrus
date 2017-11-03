using System;

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
		public event VisibleChangingDelegate VisibleChanging;
		public object Tag { get; set; }

		public static IWindow Current { get; private set; }
		public IContext Context { get; set; }

		/// <summary>
		/// Keeps refresh rate the same as monitor's refresh rate.
		/// Setting to false allows to render as much frames as possible.
		/// Works only on Windows with disabled Timer.
		/// </summary>
		public virtual bool VSync { get; set; }

		public event Action<System.Exception> UnhandledExceptionOnUpdate;

		protected CommonWindow()
		{
			if (Current == null) {
				Current = (IWindow)this;
			}
			Context = new Context(new Property(typeof(CommonWindow), nameof(Current)), this);
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
				Rendering?.Invoke();
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
	}
}
