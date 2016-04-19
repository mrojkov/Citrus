using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lime;

namespace EmptyProject
{
	public class MainMenu : Dialog
	{
		bool closing;

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
#if WIN || MAC
				Application.Instance.Exit();
#endif
			}
			if (WasCheatsHotSpotClicked()) {
				ShowCheatsMenu();
			}
		}

		private void ShowCheatsMenu()
		{
			Lime.PopupMenu.Menu exampleSubMenu = new Lime.PopupMenu.Menu(Widget.MaxLayer, 600);
			exampleSubMenu.Add(new Lime.PopupMenu.StringItem("Example Submenu item 1", () => { }));
			exampleSubMenu.Add(new Lime.PopupMenu.StringItem("Example Submenu item 2", () => { }));

			Lime.PopupMenu.Menu menu = new Lime.PopupMenu.Menu();
			menu.Add(new Lime.PopupMenu.StringItem("Example Submenu", exampleSubMenu));
			menu.Add(new Lime.PopupMenu.StringItem("Reset progress", () => {
				The.ApplicationData.Progress = new GameProgress();
			}));
			Toolbox.AddDebugInfoMenu(menu);
			menu.Show();
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
