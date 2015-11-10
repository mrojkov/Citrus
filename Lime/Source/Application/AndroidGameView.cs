#if ANDROID
using System;
using Android.Content;
using Android.Content.PM;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Java.Lang;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform.Android;


namespace Lime
{
	public sealed class GameView : AndroidGameView, ISoftKeyboard
	{
		// TODO: resolve keyboard flickering bug and remove this field;
		public static bool AllowOnscreenKeyboard;

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
				// Nika: Here we skip our unprocessed char wich is placed into buffer to make Del key work where it isn't
				// http://stackoverflow.com/questions/18581636/android-cannot-capture-backspace-delete-press-in-soft-keyboard
				if (e.Characters == FixDelKeyEditable.OneUnprocessedCharacter.CharAt(0).ToString()) {
					return true;
				}

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
		private InputMethodManager imm;
		private Input input;

		private bool softKeyboardVisible;
		private float softKeyboardHeight;

		bool ISoftKeyboard.Visible { get { return softKeyboardVisible; } }
		float ISoftKeyboard.Height { get { return softKeyboardHeight; } }
		bool ISoftKeyboard.Supported { get { return true; } }
		event Action ISoftKeyboard.Hidden { add {} remove {} }

		public GameView(Context context, Input input) : base(context)
		{
			this.input = input;
			Application.SoftKeyboard = this;
			for (int i = 0; i < Input.MaxTouches; i++) {
				pointerIds[i] = -1;
			}
			imm = (InputMethodManager) context.GetSystemService(Android.Content.Context.InputMethodService);
			keyboardHandler = new KeyboardHandler(input);
			SetOnKeyListener(keyboardHandler);
		}

		public override IInputConnection OnCreateInputConnection(EditorInfo outAttrs)
		{
			FixDelKeyInputConnection baseInputConnection = new FixDelKeyInputConnection(this, false);
			outAttrs.ActionLabel = null;
			outAttrs.InputType = InputTypes.Null;
			outAttrs.ImeOptions = (ImeFlags)ImeAction.Done;
			return baseInputConnection;
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout(changed, left, top, right, bottom);
			if (AllowOnscreenKeyboard) {
				if (changed) {
					// Changed == true never seemed go along with showing and hiding keyboard, but 
					// it results in softKeyboard.Visible = false right after device rotation.
					return;
				}
				var r = new Android.Graphics.Rect();
				this.GetWindowVisibleDisplayFrame(r);
				var totalHeight = bottom - top;
				var visibleHeight = r.Bottom - r.Top;
				if (visibleHeight == totalHeight) {
					softKeyboardVisible = false;
					softKeyboardHeight = 0;
				} else {
					softKeyboardVisible = true;
					softKeyboardHeight = totalHeight - visibleHeight;
				}
			}
		}

		protected override void OnResize(EventArgs e)
		{
			// Determine orientation using screen dimensions, because Amazon FireOS sometimes reports wrong device orientation.
			var orientation = Width < Height ? DeviceOrientation.Portrait : DeviceOrientation.LandscapeLeft;
			Application.CurrentDeviceOrientation = orientation;
			Application.RaiseDeviceRotated();
			base.OnResize(e);
			// RenderFrame once in case of Pause() has been called and
			// there is another view overlaying this view. (e.g. Chartboost video)
			OnRenderFrame(null);
		}

		void ISoftKeyboard.Show(bool show, string text)
		{
			if (AllowOnscreenKeyboard) {
				if (show) {
					Focusable = true;
					FocusableInTouchMode = true;
					this.RequestFocus();
					imm.ShowSoftInput(this, ShowFlags.Forced);
				} else {
					Focusable = false;
					FocusableInTouchMode = false;
					imm.HideSoftInputFromWindow(WindowToken, 0);
				}
				softKeyboardVisible = show;
			}
		}

		void ISoftKeyboard.ChangeText(string text)
		{
		}

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

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			if (GraphicsContext == null || GraphicsContext.IsDisposed) {
				return;
			}
			if (!GraphicsContext.IsCurrent) {
				MakeCurrent();
			}
			var allowedOrientaion = IsRotationEnabled()
				? Application.SupportedDeviceOrientations
				: Application.CurrentDeviceOrientation;
			RestrictSupportedOrientationsWith(allowedOrientaion);
			base.OnRenderFrame(e);
			SwapBuffers();
		}

		private static bool IsRotationEnabled()
		{
			var settingCode = Android.Provider.Settings.System.GetInt(Android.App.Application.Context.ContentResolver,
				Android.Provider.Settings.System.AccelerometerRotation);
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
					return ScreenOrientation.SensorLandscape;
				case DeviceOrientation.Portrait:
					return ScreenOrientation.Portrait;
				case DeviceOrientation.PortraitUpsideDown:
					return ScreenOrientation.ReversePortrait;
				case DeviceOrientation.AllPortraits:
					return ScreenOrientation.SensorPortrait;
				default:
					return ScreenOrientation.FullSensor;
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
				var position = new Vector2(pc.X, pc.Y) * input.ScreenToWorldTransform;
				input.SetTouchPosition(touchIndex, position);
				if (touchIndex == 0) {
					input.MousePosition = position;
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
			float delta;
			RefreshFrameTimeStamp(out delta);
			input.ProcessPendingKeyEvents();
			base.OnUpdateFrame(new FrameEventArgs(delta));
			AudioSystem.Update();
			keyboardHandler.ProcessTextInput();
			input.CopyKeysState();
		}

		private DateTime lastFrameTimeStamp = DateTime.UtcNow;

		private void RefreshFrameTimeStamp(out float delta)
		{
			var now = DateTime.UtcNow;
			delta = (float)(now - lastFrameTimeStamp).TotalSeconds;
			delta = delta.Clamp(0, 1 / Application.LowFPSLimit);
			lastFrameTimeStamp = now;
		}

		// Classes to help fix problem with DEL key on some devices
		public class FixDelKeyInputConnection : BaseInputConnection
		{
			private IEditable myEditable = null;

			public FixDelKeyInputConnection(View targetView, bool fullEditor) : base(targetView, fullEditor) { }

			public override IEditable Editable
			{
				get
				{
					if ((int)Android.OS.Build.VERSION.SdkInt >= 14) {
						if (myEditable == null) {
							myEditable = new FixDelKeyEditable(FixDelKeyEditable.OneUnprocessedCharacter);
							Selection.SetSelection(myEditable, 1);
						} else {
							int myEditableLength = myEditable.Length();
							if (myEditableLength == 0) {
								myEditable.Append(FixDelKeyEditable.OneUnprocessedCharacter);
								Selection.SetSelection(myEditable, 1);
							}
						}
						return myEditable;
					} else {
						return base.Editable;
					}
				}
			}

			public override bool DeleteSurroundingText(int leftLength, int rightLength)
			{
				if ((int)Android.OS.Build.VERSION.SdkInt >= 14 && (leftLength == 1 && rightLength == 0)) {
					return 
						base.SendKeyEvent(new KeyEvent(KeyEventActions.Down, Keycode.Del)) &&
						base.SendKeyEvent(new KeyEvent(KeyEventActions.Up, Keycode.Del));
				} else {
					return base.DeleteSurroundingText(leftLength, rightLength);
				}
			}
		}

		public class FixDelKeyEditable : SpannableStringBuilder
		{
			// Nika: there is no such symbol in android keyboard, so we can use it as unprocessed charcater.
			public static ICharSequence OneUnprocessedCharacter = new Java.Lang.String("\ud804\udc00");

			public FixDelKeyEditable(ICharSequence source) : base(source) { }

			public override IEditable Replace(int start, int end, ICharSequence tb, int tbstart, int tbend)
			{
				if (tbend > tbstart) {
					base.Replace(0, Length(), (ICharSequence)new Java.Lang.String(""), 0, 0);
					return base.Replace(0, 0, tb, tbstart, tbend);
				} else if (end > start) {
					base.Replace(0, Length(), (ICharSequence)new Java.Lang.String(""), 0, 0);
					return base.Replace(0, 0, OneUnprocessedCharacter, 0, 1);
				}
				return base.Replace(start, end, tb, tbstart, tbend);
			}
		}
	}
}
#endif