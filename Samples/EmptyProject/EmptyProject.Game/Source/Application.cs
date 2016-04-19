
using System.Collections.Generic;
using Lime;
using System;
using System.IO;
using ProtoBuf;
using System.Diagnostics;
using System.Xml;

#if iOS
using Foundation;
using UIKit;
#endif

namespace EmptyProject
{
	public enum DebugInfoMode
	{
		Off,
		FPS_Only
	}

	public class Application : Lime.Application
	{
		public static string AppTitle = "Empty Project"; // смените на что-нибудь своё, это используется как название папки для сохранёнки

		TaskList tasks = new TaskList();

		new public static Application Instance;

		ApplicationData data;

		public event Action OrientationChanged;

		private RenderChain renderChain = new RenderChain();

		public ResolutionChanger resolutionChanger;
		private readonly Vector2 worldSize = new Vector2(1024, 768);

		private Stopwatch stopwatch;
		private long lastMillisecondsCount = 0;

		public DebugInfoMode DebugInfoMode = DebugInfoMode.Off;

		public bool IsPortraitOrientation { get { return resolutionChanger.IsPortraitOrientation; } }

		public event Action BeforeUpdateOnce;

		public ApplicationData Data { get { return data; } }

		public Application (string[] commandLineArgs, StartupOptions options) : base(options)
		{
			Instance = this;

			Button.ButtonEffectiveRadius = 0;
			Button.TabletControlScheme = true;

			this.Activated += OnActivated;

			stopwatch = new Stopwatch();
			stopwatch.Start();
			resolutionChanger = new ResolutionChanger(worldSize);
		}

		public override void OnCreate()
		{
			Logger.Write("--- Application.OnCreate enter ---");

			//SupportedDeviceOrientations = DeviceOrientation.LandscapeLeft | DeviceOrientation.LandscapeRight;

			data = new ApplicationData();

#if WIN
			WindowSize = resolutionChanger.Current;
#endif

#if iOS
			AssetsBundle.Instance = new PackedAssetsBundle("Data.iOS", Lime.AssetBundleFlags.None);
#elif ANDROID
			AssetsBundle.Instance = new PackedAssetsBundle("Assets.Data.Android", "Assets");
#else
			// In rare cases, it looks for 'Data.Desktop' in C:\Windows\System32
			var exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
			string currentDir = Path.GetDirectoryName(exePath);
			string dataFileName = Path.Combine(currentDir, "Data.Desktop");
			AssetsBundle.Instance = new PackedAssetsBundle(dataFileName, Lime.AssetBundleFlags.None);
#endif

			The.World.IsRunning = true;
			The.World.Size = worldSize;
			The.World.Updated += tasks.Update;
			FullScreen = false;
#if !iOS && !ANDROID
			resolutionChanger.SetCurrentPresetResolution();
#endif
			OnDeviceRotate();

			DetectCurrentLang();
			LoadDictionary("Dictionary.txt");
			tasks.Add(MainTask());

			Logger.Write("--- Application.OnCreate exit ---");
		}

		private void DetectCurrentLang()
		{
			if (!string.IsNullOrEmpty(AssetsBundle.CurrentLanguage)) {
				return;
			}

			string lang = Localization.GetCurrentLanguage();
			if (lang == "en")
				AssetsBundle.CurrentLanguage = null;
			else if (lang == "fr")
				AssetsBundle.CurrentLanguage = "FR";
			else if (lang == "de")
				AssetsBundle.CurrentLanguage = "DE";
			else if (lang == "pt")
				AssetsBundle.CurrentLanguage = "BR"; // Portuguese
			else if (lang == "es")
				AssetsBundle.CurrentLanguage = "ES";
			else if (lang == "it")
				AssetsBundle.CurrentLanguage = "IT";
			else if (lang == "ja")
				AssetsBundle.CurrentLanguage = "JP";
			// Помимо русского, русский словарь также включается для украинского, белорусского, казахского, и киргизского языков.
			else if (lang == "ru" || lang == "uk" || lang == "be" || lang == "kk" || lang == "ky")
				AssetsBundle.CurrentLanguage = "RU";
			else if (lang == "nl")
				AssetsBundle.CurrentLanguage = "NL";
			else if (lang == "ko")
				AssetsBundle.CurrentLanguage = "KR";
			else if (lang == "zh" || lang == "zh-Hans") // zh-Hans - simplified Chinese, zh-Hant - traditional Chinese
				AssetsBundle.CurrentLanguage = "CN";

			//Localization.UseNumericKeys = true;
		}

		IEnumerator<object> MainTask()
		{
			yield return Splash.Show();
			new MainMenu();
			yield break;
		}

		public override void OnTerminate()
		{
		}

		private void OnActivated()
		{
		}

		public override void OnDeviceRotate()
		{
			resolutionChanger.IsPortraitOrientation = CurrentDeviceOrientation == DeviceOrientation.Portrait || CurrentDeviceOrientation == DeviceOrientation.PortraitUpsideDown;
			Console.WriteLine("ROTATED " + (resolutionChanger.IsPortraitOrientation ? "PORTRAIT" : "LANDSCAPE"));
			Console.WriteLine("RESOLUTION " + (resolutionChanger.Current.ToString()));
			OrientationChanged.SafeInvoke();
		}

		public override void OnUpdateFrame(float delta)
		{
			Lime.Debug.BreakOnButtonClick = Input.IsKeyPressed(Key.ControlLeft);
			long millisecondsCount = stopwatch.ElapsedMilliseconds;
			int myDelta = (int)(millisecondsCount - lastMillisecondsCount);
			myDelta = myDelta.Clamp(0, 40);
			lastMillisecondsCount = millisecondsCount;

#if !iOS && !ANDROID
			//if (Input.WasKeyPressed(Key.Escape)) {
			//	Exit();
			//}
			if (Input.WasKeyPressed(Key.F11)) {
				resolutionChanger.SwitchToNextPresetResolution();
				OrientationChanged.SafeInvoke();
			}
			if ((Input.IsKeyPressed(Key.RAlt) || Input.IsKeyPressed(Key.LAlt)) && Input.WasKeyPressed(Key.Enter)) {
				resolutionChanger.ToggleFullScreen();
			}
			if (Input.WasKeyPressed(Key.F12)) {
				resolutionChanger.IsPortraitOrientation = !resolutionChanger.IsPortraitOrientation;
				OrientationChanged.SafeInvoke();
			}
			if (Input.WasKeyPressed(Key.F10)) {
				Lime.TexturePool.Instance.DiscardTexturesUnderPressure();
			}
			if (Input.IsKeyPressed(Key.Tilde)) {
				myDelta = 3;
			} else if (Input.IsKeyPressed(Key.LShift)) {
				myDelta *= 10;
			}
#endif
			if (BeforeUpdateOnce != null) {
				Action ev = BeforeUpdateOnce;
				BeforeUpdateOnce = null;
				ev();
			}

			resolutionChanger.UpdateWorldSize();

			The.World.Update(myDelta * 0.001f);
		}
			
		public override void OnRenderFrame()
		{
			resolutionChanger.OnRenderFrame();

			Renderer.BeginFrame();

			The.World.AddToRenderChain(renderChain);
			renderChain.RenderAndClear();
			RenderInfo();
			Renderer.EndFrame();
		}

		private void RenderInfo()
		{
			if (DebugInfoMode != DebugInfoMode.Off) {
				Renderer.Transform1 = Matrix32.Identity;
				Renderer.Blending = Blending.Alpha;
				Renderer.Shader = ShaderId.Diffuse;
				Font font = FontPool.Instance[null];
				float height = 25 / resolutionChanger.WorldScale;
				float pos = 0;

				RenderInfoString(font, pos, height, string.Format("FPS {0}", (int)FrameRate));
				pos += height;
			}
		}

		void RenderInfoString(Font font, float pos, float height, string text)
		{
			Vector2 size = Renderer.MeasureTextLine(font, text, height);
			Toolbox.DrawQuad(new Vector2(0, pos), size + new Vector2(8, 0), new Color4(0, 0, 0, 210));
			Renderer.DrawTextLine(font, new Vector2(3, pos), text, height, Color4.White);
		}

		private void LoadDictionary(string baseName = "Dictionary")
		{
			using (var stream = AssetsBundle.Instance.OpenFileLocalized(baseName)) {
				Localization.Dictionary.Clear();
				Localization.Dictionary.ReadFromStream(stream);
			}
		}

	}
}