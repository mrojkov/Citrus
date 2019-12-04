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
			if (Input.WasKeyPressed(Key.Escape)) {
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
			var confirmation = new Confirmation("Are you sure?");
			confirmation.OkClicked += CrossfadeInto<MainMenu>;
			Open(confirmation);
		}
	}
}
