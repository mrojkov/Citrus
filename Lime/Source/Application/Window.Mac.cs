#if MONOMAC || MAC
using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lime
{
	public class Window : CommonWindow, IWindow
	{
		private OpenTK.GameWindow otkWindow;
		private FPSCounter fpsCounter;
		private Stopwatch stopwatch;

		public string Title
		{
			get { return otkWindow.Title; }
			set { otkWindow.Title = value; }
		}

		public WindowBorder Border
		{
			get { return (WindowBorder)otkWindow.WindowBorder; }
			set { otkWindow.WindowBorder = (OpenTK.WindowBorder)value; }
		}

		public bool Visible
		{
			get { return otkWindow.Visible; }
			set { otkWindow.Visible = value; }
		}

		public IntVector2 ClientPosition
		{
			get { return new Lime.IntVector2(otkWindow.ClientLocation.X, otkWindow.ClientLocation.Y); }
			set { otkWindow.ClientLocation = new System.Drawing.Point(value.X, value.X); }
		}

		public Size ClientSize
		{
			get { return new Lime.Size(otkWindow.ClientSize.Width, otkWindow.ClientSize.Height); }
			set { otkWindow.ClientSize = new System.Drawing.Size(value.Width, value.Height); }
		}

		public IntVector2 DecoratedPosition
		{
			get { return new IntVector2(otkWindow.Location.X, otkWindow.Location.Y); }
			set { otkWindow.Location = new System.Drawing.Point(value.X, value.Y); }
		}

		public Size DecoratedSize
		{
			get { return new Lime.Size(otkWindow.Size.Width, otkWindow.Size.Height); }
			set { otkWindow.Size = new System.Drawing.Size(value.Width, value.Height); }
		}

		public bool Active { get; private set; }

		public WindowState State
		{
			get { return (WindowState)otkWindow.WindowState; }
			set
			{
				if (otkWindow.WindowState == (OpenTK.WindowState)value) {
					return;
				}
				otkWindow.WindowState = (OpenTK.WindowState)value;
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

		public NSGameView NSGameView { get { return otkWindow.View; } }

		public MouseCursor Cursor { get; set; }

		public float CalcFPS() { return fpsCounter.FPS; }

		public Input Input { get; private set; }

		public Window(WindowOptions options)
		{
			Input = new Input();
			fpsCounter = new FPSCounter();
			otkWindow = new OpenTK.GameWindow(options.Size.Width, options.Size.Height,
				new GraphicsMode(new ColorFormat(32), depth: 24), options.Title, GetWindowFlags(options));
			if (Application.MainWindow != null) {
				throw new Lime.Exception("Attempt to create GameWindow twice");
			}
			Application.MainWindow = this;
			Active = true;
			otkWindow.Keyboard.KeyDown += HandleKeyDown;
			otkWindow.Keyboard.KeyUp += HandleKeyUp;
			otkWindow.KeyPress += HandleKeyPress;
			otkWindow.Mouse.ButtonDown += HandleMouseButtonDown;
			otkWindow.Mouse.ButtonUp += HandleMouseButtonUp;
			otkWindow.Mouse.Move += HandleMouseMove;
			otkWindow.Mouse.WheelChanged += HandleMouseWheel;
			otkWindow.FocusedChanged += HandleFocusedChanged;
			otkWindow.Closing += HandleClosing;
			otkWindow.Closed += HandleClosed;
			otkWindow.Move += HandleMove;
			otkWindow.Resize += HandleResize;
			otkWindow.RenderFrame += HandleRenderFrame;
			ClientSize = options.Size;
			Title = options.Title;
			Center();
			if (options.Visible) {
				Visible = true;
			}
			stopwatch = new Stopwatch();
			stopwatch.Start();
			otkWindow.Run(60);
		}

		private static GameWindowFlags GetWindowFlags(WindowOptions options)
		{
			return options.FixedSize ? GameWindowFlags.FixedWindow : GameWindowFlags.Default;
		}

		public void Center()
		{
			var displayBounds = OpenTK.DisplayDevice.Default.Bounds;
			DecoratedPosition = new IntVector2 {
				X = Math.Max(0, (displayBounds.Width - DecoratedSize.Width) / 2 + displayBounds.X),
				Y = Math.Max(0, (displayBounds.Height - DecoratedSize.Height) / 2 + displayBounds.Y)
			};
		}

		public void Close()
		{
			otkWindow.Close();
		}

		private void HandleKeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
		{
			// SDL backend bug: OpenTK doesn't send key press event for backspace
			if (e.Key == OpenTK.Input.Key.BackSpace) {
				Input.TextInput += '\b';
			}
			Input.SetKeyState((Key)e.Key, true);
			//There is no KeyUp event for regular key on Mac if Command key pressed, so we release it manualy in the same frame
			if ((Input.IsKeyPressed(Key.LWin) || Input.IsKeyPressed(Key.RWin))) {
				Input.SetKeyState((Key)e.Key, false);
			}
		}

		private void HandleKeyUp(object sender, KeyboardKeyEventArgs e)
		{
			Input.SetKeyState((Key)e.Key, false);
		}

		private void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			Input.TextInput += e.KeyChar;
		}

		private void HandleFocusedChanged(object sender, EventArgs e)
		{
			Active = otkWindow.Focused;
			if (otkWindow.Focused) {
				AudioSystem.Active = true;
				RaiseActivated();
			} else {
				AudioSystem.Active = false;
				RaiseDeactivated();
			}
		}

		private void HandleMouseButtonUp(object sender, MouseButtonEventArgs e)
		{
			switch(e.Button) {
			case OpenTK.Input.MouseButton.Left:
				Input.SetKeyState(Key.Mouse0, false);
				Input.SetKeyState(Key.Touch0, false);
				break;
			case OpenTK.Input.MouseButton.Right:
				Input.SetKeyState(Key.Mouse1, false);
				break;
			case OpenTK.Input.MouseButton.Middle:
				Input.SetKeyState(Key.Mouse2, false);
				break;
			}
		}

		private void HandleMouseButtonDown(object sender, MouseButtonEventArgs e)
		{
			switch(e.Button) {
			case OpenTK.Input.MouseButton.Left:
				Input.SetKeyState(Key.Mouse0, true);
				Input.SetKeyState(Key.Touch0, true);
				break;
			case OpenTK.Input.MouseButton.Right:
				Input.SetKeyState(Key.Mouse1, true);
				break;
			case OpenTK.Input.MouseButton.Middle:
				Input.SetKeyState(Key.Mouse2, true);
				break;
			}
		}

		private void HandleMouseMove(object sender, MouseMoveEventArgs e)
		{
			var position = new Vector2(e.X, e.Y) * Input.ScreenToWorldTransform;
			Input.MousePosition = position;
			Input.SetTouchPosition(0, position);
		}

		void HandleMouseWheel(object sender, MouseWheelEventArgs e)
		{
			// On Mac and Win we assume this as number of "lines" to scroll, not pixels to scroll
			var wheelDelta = e.Delta;
			if (e.Delta > 0) {
				if (!Input.HasPendingKeyEvent(Key.MouseWheelUp)) {
					Input.SetKeyState(Key.MouseWheelUp, true);
					Input.SetKeyState(Key.MouseWheelUp, false);
					Input.WheelScrollAmount = wheelDelta;
				} else {
					Input.WheelScrollAmount += wheelDelta;
				}
			} else {
				if (!Input.HasPendingKeyEvent(Key.MouseWheelDown)) {
					Input.SetKeyState(Key.MouseWheelDown, true);
					Input.SetKeyState(Key.MouseWheelDown, false);
					Input.WheelScrollAmount = wheelDelta;
				} else {
					Input.WheelScrollAmount += wheelDelta;
				}
			}
		}

		private void HandleClosing(object sender, OpenTK.CancelEventArgs e)
		{
			e.Cancel = RaiseClosing();
		}

		private void HandleClosed(object sender, EventArgs e)
		{
			RaiseClosed();
			TexturePool.Instance.DiscardAllTextures();
			AudioSystem.Terminate();
		}

		private void HandleRenderFrame(object sender, FrameEventArgs e)
		{
			var delta = (float)stopwatch.Elapsed.TotalSeconds;
			stopwatch.Restart();
			delta = Mathf.Clamp(delta, 0, 1 / Application.LowFPSLimit);
			Update(delta);
			Render();
		}

		private void HandleResize(object sender, EventArgs e)
		{
			RaiseResized();
		}

		private void HandleMove(object sender, EventArgs e)
		{
			RaiseMoved();
		}

		private void Render()
		{
			fpsCounter.Refresh();
			otkWindow.MakeCurrent();
			RaiseRendering();
			otkWindow.SwapBuffers();
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