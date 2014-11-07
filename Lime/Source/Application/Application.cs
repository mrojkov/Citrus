using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;

#if iOS
using MonoTouch.UIKit;
#elif MAC
using MonoMac.AppKit;
#elif ANDROID
using Android.App;
#endif

namespace Lime
{
	[Flags]
	[ProtoBuf.ProtoContract]
	public enum DeviceOrientation
	{
		Portrait = 1,
		PortraitUpsideDown = 2,
		LandscapeLeft = 4,
		LandscapeRight = 8,
		AllPortraits = Portrait | PortraitUpsideDown,
		AllLandscapes = LandscapeLeft | LandscapeRight,
		All = 15,
	}

	public static class DeviceOrientationExtensions
	{
		public static bool IsPortrait(this DeviceOrientation value)
		{
			return (value == DeviceOrientation.Portrait) || (value == DeviceOrientation.PortraitUpsideDown);
		}

		public static bool IsLandscape(this DeviceOrientation value)
		{
			return !value.IsPortrait();
		}
	}

	public enum PlatformId
	{
		iOS,
		Android,
		Mac,
		Win
	}

	public class Application
	{
		public class StartupOptions
		{
			public bool DecodeAudioInSeparateThread = true;
			public int NumStereoChannels = 8;
			public int NumMonoChannels = 16;
		}

		public static float LowFPSLimit = 20;

		public static Thread MainThread { get; private set; }
		public static bool IsMainThread { get { return Thread.CurrentThread == MainThread; } }

		public static Application Instance;
		private static readonly object scheduledActionsSync = new object();
		private static Action scheduledActions;
		public readonly StartupOptions Options;
		public string Title = "Citrus";

		public Application(StartupOptions options = null)
		{
			Instance = this;
			Options = options ?? new StartupOptions();
			MainThread = Thread.CurrentThread;
			// Use '.' as decimal separator.
			var culture = System.Globalization.CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentCulture = culture;
			SetGlobalExceptionHandler();
			GameView.DidUpdated += RunScheduledActions;
		}
		
		private void RunScheduledActions()
		{
			lock (scheduledActionsSync) {
				if (scheduledActions != null) {
					scheduledActions();
					scheduledActions = null;
				}
			}
		}

		private void SetGlobalExceptionHandler()
		{
			// Почитать и применить:	
			// http://forums.xamarin.com/discussion/931/how-to-prevent-ios-crash-reporters-from-crashing-monotouch-apps

			AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
#if WIN
				var title = "The application";
				if (GameView.Instance != null) {
					GameView.Instance.FullScreen = false;
					title = GameView.Instance.Title;
				}
				WinApi.MessageBox((IntPtr)null, e.ExceptionObject.ToString(), 
					string.Format("{0} has terminated with an error", title), 0);
#else
				Console.WriteLine(e.ExceptionObject.ToString());
#endif
			};
		}


		/// <summary>
		/// Invokes given action on the main thread between update and render. 
		/// If we are on main thread, invokes action immediately.
		/// </summary>
		/// <param name="action"></param>
		public static void InvokeOnMainThread(Action action)
		{
			if (IsMainThread) {
				action();
			} else {
				// Now we use unified way on iOS and PC platform
				lock (scheduledActionsSync) {
					scheduledActions += action;
				}
			}
		}

		public PlatformId Platform {
			get
			{
#if iOS
				return PlatformId.iOS;
#elif WIN
				return PlatformId.Win;
#elif ANDROID
				return PlatformId.Android;
#elif MAC
				return PlatformId.Mac;
#else
				throw new Lime.Exception("Unknown platform");
#endif
			}
		}
#if iOS
		public Size WindowSize { get; internal set; }

		private float pixelsPerPoints = UIScreen.MainScreen.Scale;

		public Vector2 ScreenDPI 
		{
			get { return 160 * pixelsPerPoints * Vector2.One; }
		}

		public void ShowOnscreenKeyboard(bool show, string text)
		{
			GameView.Instance.ShowOnscreenKeyboard(show, text);
		}
		
		public void ChangeOnscreenKeyboardText(string text)
		{
			GameView.Instance.ChangeOnscreenKeyboardText(text);
		}

		public bool Active { get; internal set; }

		public bool FullScreen { get { return true; } set {} }
		public float FrameRate { get { return GameView.Instance.FrameRate; } }

		public DeviceOrientation CurrentDeviceOrientation { get; internal set; }

		public void Exit()
		{
			throw new NotImplementedException();
		}

		// buz: Called before GameController is assigned to Window.RootViewController
		// Kochava SDK requires to be initialized at that point.
		public virtual void PreCreate() {}

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

		public Vector2 ScreenDPI 
		{
			get { return 240 * Vector2.One; }
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
#elif ANDROID
		public Size WindowSize 
		{
			get;
			// AndroidGameView changes the window size
			internal set;
		}

		public Vector2 ScreenDPI 
		{
			get
			{ 
				var dm = Android.Content.Res.Resources.System.DisplayMetrics;
				return new Vector2(dm.Xdpi, dm.Ydpi);
			}
		}

		public void ShowOnscreenKeyboard(bool show, string text)
		{
			GameView.Instance.ShowOnscreenKeyboard(show, text);
		}

		public void ChangeOnscreenKeyboardText(string text)
		{
		}

		public bool Active { get; internal set; }

		public bool FullScreen { get { return true; } set {} }
		public float FrameRate { get { return GameView.Instance.FrameRate; } }

		public DeviceOrientation CurrentDeviceOrientation { get; internal set; }

		public void Exit()
		{
			// There is no way to terminate an android application. 
			// The only way is to finish each its activity one by one.
		}
#elif UNITY
		public void Exit()
		{
			UnityEngine.Application.Quit();
		}

		public bool FullScreen
		{
			get { return UnityEngine.Screen.fullScreen; }
			set { UnityEngine.Screen.fullScreen = value; }
		}

		public bool Active { get; internal set; }

		public float FrameRate { get { return 30; } }

		public DeviceOrientation CurrentDeviceOrientation
		{
			get { return DeviceOrientation.LandscapeLeft; }
		}

		public Size WindowSize
		{
			get { return new Size(UnityEngine.Screen.width, UnityEngine.Screen.height); }
			set { UnityEngine.Screen.SetResolution(value.Width, value.Height, FullScreen); }
		}
#endif
		public event Action Activated;
		public event Action Deactivated;
		public event Action Created;
		public event Action Terminating;
		public event Action GraphicsContextReset;

		internal static int GraphicsContextId { get; private set; }

		public virtual void OnActivate()
		{
			if (Activated != null) {
				Activated();
			}
		}

		public virtual void OnDeactivate()
		{
			if (Deactivated != null) {
				Deactivated();
			}
		}

		public virtual void OnCreate() 
		{
			if (Created != null) {
				Created();
			}
		}

		public virtual void OnTerminate() 
		{
			if (Terminating != null) {
				Terminating();
			}
		}

		public void OnGraphicsContextReset() 
		{
			GraphicsContextId++;
			if (GraphicsContextReset != null) {
				GraphicsContextReset();
			}
		}

		public virtual void OnUpdateFrame(float delta) {}
		public virtual void OnRenderFrame() {}

		/// <summary>
		/// Called before a device rotation get finished, 
		/// but screen resolution and device orientation are already in the final state.
		/// </summary>
		public virtual void OnDeviceRotate() {}

		public virtual DeviceOrientation GetSupportedDeviceOrientations() { return DeviceOrientation.LandscapeLeft; }

		[Obsolete("Use GameView.SetCursor() instead")]
		public void SetCursor(string name, IntVector2 hotSpot)
		{
#if WIN
			GameView.Instance.SetCursor(name, hotSpot);
#endif
		}
	}
}
