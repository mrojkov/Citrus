#if MAC
using System;
using CoreGraphics;
using AppKit;
using System.Drawing;

namespace OpenTK
{
	public class GameWindow : IDisposable
	{
		public readonly NSGameView View;
		private readonly NSWindow window;
		
		public readonly Input.Mouse Mouse = new Input.Mouse();
		public readonly Input.Keyboard Keyboard = new Input.Keyboard();
	
		public MouseCursor Cursor { get; set; }
		public OpenTK.WindowState WindowState { get; set; }
		public bool Focused { get; set; }

		public event EventHandler<KeyPressEventArgs> KeyPress {
			add { Keyboard.KeyPress += value; }
			remove { Keyboard.KeyPress -= value; }
		}

		public int Width { get { return ClientSize.Width; } }
		public int Height { get { return ClientSize.Height; } }

		public Size ClientSize
		{
			get { return new Size((int)View.Bounds.Width, (int)View.Bounds.Height); }
			set { window.SetContentSize(new CGSize(value.Width, value.Height)); }
		}

		public string Title
		{
			get { return window.Title; }
			set { window.Title = value; }
		}
		
		public Point Location
		{
			get { return new Point((int)window.Frame.X, (int)window.Frame.Y); }
			set
			{
				var frame = window.Frame;
				frame.X = value.X;
				frame.Y = value.Y;
				window.SetFrame(frame, true);
			}
		}		

		public GameWindow(int width, int height, GraphicsMode graphicsMode, string title)
		{
			var rect = new CGRect(0, 0, width, height);
			View = new NSGameView(rect) {
				Mouse = Mouse,
				Keyboard = Keyboard
			};
			window = new NSWindow(rect, NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Resizable, NSBackingStore.Buffered, false);
			window.Title = title;
			window.WillClose += (s, e) => {
				View.Stop();
				NSApplication.SharedApplication.Terminate(View);
				OnClosed(e);	
			};
			window.DidResize += (s, e) => View.UpdateGLContext();
			// window.DidBecomeKey += OnFocusedChanged;
			window.ContentView = View;
			window.ReleasedWhenClosed = true;
			window.DidMove += (s, e) => OnMove(e);
			View.RenderFrame += (s, e) => {
				View.MakeCurrent();
				OnRenderFrame(new OpenTK.FrameEventArgs());
			};
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
		}
		
		protected virtual void OnClosed(EventArgs e)
		{			
		}

		protected virtual void OnMove(EventArgs e)
		{
		}
		
		public void Close()
		{
			View.Close();
		}

		public void Run(float updatesPerSecond)
		{
			window.MakeKeyAndOrderFront(window);
			View.Run(updatesPerSecond);
			NSApplication.SharedApplication.Run();
		}

		protected virtual void OnRenderFrame(OpenTK.FrameEventArgs e)
		{
		}
	}
}

#endif