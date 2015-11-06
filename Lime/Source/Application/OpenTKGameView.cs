#if MONOMAC || MAC
using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
#if !MAC
using OpenTK.Graphics.ES20;
#endif
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;

namespace Lime
{
	public class GameWindow : IWindow
	{
		private Dictionary<string, MouseCursor> cursors = new Dictionary<string, MouseCursor>();
		private MouseCursor currentCursor;
		private OpenTK.GameWindow otkWindow;

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

		public IntVector2 ClientPosition
		{
			get { return new IntVector2(otkWindow.ClientRectangle.X, otkWindow.ClientRectangle.Y); }
			set
			{
				var rc = otkWindow.ClientRectangle;
				rc.X = value.X;
				rc.Y = value.Y;
				otkWindow.ClientRectangle = rc;
			}
		}

		public bool Visible
		{
			get { return otkWindow.Visible; }
			set { otkWindow.Visible = value; }
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

		public float FPS
		{
			get { return FPSCalculator.FPS; }
		}

		public Input Input { get; private set; }

		public event Action Activated;
		public event Action Deactivated;
		public event Func<bool> Closing;
		public event Action Closed;
		public event Action Moved;
		public event Action Resized;
		public event Action<float> Updating;
		public event Action Rendering;

#if WIN
		static GameWindow()
		{
			// This is workaround an OpenTK bug.
			// On some video cards the SDL framework could not create a GLES2/Angle OpenGL context
			// if context attributes weren't set before the main window creation.
			if (Application.RenderingBackend == RenderingBackend.ES20) {
				Sdl2.Init(Sdl2.SystemFlags.VIDEO);
				Sdl2.SetAttribute(Sdl2.ContextAttribute.CONTEXT_PROFILE_MASK, 4);
				Sdl2.SetAttribute(Sdl2.ContextAttribute.CONTEXT_MAJOR_VERSION, 2);
				Sdl2.SetAttribute(Sdl2.ContextAttribute.CONTEXT_MINOR_VERSION, 0);
			}
		}
#endif

		public GameWindow(WindowOptions options)
		{
			Input = new Input();
			otkWindow = new OpenTK.GameWindow(options.Size.Width, options.Size.Height,
				new GraphicsMode(new ColorFormat(32), depth: 24), options.Title, GetWindowFlags(options)
#if WIN
				, DisplayDevice.Default, 2, 0, GetGraphicContextFlags()
#endif
			);
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
			if (options.Icon != null) {
				otkWindow.Icon = options.Icon as System.Drawing.Icon;
			}
			SetupWindowPositionAndSize();
			// Setting fullscreen with GameWindowFlags.FullScreen in OpenTK.GameWindow constructor causes
			// changing the display resolution. Desktop fullscreen is much more better.
			this.State = options.FullScreen ? WindowState.Fullscreen : WindowState.Normal;
		}

		private static GameWindowFlags GetWindowFlags(WindowOptions options)
		{
			return options.FixedSize ? GameWindowFlags.FixedWindow : GameWindowFlags.Default;
		}

#if !MAC
		private static GraphicsContextFlags GetGraphicContextFlags()
		{
			return Application.RenderingBackend == RenderingBackend.OpenGL ? 
				 GraphicsContextFlags.Default : GraphicsContextFlags.Embedded;
		}
#endif

		private void SetupWindowPositionAndSize()
		{
			var displayBounds = OpenTK.DisplayDevice.Default.Bounds;
			if (CommandLineArgs.FullscreenMode) {
				otkWindow.WindowState = OpenTK.WindowState.Fullscreen;
			} else if (CommandLineArgs.MaximizedWindow) {
				otkWindow.Location = displayBounds.Location;
				otkWindow.WindowState = OpenTK.WindowState.Maximized;
			} else {
				Center();
			}
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

		public RenderingBackend RenderingApi {
			get
			{
				if (CommandLineArgs.OpenGL) {
					return RenderingBackend.OpenGL;
				} else {
					return RenderingBackend.ES20;
				}
			}
		}

		void HandleKeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
		{
			// SDL backend bug: OpenTK doesn't send key press event for backspace
			if (e.Key == OpenTK.Input.Key.BackSpace) {
				Input.TextInput += '\b';
			}
			Input.SetKeyState((Key)e.Key, true);
#if MAC
			//There is no KeyUp event for regular key on Mac if Command key pressed, so we release it manualy in the same frame
			if ((Input.IsKeyPressed(Key.LWin) || Input.IsKeyPressed(Key.RWin))) {
				Input.SetKeyState((Key)e.Key, false);
			}
#endif
		}

		void HandleKeyUp(object sender, KeyboardKeyEventArgs e)
		{
			Input.SetKeyState((Key)e.Key, false);
		}

		void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			Input.TextInput += e.KeyChar;
		}

		private void HandleFocusedChanged(object sender, EventArgs e)
		{
			Active = otkWindow.Focused;
			if (otkWindow.Focused) {
				AudioSystem.Active = true;
				if (Activated != null) {
					Activated();
				}
			} else {
				AudioSystem.Active = false;
				if (Deactivated != null) {
					Deactivated();
				}
			}
		}

		void HandleMouseButtonUp(object sender, MouseButtonEventArgs e)
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

		void HandleMouseButtonDown(object sender, MouseButtonEventArgs e)
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

		void HandleMouseMove(object sender, MouseMoveEventArgs e)
		{
			Vector2 position = new Vector2(e.X, e.Y) * Input.ScreenToWorldTransform;
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

		private void HandleClosing(object sender, CancelEventArgs e)
		{
			if (Closing != null) {
				e.Cancel = !Closing();
			}
		}

		private void HandleClosed(object sender, EventArgs e)
		{
			if (Closed != null) {
				Closed();
			}
			TexturePool.Instance.DiscardAllTextures();
			AudioSystem.Terminate();
		}

		private void HandleRenderFrame(object sender, FrameEventArgs e)
		{
			float delta;
			RefreshFrameTimeStamp(out delta);
			Update(delta);
			Render();
			if (CommandLineArgs.Limit25FPS) {
				Limit25FPS();
			}
		}

		private void HandleResize(object sender, EventArgs e)
		{
			if (Resized != null) {
				Resized();
			}
		}

		private void HandleMove(object sender, EventArgs e)
		{
			if (Moved != null) {
				Moved();
			}
		}

		private void Limit25FPS()
		{
			int delta = (int)(DateTime.UtcNow - lastFrameTimeStamp).TotalMilliseconds;
			int delay = (1000 / 25) - delta;
			if (delay > 0) {
				System.Threading.Thread.Sleep(delay);
			}
		}

		private DateTime lastFrameTimeStamp = DateTime.UtcNow;

		private void RefreshFrameTimeStamp(out float delta)
		{
			var now = DateTime.UtcNow;
			delta = (float)(now - lastFrameTimeStamp).TotalSeconds;
			delta = delta.Clamp(0, 1 / Application.LowFPSLimit);
			lastFrameTimeStamp = now;
		}

		private void Render()
		{
			FPSCalculator.Refresh();
			otkWindow.MakeCurrent();
			if (Rendering != null) {
				Rendering();
			}
			otkWindow.SwapBuffers();
		}

		private void Update(float delta)
		{
			Input.ProcessPendingKeyEvents();
			if (Updating != null) {
				Updating(delta);
			}
			AudioSystem.Update();
			Input.TextInput = null;
			Input.CopyKeysState();
		}
		
		public void SetDefaultCursor()
		{
			currentCursor = MouseCursor.Default;
			otkWindow.Cursor = currentCursor;
		}

		public void SetCursor(string resourceName, IntVector2 hotSpot, string assemblyName = null)
		{
			var cursor = GetCursor(resourceName, hotSpot, assemblyName);
			if (cursor != currentCursor) {
				currentCursor = cursor;
				otkWindow.Cursor = cursor;
			}
		}

		internal void Run(float fps)
		{
			otkWindow.Run(fps);
		}

		private MouseCursor GetCursor(string resourceName, IntVector2 hotSpot, string assemblyName = null)
		{
			MouseCursor cursor;
			if (cursors.TryGetValue(resourceName, out cursor)) {
				return cursor;
			}
			cursor = CreateCursorFromResource(resourceName, hotSpot, assemblyName);
			cursors[resourceName] = cursor;
			return cursor;
		}

		private void WriteToLog(string format, params string[] args)
		{
#if MAC
			Logger.Write(format, args);
#endif
		}

		private MouseCursor CreateCursorFromResource(string resourceName, IntVector2 hotSpot, string assemblyName = null)
		{
			var entryAssembly = assemblyName == null
				? System.Reflection.Assembly.GetEntryAssembly()
				: System.Reflection.Assembly.Load(assemblyName);
			var fullResourceName = entryAssembly.GetName().Name + "." + resourceName;
			var a = entryAssembly.GetManifestResourceNames();
			var stream = entryAssembly.GetManifestResourceStream(fullResourceName);

			WriteToLog("Loading cursor {0}...", fullResourceName);

			using (var bitmap = new BitmapImplementation())
			{
				bitmap.LoadFromStream(stream);
				WriteToLog("Cursor loaded");

				return new MouseCursor(hotSpot.X, hotSpot.Y, bitmap.GetWidth(), bitmap.GetHeight(), bitmap.GetImageData());
			}
		}
	}
}
#endif