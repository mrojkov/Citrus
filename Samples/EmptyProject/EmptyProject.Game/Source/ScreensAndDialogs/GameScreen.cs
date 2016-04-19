using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lime;

namespace EmptyProject
{
	public class GameScreen : Dialog
	{
		bool closing;

		public GameScreen()
			: base("Shell/GameScreen")
		{
			The.SoundManager.PlayMusic("Ingame");
			//The.SoundManager.PlayAmbient("");

			Root["BtnExit"].Clicked = BackToMenu;

			OrientationChanged();
			The.Application.OrientationChanged += OrientationChanged;
			Root.Updating += Update;
		}

		void OrientationChanged()
		{
		}

		void Update(float delta)
		{
			if (Root.Input.WasKeyPressed(Key.Escape)) {
				BackToMenu();
			}
		}

		void BackToMenu()
		{
			if (!closing) {
				closing = true;
				new ScreenCrossfade(() => {
					Close();
					new MainMenu();
				});
			}
		}

		public override void Close()
		{
			The.Application.OrientationChanged -= OrientationChanged;
			//The.SoundManager.PlayAmbient(null);
			base.Close();
		}

		protected override bool HandleAndroidBackButton()
		{
			BackToMenu();
			return true;
		}

	}
}
