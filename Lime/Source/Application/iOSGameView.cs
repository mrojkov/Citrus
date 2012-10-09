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

		private long tickCount;

		public void UpdateFrame()
		{
			OnUpdateFrame(null);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			Input.ProcessPendingKeyEvents();
			Input.MouseVisible = true;

			long delta = (System.DateTime.Now.Ticks / 10000L) - tickCount;
			if (tickCount == 0) {
				tickCount = delta;
				delta = 0;
			} else {
				tickCount += delta;
			}
			Input.ProcessPendingKeyEvents();
			Input.MouseVisible = true;
			// Ensure time delta lower bound is 16.6 frames per second.
			// This is protection against time leap on inactive state
			// and multiple updates of node hierarchy.
 			delta = Math.Min(delta, 60);
			Application.Instance.OnUpdateFrame((int)delta);
			Input.TextInput = null;
			Input.CopyKeysState();
			Input.SetKeyState(Key.Enter, false);
			ProcessTextInput();
		}

		string prevText;
		void ProcessTextInput()
		{
			var currText = textField.Text;
			if (currText == null) { currText = ""; }
			if (prevText == null) { prevText = ""; }
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