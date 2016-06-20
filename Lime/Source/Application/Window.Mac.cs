#if MONOMAC || MAC
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

		public Vector2 ClientPosition
		{
			get { return DecoratedPosition - new Vector2(0, titleBarHeight); }
			set { DecoratedPosition = value + new Vector2(0, titleBarHeight); }
		}

		public Vector2 ClientSize
		{
			get { return new Vector2((float)View.Bounds.Width, (float)View.Bounds.Height); }
			set { window.SetContentSize(new CGSize(value.X.Round(), value.Y.Round())); }
		}

		public float PixelScale
		{
			get { return (float)window.BackingScaleFactor; }
		}

		public Vector2 DecoratedPosition
		{
			get { return new Vector2((float)window.Frame.X, (float)window.Frame.Y); }
			set
			{
				var frame = window.Frame;
				frame.Location = new CGPoint(value.X.Round(), value.Y.Round());
				window.SetFrame(frame, true);
			}
		}

		public Vector2 DecoratedSize
		{
			get { return new Vector2((float)window.Frame.Width, (float)window.Frame.Height); }
			set 
			{
				var frame = window.Frame;
				frame.Size = new CGSize(value.X.Round(), value.Y.Round()); 
				window.SetFrame(frame, true);
			}
		}

		public Vector2 MinimumDecoratedSize
		{
			get { return new Vector2((float)window.MinSize.Width, (float)window.MinSize.Height); }
			set { window.MinSize = new CGSize(value.X.Round(), value.Y.Round()); }
		}

		public Vector2 MaximumDecoratedSize
		{
			get { return new Vector2((float)window.MaxSize.Width, (float)window.MaxSize.Height); }
			set { window.MaxSize = new CGSize(value.X.Round(), value.Y.Round()); }
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

		private MouseCursor cursor = MouseCursor.Default;
		public MouseCursor Cursor
		{
			get { return cursor; }
			set
			{
				if (cursor != value) {
					cursor = value;
					value.NativeCursor.Set();
				}
			}
		}
		
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
			Application.Windows.Add(this);
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

		private Vector2 windowedClientSize;
		private float titleBarHeight;
		private bool shouldFixFullscreen;

		private void CreateNativeWindow(WindowOptions options)
		{
			var rect = new CGRect(0, 0, options.ClientSize.X, options.ClientSize.Y);
			View = new NSGameView(Input, rect, Platform.GraphicsMode.Default);
			NSWindowStyle style;
			if (options.Style == WindowStyle.Borderless) {
				style = NSWindowStyle.Borderless;
			} else {
				style = NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable;
			}
			if (!options.FixedSize) {
				style |= NSWindowStyle.Resizable;
			}
			window = new NSWindow(rect, style, NSBackingStore.Buffered, false);

			var contentRect = window.ContentRectFor(rect);
			titleBarHeight = ((RectangleF)rect).Height - (float)contentRect.Height;

			if (options.MinimumDecoratedSize != Vector2.Zero) {
				MinimumDecoratedSize = options.MinimumDecoratedSize;
			}
			if (options.MaximumDecoratedSize != Vector2.Zero) {
				MaximumDecoratedSize = options.MaximumDecoratedSize;
			}
			window.Title = options.Title;
			window.WindowShouldClose += (sender) => {
				return RaiseClosing();
			};
			window.WillClose += (s, e) => {
				RaiseClosed();
				View.Stop();
				Application.Windows.Remove(this);
				if (Application.MainWindow == this) {
					NSApplication.SharedApplication.Terminate(View);
					TexturePool.Instance.DiscardAllTextures();
					AudioSystem.Terminate();
				}
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
			DecoratedPosition = new Vector2 {
				X = (int)Math.Max(0, (displayBounds.Width - DecoratedSize.X) / 2 + displayBounds.Left),
				Y = (int)Math.Max(0, (displayBounds.Height - DecoratedSize.Y) / 2 + displayBounds.Top)
			};
		}

		public void Close()
		{
			window.Close();
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
			if (this == Application.MainWindow) {			
				Application.MainMenu.Refresh();
			}
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