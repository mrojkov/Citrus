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
		Win
	}

	/// <summary>
	/// Класс, предоставляющий промежуточное звено между игровой логикой и подсистемами движка.
	/// Экземпляр этого класса является синглтоном
	/// </summary>
	public class Application
	{
		public class StartupOptions
		{
			public bool DecodeAudioInSeparateThread = true;
			public int NumStereoChannels = 8;
			public int NumMonoChannels = 16;
			public bool FullScreen = false;
			public bool FixedSizeWindow = true;
			public Size WindowSize = new Size(800, 600);
			public string WindowTitle = "Citrus";
		}

		public static float LowFPSLimit = 20;

		/// <summary>
		/// Главный поток приложения. Вся игровая логика и рендеринг выполняются в главном потоке
		/// </summary>
		public static Thread MainThread { get; private set; }

		/// <summary>
		/// Возвращает true, если это главный поток
		/// </summary>
		public static bool IsMainThread { get { return Thread.CurrentThread == MainThread; } }

		/// <summary>
		/// Предоставляет доступ к программной клавиатуре (как на телефоне)
		/// </summary>
		public readonly SoftKeyboard SoftKeyboard = new SoftKeyboard();

		/// <summary>
		/// Ссылка на экземпляр класса (других экземпляров быть не должно)
		/// </summary>
		public static Application Instance;

		private static readonly object scheduledActionsSync = new object();
		private static Action scheduledActions;

		/// <summary>
		/// Начальные опции, которые были переданы в конструкторе
		/// </summary>
		public readonly StartupOptions Options;
		
		/// <summary>
		/// Заголовок окна приложения (актуально только для десктопных приложений)
		/// </summary>
		public string Title
		{
			get { return GameView.Instance.Title; }
			set { GameView.Instance.Title = value; }
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="options">Начальные опции</param>
		public Application(StartupOptions options = null)
		{
			Instance = this;
			Options = options ?? new StartupOptions();
			MainThread = Thread.CurrentThread;
			// Use '.' as decimal separator.
			var culture = System.Globalization.CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentCulture = culture;
			SetGlobalExceptionHandler();
#if !UNITY
			GameView.DidUpdated += RunScheduledActions;
#endif
		}

#if !UNITY
		private static void RunScheduledActions()
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
		/// Вызывает указанное действие в главном потоке между вызовом OnUpdateFrame и OnRenderFrame
		/// Если мы и так в главном потоке, то действие выполняется сразу
		/// </summary>
		public static void InvokeOnMainThread(Action action)
		{
			if (IsMainThread) {
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
		/// Платформа, на которой запускается приложение
		/// </summary>
		public PlatformId Platform {
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
#endif
			}
		}
#if iOS
		/// <summary>
		/// Возвращает размер экрана
		/// </summary>
		public Size WindowSize { get; internal set; }

		private float pixelsPerPoints = 0f;

		/// <summary>
		/// Возвращает количество пикселей в дюйме по горизонтали и вертикали
		/// </summary>
		public Vector2 ScreenDPI 
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
		/// Приложение активно (не свернуто). Если приложение не активно, его работа частично приостанавливается
		/// </summary>
		public bool Active { get; internal set; }

		/// <summary>
		/// Всегда возвращает true (на мобильном устройсве невозможно запустить приложение в окне)
		/// </summary>
		public bool FullScreen { get { return true; } set {} }

		/// <summary>
		/// Возвращает FPS
		/// </summary>
		public float FrameRate { get { return GameView.Instance.FrameRate; } }

		/// <summary>
		/// Возвращает ориентацию устройства
		/// </summary>
		public DeviceOrientation CurrentDeviceOrientation { get; internal set; }

		/// <summary>
		/// Генерирует исключение NotImplementedException.
		/// На iOS завершить работу приложения таким образом невозможно. Приложения завершаются по усмотрению операционной системы
		/// </summary>
		/// <exception cref="NotImplementedException"/>		
		public void Exit()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// buz:
		/// Вызывается перед тем, как GameController назначен Window.RootViewController
		/// Kochava SDK требует, чтобы он был инициализирован в этом месте
		/// </summary>
		public virtual void PreCreate() {}

#elif WIN || MAC || MONOMAC

		/// <summary>
		/// Завершает работу приложения
		/// </summary>
		public void Exit()
		{
			GameView.Instance.Close();
		}

		/// <summary>
		/// Возвращает количество пикселей в дюйме по горизонтали и вертикали (всегда возвращает (240, 240))
		/// </summary>
		public Vector2 ScreenDPI 
		{
			get { return 240 * Vector2.One; }
		}

		/// <summary>
		/// Полноэкранный режим
		/// </summary>
		public bool FullScreen {
			get { return GameView.Instance.FullScreen; }
			set { GameView.Instance.FullScreen = value; }
		}

		/// <summary>
		/// Приложение активно (не свернуто и его окно имеет фокус)
		/// </summary>
		public bool Active { get; internal set; }

		/// <summary>
		/// Возвращает FPS
		/// </summary>
		public float FrameRate { get { return GameView.Instance.FrameRate; } }

		/// <summary>
		/// Всегда возвращает DeviceOrientation.LandscapeLeft. Имеет смысл только для мобильных устройств
		/// </summary>
		public DeviceOrientation CurrentDeviceOrientation {
			get { return DeviceOrientation.LandscapeLeft; }
		}

		/// <summary>
		/// Размер окна приложения (учитываются границы окна)
		/// </summary>
		public Size WindowSize {
			get { return GameView.Instance.WindowSize; }
			set { GameView.Instance.WindowSize = value; }
		}

		/// <summary>
		/// Centers the game window on the default display
		/// </summary>
		public void CenterWindow()
		{
#if WIN || MAC
			GameView.Instance.Center();
#endif
		}

		public WindowBorder WindowBorder
		{
			get { return GameView.Instance.WindowBorder; }
			set { GameView.Instance.WindowBorder = value; }
		}
		
#elif ANDROID
		/// <summary>
		/// Возвращает размер экрана
		/// </summary>
		public Size WindowSize 
		{
			get;
			// AndroidGameView changes the window size
			internal set;
		}

		/// <summary>
		/// Возвращает количество пикселей в дюйме по горизонтали и вертикали
		/// </summary>
		public Vector2 ScreenDPI 
		{
			get
			{ 
				var dm = Android.Content.Res.Resources.System.DisplayMetrics;
				return new Vector2(dm.Xdpi, dm.Ydpi);
			}
		}

		/// <summary>
		/// Приложение активно (не свернуто). Если приложение не активно, его работа частично приостанавливается
		/// </summary>
		public bool Active { get; internal set; }

		/// <summary>
		/// Всегда возвращает true (на мобильном устройсве невозможно запустить приложение в окне)
		/// </summary>
		public bool FullScreen { get { return true; } set {} }

		/// <summary>
		/// Возвращает FPS
		/// </summary>
		public float FrameRate { get { return GameView.Instance.FrameRate; } }

		/// <summary>
		/// Возвращает ориентацию устройства
		/// </summary>
		public DeviceOrientation CurrentDeviceOrientation { get; internal set; }

		/// <summary>
		/// Ничего не делает. На Андроиде завершить работу приложения таким образом невозможно. Приложения завершаются по усмотрению операционной системы
		/// </summary>
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
		/// <summary>
		/// Генерируется, когда свойство Active стало false (например приложение было свернуто или его окно потеряло фокус)
		/// </summary>
		public event Action Activated;

		/// <summary>
		/// Генерируется, когда свойство Active стало true (например приложение было развернуто или его окно получило фокус)
		/// </summary>
		public event Action Deactivated;

		/// <summary>
		/// Генерируется при создании окна приложения
		/// </summary>
		public event Action Created;

		/// <summary>
		/// Генерируется при уничтожении окна приложения
		/// </summary>
		public event Action Terminating;

		/// <summary>
		/// Генерируется, когда был потерян графический контекст (область памяти, куда рисуется графика) и был пересоздан.
		/// Такое случается, когда окно приложения сворачивают и разворачивают
		/// </summary>
		public event Action GraphicsContextReset;

		/// <summary>
		/// Генерируется при перемещении окна
		/// </summary>
		public event Action Moved;

		public event Action Resized;

		/// <summary>
		/// Вызывается, когда свойство Active стало true (например приложение было развернуто или его окно получило фокус)
		/// </summary>
		public virtual void OnActivate()
		{
			if (Activated != null) {
				Activated();
			}
		}

		/// <summary>
		/// Вызывается, когда свойство Active стало false (например приложение было свернуто или его окно потеряло фокус)
		/// </summary>
		public virtual void OnDeactivate()
		{
			if (Deactivated != null) {
				Deactivated();
			}
		}

		/// <summary>
		/// Вызывается при создании окна приложения
		/// </summary>
		public virtual void OnCreate() 
		{
			if (Created != null) {
				Created();
			}
		}

		/// <summary>
		/// Вызывается при уничтожении окна приложения
		/// </summary>
		public virtual void OnTerminate() 
		{
			if (Terminating != null) {
				Terminating();
			}
		}

#if !UNITY
		/// <summary>
		/// Вызывается, когда был потерян графический контекст (область памяти, куда рисуется графика) и был пересоздан.
		/// Такое случается, когда окно приложения сворачивают и разворачивают
		/// </summary>
		public void OnGraphicsContextReset() 
		{
			GLObjectRegistry.Instance.DiscardObjects();
			if (GraphicsContextReset != null) {
				GraphicsContextReset();
			}
		}
#endif
		/// <summary>
		/// Вызывается при перемещении окна
		/// </summary>
		public virtual void OnMove()
		{
			if (Moved != null) {
				Moved();
			}
		}

		public virtual void OnResize()
		{
			if (Resized != null) {
				Resized();
			}
		}

		/// <summary>
		/// Вызывается при обновлении кадра. Здесь нужно вызвать Update для всех игровых объектов
		/// </summary>
		/// <param name="delta">Количество секунд, прошедшее с момента предыдущего кадра</param>
		public virtual void OnUpdateFrame(float delta) {}

		/// <summary>
		/// Вызывается при отрисовке кадра
		/// </summary>
		public virtual void OnRenderFrame() {}

		/// <summary>
		/// Вызывается перед тем, как устройство выполнит процедуру поворота,
		/// но разрешение экрана и ориентация устройства уже изменят свое состояние
		/// </summary>
		public virtual void OnDeviceRotate() {}

		/// <summary>
		/// Поддерживаемые ориентации устройства (только для мобильных платформ)
		/// </summary>
		public DeviceOrientation SupportedDeviceOrientations = DeviceOrientation.All;

		/// <summary>
		/// Устанавливает картинку курсора мыши
		/// Устаревший. Используйте GameView.SetCursor()
		/// </summary>
		/// <param name="name">Название картинки курсора из ресурсов</param>
		/// <param name="hotSpot">Активная точка</param>
		[Obsolete("Use GameView.SetCursor() instead")]
		public void SetCursor(string name, IntVector2 hotSpot)
		{
#if WIN
			GameView.Instance.SetCursor(name, hotSpot);
#endif
		}
	}
}
