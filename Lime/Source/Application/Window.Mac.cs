#if MAC
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Lime.Platform;
using AppKit;
using Foundation;
using CoreGraphics;

namespace Lime
{
	public class Window : CommonWindow, IWindow
	{
		private NSWindow window;
		private FPSCounter fpsCounter;
		private Stopwatch stopwatch;
		private bool invalidated;
		private bool modal;
		private Display display;
		private bool closed;

		public bool AsyncRendering { get; set; }

		public NSGameView View { get; private set; }

		// This line only suppresses warning: "Window.Current: a name can be simplified".
		public new static IWindow Current => CommonWindow.Current;

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
				RaiseVisibleChanging(value, false);
				modal = false;
				View.Hidden = !value;
				View.Stop();
				if (value) {
					View.Run(60, false);
					window.MakeKeyAndOrderFront(window);
				}
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

		public Vector2 WorldToWindow (Vector2 wp)
		{
			return new Vector2(wp.X.Round() + (float)window.Frame.X, wp.Y.Round() + (float)window.Frame.X);
		}

		public Vector2 DesktopToLocal(Vector2 desktopPosition)
		{
			return new Vector2(
				desktopPosition.X - DecoratedPosition.X,
				(float)NSGameView.Frame.Height - (desktopPosition.Y - DecoratedPosition.Y)
			);
		}

		public Vector2 LocalToDesktop(Vector2 localPosition)
		{
			return new Vector2(
				localPosition.X + DecoratedPosition.X,
				(float)NSGameView.Frame.Height - localPosition.Y + DecoratedPosition.Y
			);
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

		public bool FixedSize
		{
			get { return !window.StyleMask.HasFlag(NSWindowStyle.Resizable) || shouldFixFullscreen; }
			set
			{
				if (value && !FixedSize) {
					if (State == WindowState.Fullscreen) {
						shouldFixFullscreen = true;
					} else {
						window.StyleMask &= ~NSWindowStyle.Resizable;
					}
				} else if (!value && FixedSize) {
					window.StyleMask |= NSWindowStyle.Resizable;
					if (State == WindowState.Fullscreen) {
						shouldFixFullscreen = false;
					}
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

		public bool AllowDropFiles
		{
			get { return View.AllowDropFiles; }
			set { View.AllowDropFiles = value; }
		}

		public event Action<IEnumerable<string>> FilesDropped;

		public void DragFiles(string[] filenames)
		{
			foreach (var filename in filenames) {
				View.DragFile(filename, new CGRect(), false, NSApplication.SharedApplication.CurrentEvent);
			}
		}

		public float UnclampedDelta { get; private set; }

		public float FPS { get { return fpsCounter.FPS; } }

		[Obsolete("Use FPS property instead", true)]
		public float CalcFPS() { return fpsCounter.FPS; }

		public WindowInput Input { get; private set; }

		public Window(WindowOptions options)
		{
			fpsCounter = new FPSCounter();
			CreateNativeWindow(options);
			if (Application.MainWindow == null) {
				Application.MainWindow = this;
			}
			Application.Windows.Add(this);
			Input = new WindowInput (this);
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
		}

		private Vector2 windowedClientSize;
		private float titleBarHeight;
		private bool shouldFixFullscreen;
		private bool needUpdateGLContext = true;

		public IDisplay Display
		{
			get
			{
				if (display == null || window.Screen != display.NativeScreen) {
					display = new Display(window.Screen);
				}
				return display;
			}
		}

		private void CreateNativeWindow(WindowOptions options)
		{
			var rect = new CGRect(0, 0, options.ClientSize.X, options.ClientSize.Y);
			View = new NSGameView(Application.Input, rect, Platform.GraphicsMode.Default);
			NSWindowStyle style;
			if (options.Style == WindowStyle.Borderless) {
				style = NSWindowStyle.Borderless;
			} else if (options.Style == WindowStyle.Dialog) {
				style = NSWindowStyle.Titled | NSWindowStyle.Closable;
			} else {
				style = NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable;
			}
			if (!options.FixedSize) {
				style |= NSWindowStyle.Resizable;
			}
			window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
			window.TabbingMode = (NSWindowTabbingMode)options.MacWindowTabbingMode;
			if (options.Style == WindowStyle.Dialog) {
				window.StandardWindowButton(NSWindowButton.MiniaturizeButton).Hidden = true;
				window.StandardWindowButton(NSWindowButton.ZoomButton).Hidden = true;
			}

			var contentRect = window.ContentRectFor(rect);
			titleBarHeight = ((RectangleF)rect).Height - (float)contentRect.Height;

			if (options.MinimumDecoratedSize != Vector2.Zero) {
				MinimumDecoratedSize = options.MinimumDecoratedSize;
			}
			if (options.MaximumDecoratedSize != Vector2.Zero) {
				MaximumDecoratedSize = options.MaximumDecoratedSize;
			}
			window.Title = options.Title;
			window.WindowShouldClose += OnShouldClose;
			window.WillClose += OnWillClose;
			window.DidResize += (s, e) => {
				needUpdateGLContext = true;
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
			window.DidMove += HandleMove;
			window.CollectionBehavior = NSWindowCollectionBehavior.FullScreenPrimary;
			window.ContentView = View;
			View.Update += Update;
			View.RenderFrame += HandleRenderFrame;
			View.FilesDropped += RaiseFilesDropped;
			View.DidMouseEnter += () => {
				Application.WindowUnderMouse = this;
			};
			View.DidMouseExit += () => {
				if (Application.WindowUnderMouse == this) {
					Application.WindowUnderMouse = null;
				}
			};
		}

		private bool OnShouldClose(NSObject sender)
		{
			if (Application.MainWindow != this) {
				return RaiseClosing(CloseReason.UserClosing);
			}
			// This calling sequence is correct - Window.Win goes to first child
			// that requests cancelling and then calls Closing of main window itself.
			var cancel = OtherWindows.Any(w => w.RaiseClosing(CloseReason.MainWindowClosing));
			return RaiseClosing(CloseReason.UserClosing) || cancel;
		}

		private void OnWillClose(object sender, EventArgs args)
		{
			if (Application.MainWindow == this) {
				CloseMainWindow();
			} else {
				CloseWindow();
			}
		}

		private void CloseMainWindow()
		{
			if (closed) {
				return;
			}
			foreach (var window in OtherWindows) {
				window.CloseWindow();
			}
			CloseWindow();
			NSApplication.SharedApplication.Terminate(View);
			TexturePool.Instance.DiscardAllTextures();
			AudioSystem.Terminate();
		}

		private void CloseWindow()
		{
			if (closed) {
				return;
			}
			RaiseVisibleChanging(false, true);
			RaiseClosed();
			View.Stop();
			Application.Windows.Remove(this);
			// ReleasedWhenClosed = true caused native crashes, so dispose window manually
			// TODO: check the right way to dispose window and its resources (e.g. WindowController)
			window.Dispose();
			closed = true;
			if (modal) {
				NSApplication.SharedApplication.StopModal();
			}
		}

		// Reverse by convention - Window.Win behave like this.
		private IEnumerable<Window> OtherWindows =>
			Application.Windows
				.Where(w => w != this)
				.Cast<Window>()
				.Reverse();

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

		public void Activate()
		{
			window.MakeKeyAndOrderFront(window);
		}

		public void Close()
		{
			window.PerformClose(window);
		}

		public void ShowModal()
		{
			if (Visible) {
				throw new InvalidOperationException();
			}
			RaiseVisibleChanging(true, true);
			modal = true;
			View.Hidden = false;
			View.Stop();
			View.Run(60, true);
			window.MakeKeyAndOrderFront(window);
			NSApplication.SharedApplication.RunModalForWindow(window);
			RaiseVisibleChanging(false, true);
		}

		internal void HandleRenderFrame()
		{
			if (invalidated) {
				fpsCounter.Refresh();
				// Workaround macOS 10.14 issue: UpdateGLContext should be called on render frame, not on DidResize.
				if (needUpdateGLContext) {
					needUpdateGLContext = false;
					View.UpdateGLContext();
				}
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
			UnclampedDelta = (float)stopwatch.Elapsed.TotalSeconds;
			stopwatch.Restart();
			var delta = Mathf.Clamp(UnclampedDelta, 0, Application.MaxDelta);
			// Refresh mouse position on every frame to make HitTest work properly if mouse is outside of the window.
			RefreshMousePosition();
			RaiseUpdating(delta);
			AudioSystem.Update();
			if (Active || Input.IsSimulationRunning) {
				Input.CopyKeysState();
				Input.ProcessPendingKeyEvents(delta);
				Input.TextInput = null;
			}
			if (Application.AreAllWindowsInactive()) {
				Input.ClearKeyState();
			}
			RaiseSync();
		}

		private void RefreshMousePosition()
		{
			Application.Input.DesktopMousePosition = new Vector2((float) NSEvent.CurrentMouseLocation.X, (float) NSEvent.CurrentMouseLocation.Y);
			Application.Input.SetDesktopTouchPosition(0, Application.Input.DesktopMousePosition);
		}

		private void RaiseFilesDropped(IEnumerable<string> files)
		{
			using (Context.Activate().Scoped()) {
				FilesDropped?.Invoke(files);
			}
		}
	}
}
#endif
