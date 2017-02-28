using Lime;

namespace EmptyProject.Dialogs
{
	public class GameScreen : Dialog<Scenes.GameScreen>
	{
		public GameScreen()
		{
			SoundManager.PlayMusic("Ingame");
			Scene._BtnExit.It.Clicked = ReturnToMenu;
		}

		protected override void Update(float delta)
		{
			if (Root.Input.WasKeyPressed(Key.Escape)) {
				ReturnToMenu();
			}
		}

		protected override bool HandleAndroidBackButton()
		{
			ReturnToMenu();
			return true;
		}

		private void ReturnToMenu()
		{
			CrossfadeInto<MainMenu>();
		}
	}
}
