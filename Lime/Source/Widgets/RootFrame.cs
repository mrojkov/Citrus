namespace Lime
{
	public class RootFrame : Frame
	{
		/// <summary>
		/// Widget which holds mouse input focus. Before processing mouse down event you should test whether ActiveWidget == this.
		/// For revoking input focus from button, slider or any other UI control you should nullify ActiveWidget.
		/// </summary>
		public Widget ActiveWidget;
		/// <summary>
		/// On each update cycle active widget must set this flag true.
		/// </summary>
		public bool ActiveWidgetUpdated;
		/// <summary>
		/// Widget which holds text input focus. Before processing Input.TextInput string you should test whether ActiveTextWidget == this.
		/// For revoking text input focus from widget you should nullify ActiveTextWidget.
		/// </summary>
		public Widget ActiveTextWidget;
		/// <summary>
		/// On each update cycle active text widget must set this flag true.
		/// </summary>
		public bool ActiveTextWidgetUpdated;

		public static RootFrame Instance;

		public RootFrame()
		{
			Instance = this;
		}

		public override void Update(int delta)
		{
			ParticleEmitter.NumberOfUpdatedParticles = 0;
			ActiveWidgetUpdated = false;
			ActiveTextWidgetUpdated = false;
			base.Update(delta);
		}

		public override void LateUpdate(int delta)
		{
			base.LateUpdate(delta);
			if (!ActiveWidgetUpdated) {
				ActiveWidget = null;
			}
			if (!ActiveTextWidgetUpdated) {
				ActiveTextWidget = null;
			}
#if iOS
			bool showKeyboard = ActiveTextWidget != null && ActiveTextWidget.Visible;
			Application.Instance.ShowOnscreenKeyboard(showKeyboard);
#endif
		}
	}
}
