#if iOS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UIKit;

#pragma warning disable 0067

namespace Lime
{
	public class Window : IWindow
	{
		private UIWindow uiWindow;
		private FPSCounter fpsCounter = new FPSCounter();

		public event Action Activated;
		public event Action Deactivated;
		public event Func<bool> Closing;
		public event Action Closed;
		public event Action Moved;
		public event Action Resized;
		public event Action<float> Updating;
		public event Action Rendering;

		public GameController UIViewController { get; private set; }
		public GameView UIView { get { return UIViewController.View; } }
		public Input Input { get; private set; }
		public bool Active { get; private set; }
		public string Title { get; set; }
		public WindowState State { get { return WindowState.Fullscreen; } set {} }
		public bool Fullscreen { get { return true; } set {} }
		public IntVector2 ClientPosition { get { return IntVector2.Zero; } set {} }
		public Size ClientSize { get { return UIViewController.View.ClientSize; } set {} }
		public IntVector2 DecoratedPosition { get { return IntVector2.Zero; } set {} }
		public Size DecoratedSize { get { return ClientSize; } set {} }
		public bool Visible { get { return true; } set {} }
		public MouseCursor Cursor { get; set; }

		public float CalcFPS() { return fpsCounter.FPS; }
		public void Center() { }
		public void Close() { }

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
			UIViewController = new GameController(Input);
			uiWindow.RootViewController = UIViewController;
			uiWindow.MakeKeyAndVisible();
			// Run() creates OpenGL context
			UIView.Run();
			AppDelegate.Instance.Activated += () => {
				UIViewController.LockDeviceOrientation = false;
				Active = true;
				AudioSystem.Active = true;
				if (Activated != null) {
					Activated();
				}
			};
			AppDelegate.Instance.Deactivated += () => {
				UIViewController.LockDeviceOrientation = true;
				AudioSystem.Active = false;
				if (Deactivated != null) {
					Deactivated();
				}
				UIView.DoRenderFrame();
				OpenTK.Graphics.ES11.GL.Finish();
				TexturePool.Instance.DiscardTexturesUnderPressure();
				Active = false;
			};
			AppDelegate.Instance.WillTerminateEvent += () => {
				if (Closed != null) {
					Closed();
				}
			};
			UIViewController.ViewDidLayoutSubviewsEvent += () => {
				if (Resized != null) {
					Resized();
				}
			};
			UIView.RenderFrame += OnRenderFrame;
			UIView.UpdateFrame += OnUpdateFrame;
		}

		private void OnUpdateFrame(object s, Xamarin.FrameEventArgs e)
		{
			if (!Active || UIViewController.SoftKeyboardBeingShownOrHid) {
				return;
			}
			Input.ProcessPendingKeyEvents();
			if (Updating != null) {
				var delta = (float)Math.Min(e.Time, 1 / Application.LowFPSLimit);
				Updating(delta);
			}
			AudioSystem.Update();
		}

		private void OnRenderFrame(object s, Xamarin.FrameEventArgs e)
		{
			if (!Active || UIViewController.SoftKeyboardBeingShownOrHid) {
				return;
			}
			UIView.MakeCurrent();
			if (Rendering != null) {
				Rendering();
			}
			UIView.SwapBuffers();
			fpsCounter.Refresh();
		}
	}
}
#endif