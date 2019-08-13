#if iOS
using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using OpenGLES;
using UIKit;
using ObjCRuntime;

using Metal;
using MetalKit;

namespace Lime
{
	public interface IGameView
	{
		event Action RenderFrame;
		event Action<float> UpdateFrame;
		Vector2 ClientSize { get; }
		float PixelScale { get; }
		void Run();
		void Stop();
		void MakeCurrent();
		void SwapBuffers();
	}

	public class MetalGameView : MTKView, IMTKViewDelegate, IGameView
	{
		private bool suspended;
		private bool disposed;
		private System.Diagnostics.Stopwatch stopwatch;
		private TimeSpan prevUpdateTime;
		private Lime.Graphics.Platform.Vulkan.PlatformRenderContext vkContext;
		private Lime.Graphics.Platform.Vulkan.Swapchain vkSwapChain;
		private CAMetalLayer metalLayer;
		private bool shouldUpdateMetalLayerSize;

		public Vector2 ClientSize { get; private set; }
		public float PixelScale => (float)ContentScaleFactor;

		public event Action<float> UpdateFrame;
		public event Action RenderFrame;

		public static bool IsMetalSupported()
		{
			return UIDevice.CurrentDevice.CheckSystemVersion(10, 0) && MTLDevice.SystemDefault != null;
		}

		public MetalGameView(CGRect frame) : base(frame, MTLDevice.SystemDefault)
		{
			stopwatch = new System.Diagnostics.Stopwatch();
			Delegate = this;
			SetupMetal();
		}

		public void Draw(MTKView view)
		{
			if (vkContext == null) {
				vkContext = new Graphics.Platform.Vulkan.PlatformRenderContext();
				PlatformRenderer.Initialize(vkContext);
			}
			if (vkSwapChain == null) {
				shouldUpdateMetalLayerSize = true;
			}
			if (shouldUpdateMetalLayerSize) {
				var screen = Window?.Screen ?? UIScreen.MainScreen;
				var drawableSize = Bounds.Size;
				drawableSize.Width *= screen.NativeScale;
				drawableSize.Height *= screen.NativeScale;
				metalLayer.DrawableSize = drawableSize;
				if (vkSwapChain == null) {
					vkSwapChain = new Graphics.Platform.Vulkan.Swapchain(vkContext, this.Handle, (int)metalLayer.DrawableSize.Width, (int)metalLayer.DrawableSize.Height);
				} else {
					vkSwapChain.Resize((int)metalLayer.DrawableSize.Width, (int)metalLayer.DrawableSize.Height);
				}
				shouldUpdateMetalLayerSize = false;
			}
			shouldUpdateMetalLayerSize = false;
			var curUpdateTime = stopwatch.Elapsed;
			if (prevUpdateTime == TimeSpan.Zero) {
				prevUpdateTime = curUpdateTime;
			}
			var delta = (float)(curUpdateTime - prevUpdateTime).TotalSeconds;
			UpdateFrame?.Invoke(delta);
			prevUpdateTime = curUpdateTime;
			RenderFrame?.Invoke();
		}

		void IMTKViewDelegate.DrawableSizeWillChange(MTKView view, CGSize size) { }
 		
		void SetupMetal()
		{
			metalLayer = (CAMetalLayer)Layer;
			// Setup metal layer and add as sub layer to view
			metalLayer.Opaque = true;
			metalLayer.PixelFormat = MTLPixelFormat.BGRA8Unorm;
			// Change this to NO if the compute encoder is used as the last pass on the drawable texture
			metalLayer.FramebufferOnly = true;
		}

		[Export("layerClass")]
		public static Class GetLayerClass()
		{
			return new Class(typeof(CAMetalLayer));
		}

		protected override void Dispose(bool disposing)
		{
			if (disposed) {
				return;
			}
			if (disposing) {
				if (stopwatch != null)
					stopwatch.Stop();
				stopwatch = null;
			}
			base.Dispose(disposing);
			disposed = true;
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			ContentScaleFactor = Window.Screen.NativeScale;
		}

		public override nfloat ContentScaleFactor
		{
			get => base.ContentScaleFactor;
			set
			{
				base.ContentScaleFactor = value;
				shouldUpdateMetalLayerSize = true;
			}
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			ClientSize = new Vector2((float)Layer.Bounds.Size.Width, (float)Layer.Bounds.Size.Height);
			shouldUpdateMetalLayerSize = true;
		}
		
		public void MakeCurrent()
		{
			vkContext.Begin(vkSwapChain);
		}

		public void SwapBuffers()
		{
			vkContext.Present();
		}

		public void Run()
		{
			prevUpdateTime = TimeSpan.Zero;
			Resume();
		}

		public void Stop()
		{
			suspended = false;
		}

		private void Suspend()
		{
			stopwatch.Stop();
			suspended = true;
		}

		private void Resume()
		{
			stopwatch.Start();
			suspended = false;
		}

		public override void WillMoveToWindow(UIWindow window)
		{
			if (window == null && !suspended) {
				Suspend();
			} else if (window != null && suspended) {
				Resume();
			}
		}
	}
}

#endif