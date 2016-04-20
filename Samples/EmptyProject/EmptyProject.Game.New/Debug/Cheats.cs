using EmptyProject.Application;
using EmptyProject.ScreensAndDialogs;
using Lime;

namespace EmptyProject.Debug
{
	public static class Cheats
	{
		private static Input Input
		{
			get { return The.Window.Input; }
		}

		public static bool Enabled
		{
			get { return true; }
		}

		public static bool IsDebugInfoVisible;

		private static RainbowDash.Menu currentMenu;

		public static void Initialize()
		{
			//IsDebugInfoVisible = ExecutionFlag.DebugInfo.IsSet();
		}

		public static void ProcessCheatKeys()
		{
			if (Enabled) {
				if (Input.WasKeyPressed(Key.F1) || Input.WasTouchBegan(3)) {
					ShowMenu();
				}
#if WIN
				if (Input.WasKeyPressed(Key.F10)) {
					TexturePool.Instance.DiscardAllTextures();
				}
				if (Input.WasKeyPressed(Key.F11)) {
					DisplayInfo.SetNextDisplay();
				}
				if (Input.WasKeyPressed(Key.F12)) {
					DisplayInfo.SetNextDeviceOrientation();
				}
#endif
			}
		}

		public static bool WasKeyPressed(Key key)
		{
			return Enabled && Input.WasKeyPressed(key);
		}

		public static bool IsKeyPressed(Key key)
		{
			return Enabled && Input.IsKeyPressed(key);
		}

		public static bool WasTripleTouch()
		{
			var isTouchJustStarted = Input.WasTouchBegan(0) || Input.WasTouchBegan(1) || Input.WasTouchBegan(2);
			return Enabled && IsTripleTouch() && isTouchJustStarted;
		}

		public static bool IsTripleTouch()
		{
			return Enabled && Input.IsTouching(0) && Input.IsTouching(1) && Input.IsTouching(2);
		}

		public static void ShowMenu()
		{
			if (currentMenu != null) {
				return;
			}
			var menu = new RainbowDash.Menu(The.World, Layers.CheatsMenu);
			var section = menu.Section();
			Dialog.Top.FillDebugMenuItems(menu);
			menu.Show();
			currentMenu = menu;
			menu.Hidden += () => {
				currentMenu = null;
			};
		}
	}
}