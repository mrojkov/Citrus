namespace Lime
{
	public class RootFrame : Frame
	{
		/// <summary>
		/// Widget which holds input focus. Before processing mouse down event you should test whether ActiveWidget == this.
		/// For revoking input focus from button, slider or any other UI control you should nullify ActiveWidget.
		/// </summary>
		public Widget ActiveWidget;
		/// <summary>
		/// On each update cycle active widget must confirm self-existense.
		/// </summary>
		public bool ActiveWidgetUpdated;

		public static RootFrame Instance;

		public RootFrame()
		{
			Instance = this;
		}

		public override void Update(int delta)
		{
			ParticleEmitter.NumberOfUpdatedParticles = 0;
			ActiveWidgetUpdated = false;
			base.Update(delta);
		}

		public override void LateUpdate(int delta)
		{
			base.LateUpdate(delta);
			if (!ActiveWidgetUpdated) {
				ActiveWidget = null;
			}
		}
	}
}
