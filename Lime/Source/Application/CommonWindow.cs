using System;

namespace Lime
{
	public abstract class CommonWindow
	{
		public event Action Activated;
		public event Action Deactivated;
		public event Func<bool> Closing;
		public event Action Closed;
		public event Action Moved;
		public event ResizeDelegate Resized;
		public event Action<float> Updating;
		public event Action Rendering;
		public event Action<bool> VisibleChanging;
		public object Tag { get; set; }

		public static IWindow Current { get; private set; }
		public IContext Context { get; set; }

		protected CommonWindow()
		{
			if (Current == null) {
				Current = (IWindow)this;
			}
			Context = new Context(new Property(typeof(CommonWindow), "Current"), this);
		}

		protected void RaiseActivated()
		{
			using (Context.Activate().Scoped()) {
				if (Activated != null) {
					Activated();
				}
			}
		}

		protected void RaiseDeactivated()
		{
			using (Context.Activate().Scoped()) {
				if (Deactivated != null) {
					Deactivated();
				}
			}
		}

		protected void RaiseClosed()
		{
			using (Context.Activate().Scoped()) {
				if (Closed != null) {
					Closed();
				}
			}
		}

		protected void RaiseRendering()
		{
			using (Context.Activate().Scoped()) {
				if (Rendering != null) {
					Rendering();
				}
			}
		}

		protected void RaiseUpdating(float delta)
		{
			using (Context.Activate().Scoped()) {
				if (Updating != null) {
					Updating(delta);
				}
			}
		}

		protected bool RaiseClosing()
		{
			using (Context.Activate().Scoped()) {
				if (Closing != null) {
					return Closing();
				}
			}
			return true;
		}

		protected void RaiseMoved()
		{
			using (Context.Activate().Scoped()) {
				if (Moved != null) {
					Moved();
				}
			}
		}

		protected void RaiseResized(bool deviceRotated)
		{
			using (Context.Activate().Scoped()) {
				if (Resized != null) {
					Resized(deviceRotated);
				}
			}
		}

		protected void RaiseVisibleChanging(bool value)
		{
			using (Context.Activate().Scoped()) {
				if (VisibleChanging != null) {
					VisibleChanging(value);
				}
			}
		}
	}
}
