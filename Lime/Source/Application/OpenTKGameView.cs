#if WIN || MONOMAC || MAC
using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
#if !MAC
using OpenTK.Graphics.ES20;
#endif
using System.Collections.Generic;
using System.Drawing;

namespace Lime
{
	public enum WindowBorder
	{
		Resizeable,
		Fixed,
		Hidden
	}

	internal static class WindowBorderExtensions
	{
		public static OpenTK.WindowBorder ToOpenTK(this WindowBorder border)
		{
			switch (border) {
				case WindowBorder.Hidden:
					return OpenTK.WindowBorder.Hidden;
				case WindowBorder.Fixed:
					return OpenTK.WindowBorder.Fixed;
				default:
					return OpenTK.WindowBorder.Resizable;
			}
		}

		public static WindowBorder FromOpenTK(this OpenTK.WindowBorder border)
		{
			switch (border) {
				case OpenTK.WindowBorder.Hidden:
					return WindowBorder.Hidden;
				case OpenTK.WindowBorder.Fixed:
					return WindowBorder.Fixed;
				default:
					return WindowBorder.Resizeable;
			}
		}
	}

	public class GameView : OpenTK.GameWindow
	{
		private Application app;
		private Dictionary<string, MouseCursor> cursors = new Dictionary<string, MouseCursor>();
		private MouseCursor currentCursor;

		public static GameView Instance;
		// Indicates whether the game uses OpenGL or OpenGL ES 2.0
		public RenderingApi RenderingApi { get; private set; }
		internal static event Action DidUpdated;

#if WIN
		static GameView()
		{
			// This is workaround an OpenTK bug.
			// On some video cards the SDL framework could not create a GLES2/Angle OpenGL context
			// if context attributes weren't set before the main window creation.
			if (GetRenderingApi() == RenderingApi.ES20) {
				Sdl2.Init(Sdl2.SystemFlags.VIDEO);
				Sdl2.SetAttribute(Sdl2.ContextAttribute.CONTEXT_PROFILE_MASK, 4);
				Sdl2.SetAttribute(Sdl2.ContextAttribute.CONTEXT_MAJOR_VERSION, 2);
				Sdl2.SetAttribute(Sdl2.ContextAttribute.CONTEXT_MINOR_VERSION, 0);
			}
		}
#endif

		public GameView(Application app)
			: base(app.Options.WindowSize.Width, app.Options.WindowSize.Height, 
				new GraphicsMode(new ColorFormat(32), depth: 24), app.Title
#if WIN
				, GetWindowFlags(app.Options),
				DisplayDevice.Default, 2, 0, GetGraphicContextFlags()
#endif
			)
		{
			Instance = this;
			this.app = app;
			app.Active = true;
			AudioSystem.Initialize();
			app.OnCreate();
			this.Keyboard.KeyDown += HandleKeyDown;
			this.Keyboard.KeyUp += HandleKeyUp;
			this.KeyPress += HandleKeyPress;
			this.Mouse.ButtonDown += HandleMouseButtonDown;
			this.Mouse.ButtonUp += HandleMouseButtonUp;
			this.Mouse.Move += HandleMouseMove;
			this.Mouse.WheelChanged += HandleMouseWheel;
			SetupWindowLocationAndSize();
			// Setting fullscreen with GameWindowFlags.FullScreen in OpenTK.GameWindow constructor causes
			// changing the display resolution. Desktop fullscreen is much more better.
			this.FullScreen = app.Options.FullScreen;
			RenderingApi = GetRenderingApi();
		}

#if WIN
		private static GameWindowFlags GetWindowFlags(Application.StartupOptions options)
		{
			return options.FixedSizeWindow ? GameWindowFlags.FixedWindow : GameWindowFlags.Default;
		}
#endif

		public new WindowBorder WindowBorder
		{
			get { return base.WindowBorder.FromOpenTK(); }
			set { base.WindowBorder = value.ToOpenTK(); }
		}

#if !MAC
		private static GraphicsContextFlags GetGraphicContextFlags()
		{
			return GetRenderingApi() == RenderingApi.OpenGL ? 
				 GraphicsContextFlags.Default : GraphicsContextFlags.Embedded;
		}
#endif

		private void SetupWindowLocationAndSize()
		{
			var displayBounds = OpenTK.DisplayDevice.Default.Bounds;
			if (CommandLineArgs.FullscreenMode) {
				this.WindowState = OpenTK.WindowState.Fullscreen;
			} else if (CommandLineArgs.MaximizedWindow) {
				this.Location = displayBounds.Location;
				this.WindowState = OpenTK.WindowState.Maximized;
			} else {
				this.Location = new System.Drawing.Point {
					X = Math.Max(0, (displayBounds.Width - this.Width) / 2 + displayBounds.X),
					Y = Math.Max(0, (displayBounds.Height - this.Height) / 2 + displayBounds.Y)
				};
			}
		}

		private static RenderingApi GetRenderingApi()
		{
#if MAC
			return RenderingApi.OpenGL;
#else
			if (CommandLineArgs.OpenGL) {
				return RenderingApi.OpenGL;
			} else {
				return RenderingApi.ES20;
			}
#endif
		}

		void HandleKeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
		{
			// SDL backend bug: OpenTK doesn't send key press event for backspace
			if (e.Key == OpenTK.Input.Key.BackSpace) {
				Input.TextInput += '\b';
			}
			Input.SetKeyState((Key)e.Key, true);
		}

		void HandleKeyUp(object sender, KeyboardKeyEventArgs e)
		{
			Input.SetKeyState((Key)e.Key, false);
		}

		void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			Input.TextInput += e.KeyChar;
		}

		protected override void OnFocusedChanged(EventArgs e)
		{
			app.Active = this.Focused;
			if (this.Focused) {
				app.OnActivate();
			} else {
				app.OnDeactivate();
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
#if WIN
			// Unfortunately SDL gives us only scrolling direction (-1 or 1), but several times per frame.
			// So, presume minimum scrolling step - 15 pixels
			var wheelDelta = e.Delta * 15;
#else
			var wheelDelta = e.Delta;
#endif
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

		protected override void OnClosed(EventArgs e)
		{
			app.OnTerminate();
			TexturePool.Instance.DiscardAllTextures();
			AudioSystem.Terminate();
		}

		protected override void OnRenderFrame(OpenTK.FrameEventArgs e)
		{
			float delta;
			RefreshFrameTimeStamp(out delta);
			Update(delta);
			if (DidUpdated != null) {
				DidUpdated();
			}
			Render();
			SwapBuffers();
			if (CommandLineArgs.Limit25FPS) {
				Limit25FPS();
			}
		}

		protected override void OnMove(EventArgs e)
		{
			app.OnMove();
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			if (app != null)
				app.OnResize();
		}

#if MAC
		protected override void OnEnteredFullScreen(EventArgs e)
		{
			app.OnEnteredFullScreen();
		}

		protected override void OnExitedFullScreen(EventArgs e)
		{
			app.OnExitedFullScreen();
		}
#endif

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
			MakeCurrent();
			app.OnRenderFrame();
		}

		private void Update(float delta)
		{
			Input.ProcessPendingKeyEvents();
			app.OnUpdateFrame(delta);
			AudioSystem.Update();
			Input.TextInput = null;
			Input.CopyKeysState();
		}
		
		public Size WindowSize { 
			get { return new Size(ClientSize.Width, ClientSize.Height); } 
			set { this.ClientSize = new System.Drawing.Size(value.Width, value.Height); } 
		}
		
		public bool FullScreen { 
			get { return this.WindowState == WindowState.Fullscreen; }
			set
			{
				var newState = value ? WindowState.Fullscreen : WindowState.Normal;
				if (WindowState == newState) {
					return;
				}
				this.WindowState = newState;
#if WIN
				if (value) {
					app.OnEnteredFullScreen();
				} else {
					app.OnExitedFullScreen();
				}
#endif
			}
		}

		public float FrameRate { 
			get { return FPSCalculator.FPS; } 
		}

		public void SetDefaultCursor()
		{
			currentCursor = MouseCursor.Default;
			base.Cursor = currentCursor;
		}

		public void SetCursor(string resourceName, IntVector2 hotSpot, string assemblyName = null)
		{
			var cursor = GetCursor(resourceName, hotSpot, assemblyName);
			if (cursor != currentCursor) {
				currentCursor = cursor;
				base.Cursor = cursor;
			}
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