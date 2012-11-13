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

		class TextFieldDelegate : UITextFieldDelegate
		{
			public override bool ShouldReturn(UITextField textField)
			{
				Input.SetKeyState(Key.Enter, true);
				return false;
			}
		}

		public static GameView Instance;

		public GameView() : base(UIScreen.MainScreen.Bounds)
		{
			Instance = this;
			LayerRetainsBacking = false;
			LayerColorFormat = EAGLColorFormat.RGB565;
			MultipleTouchEnabled = true;
			textField = new MonoTouch.UIKit.UITextField();
			textField.Delegate = new TextFieldDelegate();
			textField.AutocorrectionType = UITextAutocorrectionType.No;
			this.Add(textField);
		}

		[Export("layerClass")]
		public static new Class GetLayerClass()
		{
			return iPhoneOSGameView.GetLayerClass();
		}

		public void ShowOnscreenKeyboard(bool show)
		{
			if (show != textField.IsFirstResponder) {
				textField.Text = null;
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
			long currentTime = GetCurrentTime();
			int delta = (int)(currentTime - prevTime);
			prevTime = currentTime;
			Mathf.Clamp(ref delta, 0, 40);
			Input.ProcessPendingKeyEvents();
			Input.MouseVisible = true;
			Input.ProcessPendingKeyEvents();
			Input.MouseVisible = true;
			Application.Instance.OnUpdateFrame(delta);
			Input.TextInput = null;
			Input.CopyKeysState();
			Input.SetKeyState(Key.Enter, false);
			ProcessTextInput();
		}

		private long startTime = 0;

		private long GetCurrentTime()
		{
			long t = DateTime.Now.Ticks / 10000L;
			if (startTime == 0) {
				startTime = t;
			}
			return t - startTime;
		}

		string prevText;
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
			MakeCurrent();
			Application.Instance.OnRenderFrame();
			SwapBuffers();
			UpdateFrameRate();
		}

		private long timeStamp;
		private int countedFrames;
		private float frameRate;

		private void UpdateFrameRate()
		{
			countedFrames++;
			long t = System.DateTime.Now.Ticks;
			long milliseconds = (t - timeStamp) / 10000;
			if (milliseconds > 1000) {
				if (timeStamp > 0)
					frameRate = (float)countedFrames / ((float)milliseconds / 1000.0f);
				timeStamp = t;
				countedFrames = 0;
			}
		}

		public float FrameRate {
			get {
				return frameRate;
			}
		}
	}
}
#endif