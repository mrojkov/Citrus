using Lime;

namespace EmptyProject.Dialogs
{
	public class MainMenu : Dialog<Scenes.MainMenu>
	{
		public MainMenu()
		{
			SoundManager.PlayMusic("Theme");
			Scene._BtnPlay.It.Clicked = BtnPlayClick;
			Scene._BtnOptions.It.Clicked = () => new Options();
		}

		protected override bool HandleAndroidBackButton()
		{
			return false;
		}

		protected override void Update(float delta)
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
