#if iOS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UIKit;

#pragma warning disable 0067

namespace Lime
{
	public class Window : CommonWindow, IWindow
	{
		private readonly Display display;

		// This line only suppresses warning: "Window.Current: a name can be simplified".
		public new static IWindow Current => CommonWindow.Current;

		private UIWindow uiWindow;
		private FPSCounter fpsCounter = new FPSCounter();

		public GameController UIViewController { get; private set; }
		public GameView UIView { get { return UIViewController.View; } }
		public Input Input { get; private set; }
		public bool Active { get; private set; }
		public string Title { get; set; }
		public WindowState State { get { return WindowState.Fullscreen; } set {} }
		public bool FixedSize { get { return true; } set {} }
		public bool Fullscreen { get { return true; } set {} }
		public Vector2 ClientPosition { get { return Vector2.Zero; } set {} }
		public Vector2 ClientSize
		{
			get { return UIView.ClientSize; }
			set { }
		}
		public Vector2 DecoratedPosition { get { return Vector2.Zero; } set {} }
		public Vector2 DecoratedSize { get { return ClientSize; } set {} }
		public Vector2 MinimumDecoratedSize { get { return Vector2.Zero; } set {} }
		public Vector2 MaximumDecoratedSize { get { return Vector2.Zero; } set {} }
		public bool Visible { get { return true; } set {} }
		public MouseCursor Cursor { get; set; }
		public float FPS { get { return fpsCounter.FPS; } }

		[Obsolete("Use FPS property instead", true)]
		public float CalcFPS() { return fpsCounter.FPS; }

		public bool AllowDropFiles { get { return false; } set {} }

		public event Action<IEnumerable<string>> FilesDropped;

		public void DragFiles(string[] filenames)
		{
			throw new NotImplementedException();
		}

		public float PixelScale
		{
			get { return (float)UIScreen.MainScreen.Scale; }
		}

		public virtual Vector2 MousePosition => Input.DesktopMousePosition;

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
			Input = new Input();
			uiWindow = new UIWindow(UIScreen.MainScreen.Bounds);
			// UIApplicationDelegate must has a Window reference. This is an Apple's requirement.
			AppDelegate.Instance.Window = uiWindow;
			UIViewController = new GameController(Input);
			uiWindow.RootViewController = UIViewController;
			uiWindow.MakeKeyAndVisible();
			AppDelegate.Instance.Activated += () => {
				// Run() creates OpenGL context
				UIView.Run();
				UIViewController.LockDeviceOrientation = false;
				Active = true;
				AudioSystem.Active = true;
				RaiseActivated();
				UIKit.UIViewController.AttemptRotationToDeviceOrientation();
			};
			AppDelegate.Instance.Deactivated += () => {
				UIView.Stop();
				UIViewController.LockDeviceOrientation = true;
				AudioSystem.Active = false;
				RaiseDeactivated();
				Active = false;
			};
			AppDelegate.Instance.WillTerminateEvent += () => {
				RaiseClosed();
			};
			UIViewController.OnResize += (sender, e) => {
				RaiseResized(((ResizeEventArgs)e).DeviceRotated);
			};
			UIView.RenderFrame += OnRenderFrame;
			UIView.UpdateFrame += OnUpdateFrame;
			display = new Display(UIScreen.MainScreen);
		}

		/// <summary>
		/// Gets the default display device.
		/// </summary>
		public IDisplay Display
		{
			get { return display; }
		}

		public Vector2 GetTouchPosition(int index)
		{
			return Input.GetDesktopTouchPosition(index);
		}

		public void Center() { }
		public void Close() { }
		public void ShowModal() { }
		public void Invalidate() { }
		public void Activate() { }

		private void OnUpdateFrame(object s, Xamarin.FrameEventArgs e)
		{
			if (!Active || UIViewController.SoftKeyboardBeingShownOrHid) {
				return;
			}
			var delta = (float)Math.Min(e.Time, Application.MaxDelta);
			RaiseUpdating(delta);
			Input.CopyKeysState();
			Input.ProcessPendingKeyEvents(delta);
			AudioSystem.Update();
		}

		private void OnRenderFrame(object s, Xamarin.FrameEventArgs e)
		{
			if (!Active || UIViewController.SoftKeyboardBeingShownOrHid) {
				return;
			}
			UIView.MakeCurrent();
			RaiseRendering();
			UIView.SwapBuffers();
			fpsCounter.Refresh();
		}
	}
}
#endif
