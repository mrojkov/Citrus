using EmptyProject.Debug;
using EmptyProject.ScreensAndDialogs;
using Lime;

namespace EmptyProject.Application
{
	public class Application
	{
		public const string ApplicationName = "EmptyProject";
		private readonly object uiSync = new object();
		public static Vector2 DefaultWorldSize = new Vector2(960, 640);

		public Application()
		{
			CreateWindow();
			AppData.Load();
			AssetsBundle.Instance = CreateAssetsBundle();
			Profile.Instance = new Profile();
			SetWindowSize();
			new MainMenu();
		}

		public static WindowWidget World { get; private set; }

		private AssetsBundle CreateAssetsBundle()
		{
#if ANDROID
			return new PackedAssetsBundle("Assets.Android.Data.Android", "Assets.Android");
#elif iOS
			return new PackedAssetsBundle("Data.iOS");
#else
			return new PackedAssetsBundle("Data.Desktop");
#endif
		}

		private void CreateWindow()
		{
			var options = new WindowOptions {Title = ApplicationName};
			World = new WindowWidget(new Window(options)) {Layer = RenderChain.LayerCount - 1};
			World.Window.Updating += OnUpdateFrame;
			World.Window.Rendering += OnRenderFrame;
			World.Window.Resized += OnResize;
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
				if (Cheats.IsKeyPressed(Key.ShiftLeft) || Cheats.IsTripleTouch()) {
					speedMultiplier = 10.0f;
				}
				if (Cheats.IsKeyPressed(Key.Tilde)) {
					speedMultiplier = 0.1f;
				}
				Lime.Debug.BreakOnButtonClick = The.Window.Input.IsKeyPressed(Key.AltLeft);

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
				Renderer.EndFrame();
			}
		}

		private static void SetupViewportAndProjectionMatrix()
		{
			Renderer.SetOrthogonalProjection(0, 0, The.World.Width, The.World.Height);
			var windowSize = The.Window.ClientSize;
			The.Window.Input.ScreenToWorldTransform = Matrix32.Scaling(The.World.Width/windowSize.X,
				The.World.Height/windowSize.Y);
		}
	}
}