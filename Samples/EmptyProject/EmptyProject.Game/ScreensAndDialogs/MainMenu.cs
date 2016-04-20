using Lime;

namespace EmptyProject.ScreensAndDialogs
{
	public class MainMenu : Dialog
	{
		private bool closing;

		public MainMenu()
			: base("Shell/MainMenu")
		{
			The.SoundManager.PlayMusic("Theme");

			Root["BtnPlay"].Clicked = BtnPlayClick;
			Root["BtnOptions"].Clicked = () => { new Options(); };

			Root.Updating += Update;
		}

		protected override bool HandleAndroidBackButton()
		{
			return false;
		}

		public override void Close()
		{
			base.Close();
		}

		void Update(float delta)
		{
			if (Root.Input.WasKeyPressed(Key.Escape)) {
				Lime.Application.Exit();
			}
		}

		private void BtnPlayClick()
		{
			if (!closing) {
				closing = true;
				new ScreenCrossfade(() => {
					Close();
					new GameScreen();
				});
			}
		}

	}
}
