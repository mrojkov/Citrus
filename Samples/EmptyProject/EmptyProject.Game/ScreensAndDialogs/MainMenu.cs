using Lime;

namespace EmptyProject.ScreensAndDialogs
{
	public class MainMenu : Dialog
	{
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

		private void Update(float delta)
		{
			if (Root.Input.WasKeyPressed(Key.Escape)) {
				Lime.Application.Exit();
			}
		}

		private void BtnPlayClick()
		{
			if (State != DialogState.Shown) return;
			State = DialogState.Closing;
			new ScreenCrossfade(() => {
				Close();
				new GameScreen();
			});
		}

		public override void FillDebugMenuItems(RainbowDash.Menu menu)
		{
			var section = menu.Section("Cheats example");
			section.Item("Red", () => Root.Color = Color4.Red);
			section.Item("Green", () => Root.Color = Color4.Green);
			section.Item("Blue", () => Root.Color = Color4.Blue);
			var section2 = menu.Section("Cheats example 2");
			section2.Item("Sample cheat", () => { });
		}
	}
}
