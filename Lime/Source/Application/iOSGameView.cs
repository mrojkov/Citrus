#if iOS
using CoreAnimation;
using Foundation;
using ObjCRuntime;
using OpenGLES;
using UIKit;

using OpenTK.Graphics.ES20;
using OpenTK.Platform.iPhoneOS;
using GL = OpenTK.Graphics.ES20.GL;

namespace Lime
{
	public class GameView : Lime.Xamarin.iPhoneOSGameView
	{
		class TextViewDelegate : UITextViewDelegate
		{
			Input input;

			public TextViewDelegate(Input input)
			{
				this.input = input;
			}

			public override bool ShouldChangeText(UITextView textView, NSRange range, string text)
			{
				if (text.Equals("\n")) {
					input.SetKeyState(Key.Enter, true);
					input.SetKeyState(Key.Enter, false);
				}
				return true;
			}

			public override void EditingEnded(UITextView textView)
			{
				input.SetKeyState(Key.DismissSoftKeyboard, true);
				input.SetKeyState(Key.DismissSoftKeyboard, false);
			}
		}

		private CustomUITextView textView;
		private UITouch[] activeTouches = new UITouch[Input.MaxTouches];
		private float screenScale;
		private Input input;
		private string pendingTextInput = string.Empty;

		public Vector2 ClientSize { get; private set; }
		public bool DisableUpdateAndRender;

		public GameView(Input input) : base(UIScreen.MainScreen.Bounds)
		{
			this.input = input;
			AutoResize = true;
			LayerRetainsBacking = false;
			LayerColorFormat = EAGLColorFormat.RGB565;
			MultipleTouchEnabled = true;
			textView = new CustomUITextView(this);
			textView.Delegate = new TextViewDelegate(input);
			textView.AutocorrectionType = UITextAutocorrectionType.No;
			textView.AutocapitalizationType = UITextAutocapitalizationType.None;
			screenScale = (float)UIScreen.MainScreen.Scale;
			this.Add(textView);
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

		private void RefreshWindowSize()
		{
			ClientSize = new Vector2 {
				X = (int)(Bounds.Width),
				Y = (int)(Bounds.Height)
			};
		}

		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			foreach (var touch in touches.ToArray<UITouch>()) {
				for (int i = 0; i < Input.MaxTouches; i++) {
					if (activeTouches[i] == null) {
						var pt = touch.LocationInView(this);
						var position = new Vector2((float)pt.X, (float)pt.Y) * input.ScreenToWorldTransform;
						if (i == 0) {
							input.MousePosition = position;
							input.SetKeyState(Key.Mouse0, true);
						}
						Key key = (Key)((int)Key.Touch0 + i);
						input.SetTouchPosition(i, position);
						activeTouches[i] = touch;
						input.SetKeyState(key, true);
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
						var position = new Vector2((float)pt.X, (float)pt.Y) * input.ScreenToWorldTransform;
						if (i == 0) {
							input.MousePosition = position;
						}
						input.SetTouchPosition(i, position);
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
						var position = new Vector2((float)pt.X, (float)pt.Y) * input.ScreenToWorldTransform;
						if (i == 0) {
							input.SetKeyState(Key.Mouse0, false);
							input.MousePosition = position;
						}
						activeTouches[i] = null;
						Key key = (Key)((int)Key.Touch0 + i);
						input.SetKeyState(key, false);
						input.SetTouchPosition(i, position);
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

		public void ShowSoftKeyboard(bool show)
		{
			if (show != textView.IsFirstResponder) {
				if (show) {
					textView.BecomeFirstResponder();
				} else {
					textView.ResignFirstResponder();
				}
			}
		}

		public void DoRenderFrame()
		{
			OnRenderFrame(null);
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
		private uint depthRenderBuffer;

		protected override void CreateFrameBuffer()
		{
			ContextRenderingApi = EAGLRenderingAPI.OpenGLES2;
			base.CreateFrameBuffer();
			// Create a depth renderbuffer
			GL.GenRenderbuffers(1, out depthRenderBuffer);
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRenderBuffer);
			
			// Allocate storage for the new renderbuffer
			GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferInternalFormat.DepthComponent16, 
				(int)(Size.Width * screenScale), (int)(Size.Height * screenScale));
			
			// Attach the renderbuffer to the framebuffer's depth attachment point
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.DepthAttachment, RenderbufferTarget.Renderbuffer, depthRenderBuffer);
			
			GL.ClearDepth(1.0f); // Depth Buffer Setup
			GL.DepthFunc(DepthFunction.Lequal); // The Type Of Depth Test To Do				
		}
		
		protected override void DeleteBuffers()
		{
			base.DeleteBuffers();
			GL.DeleteRenderbuffers(1, ref depthRenderBuffer);
			depthRenderBuffer = 0;
		}

		public override void WillMoveToWindow(UIWindow window)
		{
			// Base implementation disposes framebuffer when window is null.
			// This causes blackscreen after "Bigfish: Show More Games" screen.
			// So disable it.
		}

		protected override void OnUpdateFrame(Xamarin.FrameEventArgs e)
		{
			base.OnUpdateFrame(e);
			ProcessTextInput();
		}

		private void ProcessTextInput()
		{
			input.TextInput = pendingTextInput;
			pendingTextInput = string.Empty;
		}

		private class CustomUITextView : UITextView
		{
			private readonly GameView view;

			public CustomUITextView(GameView view)
			{
				this.view = view;
			}

			public override void InsertText(string text)
			{
				view.pendingTextInput += text;
			}

			public override void DeleteBackward()
			{
				view.pendingTextInput += '\b';
			}
		}
	}
}
#endif