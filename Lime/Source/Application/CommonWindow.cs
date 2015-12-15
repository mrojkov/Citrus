using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class CommonWindow
	{
		public event Action Activated;
		public event Action Deactivated;
		public event Func<bool> Closing;
		public event Action Closed;
		public event Action Moved;
		public event Action Resized;
		public event Action<float> Updating;
		public event Action Rendering;
		public Context Context { get; private set; }

		public CommonWindow()
		{
			Context = new Context();
			Context.MakeCurrent();
		}

		protected void RaiseActivated()
		{
			using (Context.MakeCurrent()) {
				if (Activated != null) {
					Activated();
				}
			}
		}

		protected void RaiseDeactivated()
		{
			using (Context.MakeCurrent()) {
				if (Deactivated != null) {
					Deactivated();
				}
			}
		}

		protected void RaiseClosed()
		{
			using (Context.MakeCurrent()) {
				if (Closed != null) {
					Closed();
				}
			}
		}

		protected void RaiseRendering()
		{
			using (Context.MakeCurrent()) {
				if (Rendering != null) {
					Rendering();
				}
			}
		}

		protected void RaiseUpdating(float delta)
		{
			using (Context.MakeCurrent()) {
				if (Updating != null) {
					Updating(delta);
				}
			}
		}

		protected bool RaiseClosing()
		{
			using (Context.MakeCurrent()) {
				if (Closing != null) {
					return Closing();
				}
			}
			return true;
		}

		protected void RaiseMoved()
		{
			using (Context.MakeCurrent()) {
				if (Moved != null) {
					Moved();
				}
			}
		}

		protected void RaiseResized()
		{
			using (Context.MakeCurrent()) {
				if (Resized != null) {
					Resized();
				}
			}
		}
	}
}
