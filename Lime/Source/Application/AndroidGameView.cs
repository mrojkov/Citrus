#if ANDROID
using System;
using System.Diagnostics;

using Android.Content.PM;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform.Android;

namespace Lime
{
	public sealed class GameView : AndroidGameView
	{
		private class KeyboardHandler : Java.Lang.Object, IOnKeyListener
		{
			private string textInput;
			private Input input;

			public KeyboardHandler(Input input)
			{
				this.input = input;
			}

			public void ProcessTextInput()
			{
				input.TextInput = textInput;
				textInput = null;
			}

			public bool OnKey(View v, Keycode keyCode, KeyEvent e)
			{
				if (e.KeyCode == Keycode.Del && e.Action != KeyEventActions.Up) {
					textInput += '\b';
				} else if (keyCode == Keycode.Unknown) {
					textInput += e.Characters;
				} else if (e.IsPrintingKey && e.Action != KeyEventActions.Up) {
					textInput += (char) e.UnicodeChar;
				} else if (e.KeyCode == Keycode.Space && e.Action != KeyEventActions.Up) {
					textInput += ' ';
				} else if (e.Action != KeyEventActions.Multiple) {
					var key = TranslateKeycode(keyCode);
					if (key != Key.Unknown) {
						var state = e.Action != KeyEventActions.Up;
						input.SetKeyState(key, state);
					}
				}
				return true;
			}

			private static Key TranslateKeycode(Keycode key)
			{
				switch (key) {
					case Keycode.DpadLeft:
						return Key.Left;
					case Keycode.DpadRight:
						return Key.Right;
					case Keycode.DpadUp:
						return Key.Up;
					case Keycode.DpadDown:
						return Key.Down;
					case Keycode.ForwardDel:
						return Key.Delete;
					case Keycode.Escape:
						return Key.Escape;
					case Keycode.Tab:
						return Key.Tab;
					case Keycode.Enter:
						return Key.Enter;
					case Keycode.MoveHome:
						return Key.Home;
					case Keycode.MoveEnd:
						return Key.End;
					// TODO: add all alpha-numeric keys
					default:
						return Key.Unknown;
				}
			}
		}

		private KeyboardHandler keyboardHandler;
		private Input input;
		private AndroidSoftKeyboard androidSoftKeyboard;

		private Func<bool> readyToRender;
		public bool ReadyToRender => readyToRender.Invoke();

		public GameView(Android.Content.Context context, Input input) : base(context)
		{
			this.AutoSetContextOnRenderFrame = false;
			this.input = input;
			androidSoftKeyboard = new AndroidSoftKeyboard(this);
			Application.SoftKeyboard = androidSoftKeyboard;
			for (int i = 0; i < Input.MaxTouches; i++) {
				pointerIds[i] = -1;
			}
			keyboardHandler = new KeyboardHandler(input);
			SetOnKeyListener(keyboardHandler);
			RestrictSupportedOrientationsWith(Application.SupportedDeviceOrientations);
			Application.SupportedDeviceOrientationsChanged += RestrictSupportedOrientationsWith;
			var readyToRenderGetter = typeof(OpenTK.Platform.Android.AndroidGameView).GetProperty("ReadyToRender",
				System.Reflection.BindingFlags.Instance |
				System.Reflection.BindingFlags.NonPublic).GetMethod;
			readyToRender = (Func<bool>)readyToRenderGetter.CreateDelegate(typeof(Func<bool>), this);
		}

		public override IInputConnection OnCreateInputConnection(EditorInfo outAttrs)
		{
			// Read FixDelKeyInputConnection class for details.
			// http://stackoverflow.com/questions/14560344/android-backspace-in-webview-baseinputconnection
			var baseInputConnection = new FixDelKeyInputConnection(this, false);
			outAttrs.ActionLabel = null;
			outAttrs.InputType = InputTypes.TextVariationVisiblePassword | InputTypes.TextFlagNoSuggestions;
			outAttrs.ImeOptions = ImeFlags.NoExtractUi | (ImeFlags)ImeAction.None;
			return baseInputConnection;
		}

		public override bool OnKeyPreIme(Keycode keyCode, KeyEvent e)
		{
			if (keyCode == Keycode.Back && e.Action == KeyEventActions.Up) {
				input.SetKeyState(Key.DismissSoftKeyboard, true);
				input.SetKeyState(Key.DismissSoftKeyboard, false);
				return false;
			}
			return base.DispatchKeyEvent(e);
		}

		public override void ClearFocus()
		{
			// we override this function to hide keyboard when app is stopped.
			base.ClearFocus();
			androidSoftKeyboard.Show(false);
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout(changed, left, top, right, bottom);
			if (Window.Current == null) {
				// Do not calc keyboard height before window init, because we need to know pixel scale.
				return;
			}
			var r = new Android.Graphics.Rect();
			this.GetWindowVisibleDisplayFrame(r);
			var totalHeight = bottom - top;
			var visibleHeight = r.Bottom - r.Top;
			if (visibleHeight == totalHeight) {
				androidSoftKeyboard.Height = 0;
			} else {
				androidSoftKeyboard.Height = totalHeight - visibleHeight;
			}
		}

		protected override void OnResize(EventArgs e)
		{
			// Determine orientation using screen dimensions, because Amazon FireOS sometimes reports wrong device orientation.
			var orientation = Width < Height ? DeviceOrientation.Portrait : DeviceOrientation.LandscapeLeft;
			var deviceRotated = Application.CurrentDeviceOrientation != orientation;
			Application.CurrentDeviceOrientation = orientation;
			base.OnResize(new ResizeEventArgs { DeviceRotated = deviceRotated });
			// RenderFrame once in case of Pause() has been called and
			// there is another view overlaying this view. (e.g. Chartboost video)
			OnRenderFrame(null);
		}

		internal bool IsSurfaceCreated { get; private set; }
		internal int SurfaceVersion { get; private set; }
		
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			IsSurfaceCreated = true;
		}

		protected override void OnUnload(EventArgs e)
		{
			base.OnUnload(e);
			IsSurfaceCreated = false;
			SurfaceVersion++;
		}

		public override void MakeCurrent() { }

		public void MakeCurrentActual() => base.MakeCurrent();
		
		public override bool OnCheckIsTextEditor()
		{
			return true;
		}

		private bool contextLost;

		protected override void OnContextLost(EventArgs e)
		{
			base.OnContextLost(e);
			contextLost = true;
		}

		protected override void OnContextSet(EventArgs e)
		{
			base.OnContextSet(e);
			if (contextLost) {
				contextLost = false;
				GLObjectRegistry.Instance.DiscardObjects();
			}
		}

		protected override void CreateFrameBuffer()
		{
			ContextRenderingApi = GLVersion.ES2;
			// the default GraphicsMode that is set consists of (16, 16, 0, 0, 2, false)
			try {
				Debug.Write("Creating framebuffer with default settings");
				// Enable stencil buffer
				GraphicsMode = new AndroidGraphicsMode(16, 16, 8, 0, 2, false);
				base.CreateFrameBuffer();
				return;
			} catch (Exception ex) {
				Debug.Write("{0}", ex);
			}
			// this is a graphics setting that sets everything to the lowest mode possible so
			// the device returns a reliable graphics setting.
			try {
				Debug.Write("Creating framebuffer with custom Android settings (low mode)");
				GraphicsMode = new AndroidGraphicsMode(0, 0, 0, 0, 0, false);
				base.CreateFrameBuffer();
				return;
			} catch (Exception ex) {
				Debug.Write("{0}", ex);
			}
			throw new Lime.Exception("Can't create framebuffer, aborting");
		}

		protected override void OnRenderFrame(FrameEventArgs e) { }

		public void OnRenderFrameForce(FrameEventArgs e)
		{
			if (GraphicsContext == null || GraphicsContext.IsDisposed) {
				return;
			}
			if (!GraphicsContext.IsCurrent) {
				MakeCurrent();
			}

			base.OnRenderFrame(e);
			SwapBuffers();
		}

		private static bool IsRotationEnabled()
		{
			var settingCode = Android.Provider.Settings.System.GetInt(
				Android.App.Application.Context.ContentResolver,
				Android.Provider.Settings.System.AccelerometerRotation,
				def: 0);
			return settingCode == 1;
		}

		private void RestrictSupportedOrientationsWith(DeviceOrientation orientation)
		{
			((Android.App.Activity)Context).RequestedOrientation = GetScreenOrientation(orientation);
		}

		private static ScreenOrientation GetScreenOrientation(DeviceOrientation orientation)
		{
			switch (orientation) {
				case DeviceOrientation.LandscapeLeft:
					return ScreenOrientation.Landscape;
				case DeviceOrientation.LandscapeRight:
					return ScreenOrientation.ReverseLandscape;
				case DeviceOrientation.AllLandscapes:
					return ScreenOrientation.UserLandscape;
				case DeviceOrientation.Portrait:
					return ScreenOrientation.Portrait;
				case DeviceOrientation.PortraitUpsideDown:
					return ScreenOrientation.ReversePortrait;
				case DeviceOrientation.AllPortraits:
					return ScreenOrientation.UserPortrait;
				default:
					return ScreenOrientation.FullUser;
			}
		}

		private int[] pointerIds = new int[Input.MaxTouches];

		public override bool OnTouchEvent(Android.Views.MotionEvent e)
		{
			switch (e.ActionMasked) {
			case MotionEventActions.Down:
			case MotionEventActions.PointerDown:
				HandleDownAction(e);
				break;
			case MotionEventActions.Up:
			case MotionEventActions.PointerUp:
				HandleUpAction(e);
				break;
			case MotionEventActions.Cancel:
				CancelGesture();
				break;
			case MotionEventActions.Move:
				break;
			}
			HandleMoveActions(e);
			return true;
		}

		void CancelGesture()
		{
			input.SetKeyState(Key.Mouse0, false);
			for (int i = 0; i < Input.MaxTouches; i++) {
				pointerIds[i] = -1;
				Key key = (Key)((int)Key.Touch0 + i);
				input.SetKeyState(key, false);
			}
		}

		void HandleMoveActions(Android.Views.MotionEvent e)
		{
			var pc = new Android.Views.MotionEvent.PointerCoords();
			for (int i = 0; i < e.PointerCount; i++) {
				int id = e.GetPointerId(i);
				int touchIndex = Array.IndexOf(pointerIds, id);
				if (touchIndex < 0) {
					continue;
				}
				e.GetPointerCoords(i, pc);
				var position = new Vector2(pc.X, pc.Y);
				input.SetDesktopTouchPosition(touchIndex, position);
				if (touchIndex == 0) {
					input.DesktopMousePosition = position;
				}
			}
		}

		void HandleDownAction(Android.Views.MotionEvent e)
		{
			var touchIndex = Array.IndexOf(pointerIds, -1);
			if (touchIndex < 0) {
				return;
			}
			int i = e.ActionIndex;
			pointerIds[touchIndex] = e.GetPointerId(i);
			if (touchIndex == 0) {
				input.SetKeyState(Key.Mouse0, true);
			}
			var key = (Key)((int)Key.Touch0 + touchIndex);
			input.SetKeyState(key, true);
		}

		void HandleUpAction(Android.Views.MotionEvent e)
		{
			int id = e.GetPointerId(e.ActionIndex);
			var touchIndex = Array.IndexOf(pointerIds, id);
			if (touchIndex < 0) {
				return;
			}
			pointerIds[touchIndex] = -1;
			if (touchIndex == 0) {
				input.SetKeyState(Key.Mouse0, false);
			}
			var key = (Key)((int)Key.Touch0 + touchIndex);
			input.SetKeyState(key, false);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);
			keyboardHandler.ProcessTextInput();
		}

		public override void Resume()
		{
			base.Resume();
			RestrictSupportedOrientationsWith(Application.SupportedDeviceOrientations);
		}

		/// <summary>
		/// Classes to help fix problem with DEL key event not triggered on some devices
		/// </summary>
		private class FixDelKeyInputConnection : BaseInputConnection
		{
			public FixDelKeyInputConnection(View targetView, bool fullEditor)
				: base(targetView, fullEditor)
			{
			}

			static bool IsBuggedSdk()
			{
				// Bugged SDKs are from 14 to 19, but with some third-party keyboards
				// bug may present even in newer version. Also this code should not affect
				// devices without this bug.
				return (int)Build.VERSION.SdkInt >= 14;
			}

			public override bool DeleteSurroundingText(int leftLength, int rightLength)
			{
				// leftLength == 1 and rightLength == 0 means that the user presses Backspace
				if (IsBuggedSdk() && (leftLength == 1 && rightLength == 0)) {
					// Send Del key event to handle char deleting in the OnKey() method.
					base.SendKeyEvent(new KeyEvent(KeyEventActions.Down, Keycode.Del));
					base.SendKeyEvent(new KeyEvent(KeyEventActions.Up, Keycode.Del));
					return true;
				} else {
					return base.DeleteSurroundingText(leftLength, rightLength);
				}
			}
		}
	}

	internal class ResizeEventArgs : EventArgs
	{
		public bool DeviceRotated;
	}
}
#endif
