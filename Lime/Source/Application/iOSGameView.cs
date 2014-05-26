#if iOS
using System;
using OpenTK;
using OpenTK.Platform.iPhoneOS;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;

namespace Lime
{
	public class GameView : Lime.Xamarin.iPhoneOSGameView
	{
		private const float MaxFrameDelta = 0.04f;

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

		public readonly RenderingApi RenderingApi = RenderingApi.ES20;

		internal static event Action DidUpdated;

		public static GameView Instance;

		public GameView() : base(UIScreen.MainScreen.Bounds)
		{
			Instance = this;
			AutoResize = true;
			LayerRetainsBacking = false;
			LayerColorFormat = EAGLColorFormat.RGB565;
			MultipleTouchEnabled = true;
			textField = new MonoTouch.UIKit.UITextField();
			textField.Delegate = new TextFieldDelegate();
			textField.AutocorrectionType = UITextAutocorrectionType.No;
			screenScale = UIScreen.MainScreen.Scale;
			this.Add(textField);
			RefreshWindowSize();
		}

		public override void LayoutSubviews()
		{
			if (backgroundContext == null) {
				backgroundContext = Lime.Xamarin.Utilities.CreateGraphicsContext(EAGLRenderingAPI.OpenGLES2);
			}
			RefreshWindowSize();
			base.LayoutSubviews();
		}

		void RefreshWindowSize()
		{
			var scale = UIScreen.MainScreen.Scale;
			var size = new Size {
				Width = (int)(Bounds.Width * scale),
				Height = (int)(Bounds.Height * scale)
			};
			Application.Instance.WindowSize = size;
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

		// Background context shares all resources while the main context is being recreated
		OpenTK.Graphics.IGraphicsContext backgroundContext;

		protected override void CreateFrameBuffer()
		{
			ContextRenderingApi = EAGLRenderingAPI.OpenGLES2;
			base.CreateFrameBuffer();	
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
			Input.TextInput = null;
			Input.CopyKeysState();
			Input.SetKeyState(Key.Enter, false);
			ProcessTextInput();
			if (DidUpdated != null) {
				DidUpdated();
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

		public new void RenderFrame()
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
			FPSCalculator.Refresh();
		}

		public float FrameRate {
			get {
				return FPSCalculator.FPS;
			}
		}
	}
}
#endif