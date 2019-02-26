using System;
using System.Collections.Generic;
using System.Threading;

#if iOS
using UIKit;
#elif MAC
using AppKit;
#elif MONOMAC
using MonoMac.AppKit;
#elif ANDROID
using Android.App;
#elif WIN
using System.Windows.Forms;
#endif // iOS

namespace Lime
{
	[Flags]
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
		public static bool IsPortrait(this DeviceOrientation value) =>
			(value == DeviceOrientation.Portrait) ||
			(value == DeviceOrientation.PortraitUpsideDown);

		public static bool IsLandscape(this DeviceOrientation value) => !value.IsPortrait();
	}

	public enum PlatformId
	{
		iOS,
		Android,
		Mac,
		Win
	}

	public class ApplicationOptions
	{
		public bool DecodeAudioInSeparateThread = false;
		public int NumChannels = 24;
#if iOS
		public RenderingBackend RenderingBackend = 
			MetalGameView.IsMetalSupported() ? RenderingBackend.Vulkan : RenderingBackend.ES20;
#else
		public RenderingBackend RenderingBackend = RenderingBackend.ES20;
#endif // MAC
	}

	public static class Application
	{
		public static event Action<DeviceOrientation> SupportedDeviceOrientationsChanged;
		public static readonly List<IWindow> Windows = new List<IWindow>();

		private static IWindow mainWindow;
		public static IWindow MainWindow
		{
			get { return mainWindow; }
			set
			{
				if (mainWindow != null || value == null) {
					throw new InvalidOperationException();
				}
				mainWindow = value;
				mainWindow.Updating += RunScheduledActions;
#if WIN
				(mainWindow as Window).SetMenu(mainMenu as Menu);
#endif // WIN
			}
		}

		public static IWindow WindowUnderMouse { get; internal set; }

		public static void InvalidateWindows()
		{
			foreach (var window in Windows) {
				window.Invalidate();
			}
		}

		public static bool AreAllWindowsInactive()
		{
			foreach (var window in Windows) {
				if (window.Active) {
					return false;
				}
			}
			return true;
		}

		private static bool isInputAccelerationListening = true;

		public static bool IsInputAccelerationListening {
			get { return isInputAccelerationListening; }
			set {
#if ANDROID
				AccelerometerListener.IsActive = value;
#endif
				isInputAccelerationListening = value;
			}
		}

		private static DeviceOrientation supportedDeviceOrientations = DeviceOrientation.All;
		/// <summary>
		/// Supported device orientations (only for mobile platforms)
		/// </summary>
		public static DeviceOrientation SupportedDeviceOrientations
		{
			get
			{
				return supportedDeviceOrientations;
			}
			set
			{
				if (supportedDeviceOrientations != value) {
					supportedDeviceOrientations = value;
					SupportedDeviceOrientationsChanged?.Invoke(value);
				}
			}
		}

		/// <summary>
		/// Gets the current device orientation. On desktop platforms it is always DeviceOrientation.LandscapeLeft.
		/// </summary>
#if WIN || MAC
		public static DeviceOrientation CurrentDeviceOrientation => DeviceOrientation.LandscapeLeft;
#else
		public static DeviceOrientation CurrentDeviceOrientation { get; internal set; }
#endif // WIN || MAC

#if MAC
		[System.Runtime.InteropServices.DllImport(ObjCRuntime.Constants.CoreGraphicsLibrary)]
		extern static void CGWarpMouseCursorPosition(nfloat X, nfloat Y);

		public static Vector2 DesktopMousePosition
		{
			get { return new Vector2((float)AppKit.NSEvent.CurrentMouseLocation.X, (float)AppKit.NSEvent.CurrentMouseLocation.Y); }
			set { CGWarpMouseCursorPosition(value.X, value.Y); }
		}
#elif WIN
		public static Vector2 DesktopMousePosition
		{
			get { return SDToLime.Convert(Cursor.Position, Window.Current.PixelScale); }
			set {  Cursor.Position = LimeToSD.ConvertToPoint(value, Window.Current.PixelScale); }
		}
#else
		public static Vector2 DesktopMousePosition
		{
			get { return Window.Current.Input.MousePosition; }
			set { }
		}
#endif // MAC

		// Specifies the highest possible time delta passed to Window.Updating.
		public const float MaxDelta = 1 / 33.333f;

		/// <summary>
		/// Gets the main (UI) thread. All rendering is beging processed on the main thread.
		/// </summary>
		public static Thread MainThread { get; private set; }

		/// <summary>
		/// Gets the currently running thread.
		/// </summary>
		public static Thread CurrentThread => Thread.CurrentThread;

		/// <summary>
		/// Checks whether a thread is the main thread.
		/// </summary>
		public static bool IsMain(this Thread thread) => thread == MainThread;

		/// <summary>
		/// Global Input of whole application.
		/// </summary>
		public static readonly Input Input = new Input();

		/// <summary>
		/// Software (on-screen) keyboard for mobile devices.
		/// </summary>
		public static ISoftKeyboard SoftKeyboard { get; internal set; }

		/// <summary>
		/// Gets or sets the ISO 639-1 two-letter code for current application language
		/// </summary>
		public static string CurrentLanguage { get; set; }

		/// <summary>
		/// Gets or sets CultureInfo for UI
		/// </summary>
		public static System.Globalization.CultureInfo CurrentCultureInfo { get; set; }

		private static readonly object scheduledActionsSync = new object();
		private static Action scheduledActions;
		private static IMenu mainMenu;

		public static IMenu MainMenu
		{
			get { return mainMenu; }
			set
			{
				if (mainMenu != value) {
					mainMenu = value;
#if MAC
					if (mainMenu != null) {
						mainMenu.Refresh();
						NSApplication.SharedApplication.MainMenu = ((Menu)mainMenu).NativeMenu;
					} else {
						NSApplication.SharedApplication.MainMenu = null;
					}
#elif WIN
					if (mainWindow is Window) {
						(mainWindow as Window).SetMenu(mainMenu as Menu);
					}
#endif // MAC
				}
			}
		}

		public static RenderingBackend RenderingBackend { get; internal set; }
#if WIN
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool SetProcessDPIAware();
#endif // WIN

		public static event Func<bool> Exiting;
		public static event Action Exited;

		public static bool DoExiting() => Exiting?.Invoke() ?? true;
		public static void DoExited() => Exited?.Invoke();

		public static void Initialize(ApplicationOptions options = null)
		{
			// System.Environment.SetEnvironmentVariable("METAL_DEVICE_WRAPPER_TYPE", "1");
#if MAC
			NSApplication.Init();
			NSApplication.SharedApplication.ApplicationShouldTerminate += (sender) => {
				return DoExiting() ? NSApplicationTerminateReply.Now : NSApplicationTerminateReply.Cancel;
			};
			NSApplication.SharedApplication.WillTerminate += (sender, e) => DoExited();
			NSApplication.SharedApplication.ApplicationShouldHandleReopen = (sender, hasVisibleWindows) => hasVisibleWindows;
#endif // MAC
			options = options ?? new ApplicationOptions();
			RenderingBackend = options.RenderingBackend;
			MainThread = Thread.CurrentThread;
			// Use '.' as decimal separator.
			var culture = System.Globalization.CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentCulture = culture;
			System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
			AudioSystem.Initialize(options);
#if WIN
			System.Windows.Forms.Application.EnableVisualStyles();
			if (System.Diagnostics.Debugger.IsAttached) {
				System.Windows.Forms.Application.SetUnhandledExceptionMode(
					System.Windows.Forms.UnhandledExceptionMode.ThrowException);
			} else {
				SetGlobalExceptionHandler();
				System.Windows.Forms.Application.SetUnhandledExceptionMode(
					System.Windows.Forms.UnhandledExceptionMode.CatchException);
			}
			// This function doesn't work on XP, and we don't want to add dpiAware into manifest
			// because this will require adding into every new project.
			try {
				SetProcessDPIAware();
			} catch (EntryPointNotFoundException ex) {
				Logger.Write(ex.Message);
			}
			SoftKeyboard = new DummySoftKeyboard();
#elif MAC
			SoftKeyboard = new DummySoftKeyboard();
#elif iOS
			System.IO.Directory.SetCurrentDirectory(Foundation.NSBundle.MainBundle.ResourcePath);
			UIApplication.SharedApplication.StatusBarHidden = true;
			UIApplication.SharedApplication.IdleTimerDisabled = !IsAllowedGoingToSleepMode();
#endif // WIN
		}

#if iOS
		private static bool IsAllowedGoingToSleepMode()
		{
			var obj = Foundation.NSBundle.MainBundle.ObjectForInfoDictionary("AllowSleepMode");
			return obj != null && obj.ToString() == "1";
		}
#endif // iOS

		private static void RunScheduledActions(float delta)
		{
			lock (scheduledActionsSync) {
				if (scheduledActions != null) {
					scheduledActions();
					scheduledActions = null;
				}
			}
		}

		/// <summary>
		/// Use in Orange to free references, since Orange doesn't invoke
		/// Lime.Application RunScheduledActions in main thread.
		/// This function MUST be removed as soon as new Orange will be
		/// implemented with use of OpenTK and our Widget system.
		/// </summary>
		public static void FreeScheduledActions()
		{
			lock (scheduledActionsSync) {
				scheduledActions = null;
			}
		}

		private static void SetGlobalExceptionHandler()
		{
			// Почитать и применить:
			// http://forums.xamarin.com/discussion/931/how-to-prevent-ios-crash-reporters-from-crashing-monotouch-apps
			Action<object> handler = (e) => {
#if WIN
				var title = "The application";
				if (MainWindow != null) {
					MainWindow.Fullscreen = false;
					title = MainWindow.Title;
					MainWindow.Visible = false;
				}

				using (var messageBox = new MessageBoxForm($"{title} has terminated with an error", e.ToString())) {
					messageBox.ShowDialog();
				}
#else
				Console.WriteLine(e.ToString());
#endif // WIN
			};
#if WIN
			// UI-thread exceptions on windows platform
			System.Windows.Forms.Application.ThreadException += (sender, e) => {
				handler(e.Exception);
				System.Windows.Forms.Application.Exit();
			};
#endif // WIN
			// Any other unhandled exceptions
			AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
				handler(e.ExceptionObject);
			};
		}

		/// <summary>
		/// Executes an action on the main thread.
		/// </summary>
		public static void InvokeOnMainThread(Action action)
		{
			if (CurrentThread.IsMain()) {
				action();
			} else {
				// Now we use unified way on iOS and PC platform
				lock (scheduledActionsSync) {
					scheduledActions += action;
				}
			}
		}

		public static void InvokeOnNextUpdate(Action action)
		{
			lock (scheduledActionsSync) {
				scheduledActions += action;
			}
		}

		/// <summary>
		/// Gets the current platform
		/// </summary>
		public static PlatformId Platform {
			get
			{
#if iOS
				return PlatformId.iOS;
#elif WIN
				return PlatformId.Win;
#elif ANDROID
				return PlatformId.Android;
#elif MAC || MONOMAC
				return PlatformId.Mac;
#else
				throw new Lime.Exception("Unknown platform");
#endif // iOS
			}
		}

		/// <summary>
		/// Terminates the application.
		/// </summary>
		public static void Exit()
		{
#if WIN
			MainWindow.Close();
#elif MAC
			NSApplication.SharedApplication.Terminate(new Foundation.NSObject());
#elif MONOMAC
			NSApplication.SharedApplication.Terminate(new MonoMac.Foundation.NSObject());
#elif ANDROID || iOS
			// Android: There is no way to terminate an android application.
			// The only way is to finish each its activity one by one.
#endif // WIN
		}

		/// <summary>
		/// Runs the main application loop on desktop platforms.
		/// Does nothing on iOS, Android.
		/// </summary>
		public static void Run()
		{
#if MAC
			NSApplication.SharedApplication.Run();
#elif WIN
			System.Windows.Forms.Application.Run();
#endif // MAC
		}

#if iOS
		private static float pixelsPerPoints;
#endif // iOS

		/// <summary>
		/// Returns the main display's pixel density.
		/// </summary>
		public static Vector2 ScreenDPI
		{
#if WIN || MAC || MONOMAC
			get { return Window.Current.PixelScale * 96 * Vector2.One; }
#elif iOS
			get
			{
				return 160 * MainWindow.PixelScale * Vector2.One;
			}
#elif ANDROID
			get
			{
				var dm = Android.Content.Res.Resources.System.DisplayMetrics;
				return new Vector2(dm.Xdpi, dm.Ydpi);
			}
#endif // WIN || MAC || MONOMAC
		}
	}
}
