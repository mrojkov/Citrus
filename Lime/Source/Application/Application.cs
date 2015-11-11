using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;

#if iOS
using UIKit;
#elif MAC
using AppKit;
#elif ANDROID
using Android.App;
#endif

namespace Lime
{
	/// <summary>
	/// Варианты ориентации телефона или планшета
	/// </summary>
	[Flags]
	[ProtoBuf.ProtoContract]
	public enum DeviceOrientation
	{
		/// <summary>
		/// Портретная. Высота экрана больше ширины, аппаратные кнопки внизу
		/// </summary>
		Portrait = 1,

		/// <summary>
		/// Портретная перевернутая. Высота экрана больше ширины, аппаратные кнопки вверху
		/// </summary>
		PortraitUpsideDown = 2,

		/// <summary>
		/// Альбомная. Ширина экрана больше высоты, аппаратные кнопки слева
		/// </summary>
		LandscapeLeft = 4,

		/// <summary>
		/// Альбомная. Ширина экрана больше высоты, аппаратные кнопки справа
		/// </summary>
		LandscapeRight = 8,

		/// <summary>
		/// Портретные ориентации. Высота экрана больше ширины
		/// </summary>
		AllPortraits = Portrait | PortraitUpsideDown,

		/// <summary>
		/// Альбомные ориентации. Ширина экрана больше высоты
		/// </summary>
		AllLandscapes = LandscapeLeft | LandscapeRight,

		/// <summary>
		/// Все ориентации устройства
		/// </summary>
		All = 15,
	}

	public static class DeviceOrientationExtensions
	{
		/// <summary>
		/// Портретная ориентация. Высота экрана больше ширины
		/// </summary>
		public static bool IsPortrait(this DeviceOrientation value)
		{
			return (value == DeviceOrientation.Portrait) || (value == DeviceOrientation.PortraitUpsideDown);
		}

		/// <summary>
		/// Альбомная ориентация. Ширина экрана больше высоты
		/// </summary>
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
		Win,
		Unity
	}

	public class ApplicationOptions
	{
		public bool DecodeAudioInSeparateThread = true;
		public int NumStereoChannels = 8;
		public int NumMonoChannels = 16;
#if MAC
		public RenderingBackend RenderingBackend = RenderingBackend.OpenGL;
#else
		public RenderingBackend RenderingBackend = RenderingBackend.ES20;
#endif
	}

	public static class Application
	{
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
			}
		}

		/// <summary>
		/// Supported device orientations (only for mobile platforms)
		/// </summary>
		public static DeviceOrientation SupportedDeviceOrientations = DeviceOrientation.All;

		/// <summary>
		/// Gets the current device orientation. On desktop platforms it is always DeviceOrientation.LandscapeLeft.
		/// </summary>
#if WIN || MAC
		public static DeviceOrientation CurrentDeviceOrientation { get { return DeviceOrientation.LandscapeLeft; } }
#else
		public static DeviceOrientation CurrentDeviceOrientation { get; internal set; }
#endif
		/// <summary>
		/// Occurs when the device has rotated. Only mobile platforms.
		/// </summary>
		public static event Action DeviceRotated;

		// Specifies the lowest possible 1/(time delta) passed to Window.Updating.
		// TODO: Move to IWindow
		public static float LowFPSLimit = 20;

		/// <summary>
		/// Gets the main (UI) thread. All rendering is beging processed on the main thread.
		/// </summary>
		public static Thread MainThread { get; private set; }

		/// <summary>
		/// Gets the currently running thread.
		/// </summary>
		public static Thread CurrentThread { get { return Thread.CurrentThread; } }

		/// <summary>
		/// Checks whether a thread is the main thread.
		/// </summary>
		public static bool IsMain(this Thread thread) { return thread == MainThread; }

		/// <summary>
		/// Software (on-screen) keyboard for mobile devices.
		/// </summary>
		public static ISoftKeyboard SoftKeyboard { get; internal set; }

		private static readonly object scheduledActionsSync = new object();
		private static Action scheduledActions;

		public static RenderingBackend RenderingBackend { get; private set; }

#if WIN
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool SetProcessDPIAware();
#endif

		public static void Initialize(ApplicationOptions options = null)
		{
#if MAC
			NSApplication.Init();
#endif
			options = options ?? new ApplicationOptions();
			RenderingBackend = options.RenderingBackend;
			MainThread = Thread.CurrentThread;
			// Use '.' as decimal separator.
			var culture = System.Globalization.CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentCulture = culture;
			SetGlobalExceptionHandler();
			AudioSystem.Initialize(options);
#if WIN
			System.Windows.Forms.Application.SetUnhandledExceptionMode(
				System.Windows.Forms.UnhandledExceptionMode.ThrowException
			);
			SetProcessDPIAware();
			Window.InitializeMainOpenGLContext();
			SoftKeyboard = new DummySoftKeyboard();
#elif MAC
			SoftKeyboard = new DummySoftKeyboard();
#elif iOS
			System.IO.Directory.SetCurrentDirectory(Foundation.NSBundle.MainBundle.ResourcePath);
			UIApplication.SharedApplication.StatusBarHidden = true;
			UIApplication.SharedApplication.IdleTimerDisabled = !IsAllowedGoingToSleepMode();
#endif
		}

		public static void DiscardOpenGLObjects()
		{
			GLObjectRegistry.Instance.DiscardObjects();
		}

#if iOS
		private static bool IsAllowedGoingToSleepMode()
		{
			var obj = Foundation.NSBundle.MainBundle.ObjectForInfoDictionary("AllowSleepMode");
			return obj != null && obj.ToString() == "1";
		}
#endif

		internal static void RaiseDeviceRotated()
		{
			if (DeviceRotated != null) {
				DeviceRotated();
			}
		}

#if !UNITY
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
#endif

		private static void SetGlobalExceptionHandler()
		{
			// Почитать и применить:
			// http://forums.xamarin.com/discussion/931/how-to-prevent-ios-crash-reporters-from-crashing-monotouch-apps

			AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
#if WIN
				var title = "The application";
				if (MainWindow != null) {
					MainWindow.Fullscreen = false;
					title = MainWindow.Title;
				}
				WinApi.MessageBox((IntPtr)null, e.ExceptionObject.ToString(), 
					string.Format("{0} has terminated with an error", title), 0);
#else
				Console.WriteLine(e.ExceptionObject.ToString());
#endif
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
#if UNITY
				throw new NotImplementedException();
#else
				// Now we use unified way on iOS and PC platform
				lock (scheduledActionsSync) {
					scheduledActions += action;
				}
#endif
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
#elif UNITY
				return PlatformId.Unity;
#else
				throw new Lime.Exception("Unknown platform");
#endif
			}
		}
#if iOS
		private static float pixelsPerPoints = 0f;

		/// <summary>
		/// Возвращает количество пикселей в дюйме по горизонтали и вертикали
		/// </summary>
		public static Vector2 ScreenDPI 
		{
			get {
				// Class-level initialization fails on iOS simulator in debug mode,
				// because it is called before main UI thread.
				if (pixelsPerPoints == 0)
					pixelsPerPoints = (float)UIScreen.MainScreen.Scale;
				return 160 * pixelsPerPoints * Vector2.One;
			}
		}

		/// <summary>
		/// Генерирует исключение NotImplementedException.
		/// На iOS завершить работу приложения таким образом невозможно. Приложения завершаются по усмотрению операционной системы
		/// </summary>
		/// <exception cref="NotImplementedException"/>		
		public static void Exit()
		{
			throw new NotImplementedException();
		}

#elif WIN || MAC || MONOMAC

		/// <summary>
		/// Завершает работу приложения
		/// </summary>
		public static void Exit()
		{
			MainWindow.Close();
		}

		/// <summary>
		/// Возвращает количество пикселей в дюйме по горизонтали и вертикали (всегда возвращает (240, 240))
		/// </summary>
		public static Vector2 ScreenDPI 
		{
			get { return 240 * Vector2.One; }
		}

		/// <summary>
		/// Runs the main application loop on desktop platforms.
		/// </summary>
		public static void Run()
		{
#if MAC
			NSApplication.SharedApplication.Run();
#elif WIN
			System.Windows.Forms.Application.Run();
#endif
		}

#elif ANDROID
		/// <summary>
		/// Возвращает количество пикселей в дюйме по горизонтали и вертикали
		/// </summary>
		public static Vector2 ScreenDPI 
		{
			get
			{ 
				var dm = Android.Content.Res.Resources.System.DisplayMetrics;
				return new Vector2(dm.Xdpi, dm.Ydpi);
			}
		}

		/// <summary>
		/// Ничего не делает. На Андроиде завершить работу приложения таким образом невозможно. Приложения завершаются по усмотрению операционной системы
		/// </summary>
		public static void Exit()
		{
			// There is no way to terminate an android application. 
			// The only way is to finish each its activity one by one.
		}
#elif UNITY
		public void Exit()
		{
			UnityEngine.Application.Quit();
		}
#endif
	}
}
