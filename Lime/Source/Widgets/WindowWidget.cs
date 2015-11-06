using System;

namespace Lime
{
	/// <summary>
	/// Root of the widgets hierarchy.
	/// </summary>
	public class WindowWidget : Widget, IWidgetContext
	{
		/// <summary>
		/// Widget which holds text input focus. Before processing Input.TextInput string 
		/// you should test whether ActiveTextWidget == this. For revoking text input focus from widget 
		/// you should nullify ActiveTextWidget.
		/// </summary>
		public IKeyboardInputProcessor ActiveTextWidget { get; set; }

		/// <summary>
		/// On each update cycle active text widget must set this flag true.
		/// </summary>
		public bool IsActiveTextWidgetUpdated { get; set; }

		float IWidgetContext.DistanceToNodeUnderCursor { get; set; }
		Node IWidgetContext.NodeUnderCursor { get; set; }

		private IWindow window;
		IWindow IWidgetContext.Window { get { return window; } }
		Widget IWidgetContext.Root { get { return this; } }

#if iOS || ANDROID
		private IKeyboardInputProcessor prevActiveTextWidget;
#endif

		public WindowWidget(IWindow window)
		{
			Context = this;
			this.window = window;
		}

		public override void Update(float delta)
		{
			WidgetInput.RemoveInvalidatedCaptures();
			ParticleEmitter.NumberOfUpdatedParticles = 0;
			Context.IsActiveTextWidgetUpdated = false;
			Context.DistanceToNodeUnderCursor = float.MaxValue;
			base.Update(delta);
			if (!Context.IsActiveTextWidgetUpdated) {
				Context.ActiveTextWidget = null;
			}
#if iOS || ANDROID
			if (Application.IsMainThread) {
				bool showKeyboard = Context.ActiveTextWidget != null && Context.ActiveTextWidget.Visible;
				if (prevActiveTextWidget != Context.ActiveTextWidget) {
					Application.SoftKeyboard.Show(showKeyboard, Context.ActiveTextWidget != null ? Context.ActiveTextWidget.Text : "");
				}
#if ANDROID
				if (!Application.SoftKeyboard.Visible) {
					Context.ActiveTextWidget = null;
				}
#endif
				// Handle switching between various text widgets
				if (prevActiveTextWidget != Context.ActiveTextWidget && Context.ActiveTextWidget != null && prevActiveTextWidget != null) {
					Application.SoftKeyboard.ChangeText(Context.ActiveTextWidget.Text);
				}

				prevActiveTextWidget = Context.ActiveTextWidget;
			}
#endif
		}
	}
}
