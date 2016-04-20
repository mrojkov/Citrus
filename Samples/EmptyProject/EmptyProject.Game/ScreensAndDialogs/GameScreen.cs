using Lime;

namespace EmptyProject.ScreensAndDialogs
{
	public class GameScreen : Dialog
	{
		private bool closing;

		public GameScreen(): base("Shell/GameScreen")
		{
			The.SoundManager.PlayMusic("Ingame");
			Root["BtnExit"].Clicked = BackToMenu;
			Root.Updating += Update;
		}

		private void Update(float delta)
		{
			if (Root.Input.WasKeyPressed(Key.Escape)) {
				BackToMenu();
			}
		}

		private void BackToMenu()
		{
			if (closing) return;
			closing = true;
			new ScreenCrossfade(() => {
				Close();
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
