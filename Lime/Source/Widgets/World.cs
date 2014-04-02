namespace Lime
{
	public sealed class World : Frame
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

		public static World Instance = new World();

		public Widget GetTopDialog()
		{
			return GetTopDialogHelper(this);
		}

		public Widget GetTopDialogHelper(Node parent)
		{
			foreach (var node in parent.Nodes) {
				var frame = node as Frame;
				if (frame != null && frame.DialogMode && frame.GloballyVisible) {
					return frame;
				}
				var dialog = GetTopDialogHelper(node);
				if (dialog != null) {
					return dialog;
				}
			}
			return null;
		}

		public override void Update(int delta)
		{
			var prevActiveTextWidget = ActiveTextWidget;
			ParticleEmitter.NumberOfUpdatedParticles = 0;
			IsActiveWidgetUpdated = false;
			IsActiveTextWidgetUpdated = false;
			IsTopDialogUpdated = false;
			base.Update(delta);
			if (!IsActiveWidgetUpdated) {
				ActiveWidget = null;
			}
			if (!IsActiveTextWidgetUpdated) {
				ActiveTextWidget = null;
			}
#if iOS
			bool showKeyboard = ActiveTextWidget != null && ActiveTextWidget.Visible;
			Application.Instance.ShowOnscreenKeyboard(showKeyboard, ActiveTextWidget != null ? ActiveTextWidget.Text : "");
			// Handle switching between different text widgets
			if (prevActiveTextWidget != ActiveTextWidget && ActiveTextWidget != null && prevActiveTextWidget != null) {
				Application.Instance.ChangeOnscreenKeyboardText(ActiveTextWidget.Text);
			}
#endif
		}
	}
}
