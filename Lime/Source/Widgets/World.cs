namespace Lime
{
	public sealed class World : Frame
	{
		/// <summary>
		/// Widget which holds text input focus. Before processing Input.TextInput string you should test whether ActiveTextWidget == this.
		/// For revoking text input focus from widget you should nullify ActiveTextWidget.
		/// </summary>
		public TextBox ActiveTextWidget;
		/// <summary>
		/// On each update cycle active text widget must set this flag true.
		/// </summary>
		public bool IsActiveTextWidgetUpdated;

		public static World Instance = new World();

		private TextBox prevActiveTextWidget;

		protected override void SelfUpdate(float delta)
		{
			WidgetInput.RemoveInvalidatedCaptures();
			prevActiveTextWidget = ActiveTextWidget;
			ParticleEmitter.NumberOfUpdatedParticles = 0;
			IsActiveTextWidgetUpdated = false;
		}

		protected override void SelfLateUpdate(float delta)
		{
			if (!IsActiveTextWidgetUpdated) {
				ActiveTextWidget = null;
			}
#if iOS || ANDROID
			if (Application.IsMainThread) {
				bool showKeyboard = ActiveTextWidget != null && ActiveTextWidget.Visible;
				Application.Instance.ShowOnscreenKeyboard(showKeyboard, ActiveTextWidget != null ? ActiveTextWidget.Text : "");
				// Handle switching between different text widgets
				if (prevActiveTextWidget != ActiveTextWidget && ActiveTextWidget != null && prevActiveTextWidget != null) {
					Application.Instance.ChangeOnscreenKeyboardText(ActiveTextWidget.Text);
				}
			}
#endif
		}
	}
}
