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
		private UIWindow uiWindow;
		private FPSCounter fpsCounter = new FPSCounter();

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
		public Size MinimumDecoratedSize { get { return Size.Zero; } set {} }
		public Size MaximumDecoratedSize { get { return Size.Zero; } set {} }
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
			// UIApplicationDelegate must has a Window reference. This is an Apple's requirement.
			AppDelegate.Instance.Window = uiWindow;
			UIViewController = new GameController(Input);
			uiWindow.RootViewController = UIViewController;
			uiWindow.MakeKeyAndVisible();
			// Run() creates OpenGL context
			UIView.Run();
			AppDelegate.Instance.Activated += () => {
				UIViewController.LockDeviceOrientation = false;
				Active = true;
				AudioSystem.Active = true;
				RaiseActivated();
			};
			AppDelegate.Instance.Deactivated += () => {
				UIViewController.LockDeviceOrientation = true;
				AudioSystem.Active = false;
				RaiseDeactivated();
				UIView.DoRenderFrame();
				OpenTK.Graphics.ES11.GL.Finish();
				TexturePool.Instance.DiscardTexturesUnderPressure();
				Active = false;
			};
			AppDelegate.Instance.WillTerminateEvent += () => {
				RaiseClosed();
			};
			UIViewController.ViewDidLayoutSubviewsEvent += () => {
				using (Context.MakeCurrent()) {
					Application.RaiseDeviceRotated();
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
			var delta = (float)Math.Min(e.Time, 1 / Application.LowFPSLimit);
			RaiseUpdating(delta);
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