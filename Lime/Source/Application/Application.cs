using System;

#if iOS
using MonoTouch.UIKit;
#elif MAC
using MonoMac.AppKit;
#endif

namespace Lime
{
	[Flags]
	public enum DeviceOrientation
	{
		Portrait = 1,
		PortraitUpsideDown = 2,
		LandscapeLeft = 4,
		LandscapeRight = 8,
	}

	public enum PlatformId
	{
		iOS,
		Mac,
		Win
	}

	public class Application
	{
		public static Application Instance;
		// FrameSync защищает код Update, Render
		// Используй этот объект для синхронизации коллбэков (от appstore, gamecenter)
		public static readonly object FrameSync = new object();

		public Application()
		{
			Instance = this;
			// Use '.' as decimal separator.
			var culture = System.Globalization.CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentCulture = culture;
			SetGlobalExceptionHandler();
		}

		private void SetGlobalExceptionHandler()
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
				Console.WriteLine(e.ExceptionObject.ToString());
#if WIN
				var title = "The application";
				if (GameView.Instance != null) {
					GameView.Instance.FullScreen = false;
					title = GameView.Instance.Title;
				}
				WinApi.MessageBox(IntPtr.Zero, e.ExceptionObject.ToString(), 
					string.Format("{0} has terminated with an error", title), 0);
#endif
			};
		}

		public static string[] GetCommandLineArgs()
		{
			return System.Environment.GetCommandLineArgs();
		}

		public static bool CheckCommandLineArg(string name)
		{
			if (Array.IndexOf(GetCommandLineArgs(), name) >= 0) {
				return true;
			}
			return false;
		}

		public PlatformId Platform {
			get
			{
#if iOS
				return PlatformId.iOS;
#elif WIN
				return PlatformId.Win;
#elif MAC
				return PlatformId.Mac;
#else
				throw new Lime.Exception("Unknown platform");
#endif
			}
		}
#if iOS
		public Size WindowSize {
			get {
				var size = GameView.Instance.Size;
				return new Size(size.Width, size.Height);
			}
			set {}
		}

		public void ShowOnscreenKeyboard(bool show, string text)
		{
			GameView.Instance.ShowOnscreenKeyboard(show, text);
		}

		public bool FullScreen { get { return true; } set {} }
		public float FrameRate { get { return GameView.Instance.FrameRate; } }

		public DeviceOrientation CurrentDeviceOrientation {
			get {
				switch (GameController.Instance.InterfaceOrientation) {
				case UIInterfaceOrientation.Portrait:
					return DeviceOrientation.Portrait;
				case UIInterfaceOrientation.PortraitUpsideDown:
					return DeviceOrientation.PortraitUpsideDown;
				case UIInterfaceOrientation.LandscapeLeft:
					return DeviceOrientation.LandscapeLeft;
				case UIInterfaceOrientation.LandscapeRight:
				default:
					return DeviceOrientation.LandscapeRight;
				}
			}
		}
#elif MAC
		public void Exit()
		{
			GameController.Instance.Exit();
		}

		public bool FullScreen {
			get { return GameController.Instance.FullScreen; }
			set { GameController.Instance.FullScreen = value; }
		}

		public float FrameRate { get { return GameView.Instance.FrameRate; } }

		public DeviceOrientation CurrentDeviceOrientation {
			get { return DeviceOrientation.LandscapeLeft; }
		}

		public Size WindowSize {
			get { return GameController.Instance.WindowSize; }
			set { GameController.Instance.WindowSize = value; }
		}
#elif WIN
		public void Exit()
		{
			GameView.Instance.Exit();
		}

		public bool FullScreen {
			get { return GameView.Instance.FullScreen; }
			set { GameView.Instance.FullScreen = value; }
		}

		public bool Active { get; internal set; }

		public float FrameRate { get { return GameView.Instance.FrameRate; } }

		public DeviceOrientation CurrentDeviceOrientation {
			get { return DeviceOrientation.LandscapeLeft; }
		}

		public Size WindowSize {
			get { return GameView.Instance.WindowSize; }
			set { GameView.Instance.WindowSize = value; }
		}
#endif

		public event BareEventHandler Activated;
		public event BareEventHandler Deactivated;

		internal void OnActivate()
		{
			if (Activated != null) {
				Activated();
			}
		}

		internal void OnDeactivate()
		{
			if (Deactivated != null) {
				Deactivated();
			}
		}

		public virtual void OnCreate() {}
		public virtual void OnGLCreate() {}
		public virtual void OnGLDestroy() {}
		public virtual void OnUpdateFrame(int delta) {}
		public virtual void OnRenderFrame() {}
		public virtual void OnDeviceRotated() {}
		public virtual DeviceOrientation GetSupportedDeviceOrientations() { return DeviceOrientation.LandscapeLeft; }

		public void SetCursor(string name, IntVector2 hotSpot)
		{
#if WIN
			WinCursors.SetCursor(name, hotSpot);
#endif
		}
	}
}
