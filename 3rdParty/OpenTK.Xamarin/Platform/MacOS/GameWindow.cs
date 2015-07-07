#if MAC
using System;
using CoreGraphics;
using AppKit;
using System.Drawing;

namespace OpenTK
{
	public class GameWindow : IDisposable
	{
		private readonly GameView view;
		private readonly NSWindow window;
		
		public readonly Input.Mouse Mouse = new Input.Mouse();
		public readonly Input.Keyboard Keyboard = new Input.Keyboard();
	
		public MouseCursor Cursor { get; set; }
		public OpenTK.WindowState WindowState { get; set; }
		public bool Focused { get; set; }

		public event EventHandler<KeyPressEventArgs> KeyPress;

		public int Width { get { return ClientSize.Width; } }
		public int Height { get { return ClientSize.Height; } }

		public Size ClientSize
		{
			get { return new Size((int)view.Bounds.Width, (int)view.Bounds.Height); }
			set { window.SetContentSize(new CGSize(value.Width, value.Height)); }
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
		
		static GameWindow()
		{
			NSApplication.Init();	
		}

		public GameWindow(int width, int height, GraphicsMode graphicsMode, string title)
		{
			var rect = new CGRect(0, 0, width, height);
			view = new GameView(rect) {
				Mouse = Mouse,
				Keyboard = Keyboard
			};
			window = new NSWindow(rect, NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Resizable, NSBackingStore.Buffered, false);
			window.Title = title;
			window.WillClose += (s, e) => {
				view.Stop();
				NSApplication.SharedApplication.Terminate(view);
				OnClosed(e);	
			};
			window.DidResize += (s, e) => view.UpdateGLContext();
			// window.DidBecomeKey += OnFocusedChanged;
			window.ContentView = view;
			window.MakeKeyAndOrderFront(view);
			window.ReleasedWhenClosed = true;
			window.DidMove += (s, e) => OnMove(e);	
			view.RenderFrame += (s, e) => {
				view.MakeCurrent();
				OnRenderFrame(new OpenTK.FrameEventArgs());
			};
		}

		public void Dispose()
		{
			window.Dispose();
		}

		public void SwapBuffers()
		{
			view.SwapBuffers();
		}

		public void MakeCurrent()
		{
			view.MakeCurrent();
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
			view.Close();
		}

		public void Run(float updatesPerSecond)
		{
			view.Run(updatesPerSecond);
			NSApplication.SharedApplication.Run();
		}

		protected virtual void OnRenderFrame(OpenTK.FrameEventArgs e)
		{
		}
	}
}

#endif