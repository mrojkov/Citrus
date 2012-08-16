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

		public Application()
		{
			Instance = this;
			// Use '.' as decimal separator.
			var culture = System.Globalization.CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentCulture = culture;
		}

		public string[] GetCommandLineArgs()
		{
			return System.Environment.GetCommandLineArgs();
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

		public void ShowOnscreenKeyboard(bool show)
		{
			GameView.Instance.ShowOnscreenKeyboard(show);
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

		public event BareEventHandler OnActivated;
		public event BareEventHandler OnDeactivated;

		internal void Activate()
		{
			if (OnActivated != null) {
				OnActivated();
			}
		}

		internal void Deactivate() 
		{
			if (OnDeactivated != null) {
				OnDeactivated();
			}
		}

		public virtual void OnCreate() {}
		public virtual void OnGLCreate() {}
		public virtual void OnGLDestroy() {}
		public virtual void OnUpdateFrame(int delta) {}
		public virtual void OnRenderFrame() {}
		public virtual void OnDeviceRotated() {}
		public virtual DeviceOrientation GetSupportedDeviceOrientations() { return DeviceOrientation.LandscapeLeft; }
	}
}