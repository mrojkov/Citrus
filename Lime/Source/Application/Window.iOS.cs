#if iOS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Foundation;
using UIKit;

#pragma warning disable 0067

namespace Lime
{
	public class Window : CommonWindow, IWindow
	{
		// This line only suppresses warning: "Window.Current: a name can be simplified".
		public new static IWindow Current => CommonWindow.Current;

		private UIWindow uiWindow;
		private FPSCounter fpsCounter = new FPSCounter();

		public GameController UIViewController { get; private set; }
		public UIView UIView { get => UIViewController.View; }
		public WindowInput Input { get; private set; }
		public bool AsyncRendering { get { return false; } private set { } }
		public bool Active { get; private set; }
		public string Title { get; set; }
		public WindowState State { get { return WindowState.Fullscreen; } set {} }
		public bool FixedSize { get => true; set {} }
		public bool Fullscreen { get => true; set {} }
		public Vector2 ClientPosition { get => Vector2.Zero; set {} }
		public Vector2 ClientSize { get => ((IGameView)UIView).ClientSize; set {} }
		public Vector2 DecoratedPosition { get => Vector2.Zero; set {} }
		public Vector2 DecoratedSize { get => ClientSize; set {} }
		public Vector2 MinimumDecoratedSize { get => Vector2.Zero; set {} }
		public Vector2 MaximumDecoratedSize { get => Vector2.Zero; set {} }
		public bool Visible { get => true; set {} }
		public MouseCursor Cursor { get; set; }
		public float UnclampedDelta { get; private set; }
		public float FPS { get => fpsCounter.FPS; }
		public IDisplay Display { get; private set; }

		[Obsolete("Use FPS property instead", true)]
		public float CalcFPS() => fpsCounter.FPS;

		public bool AllowDropFiles { get => false; set {} }

		public event Action<IEnumerable<string>> FilesDropped;

		public void DragFiles(string[] filenames)
		{
			throw new NotImplementedException();
		}

		public float PixelScale
		{
			get { return ((IGameView)UIView).PixelScale; }
		}

		public Vector2 LocalToDesktop(Vector2 localPosition)
		{
			return localPosition;
		}

		public Vector2 DesktopToLocal(Vector2 desktopPosition)
		{
			return desktopPosition;
		}

		public Window()
			: this(new WindowOptions())
		{
		}

		public Window(WindowOptions options)
		{
			if (Application.MainWindow != null) {
				throw new Lime.Exception("Attempt to create a second window.");
			}
			Application.MainWindow = this;
			Active = true;
			AsyncRendering = options.AsyncRendering;
			Input = new WindowInput(this);
			uiWindow = new UIWindow(UIScreen.MainScreen.Bounds);
			// UIApplicationDelegate must has a Window reference. This is an Apple's requirement.
			AppDelegate.Instance.Window = uiWindow;
			UIViewController = new GameController(Application.Input);
			uiWindow.RootViewController = UIViewController;
			uiWindow.MakeKeyAndVisible();
			var view = (IGameView)UIView;
			AppDelegate.Instance.Activated += () => {
				// Run() creates OpenGL context
				view.Run();
				UIViewController.LockDeviceOrientation = false;
				Active = true;
				AudioSystem.Active = true;
				RaiseActivated();
				UIKit.UIViewController.AttemptRotationToDeviceOrientation();
			};
			AppDelegate.Instance.Deactivated += () => {
				view.Stop();
				UIViewController.LockDeviceOrientation = true;
				AudioSystem.Active = false;
				RaiseDeactivated();
				Active = false;
			};
			AppDelegate.Instance.WillTerminateEvent += () => {
				RaiseClosed();
			};

			UIApplication.Notifications.ObserveDidChangeStatusBarFrame(OnStatusBarChanged);
			UIApplication.Notifications.ObserveDidChangeStatusBarOrientation(OnStatusBarChanged);
			SafeAreaInsets = FetchSafeAreaInsets();

			UIViewController.OnResize += (sender, e) => {
				RaiseResized(((ResizeEventArgs)e).DeviceRotated);
			};
			view.RenderFrame += OnRenderFrame;
			view.UpdateFrame += OnUpdateFrame;
			Display = new Display(UIScreen.MainScreen);
			Application.WindowUnderMouse = this;
		}

		public void Center() { }
		public void Close() { }
		public void ShowModal() { }
		public void Invalidate() { }
		public void Activate() { }

		private void OnUpdateFrame(float delta)
		{
			if (!Active || UIViewController.SoftKeyboardBeingShownOrHid) {
				return;
			}
			UnclampedDelta = delta;
			var clampedDelta = Math.Min(UnclampedDelta, Application.MaxDelta);
			Input.ProcessPendingKeyEvents(clampedDelta);
			RaiseUpdating(clampedDelta);
			Input.CopyKeysState();
			AudioSystem.Update();
			RaiseSync();
		}

		private void OnRenderFrame()
		{
			if (!Active || UIViewController.SoftKeyboardBeingShownOrHid) {
				return;
			}
			var view = (IGameView)UIView;
			view.MakeCurrent();
			RaiseRendering();
			view.SwapBuffers();
			fpsCounter.Refresh();
		}

		private void OnStatusBarChanged(object sender, NSNotificationEventArgs e)
		{
			SafeAreaInsets = FetchSafeAreaInsets();
			RaiseSafeAreaInsetsChanged();
		}

		private Rectangle FetchSafeAreaInsets()
		{
			var insets = UIApplication.SharedApplication.KeyWindow.SafeAreaInsets;
			return new Rectangle(
				(float) insets.Left,
				(float) insets.Top,
				(float) insets.Right,
				(float) insets.Bottom
			);
		}
	}
}
#endif
