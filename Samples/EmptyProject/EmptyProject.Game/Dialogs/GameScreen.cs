using Lime;

namespace EmptyProject.Dialogs
{
	public class GameScreen : Dialog
	{
		public GameScreen() : base("Shell/GameScreen")
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
