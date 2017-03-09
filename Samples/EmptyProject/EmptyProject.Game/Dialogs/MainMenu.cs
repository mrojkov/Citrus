using Lime;

namespace EmptyProject.Dialogs
{
	public class MainMenu : Dialog<Scenes.MainMenu>
	{
		public MainMenu()
		{
			SoundManager.PlayMusic("Theme");
			Scene._BtnPlay.It.Clicked = CrossfadeInto<GameScreen>;
			Scene._BtnOptions.It.Clicked = Open<Options>;
		}

		protected override bool HandleAndroidBackButton()
		{
			return false;
		}

		protected override void Update(float delta)
		{
			if (Input.WasKeyPressed(Key.Escape)) {
				Lime.Application.Exit();
			}
		}
	}
}
