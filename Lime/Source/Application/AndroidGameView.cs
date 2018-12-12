#if ANDROID
using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;

using Javax.Microedition.Khronos.Egl;

namespace Lime
{
	public sealed class GameView : SurfaceView, ISurfaceHolderCallback
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
		private ISurfaceHolder holder;

		private IEGL10 egl;
		private EGLDisplay eglDisplay;
		private EGLConfig eglConfig;
		private EGLContext eglContext;
		private EGLSurface eglSurface;

		public System.Drawing.Size Size { get; private set; }
		public event Action<object, EventArgs> Resize;

		public event Action SurfaceCreating;
		public event Action SurfaceDestroing;

		public GameView(Android.Content.Context context, Input input) : base(context)
		{
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

			egl = EGLContext.EGL.JavaCast<IEGL10>();

			holder = Holder;
			holder.AddCallback(this);
			holder.SetType(SurfaceType.Gpu);
		}

		protected override void Dispose(bool disposing)
		{
			DestroyEglSurface();
			DestroyEglContext();
			DestroyEglDisplay();
			base.Dispose(disposing);
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

		internal bool IsSurfaceCreated => eglSurface != null;

		public override bool OnCheckIsTextEditor()
		{
			return true;
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

		public void ProcessTextInput()
		{
			keyboardHandler.ProcessTextInput();
		}

		void ISurfaceHolderCallback.SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
		{
			var surfaceRect = holder.SurfaceFrame;
			Size = new System.Drawing.Size(surfaceRect.Right - surfaceRect.Left, surfaceRect.Bottom - surfaceRect.Top);
			// Determine orientation using screen dimensions, because Amazon FireOS sometimes reports wrong device orientation.
			var orientation = Width < Height ? DeviceOrientation.Portrait : DeviceOrientation.LandscapeLeft;
			var deviceRotated = Application.CurrentDeviceOrientation != orientation;
			Application.CurrentDeviceOrientation = orientation;
			Resize?.Invoke(this, new ResizeEventArgs { DeviceRotated = deviceRotated });
		}

		void ISurfaceHolderCallback.SurfaceCreated(ISurfaceHolder holder)
		{
			SurfaceCreating?.Invoke();
			CreateEglSurface();
		}

		void ISurfaceHolderCallback.SurfaceDestroyed(ISurfaceHolder holder)
		{
			SurfaceDestroing?.Invoke();
			DestroyEglSurface();
		}

		public void MakeCurrent()
		{
			if (TryMakeCurrent()) return;
			var error = egl.EglGetError();
			if (error == EGL11.EglContextLost) {
				OnEglContextLost();
				if (TryMakeCurrent()) return;
				error = egl.EglGetError();
			}
			throw new System.Exception($"Could not make current EGL context, error {GetEglErrorString(error)}");
		}

		private bool TryMakeCurrent()
		{
			EnsureEglContextCreated();
			return egl.EglMakeCurrent(eglDisplay, eglSurface, eglSurface, eglContext);
		}

		public void UnbindContext()
		{
			if (!egl.EglMakeCurrent(eglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext)) {
				throw new System.Exception($"Could not unbind EGL context, error {GetEglErrorString()}");
			}
		}

		public void SwapBuffers()
		{
			if (!egl.EglSwapBuffers(eglDisplay, eglSurface)) {
				var error = egl.EglGetError();
				if (error == EGL11.EglContextLost) {
					OnEglContextLost();
				} else {
					throw new System.Exception($"Could not swap buffers, error {GetEglErrorString(error)}");
				}
			}
		}

		private void OnEglContextLost()
		{
			eglContext = null;
			GLObjectRegistry.Instance.DiscardObjects();
		}

		private void EnsureEglDisplayCreated()
		{
			if (eglDisplay == null) CreateEglDisplay();
		}

		private void CreateEglDisplay()
		{
			var display = egl.EglGetDisplay(EGL10.EglDefaultDisplay);
			if (display == null || display == EGL10.EglNoDisplay) {
				throw new System.Exception($"Could not get EGL display, error {GetEglErrorString()}");
			}
			var version = new int[2];
			if (!egl.EglInitialize(display, version)) {
				throw new System.Exception($"Could not initialize EGL display, error {GetEglErrorString()}");
			}
			var attribLists = new[] {
				BuildEglConfigAttribs(red: 8, green: 8, blue: 8, alpha: 8, depth: 24, stencil: 8),
				BuildEglConfigAttribs(red: 8, green: 8, blue: 8, alpha: 8, depth: 16, stencil: 8),
				BuildEglConfigAttribs(red: 4, green: 4, blue: 4, alpha: 4, depth: 24, stencil: 8),
				BuildEglConfigAttribs(red: 4, green: 4, blue: 4, alpha: 4, depth: 16, stencil: 8)
			};
			var numConfigs = new int[1];
			if (!egl.EglGetConfigs(display, null, 0, numConfigs)) {
				throw new System.Exception($"Could not get EGL config count, error {GetEglErrorString()}");
			}
			var configs = new EGLConfig[numConfigs[0]];
			var configFound = false;
			foreach (var attribs in attribLists) {
				configFound = egl.EglChooseConfig(display, attribs, configs, 1, numConfigs) && numConfigs[0] > 0;
				if (configFound) break;
			}
			if (!configFound) {
				throw new System.Exception($"Could not choose EGL config, error {GetEglErrorString()}");
			}
			eglDisplay = display;
			eglConfig = configs[0];
		}

		private void DestroyEglDisplay()
		{
			if (eglDisplay != null) {
				if (!egl.EglTerminate(eglDisplay)) {
					throw new System.Exception($"Could not terminate EGL display, error {GetEglErrorString()}");
				}
				eglDisplay = null;
			}
		}

		private void EnsureEglContextCreated()
		{
			if (eglContext == null) CreateEglContext();
		}

		private void CreateEglContext()
		{
			EnsureEglDisplayCreated();
			const int EglContextClientVersion = 0x3098;
			var context = egl.EglCreateContext(eglDisplay, eglConfig, EGL10.EglNoContext, new[] { EglContextClientVersion, 2, EGL10.EglNone });
			if (context == null || context == EGL10.EglNoContext) {
				throw new System.Exception($"Could not create EGL context, error {GetEglErrorString()}");
			}
			eglContext = context;
		}

		private void DestroyEglContext()
		{
			if (eglContext != null) {
				if (!egl.EglDestroyContext(eglDisplay, eglContext)) {
					throw new System.Exception($"Could not destroy EGL context, error {GetEglErrorString()}");
				}
				eglContext = null;
			}
		}

		private void CreateEglSurface()
		{
			EnsureEglDisplayCreated();
			var surface = egl.EglCreateWindowSurface(eglDisplay, eglConfig, (Java.Lang.Object)Holder, null);
			if (surface == null || surface == EGL10.EglNoSurface) {
				throw new System.Exception($"Could not create EGL surface, error {GetEglErrorString()}");
			}
			eglSurface = surface;
		}

		private void DestroyEglSurface()
		{
			if (eglSurface != null) {
				if (!egl.EglDestroySurface(eglDisplay, eglSurface)) {
					throw new System.Exception($"Could not destroy EGL surface, error {GetEglErrorString()}");
				}
				eglSurface = null;
			}
		}

		private static int[] BuildEglConfigAttribs(int red = 0, int green = 0, int blue = 0, int alpha = 0, int depth = 0, int stencil = 0)
		{
			var attribs = new List<int>();
			if (red != 0) {
				attribs.Add(EGL10.EglRedSize);
				attribs.Add(red);
			}
			if (green != 0) {
				attribs.Add(EGL10.EglGreenSize);
				attribs.Add(green);
			}
			if (blue != 0) {
				attribs.Add(EGL10.EglBlueSize);
				attribs.Add(blue);
			}
			if (alpha != 0) {
				attribs.Add(EGL10.EglAlphaSize);
				attribs.Add(alpha);
			}
			if (depth != 0) {
				attribs.Add(EGL10.EglDepthSize);
				attribs.Add(depth);
			}
			if (stencil != 0) {
				attribs.Add(EGL10.EglStencilSize);
				attribs.Add(stencil);
			}
			attribs.Add(EGL10.EglRenderableType);
			attribs.Add(4);
			attribs.Add(EGL10.EglNone);
			return attribs.ToArray();
		}

		private string GetEglErrorString()
		{
			return GetEglErrorString(egl.EglGetError());
		}

		private static string GetEglErrorString(int error)
		{
			switch (error) {
				case EGL11.EglSuccess:
					return "Success";
				case EGL11.EglNotInitialized:
					return "Not Initialized";
				case EGL11.EglBadAccess:
					return "Bad Access";
				case EGL11.EglBadAlloc:
					return "Bad Allocation";
				case EGL11.EglBadAttribute:
					return "Bad Attribute";
				case EGL11.EglBadConfig:
					return "Bad Config";
				case EGL11.EglBadContext:
					return "Bad Context";
				case EGL11.EglBadCurrentSurface:
					return "Bad Current Surface";
				case EGL11.EglBadDisplay:
					return "Bad Display";
				case EGL11.EglBadMatch:
					return "Bad Match";
				case EGL11.EglBadNativePixmap:
					return "Bad Native Pixmap";
				case EGL11.EglBadNativeWindow:
					return "Bad Native Window";
				case EGL11.EglBadParameter:
					return "Bad Parameter";
				case EGL11.EglBadSurface:
					return "Bad Surface";
				case EGL11.EglContextLost:
					return "Context Lost";
				default:
					return "Unknown Error";
			}
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
