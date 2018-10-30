 #if WIN
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenTK.Graphics;
using WinFormsCloseReason = System.Windows.Forms.CloseReason;

namespace Lime
{
	public class Window : CommonWindow, IWindow
	{
		private ManualResetEvent renderReady = new ManualResetEvent(false);
		private ManualResetEvent renderCompleted = new ManualResetEvent(true);

		private Thread renderThread;
		private CancellationTokenSource renderThreadTokenSource;
		private CancellationToken renderThreadToken;

		// This line only suppresses warning: "Window.Current: a name can be simplified".
		public new static IWindow Current => CommonWindow.Current;

		// We must perform no more than single render per update.
		// So we defer Invalidate() calls until next Update().
		private enum RenderingState
		{
			Updated,
			RenderDeferred,
			Rendered,
		}
		private readonly System.Windows.Forms.Timer timer;
		private GLControl glControl;
		private Form form;
		private Stopwatch stopwatch;
		private bool active;
		private RenderingState renderingState = RenderingState.Rendered;
		private Point lastMousePosition;
		private bool isInvalidated;
		private bool vSync;

		public WindowInput Input { get; private set; }

		public bool Active => active;
		public Form Form => form;

		public string Title
		{
			get { return form.Text; }
			set { form.Text = value; }
		}

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

		public bool FixedSize
		{
			get
			{
				return borderStyle != FormBorderStyle.Sizable;
			}

			set
			{
				if (value && borderStyle == FormBorderStyle.Sizable) {
					borderStyle = FormBorderStyle.FixedSingle;
				} else if (!value && borderStyle == FormBorderStyle.FixedSingle) {
					borderStyle = FormBorderStyle.Sizable;
				}

				if (form.FormBorderStyle != FormBorderStyle.None) {
					form.FormBorderStyle = borderStyle;
				}
				form.MaximizeBox = !FixedSize;
			}
		}

		public bool Fullscreen
		{
			get
			{
				return State == WindowState.Fullscreen;
			}

			set
			{
				if (value && State == WindowState.Fullscreen || !value && State != WindowState.Fullscreen) {
					return;
				}
				State = value ? WindowState.Fullscreen : WindowState.Normal;
			}
		}

		public Vector2 ClientPosition
		{
			get { return SDToLime.Convert(glControl.PointToScreen(new Point(0, 0)), PixelScale); }
			set { DecoratedPosition = value + DecoratedPosition - ClientPosition; }
		}

		public Vector2 ClientSize
		{
			get { return SDToLime.Convert(glControl.ClientSize, PixelScale); }
			set { DecoratedSize = value + DecoratedSize - ClientSize; }
		}

		public Vector2 DecoratedPosition
		{
			get { return SDToLime.Convert(form.Location, PixelScale); }
			set { form.Location = LimeToSD.ConvertToPoint(value, PixelScale); }
		}

		public Vector2 DecoratedSize
		{
			get { return SDToLime.Convert(form.Size, PixelScale); }
			set { form.Size = LimeToSD.ConvertToSize(value, PixelScale); }
		}

		public Vector2 MinimumDecoratedSize
		{
			get { return SDToLime.Convert(form.MinimumSize, PixelScale); }
			set { form.MinimumSize = LimeToSD.ConvertToSize(value, PixelScale); }
		}

		public Vector2 MaximumDecoratedSize
		{
			get { return SDToLime.Convert(form.MaximumSize, PixelScale); }
			set { form.MaximumSize = LimeToSD.ConvertToSize(value, PixelScale); }
		}

		public Vector2 WorldToWindow(Vector2 wp)
		{
			var sp = LimeToSD.ConvertToPoint(wp, PixelScale);
			return new Vector2(sp.X + glControl.Left, sp.Y + glControl.Top);
		}

		public Vector2 LocalToDesktop(Vector2 localPosition)
		{
			return SDToLime.Convert(
				glControl.PointToScreen(LimeToSD.ConvertToPoint(localPosition, PixelScale)),
				PixelScale
			);
		}

		public Vector2 DesktopToLocal(Vector2 desktopPosition)
		{
			return SDToLime.Convert(
				glControl.PointToClient(new Point((int) desktopPosition.X, (int) desktopPosition.Y)),
				PixelScale
			);
		}

		public bool AsyncRendering { get; }

		public float UnclampedDelta { get; private set; }

		FPSCounter fpsCounter = new FPSCounter();
		public float FPS { get { return fpsCounter.FPS; } }

		[Obsolete("Use FPS property instead", true)]
		public float CalcFPS() { return fpsCounter.FPS; }

		public bool Visible
		{
			get { return form.Visible; }
			set
			{
				RaiseVisibleChanging(value, false);
				form.Visible = value;
			}
		}

		private MouseCursor cursor;
		public MouseCursor Cursor
		{
			get { return cursor; }
			set
			{
				cursor = value;
				if (form.Cursor != value.NativeCursor) {
					form.Cursor = value.NativeCursor;
				}
			}
		}

		public float PixelScale { get; private set; }

		public void Center()
		{
			var screen = Screen.FromControl(form).WorkingArea;
			var x = (int)((screen.Width / PixelScale - DecoratedSize.X) / 2);
			var y = (int)((screen.Height / PixelScale - DecoratedSize.Y) / 2);
			var position = new Vector2(screen.X + x, screen.Y + y);
			DecoratedPosition = position;
		}

		public void Close()
		{
			RaiseVisibleChanging(false, false);
			form.Close();
		}

		private class GLControl : OpenTK.GLControl
		{
			public event Action BeforeBoundsChanged;

			public GLControl(GraphicsMode mode, int major, int minor, GraphicsContextFlags flags)
				: base(mode, major, minor, flags)
			{

			}
			// Without this at least Left, Right, Up, Down and Tab keys are not submitted OnKeyDown
			protected override bool IsInputKey(Keys keyData)
			{
				return true;
			}

			protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
			{
				BeforeBoundsChanged?.Invoke();
				base.SetBoundsCore(x, y, width, height, specified);
			}
		}

		private static GLControl CreateGLControl()
		{
			return new GLControl(new GraphicsMode(32, 16, 8), 2, 0,
				Application.RenderingBackend == RenderingBackend.OpenGL ?
					OpenTK.Graphics.GraphicsContextFlags.Default :
					OpenTK.Graphics.GraphicsContextFlags.Embedded
			);
		}

		static Window()
		{
			GraphicsContext.ShareContexts = true;
		}

		public Window()
			: this(new WindowOptions())
		{
		}

		FormBorderStyle borderStyle;
		private WindowState prevWindowState;

		public Window(WindowOptions options)
		{
			if (Application.MainWindow != null && Application.RenderingBackend == RenderingBackend.ES20) {
				// ES20 doesn't allow multiple contexts for now, because of a bug in OpenTK
				throw new Lime.Exception("Attempt to create a second window for ES20 rendering backend. Use OpenGL backend instead.");
			}
			if (options.UseTimer && options.AsyncRendering) {
				throw new Lime.Exception("Can't use both timer and async rendering");
			}
			if (options.ToolWindow) {
				form = new ToolForm();
			} else {
				form = new Form();
			}
			Input  = new WindowInput(this);
			using (var graphics = form.CreateGraphics()) {
				PixelScale = CalcPixelScale(graphics.DpiX);
			}
			if (options.Style == WindowStyle.Borderless) {
				borderStyle = FormBorderStyle.None;
			} else {
				borderStyle = options.FixedSize ? FormBorderStyle.FixedSingle : FormBorderStyle.Sizable;
			}
			form.FormBorderStyle = borderStyle;
			form.MaximizeBox = !options.FixedSize;
			if (options.MinimumDecoratedSize != Vector2.Zero) {
				MinimumDecoratedSize = options.MinimumDecoratedSize;
			}
			if (options.MaximumDecoratedSize != Vector2.Zero) {
				MaximumDecoratedSize = options.MaximumDecoratedSize;
			}
			glControl = CreateGLControl();
			glControl.CreateControl();
			glControl.Context.MakeCurrent(null);
			glControl.Dock = DockStyle.Fill;
			glControl.Paint += OnPaint;
			glControl.KeyDown += OnKeyDown;
			glControl.KeyUp += OnKeyUp;
			glControl.KeyPress += OnKeyPress;
			glControl.MouseDown += OnMouseDown;
			glControl.MouseUp += OnMouseUp;
			glControl.Resize += OnResize;
			glControl.MouseWheel += OnMouseWheel;
			glControl.MouseEnter += (sender, args) => {
				Application.WindowUnderMouse = this;
			};
			glControl.MouseLeave += (sender, args) => {
				if (Application.WindowUnderMouse == this) {
					Application.WindowUnderMouse = null;
				}
			};
			glControl.BeforeBoundsChanged += WaitForRendering;
			form.Move += OnMove;
			form.Activated += OnActivated;
			form.Deactivate += OnDeactivate;
			form.FormClosing += OnClosing;
			form.FormClosed += OnClosed;
			active = Form.ActiveForm == form;

			if (options.UseTimer) {
				timer = new System.Windows.Forms.Timer {
					Interval = (int)(1000.0 / 65),
					Enabled = true,
				};
				timer.Tick += OnTick;
			} else {
				vSync = options.VSync;
				glControl.MakeCurrent();
				glControl.VSync = vSync;
				glControl.Context.MakeCurrent(null);
				System.Windows.Forms.Application.Idle += OnTick;
			}

			form.Controls.Add(glControl);
			stopwatch = new Stopwatch();
			stopwatch.Start();

			if (options.Icon != null) {
				form.Icon = (System.Drawing.Icon)options.Icon;
			}
			Cursor = MouseCursor.Default;
			Title = options.Title;
			ClientSize = options.ClientSize;
			if (options.Visible) {
				Visible = true;
			}
			if (options.Screen != null && options.Screen >= 0 && Screen.AllScreens.Length > options.Screen) {
				form.Location = GetCenter(Screen.AllScreens[options.Screen.Value].WorkingArea);
			}
			if (options.Centered) {
				Center();
			}
			if (Application.MainWindow == null) {
				Application.MainWindow = this;
				Closing += reason => Application.DoExiting();
				Closed += Application.DoExited;
			} else {
				Form.Owner = Application.MainWindow.Form;
				Form.StartPosition = FormStartPosition.CenterParent;
			}
			AsyncRendering = options.AsyncRendering;
			if (AsyncRendering) {
				renderThreadTokenSource = new CancellationTokenSource();
				renderThreadToken = renderThreadTokenSource.Token;
				renderThread = new Thread(RenderLoop);
				renderThread.IsBackground = true;
				renderThread.Start();
			}
			Application.Windows.Add(this);
		}

		public override bool VSync
		{
			get
			{
				return vSync;
			}

			set
			{
				if (vSync != value && timer == null) {
					vSync = value;
					WaitForRendering();
					glControl.MakeCurrent();
					glControl.VSync = value;
					glControl.Context.MakeCurrent(null);
				}
			}
		}

		public void ShowModal()
		{
			using (Context.Activate().Scoped()) {
				RaiseVisibleChanging(true, true);
				form.ShowDialog();
				RaiseVisibleChanging(false, true);
			};
		}

		public void Activate()
		{
			form.Activate();
		}

		/// <summary>
		/// Gets the display device containing the largest portion of this window.
		/// </summary>
		public IDisplay Display => Lime.Display.GetDisplay(Screen.FromControl(form));

		private static Point GetCenter(System.Drawing.Rectangle rect)
		{
			return new Point(
				rect.X + (rect.Width / 2),
				rect.Y + (rect.Height / 2)
			);
		}

		private static float CalcPixelScale(float Dpi)
		{
			return Dpi * 1f / 96f;
		}

		private void OnMouseWheel(object sender, MouseEventArgs e)
		{
			Input.SetWheelScrollAmount(e.Delta);
		}

		private void OnClosed(object sender, FormClosedEventArgs e)
		{
			if (AsyncRendering) {
				renderThreadTokenSource.Cancel();
				renderReady.Set();
				renderThread.Join();
			}
			RaiseClosed();
			Application.Windows.Remove(this);
			if (this == Application.MainWindow) {
				System.Windows.Forms.Application.Exit();
			}
			if (timer == null) {
				System.Windows.Forms.Application.Idle -= OnTick;
			} else {
				timer.Dispose();
			}
		}

		private void OnClosing(object sender, FormClosingEventArgs e)
		{
			CloseReason reason;
			switch (e.CloseReason) {
				case WinFormsCloseReason.None:
					reason = CloseReason.Unknown;
					break;
				case WinFormsCloseReason.WindowsShutDown:
				case WinFormsCloseReason.MdiFormClosing:
				case WinFormsCloseReason.TaskManagerClosing:
				case WinFormsCloseReason.FormOwnerClosing:
				case WinFormsCloseReason.ApplicationExitCall:
					reason = CloseReason.MainWindowClosing;
					break;
				case WinFormsCloseReason.UserClosing:
					reason = CloseReason.UserClosing;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			e.Cancel = !RaiseClosing(reason);
		}

		private void OnMove(object sender, EventArgs e)
		{
			bool hasBeenMinimized, hasBeenRestored;
			HasWindowStateChanged(out hasBeenMinimized, out hasBeenRestored);

			// We should ignore this event after minimize or restore.
			// Calling to RaiseMoved() after minimize can lead to various bugs because window position is negative
			if (!hasBeenRestored && !hasBeenMinimized) {
				RaiseMoved();
			}
		}

		private void OnActivated(object sender, EventArgs e)
		{
			if (!active) {
				active = true;
				RaiseActivated();
			}
		}

		private void OnDeactivate(object sender, EventArgs e)
		{
			if (active) {
				active = false;
				// Andrey Tyshchenko: clear Input state, the next activated window
				// will receive KeyDown event and restore Input state
				Input.ClearKeyState(clearMouseButtons: false);
				RaiseDeactivated();
			}
		}

		private void OnResize(object sender, EventArgs e)
		{
			bool hasBeenMinimized, hasBeenRestored;
			HasWindowStateChanged(out hasBeenMinimized, out hasBeenRestored);
			prevWindowState = State;

			// This will produce extra invokes, but will keep "active" flag in consistant state when minimizing app by
			// clicking on taskbar icon
			if (hasBeenRestored) {
				OnActivated(this, EventArgs.Empty);
			}
			if (hasBeenMinimized) {
				OnDeactivate(this, EventArgs.Empty);
			}

			// We should ignore this event after minimize or restore.
			// Calling to RaiseResized() after minimize can lead to various bugs because window size is 0x0
			if (!hasBeenRestored && !hasBeenMinimized) {
				RaiseResized(deviceRotated: false);
			}
		}

		private void HasWindowStateChanged(out bool hasBeenMinimized, out bool hasBeenRestored)
		{
			hasBeenMinimized = prevWindowState != WindowState.Minimized && State == WindowState.Minimized;
			hasBeenRestored = prevWindowState == WindowState.Minimized && State != WindowState.Minimized;
		}

		private void OnMouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) {
				if (e.Clicks == 1) {
					Input.SetKeyState(Key.Mouse0, true);
					Input.SetKeyState(Key.Touch0, true);
				} else if (e.Clicks == 2) {
					Input.SetKeyState(Key.Mouse0DoubleClick, true);
				}
			} else if (e.Button == MouseButtons.Right) {
				if (e.Clicks == 1) {
					Input.SetKeyState(Key.Mouse1, true);
				} else if (e.Clicks == 2) {
					Input.SetKeyState(Key.Mouse1DoubleClick, true);
				}
			} else if (e.Button == MouseButtons.Middle) {
				Input.SetKeyState(Key.Mouse2, true);
			}
			else if (e.Button == MouseButtons.XButton1) {
				Input.SetKeyState(Key.MouseBack, true);
			}
			else if (e.Button == MouseButtons.XButton2) {
				Input.SetKeyState(Key.MouseForward, true);
			}
			Input.SetKeyState(Key.Control, Control.ModifierKeys.HasFlag(Keys.Control));
			Input.SetKeyState(Key.Shift, Control.ModifierKeys.HasFlag(Keys.Shift));
			Input.SetKeyState(Key.Alt, Control.ModifierKeys.HasFlag(Keys.Alt));
			Input.SetKeyState(Key.Win, Control.ModifierKeys.HasFlag(Keys.LWin) || Control.ModifierKeys.HasFlag(Keys.RWin));
		}

		private void OnMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) {
				Input.SetKeyState(Key.Mouse0, false);
				Input.SetKeyState(Key.Touch0, false);
				Input.SetKeyState(Key.Mouse0DoubleClick, false);
			} else if (e.Button == MouseButtons.Right) {
				Input.SetKeyState(Key.Mouse1, false);
				Input.SetKeyState(Key.Mouse1DoubleClick, false);
			} else if (e.Button == MouseButtons.Middle) {
				Input.SetKeyState(Key.Mouse2, false);
			}
			else if (e.Button == MouseButtons.XButton1) {
				Input.SetKeyState(Key.MouseBack, false);
			}
			else if (e.Button == MouseButtons.XButton2) {
				Input.SetKeyState(Key.MouseForward, false);
			}
		}

		private void RefreshMousePosition()
		{
			if (lastMousePosition == Control.MousePosition) {
				return;
			}
			lastMousePosition = Control.MousePosition;
			Application.Input.DesktopMousePosition = new Vector2(lastMousePosition.X, lastMousePosition.Y);
			Application.Input.SetDesktopTouchPosition(0, Application.Input.DesktopMousePosition);
		}

		private void OnTick(object sender, EventArgs e)
		{
			Update();
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			var k = TranslateKey(e.KeyCode);
			if (k != Key.Unknown) {
				Input.SetKeyState(k, true);
			}
			if ((e.Modifiers & Keys.Shift) != 0) {
				Input.SetKeyState(Key.Shift, true);
			}
			if ((e.Modifiers & Keys.Alt) != 0) {
				Input.SetKeyState(Key.Alt, true);
			}
			if ((e.Modifiers & Keys.Control) != 0) {
				Input.SetKeyState(Key.Control, true);
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

		private void RenderLoop()
		{
			while (true) {
				renderReady.WaitOne();
				renderReady.Reset();
				if (renderThreadToken.IsCancellationRequested) {
					return;
				}
				glControl.MakeCurrent();
				RaiseRendering();
				glControl.SwapBuffers();
				glControl.Context.MakeCurrent(null);
				renderCompleted.Set();
			}
		}

		private void WaitForRendering()
		{
			if (AsyncRendering) {
				renderCompleted.WaitOne();
			}
		}

		private void OnPaint(object sender, PaintEventArgs e)
		{
			switch (renderingState) {
				case RenderingState.Updated:
					PixelScale = CalcPixelScale(e.Graphics.DpiX);
					if (!AsyncRendering && glControl.IsHandleCreated && form.Visible && !glControl.IsDisposed) {
						glControl.MakeCurrent();
						RaiseRendering();
						glControl.SwapBuffers();
					}
					renderingState = RenderingState.Rendered;
					break;
				case RenderingState.Rendered:
					renderingState = RenderingState.RenderDeferred;
					break;
				case RenderingState.RenderDeferred:
					break;
			}
		}

		private void Update()
		{
			var wasInvalidated = isInvalidated;
			isInvalidated = false;
			if (!form.Visible || !form.CanFocus) {
				return;
			}
			UnclampedDelta = (float)stopwatch.Elapsed.TotalSeconds;
			float delta = Mathf.Clamp(UnclampedDelta, 0, Application.MaxDelta);
			stopwatch.Restart();
			if (this == Application.MainWindow && Application.MainMenu != null) {
				Application.MainMenu.Refresh();
			}
			fpsCounter.Refresh();
			// Refresh mouse position of every frame to make HitTest work properly if mouse is outside of the screen.
			RefreshMousePosition();
			if (active || Input.IsSimulationRunning) {
				Input.ProcessPendingKeyEvents(delta);
			}
			RaiseUpdating(delta);
			AudioSystem.Update();
			if (active || Input.IsSimulationRunning) {
				Input.CopyKeysState();
				Input.TextInput = null;
			}
			if (wasInvalidated || renderingState == RenderingState.RenderDeferred) {
				glControl.Invalidate();
			}
			renderingState = RenderingState.Updated;
			if (AsyncRendering) {
				renderCompleted.WaitOne();
				renderCompleted.Reset();
			}
			RaiseSync();
			if (AsyncRendering) {
				renderReady.Set();
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
					return Key.LBracket;
				case Keys.Oem6:
					return Key.RBracket;
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
				case Keys.Insert:
					return Key.Insert;
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
					return Key.Alt;
				case Keys.ControlKey:
					return Key.Control;
				case Keys.ShiftKey:
					return Key.Shift;
				case Keys.Apps:
					return Key.Menu;
				case Keys.Tab:
					return Key.Tab;
				case Keys.Escape:
					return Key.Escape;
				case Keys.Oemplus:
					return Key.EqualsSign;
				case Keys.OemMinus:
					return Key.Minus;
				case Keys.OemPeriod:
					return Key.Period;
				case Keys.Oemcomma:
					return Key.Comma;
				case Keys.NumPad0:
					return Key.Keypad0;
				case Keys.NumPad1:
					return Key.Keypad1;
				case Keys.NumPad2:
					return Key.Keypad2;
				case Keys.NumPad3:
					return Key.Keypad3;
				case Keys.NumPad4:
					return Key.Keypad4;
				case Keys.NumPad5:
					return Key.Keypad5;
				case Keys.NumPad6:
					return Key.Keypad6;
				case Keys.NumPad7:
					return Key.Keypad7;
				case Keys.NumPad8:
					return Key.Keypad8;
				case Keys.NumPad9:
					return Key.Keypad9;
				case Keys.Multiply:
					return Key.KeypadMultiply;
				case Keys.Add:
					return Key.KeypadPlus;
				case Keys.Decimal:
					return Key.KeypadDecimal;
				case Keys.Subtract:
					return Key.KeypadMinus;
				case Keys.Divide:
					return Key.KeypadDivide;

				default:
					return Key.Unknown;
			}
		}

		public void Invalidate()
		{
			isInvalidated = true;
		}

		internal void SetMenu(Menu menu)
		{
			if (form.MainMenuStrip != null) {
				form.Controls.Remove(form.MainMenuStrip);
				form.MainMenuStrip = null;
			}
			if (menu != null) {
				menu.Refresh();
				form.Controls.Add(menu.NativeMainMenu);
				form.MainMenuStrip = menu.NativeMainMenu;
			}
		}

		public bool AllowDropFiles
		{
			get { return form.AllowDrop; }
			set
			{
				if (form.AllowDrop != value) {
					form.AllowDrop = value;
					if (value) {
						form.DragEnter += Form_DragEnter;
						form.DragDrop += Form_DragDrop;
						form.QueryContinueDrag += Form_QueryContinueDrag;
					} else {
						form.DragEnter -= Form_DragEnter;
						form.DragDrop -= Form_DragDrop;
						form.QueryContinueDrag -= Form_QueryContinueDrag;
					}
				}
			}
		}

		private void Form_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
		{
			if (e.Action == DragAction.Drop) {
				Input.SetKeyState(Key.Mouse0, false);
				Input.SetKeyState(Key.Mouse1, false);
				Input.SetKeyState(Key.Mouse2, false);
				Input.SetKeyState(Key.Touch0, false);
				Input.SetKeyState(Key.Touch1, false);
				Input.SetKeyState(Key.Touch2, false);
				Input.SetKeyState(Key.Touch3, false);
			}
		}

		private void Form_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				e.Effect = DragDropEffects.All;
			}
		}

		private void Form_DragDrop(object sender, DragEventArgs e)
		{
			var files = ((string[])e.Data.GetData(DataFormats.FileDrop, false));
			using (Context.Activate().Scoped()) {
				Application.WindowUnderMouse = this;
				FilesDropped?.Invoke(files);
			}
		}

		public event Action<IEnumerable<string>> FilesDropped;
		public void DragFiles(string[] filenames)
		{
			var dragObject = new DataObject(DataFormats.FileDrop, filenames);
			form.DoDragDrop(dragObject, DragDropEffects.All);
		}
	}

	static class SDToLime
	{
		public static Vector2 Convert(Point p, float pixelScale)
		{
			return new Vector2(p.X, p.Y) / pixelScale;
		}
		public static Vector2 Convert(System.Drawing.Size p, float pixelScale)
		{
			return new Vector2(p.Width, p.Height) / pixelScale;
		}
	}

	static class LimeToSD
	{
		public static Point ConvertToPoint(Vector2 p, float pixelScale)
		{
			return (Point)ConvertToSize(p, pixelScale);
		}
		public static System.Drawing.Size ConvertToSize(Vector2 p, float pixelScale)
		{
			p = (p * pixelScale);
			return new System.Drawing.Size(p.X.Round(), p.Y.Round());
		}
	}

	public class ToolForm : Form
	{
		public ToolForm() : base()
		{
			this.ShowInTaskbar = false;
		}

		protected override CreateParams CreateParams
		{
			get {
				CreateParams cp = base.CreateParams;
				cp.Style &= ~0x20000;
				return cp;
			}
		}
	}
}
#endif
