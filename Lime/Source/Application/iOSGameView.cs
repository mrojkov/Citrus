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

		internal static event Action DidUpdated;

		public static GameView Instance;
		public bool IsRetinaDisplay { get; internal set; }

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

		string prevText;
		public void ShowOnscreenKeyboard(bool show, string text)
		{
			if (show != textField.IsFirstResponder) {
				prevText = text;
				textField.Text = text;
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
			eaglLayer.ContentsScale = UIScreen.MainScreen.Scale;
			if (UIScreen.MainScreen.Scale > 1.0f) {
				IsRetinaDisplay = true;
			}
		}

		protected override void CreateFrameBuffer()
		{
			// OpenTK bug - sometimes CreateFrameBuffer is being called twise, 
			// so within OpenTK an exception ocurred cause of duplication GraphicsContext keys in a dictionary.
			// Force garbage collection to eliminate weak keys in the dictionary.
			System.GC.Collect();
			System.GC.WaitForPendingFinalizers();
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
			long currentTime = TimeUtils.GetMillisecondsSinceGameStarted();
			int delta = (int)(currentTime - prevTime);
			prevTime = currentTime;
			delta = delta.Clamp(0, 40);
			Input.ProcessPendingKeyEvents();
			Input.MouseVisible = true;
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
			TimeUtils.RefreshFrameRate();
		}

		public float FrameRate {
			get {
				return TimeUtils.FrameRate;
			}
		}
	}
}
#endif