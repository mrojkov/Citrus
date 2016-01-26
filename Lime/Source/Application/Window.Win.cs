#if WIN
using System;
using System.Diagnostics;
using System.Windows.Forms;
using OpenTK.Graphics;

namespace Lime
{
	static class SDToLime
	{
		public static IntVector2 Convert(System.Drawing.Point p)
		{
			return new IntVector2(p.X, p.Y);
		}

		public static Size Convert(System.Drawing.Size p)
		{
			return new Size(p.Width, p.Height);
		}
	}

	static class LimeToSD
	{
		public static System.Drawing.Point Convert(IntVector2 p)
		{
			return new System.Drawing.Point(p.X, p.Y);
		}

		public static System.Drawing.Size Convert(Size p)
		{
			return new System.Drawing.Size(p.Width, p.Height);
		}
	}

	public class Window : CommonWindow, IWindow
	{
		private OpenTK.GLControl glControl;
		private Form form;
		private Timer timer;
		private Stopwatch stopwatch;
		private bool active;
		private System.Drawing.Point lastMousePosition;

		public Input Input { get; private set; }
		public bool Active { get { return active; } }
		public string Title { get { return form.Text; } set { form.Text = value; } }
		public WindowState State
		{
			get
			{
				if (form.FormBorderStyle == FormBorderStyle.None) {
					return WindowState.Fullscreen;
				} else if (form.WindowState == FormWindowState.Maximized) {
					return WindowState.Maximized;
				} else if (form.WindowState == FormWindowState.Minimized) {
					return WindowState.Minimized;
				} else {
					return WindowState.Normal;
				}
			}
			set
			{
				if (value == WindowState.Fullscreen) {
					form.WindowState = FormWindowState.Normal;
					form.FormBorderStyle = FormBorderStyle.None;
					form.WindowState = FormWindowState.Maximized;
				} else {
					form.FormBorderStyle = borderStyle;
					if (value == WindowState.Maximized) {
						form.WindowState = FormWindowState.Maximized;
					} else if (value == WindowState.Minimized) {
						form.WindowState = FormWindowState.Minimized;
					} else {
						form.WindowState = FormWindowState.Normal;
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

		public IntVector2 ClientPosition
		{
			get { return SDToLime.Convert(form.PointToScreen(new System.Drawing.Point(0, 0))); }
			set { DecoratedPosition = value + DecoratedPosition - ClientPosition; }
		}

		public Size ClientSize
		{
			get { return SDToLime.Convert(form.ClientSize); }
			set { form.ClientSize = LimeToSD.Convert(value); }
		}

		public IntVector2 DecoratedPosition
		{
			get { return SDToLime.Convert(form.Location); }
			set { form.Location = LimeToSD.Convert(value); }
		}

		public Size DecoratedSize
		{
			get { return SDToLime.Convert(form.Size); }
			set { form.Size = LimeToSD.Convert(value); }
		}

		public Size MinimumDecoratedSize
		{
			get { return SDToLime.Convert(form.MinimumSize); }
			set { form.MinimumSize = LimeToSD.Convert(value); }
		}

		public Size MaximumDecoratedSize
		{
			get { return SDToLime.Convert(form.MaximumSize); }
			set { form.MaximumSize = LimeToSD.Convert(value); }
		}

		FPSCounter fpsCounter = new FPSCounter();
		public float CalcFPS() { return fpsCounter.FPS; }

		public bool Visible
		{
			get { return form.Visible; }
			set { form.Visible = value; }
		}

		private MouseCursor cursor;
		public MouseCursor Cursor
		{
			get { return cursor; }
			set {
				cursor = value;
				form.Cursor = value.WinFormsCursor;
			}
		}

		public float PixelScale
		{
			get { return 1.0f; }
		}

		public void Center() { }
		public void Close() { }

		private class GLControl : OpenTK.GLControl
		{
			public GLControl(GraphicsMode mode, int major, int minor, GraphicsContextFlags flags)
				: base(mode, major, minor, flags)
			{

			}
			// Without this at least Left, Right, Up, Down and Tab keys are not submitted OnKeyDown
			protected override bool IsInputKey(Keys keyData)
			{
				return true;
			}
		}

		private static OpenTK.GLControl mainGLControl;

		private static OpenTK.GLControl CreateGLControl()
		{
			return new GLControl(OpenTK.Graphics.GraphicsMode.Default, 2, 0,
				Application.RenderingBackend == RenderingBackend.OpenGL ?
				OpenTK.Graphics.GraphicsContextFlags.Default :
				OpenTK.Graphics.GraphicsContextFlags.Embedded
			);
		}

		internal static void InitializeMainOpenGLContext()
		{
			OpenTK.Graphics.GraphicsContext.ShareContexts = false;
			mainGLControl = CreateGLControl();
			mainGLControl.MakeCurrent();
		}

		public Window()
			: this(new WindowOptions())
		{
		}

		FormBorderStyle borderStyle;

		public Window(WindowOptions options)
		{
			if (Application.MainWindow == null) {
				Application.MainWindow = this;
			} else if (Application.RenderingBackend == RenderingBackend.ES20) {
				// ES20 doesn't allow multiple contexts for now, because of a bug in OpenTK
				throw new Lime.Exception("Attempt to create a second window for ES20 rendering backend. Use OpenGL backend instead.");
			}
			Input = new Input();
			form = new Form();
			borderStyle = options.FixedSize ? FormBorderStyle.FixedSingle : FormBorderStyle.Sizable;
			form.FormBorderStyle = borderStyle;
			form.MaximizeBox = !options.FixedSize;
			if (Application.RenderingBackend == RenderingBackend.ES20) {
				glControl = mainGLControl;
			} else {
				glControl = CreateGLControl();
			}
			ClientSize = options.ClientSize;
			Title = options.Title;
			glControl.Dock = DockStyle.Fill;
			glControl.Paint += OnPaint;
			glControl.KeyDown += OnKeyDown;
			glControl.KeyUp += OnKeyUp;
			glControl.KeyPress += OnKeyPress;
			// Refresh mouse position of every frame to make HitTest work properly if mouse is outside of the screen.
			Updating += RefreshMousePosition;
			glControl.MouseDown += OnMouseDown;
			glControl.MouseUp += OnMouseUp;
			glControl.Resize += OnResize;
			glControl.Move += OnMove;
			glControl.MouseWheel += OnMouseWheel;
			form.Activated += OnActivated;
			form.Deactivate += OnDeactivate;
			form.FormClosing += OnClosing;
			form.FormClosed += OnClosed;
			form.Controls.Add(glControl);
			stopwatch = new Stopwatch();
			stopwatch.Start();
			timer = new Timer();
			timer.Interval = 1000 / 65;
			timer.Tick += OnTick;
			timer.Start();
			if (options.Icon != null) {
				form.Icon = (System.Drawing.Icon)options.Icon;
			}
			mainGLControl.MakeCurrent();
			Cursor = MouseCursor.Default;
			if (options.Visible) {
				Visible = true;
			}
		}

		private void OnMouseWheel(object sender, MouseEventArgs e)
		{
			Input.SetWheelScrollAmount(e.Delta);
		}

		private void OnClosed(object sender, FormClosedEventArgs e)
		{
			RaiseClosed();
			if (this == Application.MainWindow) {
				System.Windows.Forms.Application.Exit();
			}
		}

		private void OnClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = !RaiseClosing();
		}

		private void OnMove(object sender, EventArgs e)
		{
			RaiseMoved();
		}

		private void OnActivated(object sender, EventArgs e)
		{
			active = true;
			glControl.Invalidate();
			RaiseActivated();
		}

		private void OnDeactivate(object sender, EventArgs e)
		{
			foreach (var key in (Key[])Enum.GetValues(typeof(Key))) {
				Input.SetKeyState(key, false);
			}
			active = false;
			RaiseDeactivated();
		}

		private WindowState prevWindowState;
		private void OnResize(object sender, EventArgs e)
		{
			// We should ingore this event after minimize or unminimize
			// Calling to RaiseResized() after minimize can lead to various bugs because window size is 0x0
			// Calling to RaiseResized() after unminimize leads to useless screen recreating and some screen state loss
			// (like scroll position in scrollview)
			bool HasBeenMinizedOrUnminimized = 
				(prevWindowState != WindowState.Minimized && State == WindowState.Minimized) ||
				(prevWindowState == WindowState.Minimized && State != WindowState.Minimized);
			prevWindowState = State;
			if (!HasBeenMinizedOrUnminimized) {
				RaiseResized();
			}
		}

		private void OnMouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) {
				Input.SetKeyState(Key.Mouse0, true);
				Input.SetKeyState(Key.Touch0, true);
			} else if (e.Button == MouseButtons.Right) {
				Input.SetKeyState(Key.Mouse1, true);
			} else if (e.Button == MouseButtons.Middle) {
				Input.SetKeyState(Key.Mouse2, true);
			}
		}

		private void OnMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) {
				Input.SetKeyState(Key.Mouse0, false);
				Input.SetKeyState(Key.Touch0, false);
			} else if (e.Button == MouseButtons.Right) {
				Input.SetKeyState(Key.Mouse1, false);
			} else if (e.Button == MouseButtons.Middle) {
				Input.SetKeyState(Key.Mouse2, false);
			}
		}

		private void RefreshMousePosition(float delta)
		{
			if (lastMousePosition == Control.MousePosition) {
				return;
			}
			lastMousePosition = Control.MousePosition;
			var position = (Vector2)SDToLime.Convert(form.PointToClient(Control.MousePosition));
			Input.MousePosition = position * Input.ScreenToWorldTransform;
			Input.SetTouchPosition(0, Input.MousePosition);
		}

		private void OnTick(object sender, EventArgs e)
		{
			glControl.Invalidate();
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			var k = TranslateKey(e.KeyCode);
			if (k != Key.Unknown) {
				Input.SetKeyState(k, true);
			}
		}

		private void OnKeyUp(object sender, KeyEventArgs e)
		{
			var k = TranslateKey(e.KeyCode);
			if (k != Key.Unknown) {
				Input.SetKeyState(k, false);
			}
		}

		private void OnKeyPress(object sender, KeyPressEventArgs e)
		{
			Input.TextInput += e.KeyChar;
		}

		private void OnPaint(object sender, PaintEventArgs e)
		{
			// Do not allow updating if modal dialog is shown, otherwise it will lead to recursive OnPaint.
			if (form.Visible && form.CanFocus) {
				var delta = (float)stopwatch.Elapsed.TotalSeconds;
				stopwatch.Restart();
				delta = Mathf.Clamp(delta, 0, 1 / Application.LowFPSLimit);
				Input.ProcessPendingKeyEvents();
				RaiseUpdating(delta);
				AudioSystem.Update();
				Input.TextInput = null;
				Input.CopyKeysState();
				fpsCounter.Refresh();
				mainGLControl.Context.MakeCurrent(glControl.WindowInfo);
				RaiseRendering();
				mainGLControl.SwapBuffers();
			}
		}

		private static Key TranslateKey(Keys key)
		{
			switch (key) {
				case Keys.Oem1:
					return Key.Semicolon;
				case Keys.Oem2:
					return Key.Slash;
				case Keys.Oem7:
					return Key.Quote;
				case Keys.Oem4:
					return Key.BracketLeft;
				case Keys.Oem6:
					return Key.BracketRight;
				case Keys.Oem5:
					return Key.BackSlash;
				case Keys.D0:
					return Key.Number0;
				case Keys.D1:
					return Key.Number1;
				case Keys.D2:
					return Key.Number2;
				case Keys.D3:
					return Key.Number3;
				case Keys.D4:
					return Key.Number4;
				case Keys.D5:
					return Key.Number5;
				case Keys.D6:
					return Key.Number6;
				case Keys.D7:
					return Key.Number7;
				case Keys.D8:
					return Key.Number8;
				case Keys.D9:
					return Key.Number9;
				case Keys.Oem3:
					return Key.Tilde;
				case Keys.Q:
					return Key.Q;
				case Keys.W:
					return Key.W;
				case Keys.E:
					return Key.E;
				case Keys.R:
					return Key.R;
				case Keys.T:
					return Key.T;
				case Keys.Y:
					return Key.Y;
				case Keys.U:
					return Key.U;
				case Keys.I:
					return Key.I;
				case Keys.O:
					return Key.O;
				case Keys.P:
					return Key.P;
				case Keys.A:
					return Key.A;
				case Keys.S:
					return Key.S;
				case Keys.D:
					return Key.D;
				case Keys.F:
					return Key.F;
				case Keys.G:
					return Key.G;
				case Keys.H:
					return Key.H;
				case Keys.J:
					return Key.J;
				case Keys.K:
					return Key.K;
				case Keys.L:
					return Key.L;
				case Keys.Z:
					return Key.Z;
				case Keys.X:
					return Key.X;
				case Keys.C:
					return Key.C;
				case Keys.V:
					return Key.V;
				case Keys.B:
					return Key.B;
				case Keys.N:
					return Key.N;
				case Keys.M:
					return Key.M;
				case Keys.F1:
					return Key.F1;
				case Keys.F2:
					return Key.F2;
				case Keys.F3:
					return Key.F3;
				case Keys.F4:
					return Key.F4;
				case Keys.F5:
					return Key.F5;
				case Keys.F6:
					return Key.F6;
				case Keys.F7:
					return Key.F7;
				case Keys.F8:
					return Key.F8;
				case Keys.F9:
					return Key.F9;
				case Keys.F10:
					return Key.F10;
				case Keys.F11:
					return Key.F11;
				case Keys.F12:
					return Key.F12;
				case Keys.Left:
					return Key.Left;
				case Keys.Right:
					return Key.Right;
				case Keys.Up:
					return Key.Up;
				case Keys.Down:
					return Key.Down;
				case Keys.Space:
					return Key.Space;
				case Keys.Return:
					return Key.Enter;
				case Keys.Delete:
					return Key.Delete;
				case Keys.Back:
					return Key.BackSpace;
				case Keys.PageUp:
					return Key.PageUp;
				case Keys.PageDown:
					return Key.PageDown;
				case Keys.Home:
					return Key.Home;
				case Keys.End:
					return Key.End;
				case Keys.Pause:
					return Key.Pause;
				case Keys.Menu:
					return Key.AltLeft;
				case Keys.ControlKey:
					return Key.ControlLeft;
				case Keys.ShiftKey:
					return Key.LShift;
				case Keys.LShiftKey:
					return Key.LShift;
				case Keys.RShiftKey:
					return Key.RShift;
				case Keys.Tab:
					return Key.Tab;
				case Keys.Escape:
					return Key.Escape;
				case Keys.Oemplus:
					return Key.Plus;
				case Keys.OemMinus:
					return Key.Minus;
				case Keys.OemPeriod:
					return Key.Period;
				case Keys.Oemcomma:
					return Key.Comma;
				default:
					return Key.Unknown;
			}
		}
	}
}
#endif