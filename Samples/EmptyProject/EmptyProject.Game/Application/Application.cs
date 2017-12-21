using EmptyProject.Debug;
using EmptyProject.Dialogs;
using Lime;
using System.IO;

namespace EmptyProject.Application
{
	public class Application
	{
		public static Application Instance;

		public const string ApplicationName = "EmptyProject";
		public static readonly Vector2 DefaultWorldSize = new Vector2(1024, 768);

		private readonly object uiSync = new object();

		public static void Initialize()
		{
			Instance = new Application();
			Instance.Load();
		}

		private void Load()
		{
			World = CreateWorld();

			AppData.Load();
			AssetBundle.Current = CreateAssetBundle();
			Profile.Instance = new Profile();

			LoadFonts();
			LoadDictionary();
			SetWindowSize();

			if (AppData.Instance.EnableSplashScreen) {
				The.DialogManager.Open<SplashScreen>();
			}
			else {
				The.DialogManager.Open<MainMenu>();
			}
		}

		public WindowWidget World { get; private set; }

		private AssetBundle CreateAssetBundle()
		{
#if ANDROID
			return new PackedAssetBundle("Assets.Android.Data.Android", "Assets.Android");
#elif iOS
			return new PackedAssetBundle("Data.iOS");
#elif WIN
			return new PackedAssetBundle("Data.Win");
#elif MAC
			return new PackedAssetBundle("Data.Mac");
#endif
		}

		private static void LoadFonts()
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

			if (!AssetBundle.Current.FileExists(fileName)) {
				return;
			}

			Localization.Dictionary.Clear();
			using (var stream = AssetBundle.Current.OpenFile(fileName)) {
				Localization.Dictionary.ReadFromStream(new LocalizationDictionaryTextSerializer(), stream);
			}
		}

		private WindowWidget CreateWorld()
		{
			var options = new WindowOptions { Title = ApplicationName };
			var window = new Window(options);
			window.Updating += OnUpdateFrame;
			window.Rendering += OnRenderFrame;
			window.Resized += OnResize;
			var world = new WindowWidget(window) { Layer = RenderChain.LayerCount - 1 };
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
				Cheats.RenderDebugInfo();
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
	}
}