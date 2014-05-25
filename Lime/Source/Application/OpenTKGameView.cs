#if WIN
using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using System.Collections.Generic;
using System.Drawing;

namespace Lime
{
	public class GameView : OpenTK.GameWindow
	{
		private const float MaxFrameDelta = 0.04f;

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
			: base(800, 600, GraphicsMode.Default, 
			"Citrus", GameWindowFlags.Default, DisplayDevice.Default,
			2, 0, GetGraphicContextFlags())
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
			RenderingApi = GetRenderingApi();
		}

		private static GraphicsContextFlags GetGraphicContextFlags()
		{
			return GetRenderingApi() == RenderingApi.OpenGL ? 
				 GraphicsContextFlags.Default : GraphicsContextFlags.Embedded;
		}

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
			if (CommandLineArgs.OpenGL) {
				return RenderingApi.OpenGL;
			} else {
				return RenderingApi.ES20;
			}
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
			Application.Instance.Active = this.Focused;
			if (this.Focused) {
				Application.Instance.OnActivate();
			} else {
				Application.Instance.OnDeactivate();
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
			if (e.Delta > 0) {
				Input.SetKeyState(Key.MouseWheelUp, true);
				Input.SetKeyState(Key.MouseWheelUp, false);
			} else if (e.Delta < 0) {
				Input.SetKeyState(Key.MouseWheelDown, true);
				Input.SetKeyState(Key.MouseWheelDown, false);
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
			if (CommandLineArgs.Limit25FPS) {
				Limit25FPS();
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
			delta = delta.Clamp(0, MaxFrameDelta);
			lastFrameTimeStamp = now;
		}

		private void Render()
		{
			FPSCalculator.Refresh();
			MakeCurrent();
			app.OnRenderFrame();
			SwapBuffers();
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
			get { 
				return this.WindowState == WindowState.Fullscreen;
			}
			set { 
				this.WindowState = value ? WindowState.Fullscreen : WindowState.Normal;
			}
		}

		public float FrameRate { 
			get { return FPSCalculator.FPS; } 
		}

		public void SetCursor(string resourceName, IntVector2 hotSpot)
		{
			var cursor = GetCursor(resourceName, hotSpot);
			if (cursor != currentCursor) {
				currentCursor = cursor;
				base.Cursor = cursor;
			}
		}

		private MouseCursor GetCursor(string resourceName, IntVector2 hotSpot)
		{
			MouseCursor cursor;
			if (cursors.TryGetValue(resourceName, out cursor)) {
				return cursor;
			}
			cursor = CreateCursorFromResource(resourceName, hotSpot);
			cursors[resourceName] = cursor;
			return cursor;
		}

		private MouseCursor CreateCursorFromResource(string resourceName, IntVector2 hotSpot)
		{
			var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
			var fullResourceName = entryAssembly.GetName().Name + "." + resourceName;
			var file = entryAssembly.GetManifestResourceStream(fullResourceName);
			using (var bitmap = new Bitmap(file)) {
				var lockRect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
				var lockMode = System.Drawing.Imaging.ImageLockMode.ReadOnly;
				var data = bitmap.LockBits(lockRect, lockMode, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				if (data.Width * 4 != data.Stride) {
					throw new Lime.Exception("Invalid cursor bitmap (resource name: {0}, stride: {1}, width: {2})",
						resourceName, data.Stride, data.Width);
				}
				var cursor = new MouseCursor(hotSpot.X, hotSpot.Y, data.Width, data.Height, data.Scan0);
				bitmap.UnlockBits(data);
				return cursor;
			}
		}
	}
}
#endif