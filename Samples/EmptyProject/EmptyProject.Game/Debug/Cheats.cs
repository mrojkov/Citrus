using System.Collections.Generic;
using System.Linq;
using EmptyProject.Application;
using Lime;

namespace EmptyProject.Debug
{
	public static class Cheats
	{
		private static readonly List<string> debugInfoStrings = new List<string>();
		public static bool Enabled { get; set; }
		public static bool IsDebugInfoVisible { get; set; }

		private static Input Input => The.Window.Input;
		private static RainbowDash.Menu currentMenu;

		static Cheats()
		{
#if DEBUG
			Enabled = true;
			IsDebugInfoVisible = true;
#endif
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

			InitialFill(menu);
			The.DialogManager.Top.FillDebugMenuItems(menu);

			menu.Show();
			currentMenu = menu;
			menu.Hidden += () => {
				currentMenu = null;
			};
		}

		private static void InitialFill(RainbowDash.Menu menu)
		{
			var debugSection = menu.Section("Debug");

			debugSection.Item("Toggle Debug Info", () =>
				IsDebugInfoVisible = !IsDebugInfoVisible
			);

			debugSection.Item("Enable Splash Screen", () => {
				The.AppData.EnableSplashScreen = true;
				The.AppData.Save();
			}, () => !The.AppData.EnableSplashScreen);

			debugSection.Item("Disable Splash Screen", () => {
				The.AppData.EnableSplashScreen = false;
				The.AppData.Save();
			}, () => The.AppData.EnableSplashScreen);
		}

		public static void AddDebugInfo(string info)
		{
			debugInfoStrings.Add(info);
		}

		public static void RenderDebugInfo()
		{
			if (!IsDebugInfoVisible)
			{
				return;
			}

			Renderer.Transform1 = Matrix32.Identity;
			Renderer.Blending = Blending.Alpha;
			Renderer.Shader = ShaderId.Diffuse;
			IFont font = FontPool.Instance[null];
			float height = 25.0f * The.World.Scale.X;

			float x = 5;
			float y = 0;

			var fields = new[] {
				$"FPS: {The.Window.FPS}",
				$"Window Size: {The.Window.ClientSize}",
				$"World Size: {The.World.Size}"
			};

			var text = string.Join("\n", fields.Concat(debugInfoStrings));
			
			Renderer.DrawTextLine(font, new Vector2(x + 1, y + 1), text, height, new Color4(0, 0, 0), 0); // shadow
			Renderer.DrawTextLine(font, new Vector2(x, y), text, height, new Color4(255, 255, 255), 0);

			debugInfoStrings.Clear();
		}
	}
}