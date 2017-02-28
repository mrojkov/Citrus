using EmptyProject.Debug;
using EmptyProject.Dialogs;
using Lime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EmptyProject.Application
{
	public class Application
	{
		public static Application Instance;

		public const string ApplicationName = "EmptyProject";
		public static readonly Vector2 DefaultWorldSize = new Vector2(1024, 768);

		private readonly object uiSync = new object();
		private static List<string> debugInfoStrings = new List<string>();

		public Application()
		{
			World = CreateWorld();

			AppData.Load();
			AssetBundle.Instance = CreateAssetBundle();
			Profile.Instance = new Profile();

			LoadFonts();
			LoadDictionary();
			SetWindowSize();

			if (AppData.Instance.EnableSplashScreen) {
				The.DialogContext.Open<SplashScreen>();
			}
			else {
				The.DialogContext.Open<MainMenu>();
			}

			Instance = this;
		}

		public WindowWidget World { get; }

		private AssetBundle CreateAssetBundle()
		{
#if ANDROID
			return new PackedAssetBundle("Assets.Android.Data.Android", "Assets.Android");
#elif iOS
			return new PackedAssetBundle("Data.iOS");
#else
			return new PackedAssetBundle("Data.Win");
#endif
		}

		private void LoadFonts()
		{
			FontPool.Instance.AddFont("regular", new DynamicFont("Dynamic/Roboto-Regular.ttf"));
			FontPool.Instance.AddFont("bold", new DynamicFont("Dynamic/Roboto-Bold.ttf"));
			FontPool.Instance.AddFont("italic", new DynamicFont("Dynamic/Roboto-Italic.ttf"));
			FontPool.Instance.AddFont("bolditalic", new DynamicFont("Dynamic/Roboto-BoldItalic.ttf"));
		}

		private void LoadDictionary()
		{
			var fileName = "Dictionary.txt";
#if WIN
			if (File.Exists(fileName)) {
				Localization.Dictionary.Clear();
				using (var stream = new FileStream(fileName, FileMode.Open)) {
					Localization.Dictionary.ReadFromStream(new LocalizationDictionaryTextSerializer(), stream);
				}

				return;
			}
#endif

			if (!AssetBundle.Instance.FileExists(fileName)) {
				return;
			}

			Localization.Dictionary.Clear();
			using (var stream = AssetBundle.Instance.OpenFile(fileName)) {
				Localization.Dictionary.ReadFromStream(new LocalizationDictionaryTextSerializer(), stream);
			}
		}

		private WindowWidget CreateWorld()
		{
			var options = new WindowOptions { Title = ApplicationName };
			var world = new WindowWidget(new Window(options)) { Layer = RenderChain.LayerCount - 1 };
			world.Window.Updating += OnUpdateFrame;
			world.Window.Rendering += OnRenderFrame;
			world.Window.Resized += OnResize;
			return world;
		}

		private static void SetWindowSize()
		{
#if WIN
			The.Window.ClientSize = DisplayInfo.GetResolution();
#endif
			DisplayInfo.HandleOrientationOrResolutionChange();
		}

		private void OnUpdateFrame(float delta)
		{
			lock (uiSync) {
				Cheats.ProcessCheatKeys();
				var speedMultiplier = 1.0f;
				if (Cheats.IsKeyPressed(Key.Shift) || Cheats.IsTripleTouch()) {
					speedMultiplier = 10.0f;
				}
				if (Cheats.IsKeyPressed(Key.Tilde)) {
					speedMultiplier = 0.1f;
				}
				Lime.Debug.BreakOnButtonClick = The.Window.Input.IsKeyPressed(Key.Alt);

				delta *= speedMultiplier;
				The.World.Update(delta);
			}
		}

		private void OnResize(bool isDeviceRotated)
		{
			lock (uiSync) {
				DisplayInfo.HandleOrientationOrResolutionChange();
			}
		}

		private void OnRenderFrame()
		{
			lock (uiSync) {
				Renderer.BeginFrame();
				SetupViewportAndProjectionMatrix();
				World.RenderAll();
				RenderInfo();
				Renderer.EndFrame();
			}
		}

		private static void SetupViewportAndProjectionMatrix()
		{
			Renderer.SetOrthogonalProjection(0, 0, The.World.Width, The.World.Height);
			var windowSize = The.Window.ClientSize;
			The.Window.Input.ScreenToWorldTransform = Matrix32.Scaling(The.World.Width / windowSize.X,
				The.World.Height / windowSize.Y);
		}

		public static void RenderDebugInfo(string info)
		{
			debugInfoStrings.Add(info);
		}

		protected void RenderInfo()
		{
			if (!Cheats.IsDebugInfoVisible) {
				return;
			}

			Renderer.Transform1 = Matrix32.Identity;
			Renderer.Blending = Blending.Alpha;
			Renderer.Shader = ShaderId.Diffuse;
			IFont font = FontPool.Instance[null];
			float height = 25.0f * World.Scale.X;

			float x = 5;
			float y = 0;

			var fields = new string[] {
				String.Format("FPS: {0}", The.Window.FPS),
				String.Format("Window Size: {0}", The.Window.ClientSize),
				String.Format("World Size: {0}", The.World.Size)
			};

			var text = String.Join("\n", fields.Concat(debugInfoStrings));

			Renderer.DrawTextLine(font, new Vector2(x + 1, y + 1), text, height, new Color4(0, 0, 0, 255)); // shadow
			Renderer.DrawTextLine(font, new Vector2(x, y), text, height, new Color4(255, 255, 255, 255));

			debugInfoStrings.Clear();
		}
	}
}