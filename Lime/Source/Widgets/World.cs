namespace Lime
{
	public class World : Frame
	{
		/// <summary>
		/// Widget which holds mouse input focus. Before processing mouse down event you should test whether ActiveWidget == this.
		/// For revoking input focus from button, slider or any other UI control you should nullify ActiveWidget.
		/// </summary>
		public Widget ActiveWidget;
		/// <summary>
		/// On each update cycle active widget must set this flag true.
		/// </summary>
		public bool IsActiveWidgetUpdated;
		/// <summary>
		/// Widget which holds text input focus. Before processing Input.TextInput string you should test whether ActiveTextWidget == this.
		/// For revoking text input focus from widget you should nullify ActiveTextWidget.
		/// </summary>
		public TextBox ActiveTextWidget;
		/// <summary>
		/// On each update cycle active text widget must set this flag true.
		/// </summary>
		public bool IsActiveTextWidgetUpdated;
		/// <summary>
		/// On each update cycle modal dialog frame must set this flag true.
		/// </summary>
		public bool IsTopDialogUpdated;

		public static World Instance;

		public World()
		{
			Instance = this;
		}

		public override void Update(int delta)
		{
			ParticleEmitter.NumberOfUpdatedParticles = 0;
			IsActiveWidgetUpdated = false;
			IsActiveTextWidgetUpdated = false;
			IsTopDialogUpdated = false;
			base.Update(delta);
		}

		public override void LateUpdate(int delta)
		{
			base.LateUpdate(delta);
			if (!IsActiveWidgetUpdated) {
				ActiveWidget = null;
			}
			if (!IsActiveTextWidgetUpdated) {
				ActiveTextWidget = null;
			}
#if iOS
			bool showKeyboard = ActiveTextWidget != null && ActiveTextWidget.Visible;
			Application.Instance.ShowOnscreenKeyboard(showKeyboard, ActiveTextWidget != null ? ActiveTextWidget.Text : "");
#endif
		}
	}
}
