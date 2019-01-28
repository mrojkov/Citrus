#if iOS
using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using OpenGLES;
using UIKit;
using ObjCRuntime;
using Lime.Graphics.Platform.OpenGL;

namespace Lime
{
	public class GLGameView : UIView, IGameView
	{
		private bool suspended;
		private bool disposed;
		private int framebuffer;
		private int colorRenderbuffer;
		private int depthRenderbuffer;
		private ITimeSource timeSource;
		private System.Diagnostics.Stopwatch stopwatch;
		private TimeSpan prevUpdateTime;
		private EAGLContext mgleaglContext;
		private PlatformRenderContext renderContext;
		private WeakReference framebufferWindow;
		private WeakReference framebufferLayer;

		public Vector2 ClientSize { get; private set; }
		public float PixelScale { get; private set; }

		public event Action<float> UpdateFrame;
		public event Action RenderFrame;

		public GLGameView(CGRect frame) : base(frame)
		{
			stopwatch = new System.Diagnostics.Stopwatch();
		}

		[Export("layerClass")]
		public static Class GetLayerClass()
		{
			return new Class(typeof(CAEAGLLayer));
		}

		private void SetupContextAndFramebuffer()
		{
			if (mgleaglContext == null) {
				ConfigureLayer();
				mgleaglContext = new EAGLContext(EAGLRenderingAPI.OpenGLES2);
				MakeCurrent();
				renderContext = new PlatformRenderContext();
				RenderContextManager.MakeCurrent(renderContext);
			}
			DestroyFramebuffer();
			CreateFramebuffer();					
			framebufferWindow = new WeakReference(Window);
			framebufferLayer = new WeakReference(Layer);
		}
		
		private void ConfigureLayer()
		{
			var eaglLayer = (CAEAGLLayer)Layer;
			eaglLayer.Opaque = true;
			// Grisha: support retina displays
			// read
			// http://stackoverflow.com/questions/4884176/retina-display-image-quality-problem/9644622
			// for more information.
			PixelScale = (float)UIScreen.MainScreen.Scale;
			eaglLayer.ContentsScale = PixelScale;
		}
		
		private void CreateFramebuffer()
		{
			if (framebuffer != 0) {
				return;
			}
			MakeCurrent();
			framebuffer = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
			GLHelper.CheckGLErrors();
			
			colorRenderbuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, colorRenderbuffer);
			GLHelper.CheckGLErrors();
			
			var eaglLayer = (CAEAGLLayer)Layer;
			if (!mgleaglContext.RenderBufferStorage((uint)RenderbufferTarget.Renderbuffer, eaglLayer)) {
				throw new InvalidOperationException("EAGLContext.RenderBufferStorage() failed");
			}
			
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, RenderbufferTarget.Renderbuffer, colorRenderbuffer);
			GL.Viewport(0, 0, (int)ClientSize.X, (int)ClientSize.Y);
			GL.Scissor(0, 0, (int)ClientSize.X, (int)ClientSize.Y);
			GLHelper.CheckGLErrors();

			// Create a depth renderbuffer
			depthRenderbuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRenderbuffer);

			// Allocate storage for the new renderbuffer
			var scale = (float)UIScreen.MainScreen.Scale;
			GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, (RenderbufferInternalFormat)All.Depth24Stencil8Oes, 
				(int)(ClientSize.X * scale), (int)(ClientSize.Y * scale));
			// Attach the renderbuffer to the framebuffer's depth attachment point
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.DepthAttachment, RenderbufferTarget.Renderbuffer, depthRenderbuffer);
		  	// Attach the renderbuffer to the framebuffer's stencil attachment point
		  	GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.StencilAttachment, RenderbufferTarget.Renderbuffer, depthRenderbuffer);

			GL.ClearStencil(0);
			GL.ClearDepth(1.0f);

			GL.DepthFunc(DepthFunction.Lequal);
			GLHelper.CheckGLErrors();
		}

		private void DestroyFramebuffer()
		{
			if (framebuffer == 0) {
				return;
			}
			MakeCurrent();
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
			GL.DeleteRenderbuffer(colorRenderbuffer);
			GL.DeleteRenderbuffer(depthRenderbuffer);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			GL.DeleteFramebuffer(framebuffer);
			GLHelper.CheckGLErrors();
			colorRenderbuffer = 0;
			depthRenderbuffer = 0;
			framebuffer = 0;
			EAGLContext.SetCurrentContext(null);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposed) {
				return;
			}
			if (disposing) {
				if (timeSource != null) {
					timeSource.Invalidate();
				}
				timeSource = null;
				if (stopwatch != null) {
					stopwatch.Stop();
				}
				stopwatch = null;
				DestroyFramebuffer();
			}
			base.Dispose(disposing);
			disposed = true;
		}

		public override void LayoutSubviews()
		{
			var bounds = Bounds;
			if ((float)bounds.Width != ClientSize.X || (float)bounds.Height != ClientSize.Y) {
				ClientSize = new Vector2((float)Layer.Bounds.Size.Width, (float)Layer.Bounds.Size.Height);
			    SetupContextAndFramebuffer();
			}
		}

		public void MakeCurrent()
		{
			if (!EAGLContext.SetCurrentContext(mgleaglContext)) {
				throw new InvalidOperationException("Unable to change current EAGLContext.");
			}
			GLHelper.CheckGLErrors();
		}

		public void SwapBuffers()
		{
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, colorRenderbuffer);
			if (!mgleaglContext.PresentRenderBuffer((uint)All.Renderbuffer)) {
				throw new InvalidOperationException("EAGLContext.PresentRenderbuffer failed.");
			}
		}

		public void Run()
		{
			if (timeSource != null) {
				timeSource.Invalidate();
			}
			timeSource = new CADisplayLinkTimeSource(this, frameInterval: 1);
			SetupContextAndFramebuffer();
			prevUpdateTime = TimeSpan.Zero;
			Resume();
		}

		public void Stop()
		{
			if (timeSource != null) {
				timeSource.Invalidate();
				timeSource = null;
			}
			suspended = false;
			DestroyFramebuffer();
		}

		private void Suspend()
		{
			if (timeSource != null) {
				timeSource.Suspend();
			}
			stopwatch.Stop();
			suspended = true;
		}

		private void Resume()
		{
			if (timeSource != null) {
				timeSource.Resume();
			}
			stopwatch.Start();
			suspended = false;
		}

		//public UIImage Capture()
		//{		
		//	// Source: https://developer.apple.com/library/ios/#qa/qa2010/qa1704.html
		//	int backingWidth = 0, backingHeight = 0;

		//	// Bind the color renderbuffer used to render the OpenGL ES view
		//	// If your application only creates a single color renderbuffer which is already bound at this point,
		//	// this call is redundant, but it is needed if you're dealing with multiple renderbuffers.
		//	// Note, replace "_colorRenderbuffer" with the actual name of the renderbuffer object defined in your class.
		//	GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, colorRenderbuffer);

		//	// Get the size of the backing CAEAGLLayer
		//	GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferWidth, out backingWidth);
		//	GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferHeight, out backingHeight);

		//	int width = backingWidth, height = backingHeight;
		//	int dataLength = width * height * 4;
		//	var data = new byte[dataLength];

		//	// Read pixel data from the framebuffer
		//	GL.PixelStore(PixelStoreParameter.PackAlignment, 4);
		//	GL.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data);

		//	// Create a CGImage with the pixel data
		//	// If your OpenGL ES content is opaque, use kCGImageAlphaNoneSkipLast to ignore the alpha channel
		//	// otherwise, use kCGImageAlphaPremultipliedLast
		//	using (var data_provider = new CGDataProvider(data, 0, data.Length)) {
		//		using (var colorspace = CGColorSpace.CreateDeviceRGB()) {
		//			using (var iref = new CGImage(width, height, 8, 32, width * 4, colorspace,
		//				(CGImageAlphaInfo)((int)CGBitmapFlags.ByteOrder32Big |(int)CGImageAlphaInfo.PremultipliedLast),
		//				data_provider, null, true, CGColorRenderingIntent.Default))
		//			{
		//				// OpenGL ES measures data in PIXELS
		//				// Create a graphics context with the target size measured in POINTS
		//				int widthInPoints, heightInPoints;
		//				float scale = (float)ContentScaleFactor;
		//				widthInPoints = (int)(width / scale);
		//				heightInPoints = (int)(height / scale);
		//				UIGraphics.BeginImageContextWithOptions(new System.Drawing.SizeF(widthInPoints, heightInPoints), false, scale);
		//				try {
		//					var cgcontext = UIGraphics.GetCurrentContext();
		//					// UIKit coordinate system is upside down to GL/Quartz coordinate system
		//					// Flip the CGImage by rendering it to the flipped bitmap context
		//					// The size of the destination area is measured in POINTS
		//					cgcontext.SetBlendMode(CGBlendMode.Copy);
		//					cgcontext.DrawImage(new System.Drawing.RectangleF(0, 0, widthInPoints, heightInPoints), iref);
		//					// Retrieve the UIImage from the current context
		//					var image = UIGraphics.GetImageFromCurrentImageContext();
		//					return image;
		//				} finally {
		//					UIGraphics.EndImageContext();
		//				}
		//			}
		//		}
		//	}
		//}

		public override void WillMoveToWindow(UIWindow window)
		{
			if (window == null && !suspended) {
				Suspend();
			} else if (window != null && suspended) {
				if (framebufferLayer != null &&((CALayer)framebufferLayer.Target) != Layer ||
					framebufferWindow != null &&((UIWindow)framebufferWindow.Target) != window) 
				{
					SetupContextAndFramebuffer();
				}
			}
		}
		
		private void RunIteration(NSTimer timer)
		{
			var curUpdateTime = stopwatch.Elapsed;
			if (prevUpdateTime == TimeSpan.Zero)
				prevUpdateTime = curUpdateTime;
			var delta = (float)(curUpdateTime - prevUpdateTime).TotalSeconds;
			UpdateFrame?.Invoke(delta);
			prevUpdateTime = curUpdateTime;
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
			renderContext.Begin(framebuffer);
			RenderFrame?.Invoke();
			renderContext.End();
		}

		[Register]
		private class CADisplayLinkTimeSource : NSObject, ITimeSource
		{
			private static Selector selRunIteration = new Selector("runIteration");
	
			private GLGameView view;
			private CADisplayLink displayLink;
	
			public CADisplayLinkTimeSource(GLGameView view, int frameInterval)
			{
				this.view = view;
				if (displayLink != null) {
					displayLink.Invalidate();
				}
				displayLink = CADisplayLink.Create(this, selRunIteration);
				displayLink.FrameInterval = frameInterval;
				displayLink.Paused = true;
			}
	
			public void Suspend()
			{
				displayLink.Paused = true;
			}
	
			public void Resume()
			{
				displayLink.Paused = false;
				displayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoop.NSDefaultRunLoopMode);
			}
	
			public void Invalidate()
			{
				if (displayLink != null) {
					displayLink.Invalidate();
					displayLink = null;
				}
			}
	
			[Export("runIteration")]
			[Preserve(Conditional = true)]
			void RunIteration()
			{
				view.RunIteration(null);
			}
		}

		private interface ITimeSource
		{
			void Suspend();
			void Resume();
	
			void Invalidate();
		}
	
		private class NSTimerTimeSource : ITimeSource
		{
			private TimeSpan timeout;
			private NSTimer timer;
			private GLGameView view;
	
			public NSTimerTimeSource(GLGameView view, double updatesPerSecond)
			{
				this.view = view;
	
				// Can't use TimeSpan.FromSeconds() as that only has 1ms
				// resolution, and we need better(e.g. 60fps doesn't fit nicely
				// in 1ms resolution, but does in ticks).
				timeout = new TimeSpan((long)(((1.0 * TimeSpan.TicksPerSecond) / updatesPerSecond) + 0.5));
			}
	
			public void Suspend()
			{
				if (timer != null) {
					timer.Invalidate();
					timer = null;
				}
			}
	
			public void Resume()
			{
				if (timeout != new TimeSpan(-1))
					timer = NSTimer.CreateRepeatingScheduledTimer(timeout, view.RunIteration);
			}
	
			public void Invalidate()
			{
				if (timer != null) {
					timer.Invalidate();
					timer = null;
				}
			}
		}
	}
}

#endif