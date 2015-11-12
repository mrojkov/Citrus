#if MAC
using System;
using CoreGraphics;
using AppKit;
using System.Drawing;

namespace OpenTK
{
	public enum GameWindowFlags
	{
		Default = 0,
		FullScreen = 1,	
		FixedWindow = 2,
	}
	
	public class CancelEventArgs : EventArgs
	{
		public bool Cancel;
	}
	
	public class GameWindow : IDisposable
	{
		public readonly NSGameView View;
		private readonly NSWindow window;

		public event EventHandler<EventArgs> FocusedChanged;
		public event EventHandler<CancelEventArgs> Closing;
		public event EventHandler<EventArgs> Closed;
		public event EventHandler<EventArgs> Move;
		public event EventHandler<EventArgs> Resize;
		public event EventHandler<FrameEventArgs> RenderFrame;
		
		public readonly Input.Mouse Mouse = new Input.Mouse();
		public readonly Input.Keyboard Keyboard = new Input.Keyboard();
		
		public MouseCursor Cursor { get; set; }
		public OpenTK.WindowState WindowState 
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
				if (value == WindowState.Minimized) {
					window.Miniaturize(null);
					return;
				}
				if (value == WindowState.Fullscreen && WindowState != WindowState.Fullscreen) {
					window.ToggleFullScreen(null);
				} else if (value != WindowState.Fullscreen && WindowState == WindowState.Fullscreen) {
					window.ToggleFullScreen(null);
				}
			}
		}
		
		public bool Focused { get; set; }

		public event EventHandler<KeyPressEventArgs> KeyPress 
		{
			add { Keyboard.KeyPress += value; }
			remove { Keyboard.KeyPress -= value; }
		}

		public int Width { get { return ClientSize.Width; } }
		public int Height { get { return ClientSize.Height; } }

		public bool Visible
		{
			get { return View.Visible; }
			set { View.Visible = value; }
		}

		public Size ClientSize
		{
			get { return new Size((int)View.Bounds.Width, (int)View.Bounds.Height); }
			set { window.SetContentSize(new CGSize(value.Width, value.Height)); }
		}

		public Size Size
		{
			get { return new Size((int)window.Frame.Width, (int)window.Frame.Height); }
			set 
			{
				var frame = window.Frame;
				frame.Size = new CGSize(value.Width, value.Height); 
				window.SetFrame(frame, true);
			}
		}

		public string Title
		{
			get { return window.Title; }
			set { window.Title = value; }
		}

		public Point ClientLocation
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public Point Location
		{
			get { return new Point((int)window.Frame.X, (int)window.Frame.Y); }
			set
			{
				var frame = window.Frame;
				frame.Location = new CGPoint(value.X, value.Y);
				window.SetFrame(frame, true);
			}
		}

		private Size windowedClientSize;

		public GameWindow(int width, int height, GraphicsMode graphicsMode, string title, GameWindowFlags flags = GameWindowFlags.Default)
		{
			var rect = new CGRect(0, 0, width, height);
			View = new NSGameView(rect) {
				Mouse = Mouse,
				Keyboard = Keyboard
			};
			var style = NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable;
			if ((flags & GameWindowFlags.FixedWindow) == 0) {
				style |= NSWindowStyle.Resizable;
			}
			window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
			window.Title = title;
			window.WillClose += (s, e) => {
				View.Stop();
				NSApplication.SharedApplication.Terminate(View);
				OnClosed(e);	
			};
			// Set window minimum size to prevent render bugs in split-screen mode.
			window.MinSize = new CGSize(480, 480);
			window.DidResize += (s, e) => {
				View.UpdateGLContext();
				OnResize(e);
			};
			window.WillEnterFullScreen += (sender, e) => {
				windowedClientSize = ClientSize;
			};
			window.DidExitFullScreen += (sender, e) => {
				ClientSize = windowedClientSize;
				window.Center();
			};
			window.DidMove += (s, e) => OnMove(e);
			window.CollectionBehavior = NSWindowCollectionBehavior.FullScreenPrimary;
			window.ContentView = View;
			window.ReleasedWhenClosed = true;
			View.RenderFrame += (s, e) => {
				View.MakeCurrent();
				OnRenderFrame(new OpenTK.FrameEventArgs());
			};
		}
		
		public WindowBorder WindowBorder
		{
			get 
			{
				if ((window.StyleMask & NSWindowStyle.Resizable) != 0) {
					return WindowBorder.Resizable;
				} else {
					return WindowBorder.Fixed;
				}
			}
			set
			{
				if (value == WindowBorder.Fixed) {
					window.StyleMask &= ~NSWindowStyle.Resizable;
				} else if (value == WindowBorder.Resizable) {
					window.StyleMask |= NSWindowStyle.Resizable;
				}
			}
		}

		public void Dispose()
		{
			window.Dispose();
		}

		public void SwapBuffers()
		{
			View.SwapBuffers();
		}

		public void MakeCurrent()
		{
			View.MakeCurrent();
		}

		protected virtual void OnFocusedChanged(EventArgs e)
		{
			if (FocusedChanged != null) {
				FocusedChanged(this, e);
			}
		}
		
		protected virtual void OnClosed(EventArgs e)
		{
			if (Closed != null) {
				Closed(this, e);
			}
		}

		protected virtual void OnMove(EventArgs e)
		{
			if (Move != null) {
				Move(this, e);
			}
		}
		
		protected virtual void OnResize(EventArgs e)
		{
			if (Resize != null) {
				Resize(this, e);
			}
		}

		public void Close()
		{
			if (Closed != null) {
				Closed(this, new EventArgs());
			}
			View.Close();
		}

		public void Run(float updatesPerSecond)
		{
			window.MakeKeyAndOrderFront(window);
			View.Run(updatesPerSecond);
		}

		protected virtual void OnRenderFrame(OpenTK.FrameEventArgs e)
		{
			if (RenderFrame != null) {
				RenderFrame(this, e);
			}
		}
	}
}

#endif