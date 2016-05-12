#if MONOMAC || MAC
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lime.Platform;
#if MAC
using AppKit;
using CoreGraphics;
using OpenTK.Graphics;
#else
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.OpenGL;
#endif

namespace Lime
{
	public class Window : CommonWindow, IWindow
	{
		private NSWindow window;
		private FPSCounter fpsCounter;
		private Stopwatch stopwatch;
		private bool invalidated;

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
			get { return new Size((int)View.Bounds.Width, (int)View.Bounds.Height); }
			set { window.SetContentSize(new CGSize(value.Width, value.Height)); }
		}

		public float PixelScale
		{
			get { return (float)window.BackingScaleFactor; }
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
			get { return new Size((int)window.MinSize.Width, (int)window.MinSize.Height); }
			set { window.MinSize = new CGSize(value.Width, value.Height); }
		}

		public Size MaximumDecoratedSize
		{
			get { return new Size((int)window.MaxSize.Width, (int)window.MaxSize.Height); }
			set { window.MaxSize = new CGSize(value.Width, value.Height); }
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

		public float FPS { get { return fpsCounter.FPS; } }

		[Obsolete("Use FPS property instead", true)]
		public float CalcFPS() { return fpsCounter.FPS; }

		public Input Input { get; private set; }

		public Window(WindowOptions options)
		{
			Input = new Input();
			fpsCounter = new FPSCounter();
			CreateNativeWindow(options);
			if (Application.MainWindow == null) {
				Application.MainWindow = this;
			}
			ClientSize = options.ClientSize;
			Title = options.Title;
			if (options.Visible) {
				Visible = true;
			}
			if (options.Centered) {
				Center();
			}
			stopwatch = new Stopwatch();
			stopwatch.Start();
			window.MakeKeyAndOrderFront(window);
			View.Run(options.RefreshRate);
		}

		private Size windowedClientSize;
		private bool shouldFixFullscreen;

		private void CreateNativeWindow(WindowOptions options)
		{
			var rect = new CGRect(0, 0, options.ClientSize.Width, options.ClientSize.Height);
			View = new NSGameView(Input, rect, Platform.GraphicsMode.Default);
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
				shouldFixFullscreen = !window.StyleMask.HasFlag(NSWindowStyle.Resizable);
				if (shouldFixFullscreen) {
					window.StyleMask |= NSWindowStyle.Resizable;
				}
				windowedClientSize = ClientSize;
			};
			window.WillExitFullScreen += (sender, e) => {
				ClientSize = windowedClientSize;
			};
			window.DidExitFullScreen += (sender, e) => {
				if (shouldFixFullscreen) {
					window.StyleMask &= ~NSWindowStyle.Resizable;
				}
			};
			window.DidBecomeKey += (sender, e) => {
				RaiseActivated();
			};
			window.DidResignKey += (sender, e) => {
				RaiseDeactivated();
			};
#if MAC
			window.DidMove += HandleMove;
#else
			window.DidMoved += HandleMove;
#endif
			window.CollectionBehavior = NSWindowCollectionBehavior.FullScreenPrimary;
			window.ContentView = View;
			window.ReleasedWhenClosed = true;
			View.Update += Update;
			View.RenderFrame += HandleRenderFrame;
		}

		public void Invalidate()
		{
			invalidated = true;
		}

		public void Center()
		{
			var displayBounds = window.Screen.VisibleFrame;
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
			if (invalidated) {
				fpsCounter.Refresh();
				View.MakeCurrent();
				RaiseRendering();
				View.SwapBuffers();
				invalidated = false;
			}
		}

		private void HandleResize(object sender, EventArgs e)
		{
			RaiseResized(deviceRotated: false);
			Invalidate();
		}

		private void HandleMove(object sender, EventArgs e)
		{
			RaiseMoved();
		}

		private void Update()
		{
			var delta = (float)stopwatch.Elapsed.TotalSeconds;
			stopwatch.Restart();
			delta = Mathf.Clamp(delta, 0, 1 / Application.LowFPSLimit);
			// Refresh mouse position on every frame to make HitTest work properly if mouse is outside of the window.
			RefreshMousePosition();
			Input.ProcessPendingKeyEvents(delta);
			RaiseUpdating(delta);
			AudioSystem.Update();
			Input.TextInput = null;
			Input.CopyKeysState();
		}

		private void RefreshMousePosition()
		{
			var p = window.MouseLocationOutsideOfEventStream;
			Input.MousePosition = new Vector2((int)p.X, (int)NSGameView.Frame.Height - (int)p.Y) * Input.ScreenToWorldTransform;
		}
	}
}

#endif