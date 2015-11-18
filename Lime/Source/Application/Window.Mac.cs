#if MONOMAC || MAC
using System;
using OpenTK.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using AppKit;
using CoreGraphics;
using Lime.Platform;

namespace Lime
{
	public class Window : CommonWindow, IWindow
	{
		private NSWindow window;
		private FPSCounter fpsCounter;
		private Stopwatch stopwatch;

		public NSGameView View { get; private set; }

		public string Title
		{
			get { return window.Title; }
			set { window.Title = value; }
		}

		public bool Visible
		{
			get { return !View.Hidden; }
			set
			{
				if (View.Hidden != !value)
					View.Hidden = !value; 
			}
		}

		public IntVector2 ClientPosition
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public Size ClientSize
		{
			get 
			{
				var b = View.ConvertRectToBacking(View.Bounds);
				return new Size((int)b.Width, (int)b.Height); 
			}
			set 
			{ 
				var s = View.ConvertSizeFromBacking(new CGSize(value.Width, value.Height));
				window.SetContentSize(new CGSize(s.Width, s.Height)); 
			}
		}

		public IntVector2 DecoratedPosition
		{
			get { return new IntVector2((int)window.Frame.X, (int)window.Frame.Y); }
			set
			{
				var frame = window.Frame;
				frame.Location = new CGPoint(value.X, value.Y);
				window.SetFrame(frame, true);
			}
		}

		public Size DecoratedSize
		{
			get { return new Size((int)window.Frame.Width, (int)window.Frame.Height); }
			set 
			{
				var frame = window.Frame;
				frame.Size = new CGSize(value.Width, value.Height); 
				window.SetFrame(frame, true);
			}
		}

		public Size MinimumDecoratedSize
		{
			get
			{
				var s = View.ConvertSizeToBacking(window.MinSize);
				return new Size((int)s.Width, (int)s.Height); 
			}
			set
			{
				var s = new CGSize(value.Width, value.Height);
				window.MinSize = View.ConvertSizeFromBacking(s);
			}
		}

		public Size MaximumDecoratedSize
		{
			get
			{
				var s = View.ConvertSizeToBacking(window.MaxSize);
				return new Size((int)s.Width, (int)s.Height); 
			}
			set
			{
				var s = new CGSize(value.Width, value.Height);
				window.MaxSize = View.ConvertSizeFromBacking(s);
			}
		}

		public bool Active
		{
			get { return window.IsKeyWindow; }
		}

		public WindowState State 
		{
			get 
			{
				if (window.IsMiniaturized) {
					return WindowState.Minimized;
				}
				if ((window.StyleMask & NSWindowStyle.FullScreenWindow) != 0) {
					return WindowState.Fullscreen;
				}
				return WindowState.Normal;
			}
			set
			{
				if (State == value) {
					return;
				}
				if (value == WindowState.Minimized) {
					window.Miniaturize(null);
					return;
				}
				if (value == WindowState.Fullscreen && State != WindowState.Fullscreen) {
					window.ToggleFullScreen(null);
				} else if (value != WindowState.Fullscreen && State == WindowState.Fullscreen) {
					window.ToggleFullScreen(null);
				}
			}
		}

		public bool Fullscreen
		{
			get { return State == WindowState.Fullscreen; }
			set
			{
				if (value && State == WindowState.Fullscreen || !value && State != WindowState.Fullscreen) {
					return;
				}
				State = value ? WindowState.Fullscreen : WindowState.Normal;
			}
		}

		public NSGameView NSGameView { get { return View; } }

		public MouseCursor Cursor { get; set; }

		public float CalcFPS() { return fpsCounter.FPS; }

		public Input Input { get; private set; }

		public Window(WindowOptions options)
		{
			Input = new Input();
			fpsCounter = new FPSCounter();
			CreateNativeWindow(options);
			if (Application.MainWindow != null) {
				throw new Lime.Exception("Attempt to create GameWindow twice");
			}
			Application.MainWindow = this;
			ClientSize = options.ClientSize;
			Title = options.Title;
			Center();
			if (options.Visible) {
				Visible = true;
			}
			stopwatch = new Stopwatch();
			stopwatch.Start();
			window.MakeKeyAndOrderFront(window);
			View.Run(60);
		}

		private Size windowedClientSize;

		private void CreateNativeWindow(WindowOptions options)
		{
			var rect = new CGRect(0, 0, options.ClientSize.Width, options.ClientSize.Height);
			View = new NSGameView(Input, rect, null, Platform.GraphicsMode.Default);
			var style = NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable;
			if (!options.FixedSize) {
				style |= NSWindowStyle.Resizable;
			}
			window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
			if (options.MinimumDecoratedSize != Size.Zero) {
				MinimumDecoratedSize = options.MinimumDecoratedSize;
			}
			if (options.MaximumDecoratedSize != Size.Zero) {
				MaximumDecoratedSize = options.MaximumDecoratedSize;
			}
			window.Title = options.Title;
			window.WillClose += (s, e) => {
				View.Stop();
				NSApplication.SharedApplication.Terminate(View);
				HandleClosed(s, e);
			};
			window.DidResize += (s, e) => {
				View.UpdateGLContext();
				HandleResize(s, e);
			};
			window.WillEnterFullScreen += (sender, e) => {
				windowedClientSize = ClientSize;
			};
			window.DidExitFullScreen += (sender, e) => {
				ClientSize = windowedClientSize;
				window.Center();
			};
			window.DidBecomeKey += (sender, e) => {
				AudioSystem.Active = true;
				RaiseActivated();
			};
			window.DidResignKey += (sender, e) => {
				AudioSystem.Active = false;
				RaiseActivated();
			};
			window.DidMove += HandleMove;
			window.CollectionBehavior = NSWindowCollectionBehavior.FullScreenPrimary;
			window.ContentView = View;
			window.ReleasedWhenClosed = true;
			View.RenderFrame += HandleRenderFrame;
		}

		public void Center()
		{
			var displayBounds = DisplayDevice.Default.Bounds;
			DecoratedPosition = new IntVector2 {
				X = (int)Math.Max(0, (displayBounds.Width - DecoratedSize.Width) / 2 + displayBounds.Left),
				Y = (int)Math.Max(0, (displayBounds.Height - DecoratedSize.Height) / 2 + displayBounds.Top)
			};
		}

		public void Close()
		{
			window.Close();
		}

		private void HandleClosed(object sender, EventArgs e)
		{
			RaiseClosed();
			TexturePool.Instance.DiscardAllTextures();
			AudioSystem.Terminate();
		}

		private void HandleRenderFrame()
		{
			var delta = (float)stopwatch.Elapsed.TotalSeconds;
			stopwatch.Restart();
			delta = Mathf.Clamp(delta, 0, 1 / Application.LowFPSLimit);
			Update(delta);
			fpsCounter.Refresh();
			View.MakeCurrent();
			RaiseRendering();
			View.SwapBuffers();
		}

		private void HandleResize(object sender, EventArgs e)
		{
			RaiseResized();
		}

		private void HandleMove(object sender, EventArgs e)
		{
			RaiseMoved();
		}

		private void Update(float delta)
		{
			Input.ProcessPendingKeyEvents();
			RaiseUpdating(delta);
			AudioSystem.Update();
			Input.TextInput = null;
			Input.CopyKeysState();
		}
	}
}
#endif