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

		public GameView(Context context)
			: base(context)
		{
			Focusable = true;
			FocusableInTouchMode = true;
			Instance = this;
			for (int i = 0; i < Input.MaxTouches; i++) {
				pointerIds[i] = -1;
			}
		}

		public override bool OnCheckIsTextEditor()
		{
			return true;
		}
			
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Lime.Application.Instance.OnCreate();
			Run(60);
		}
			
		protected override void CreateFrameBuffer()
		{
			this.GLContextVersion = GLContextVersion.Gles2_0;
			GraphicsMode = new AndroidGraphicsMode(0, 0, 0, 0, 0, false);
			base.CreateFrameBuffer();	
		}

		public override Android.Views.InputMethods.IInputConnection OnCreateInputConnection(Android.Views.InputMethods.EditorInfo outAttrs)
		{
			if (inputConnection == null) {
				inputConnection = new InputConnection(this);
			}
			return inputConnection;
		}
			
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			if (!Application.Instance.Active) {
				return;
			}
			FPSCalculator.Refresh();
			MakeCurrent();
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
			Input.TextInput = inputConnection.TextInput;
			inputConnection.TextInput = null;
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

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			Lime.Application.Instance.WindowSize = new Lime.Size(Width, Height);
			//MakeCurrent();
		}
	}
}
#endif