#if ANDROID
using System;
using OpenTK.Platform;
using OpenTK.Platform.Android;
using OpenTK;
using Android.Content;
using OpenTK.Graphics;
using System.Collections.Generic;
using Android.Content.PM;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace Lime
{
	public class GameView : AndroidGameView
	{
		// TODO: resolve keyboard flickering bug and remove this field;
		public static bool AllowOnscreenKeyboard;

		private class KeyboardHandler : Java.Lang.Object, IOnKeyListener
		{
			public string TextInput;

			public bool OnKey(View v, Keycode keyCode, KeyEvent e)
			{
				if (e.KeyCode == Keycode.Del && e.Action != KeyEventActions.Up) {
					TextInput += '\b';
				} else if (keyCode == Keycode.Unknown) {
					TextInput += e.Characters;
				} else if (e.IsPrintingKey && e.Action != KeyEventActions.Up) {
					TextInput += (char) e.UnicodeChar;
				} else if (e.KeyCode == Keycode.Space && e.Action != KeyEventActions.Up) {
					TextInput += ' ';
				} else if (e.Action != KeyEventActions.Multiple) {
					var key = TranslateKeycode(keyCode);
					if (key != Key.KeyCount) {
						var state = e.Action != KeyEventActions.Up;
						Input.SetKeyState(key, state);
					}
				}
				return true;
			}
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
					return Key.KeyCount;
			}
		}

		internal static event Action DidUpdated;
		public static GameView Instance;

		public readonly RenderingApi RenderingApi = RenderingApi.ES20;

		private KeyboardHandler keyboardHandler;
		private InputMethodManager imm;

		public GameView(Context context) : base(context)
		{
			Instance = this;
			for (int i = 0; i < Input.MaxTouches; i++) {
				pointerIds[i] = -1;
			}
			imm = (InputMethodManager) context.GetSystemService(Android.Content.Context.InputMethodService);
		}

		public void OnCreate()
		{
			keyboardHandler = new KeyboardHandler();
			SetOnKeyListener(keyboardHandler);
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
				var app = Application.Instance;
				if (visibleHeight == totalHeight) {
					app.SoftKeyboard.Visible = false;
					app.SoftKeyboard.Height = 0;
				} else {
					app.SoftKeyboard.Height = totalHeight - visibleHeight;
					app.SoftKeyboard.Visible = true;
				}
			}
		}

		protected override void OnResize(EventArgs e)
		{
			var app = Lime.Application.Instance;
			app.WindowSize = new Lime.Size(Width, Height);
			// Determine orientation using screen dimensions, because Amazon FireOS sometimes reports wrong device orientation.
			var orientation = Width < Height ? DeviceOrientation.Portrait : DeviceOrientation.LandscapeLeft;
			app.CurrentDeviceOrientation = orientation;
			app.OnDeviceRotate();
			base.OnResize(e);
			// RenderFrame once in case of Pause() has been called and
			// there is another view overlaying this view. (e.g. Chartboost video)
			RenderFrame();
		}

		public void ShowSoftKeyboard(bool show, string text)
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
				Application.Instance.SoftKeyboard.Visible = show;
			}
		}

		public void ChangeSoftKeyboardText(string text)
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
				Application.Instance.OnGraphicsContextReset();
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
				? Application.Instance.SupportedDeviceOrientations
				: Application.Instance.CurrentDeviceOrientation;
			RestrictSupportedOrientationsWith(allowedOrientaion);
			base.OnRenderFrame(e);
			FPSCalculator.Refresh();
			Application.Instance.OnRenderFrame();
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

		public float FrameRate {
			get {
				return FPSCalculator.FPS;
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
			Input.SetKeyState(Key.Mouse0, false);
			for (int i = 0; i < Input.MaxTouches; i++) {
				pointerIds[i] = -1;
				Key key = (Key)((int)Key.Touch0 + i);
				Input.SetKeyState(key, false);
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
				Vector2 position = new Vector2(pc.X, pc.Y) * Input.ScreenToWorldTransform;
				Input.SetTouchPosition(touchIndex, position);
				if (touchIndex == 0) {
					Input.MousePosition = position;
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
				Input.SetKeyState(Key.Mouse0, true);
			}
			var key = (Key)((int)Key.Touch0 + touchIndex);
			Input.SetKeyState(key, true);
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
				Input.SetKeyState(Key.Mouse0, false);
			}
			var key = (Key)((int)Key.Touch0 + touchIndex);
			Input.SetKeyState(key, false);
		}

		public new void UpdateFrame()
		{
			OnUpdateFrame(null);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			float delta;
			RefreshFrameTimeStamp(out delta);
			Input.ProcessPendingKeyEvents();
			Application.Instance.OnUpdateFrame(delta);
			AudioSystem.Update();
			Input.TextInput = keyboardHandler.TextInput;
			keyboardHandler.TextInput = null;
			Input.CopyKeysState();
			if (DidUpdated != null) {
				DidUpdated();
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

		public new void RenderFrame()
		{
			OnRenderFrame(null);
		}
	}
}
#endif