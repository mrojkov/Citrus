#if iOS
using System;
using OpenTK;
using OpenTK.Graphics.ES20;
using GL1 = OpenTK.Graphics.ES11.GL;
using All1 = OpenTK.Graphics.ES11.All;
using OpenTK.Platform.iPhoneOS;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;

namespace Lime
{
	public class GameView : iPhoneOSGameView
	{
		UITextField textField;
		UITouch[] activeTouches = new UITouch[Input.MaxTouches];
		float screenScale;

		class TextFieldDelegate : UITextFieldDelegate
		{
			public override bool ShouldReturn(UITextField textField)
			{
				Input.SetKeyState(Key.Enter, true);
				return false;
			}
		}

		internal static event Action DidUpdated;

		public static GameView Instance;

		public GameView() : base(UIScreen.MainScreen.Bounds)
		{
			AutoResize = true;
			Instance = this;
			LayerRetainsBacking = false;
			LayerColorFormat = EAGLColorFormat.RGB565;
			MultipleTouchEnabled = true;
			textField = new MonoTouch.UIKit.UITextField();
			textField.Delegate = new TextFieldDelegate();
			textField.AutocorrectionType = UITextAutocorrectionType.No;
			screenScale = UIScreen.MainScreen.Scale;
			this.Add(textField);
		}

		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			foreach (var touch in touches.ToArray<UITouch>()) {
				for (int i = 0; i < Input.MaxTouches; i++) {
					if (activeTouches[i] == null) {
						var pt = touch.LocationInView(this);
						Vector2 position = new Vector2(pt.X, pt.Y) * screenScale * Input.ScreenToWorldTransform;
						if (i == 0) {
							Input.MousePosition = position;
							Input.SetKeyState(Key.Mouse0, true);
						}
						Key key = (Key)((int)Key.Touch0 + i);
						Input.SetTouchPosition(i, position);
						activeTouches[i] = touch;
						Input.SetKeyState(key, true);
						break;
					}
				}
			}
		}

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			foreach (var touch in touches.ToArray<UITouch>()) {
				for (int i = 0; i < Input.MaxTouches; i++) {
					if (activeTouches[i] == touch) {
						var pt = touch.LocationInView(this);
						Vector2 position = new Vector2(pt.X, pt.Y) * screenScale * Input.ScreenToWorldTransform;
						if (i == 0) {
							Input.MousePosition = position;
						}
						Input.SetTouchPosition(i, position);
					}
				}
			}
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			foreach (var touch in touches.ToArray<UITouch>()) {
				for (int i = 0; i < Input.MaxTouches; i++) {
					if (activeTouches[i] == touch) {
						var pt = touch.LocationInView(this);
						Vector2 position = new Vector2(pt.X, pt.Y) * screenScale * Input.ScreenToWorldTransform;
						if (i == 0) {
							Input.SetKeyState(Key.Mouse0, false);
						}
						activeTouches[i] = null;
						Key key = (Key)((int)Key.Touch0 + i);
						Input.SetKeyState(key, false);
					}
				}
			}
		}
	
		public override void LayoutSubviews()
		{
			// mike: the basic implementation recreates GL context each time user rotates a device.
			// This is a workaround.
			//var ctx = this.GraphicsContext;
			//if (ctx != null) {
			//	this.GraphicsContext = null;
			base.LayoutSubviews();
			//	this.GraphicsContext = ctx;
			//} else {
			//	base.LayoutSubviews();
			//}
			TexturePool.Instance.DiscardAllTextures();
		}

		public override void TouchesCancelled(NSSet touches, UIEvent evt)
		{
			TouchesEnded(touches, evt);
		}

		[Export("layerClass")]
		public static new Class GetLayerClass()
		{
			return iPhoneOSGameView.GetLayerClass();
		}

		string prevText;

		public void ChangeOnscreenKeyboardText(string text)
		{
			prevText = text;
			textField.Text = text;
		}

		public void ShowOnscreenKeyboard(bool show, string text)
		{
			if (show != textField.IsFirstResponder) {
				ChangeOnscreenKeyboardText(text);
				if (show) {
					textField.BecomeFirstResponder();
				} else {
					textField.ResignFirstResponder();
				}
			}
		}

		protected override void ConfigureLayer(CAEAGLLayer eaglLayer)
		{
			eaglLayer.Opaque = true;
			// Grisha: support retina displays
			// read
			// http://stackoverflow.com/questions/4884176/retina-display-image-quality-problem/9644622
			// for more information.
			eaglLayer.ContentsScale = screenScale;
		}

		protected override void CreateFrameBuffer()
		{
			ContextRenderingApi = EAGLRenderingAPI.OpenGLES1;
			base.CreateFrameBuffer();
		}

		public void UpdateFrame()
		{
			OnUpdateFrame(null);
		}

		private long prevTime = 0;

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			long currentTime = ApplicationToolbox.GetMillisecondsSinceGameStarted();
			int delta = (int)(currentTime - prevTime);
			prevTime = currentTime;
			delta = delta.Clamp(0, 40);
			Input.ProcessPendingKeyEvents();
			Input.MouseVisible = true;
			Application.Instance.OnUpdateFrame(delta);
			Input.TextInput = null;
			Input.CopyKeysState();
			Input.SetKeyState(Key.Enter, false);
			ProcessTextInput();
			if (DidUpdated != null) {
				DidUpdated();
			}
		}

		void ProcessTextInput()
		{
			var currText = textField.Text ?? "";
			prevText = prevText ?? "";
			if (currText.Length > prevText.Length) {
				Input.TextInput = currText.Substring(prevText.Length);
			} else {
				for (int i = 0; i < prevText.Length - currText.Length; i++) {
					Input.TextInput += '\b';
				}
			}
			prevText = currText;
		}

		public void RenderFrame()
		{
			OnRenderFrame(null);
		}

		public override void WillMoveToWindow(UIWindow window)
		{
			// Base implementation disposes framebuffer when window is null.
			// This causes blackscreen after "Bigfish: Show More Games" screen.
			// So disable it.
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			if (!Application.Instance.Active) {
				return;
			}
			MakeCurrent();
			Application.Instance.OnRenderFrame();
			SwapBuffers();
			ApplicationToolbox.RefreshFrameRate();
		}

		public float FrameRate {
			get {
				return ApplicationToolbox.FrameRate;
			}
		}
	}
}
#endif