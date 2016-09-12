using Lime;

namespace EmptyProject.Dialogs
{
	public class MainMenu : Dialog
	{
		public MainMenu()
			: base("Shell/MainMenu")
		{
			The.SoundManager.PlayMusic("Theme");

			Root["BtnPlay"].Clicked = BtnPlayClick;
			Root["BtnOptions"].Clicked = () => new Options();

			Root.Updating += Update;
		}

		protected override bool HandleAndroidBackButton()
		{
			return false;
		}

		private void Update(float delta)
		{
			if (Root.Input.WasKeyPressed(Key.Escape)) {
				Lime.Application.Exit();
			}
		}

		private void BtnPlayClick()
		{
			new ScreenCrossfade(() => {
				CloseImmediately();
				new GameScreen();
			});
		}
	}
}
