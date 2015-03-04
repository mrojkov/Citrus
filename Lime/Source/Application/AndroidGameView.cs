#if ANDROID
using System;
using OpenTK.Platform;
using OpenTK.Platform.Android;
using OpenTK;
using Android.Content;
using OpenTK.Graphics;
using System.Collections.Generic;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace Lime
{
	public class GameView : AndroidGameView
	{
		// TODO: resolve keyboard flickering bug and remove this field;
		public static bool AllowOnscreenKeyboard;

		class InputConnection : BaseInputConnection
		{
			public string TextInput;

			public InputConnection(View view)
				: base(view, true)
			{
			}

			public override bool SendKeyEvent(KeyEvent e)
			{
				if (e.KeyCode == Keycode.Del && e.Action != KeyEventActions.Up) {
					TextInput += '\b';
				}
				if (e.IsPrintingKey && e.Action == KeyEventActions.Down) {
					CommitText(new String((char)e.UnicodeChar, 1), 1);
				}
				return base.SendKeyEvent(e);
			}

			public override bool CommitText(Java.Lang.ICharSequence text, int newCursorPosition)
			{
				TextInput += text.ToString();
				return base.CommitText(text, newCursorPosition);
			}
		}

		internal static event Action DidUpdated;
		public static GameView Instance;

		public readonly RenderingApi RenderingApi = RenderingApi.ES20;

		private InputConnection inputConnection;
		private InputMethodManager imm;

		public GameView(Context context)
			: base(context)
		{
			Instance = this;
			for (int i = 0; i < Input.MaxTouches; i++) {
				pointerIds[i] = -1;
			}
			imm = (InputMethodManager)context.GetSystemService(Android.Content.Context.InputMethodService);
		}

		private bool isOnscreenKeyboardVisible;

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout(changed, left, top, right, bottom);
			if (AllowOnscreenKeyboard) {
				var r = new Android.Graphics.Rect();
				this.GetWindowVisibleDisplayFrame(r);
				var totalHeight = bottom - top;
				var visibleHeight = r.Bottom - r.Top;
				var app = Application.Instance;
				if (visibleHeight == totalHeight) {
					isOnscreenKeyboardVisible = false;
					app.OnscreenKeyboardHeight = 0;
				} else {
					app.OnscreenKeyboardHeight = totalHeight - visibleHeight;
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
		}

		public void ShowOnscreenKeyboard(bool show, string text)
		{
			if (AllowOnscreenKeyboard) {
				if (show) {
					Focusable = true;
					FocusableInTouchMode = true;
					this.RequestFocus();
					imm.ShowSoftInput(this, ShowFlags.Forced);
				} else if (!show) {
					Focusable = false;
					FocusableInTouchMode = false;
					imm.HideSoftInputFromWindow(WindowToken, 0);
				}
				isOnscreenKeyboardVisible = show;
			}
		}

		public bool IsOnscreenKeyboardVisible()
		{
			return isOnscreenKeyboardVisible;
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

		public override Android.Views.InputMethods.IInputConnection OnCreateInputConnection(Android.Views.InputMethods.EditorInfo outAttrs)
		{
			if (!AllowOnscreenKeyboard) {
				return base.OnCreateInputConnection(outAttrs);
			} else {
				if (inputConnection == null) {
					inputConnection = new InputConnection(this);
				}
				return inputConnection;
			}
		}
			
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			if (GraphicsContext == null || GraphicsContext.IsDisposed) {
				return;
			}
			if (!GraphicsContext.IsCurrent) {
				MakeCurrent();
			}
			base.OnRenderFrame(e);
			FPSCalculator.Refresh();
			Application.Instance.OnRenderFrame();
			SwapBuffers();
			FPSCalculator.Refresh();
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
			if (inputConnection != null) {
				Input.TextInput = inputConnection.TextInput;
				inputConnection.TextInput = null;
			}
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