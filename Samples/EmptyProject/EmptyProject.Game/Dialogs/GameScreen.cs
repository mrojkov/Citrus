using Lime;

namespace EmptyProject.Dialogs
{
	public class GameScreen : Dialog<Scenes.GameScreen>
	{
		public GameScreen()
		{
			The.SoundManager.PlayMusic("Ingame");
			Scene._BtnExit.It.Clicked = BackToMenu;
		}

		protected override void Update(float delta)
		{
			if (Root.Input.WasKeyPressed(Key.Escape)) {
				BackToMenu();
			}
		}

		private void BackToMenu()
		{
			new ScreenCrossfade(() => {
				CloseImmediately();
				new MainMenu();
			});
		}

		protected override bool HandleAndroidBackButton()
		{
			BackToMenu();
			return true;
		}
	}
}
