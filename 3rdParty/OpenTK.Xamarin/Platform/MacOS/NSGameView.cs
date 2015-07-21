#if MAC
using System;
using AppKit;
using CoreVideo;
using Foundation;
using OpenGL;
using System.ComponentModel;
using OpenTK;
using CoreGraphics;
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Input;

namespace OpenTK
{
	public class NSGameView : NSView
	{
		private NSOpenGLContext openGLContext;
		private NSOpenGLPixelFormat pixelFormat;
		private CVDisplayLink displayLink;
		private NSTimer animationTimer;
		private NSTrackingArea trackingArea;

		private bool disposed;
		private bool animating;
		private bool displayLinkSupported = false;

		private WindowState windowState = WindowState.Normal;
		private WindowBorder windowBorder;

		private DateTime prevUpdateTime;
		private DateTime prevRenderTime;

		public Input.Mouse Mouse;
		public Input.Keyboard Keyboard;

		public NSGameView(CGRect frame)
			: this(frame, null, GraphicsMode.Default, 1, 0)
		{
		}

		public NSGameView(CGRect frame, NSOpenGLContext context)
			: this(frame, context, GraphicsMode.Default, 1, 0)
		{
		}

		public NSGameView(CGRect frame, int major, int minor)
			: this(frame, null, GraphicsMode.Default, major, minor)
		{
		}

		public NSGameView(CGRect frame, GraphicsMode mode, int major, int minor)
			: this(frame, null, mode, major, minor)
		{
		}

		public NSGameView(CGRect frame, NSOpenGLContext shareContext, GraphicsMode mode, int major, int minor)
			: base(frame)
		{
			pixelFormat = SelectPixelFormat(mode, major, minor);

			if (pixelFormat == null) {
				throw new InvalidOperationException(string.Format("Failed to contruct NSOpenGLPixelFormat for GraphicsMode {0}", mode));
			}				

			openGLContext = new NSOpenGLContext(pixelFormat, shareContext);

			if (openGLContext == null) {
				throw new InvalidOperationException(string.Format("Failed to construct NSOpenGLContext {0}", mode));
			}

			openGLContext.MakeCurrentContext();

			SwapInterval = true;
			DisplaylinkSupported = true;
		}

		public override void UpdateTrackingAreas()
		{
			if (trackingArea != null) {
				RemoveTrackingArea(trackingArea);
				trackingArea.Dispose();
			}
			var viewBounds = this.Bounds;
			var options = NSTrackingAreaOptions.MouseMoved | NSTrackingAreaOptions.ActiveInKeyWindow | NSTrackingAreaOptions.MouseEnteredAndExited;
			trackingArea = new NSTrackingArea(viewBounds, options, this, null);
			AddTrackingArea(trackingArea);
		}
			
		public override void MouseDown(NSEvent theEvent)
		{
			if (Mouse != null) {
				Mouse.OnButtonDown(new MouseButtonEventArgs() { Button = MouseButton.Left });
			}
		}

		public override void RightMouseDown(NSEvent theEvent)
		{
			if (Mouse != null) {
				Mouse.OnButtonDown(new MouseButtonEventArgs() { Button = MouseButton.Right });
			}			
		}

		public override void RightMouseUp(NSEvent theEvent)
		{
			if (Mouse != null) {
				Mouse.OnButtonUp(new MouseButtonEventArgs() { Button = MouseButton.Right });
			}			
		}				
												
		public override void MouseUp(NSEvent theEvent)
		{
			if (Mouse != null) {
				Mouse.OnButtonUp(new MouseButtonEventArgs() { Button = MouseButton.Left });
			}
		}	

		public override void MouseDragged(NSEvent theEvent)
		{
			if (Mouse != null) {
				var point = new Point((int)theEvent.LocationInWindow.X, (int)Frame.Height - (int)theEvent.LocationInWindow.Y);			
				Mouse.OnMove(new MouseMoveEventArgs() { X = point.X, Y = point.Y });
			}
		}

		public override void MouseMoved(NSEvent theEvent)
		{
			if (Mouse != null) {
				var point = new Point((int)theEvent.LocationInWindow.X, (int)Frame.Height - (int)theEvent.LocationInWindow.Y);			
				Mouse.OnMove(new MouseMoveEventArgs() { X = point.X, Y = point.Y });
			}
		}

		public override void KeyDown(NSEvent theEvent)
		{
			if (Keyboard != null) {				
				var key = MacOSKeyMap.GetKey((MacOSKeyCode)theEvent.KeyCode);
				Keyboard.OnKeyDown(new KeyboardKeyEventArgs {Key = key});			
				foreach(var c in theEvent.Characters)
					// Imitation of original OpenTK backspace bug
					if ((int)c != 127)
						Keyboard.OnKeyPress(new KeyPressEventArgs{KeyChar = c});
			}
		}

		public override void KeyUp(NSEvent theEvent)
		{
			if (Keyboard != null) {
				var key = MacOSKeyMap.GetKey((MacOSKeyCode)theEvent.KeyCode);
				Keyboard.OnKeyUp(new KeyboardKeyEventArgs {Key = key});
			}
		}

		private NSOpenGLPixelFormat SelectPixelFormat(GraphicsMode mode, int majorVersion, int minorVersion)
		{
			var attributes = new List<NSOpenGLPixelFormatAttribute>();
			var profile = NSOpenGLProfile.VersionLegacy;

			if (majorVersion > 3 || (majorVersion == 3 && minorVersion >= 2)) {
				profile = NSOpenGLProfile.Version3_2Core;
			}

			attributes.Add(NSOpenGLPixelFormatAttribute.OpenGLProfile);
			attributes.Add((NSOpenGLPixelFormatAttribute)profile); 

			if (mode.ColorFormat.BitsPerPixel > 0) {
				attributes.Add(NSOpenGLPixelFormatAttribute.ColorSize);
				attributes.Add((NSOpenGLPixelFormatAttribute)mode.ColorFormat.BitsPerPixel); 
			}

			if (mode.Depth > 0) {
				attributes.Add(NSOpenGLPixelFormatAttribute.DepthSize);
				attributes.Add((NSOpenGLPixelFormatAttribute)mode.Depth); 
			}

			if (mode.Stencil > 0) {
				attributes.Add(NSOpenGLPixelFormatAttribute.StencilSize);
				attributes.Add((NSOpenGLPixelFormatAttribute)mode.Stencil); 
			}

			if (mode.AccumulatorFormat.BitsPerPixel > 0) {
				attributes.Add(NSOpenGLPixelFormatAttribute.AccumSize);
				attributes.Add((NSOpenGLPixelFormatAttribute)mode.AccumulatorFormat.BitsPerPixel); 
			}

			if (mode.Samples > 1) {
				attributes.Add(NSOpenGLPixelFormatAttribute.SampleBuffers);
				attributes.Add((NSOpenGLPixelFormatAttribute)1); 
				attributes.Add(NSOpenGLPixelFormatAttribute.Samples);
				attributes.Add((NSOpenGLPixelFormatAttribute)mode.Samples); 
			}

			if (mode.Buffers > 1) {
				attributes.Add(NSOpenGLPixelFormatAttribute.DoubleBuffer);
			}

			attributes.Add(NSOpenGLPixelFormatAttribute.Accelerated);
			attributes.Add((NSOpenGLPixelFormatAttribute)0); 

			return new NSOpenGLPixelFormat(attributes.ToArray());
		}

#region Public

		public void UpdateGLContext()
		{
			openGLContext.CGLContext.Lock();
			openGLContext.Update();
			openGLContext.CGLContext.Unlock();					
		}

		public NSOpenGLContext OpenGLContext
		{
			get { return openGLContext; }
		}

		public NSOpenGLPixelFormat PixelFormat
		{
			get { return pixelFormat; }
		}

		public void Stop()
		{
			if (animating) {
				if (displayLinkSupported) {
					if (displayLink != null && displayLink.IsRunning)
						displayLink.Stop();

				} else {
					animationTimer.Invalidate();
					animationTimer = null;
				}
			}
			animating = false;
		}

		public void Run()
		{
			AssertNonDisposed();
			OnLoad(EventArgs.Empty);

			openGLContext.SwapInterval = SwapInterval;

			if (displayLinkSupported)
				SetupDisplayLink();

			StartAnimation(0.0);
		}

		public void Run(double updatesPerSecond)
		{
			AssertNonDisposed();
			if (updatesPerSecond == 0.0) {
				Run();
				return;
			}

			OnLoad(EventArgs.Empty);

			SwapInterval = false;
			DisplaylinkSupported = false;

			openGLContext.SwapInterval = SwapInterval;

			if (displayLinkSupported)
				SetupDisplayLink();

			StartAnimation(updatesPerSecond);
		}

#region Override

		public override bool AcceptsFirstResponder()
		{
			// We want this view to be able to receive key events
			return true;
		}

		public override void DrawRect(CGRect dirtyRect)
		{
			if (animating) {
				if (displayLinkSupported) {
					if (displayLink.IsRunning)
						RenderScene();
				} else {
					RenderScene();
				}
			}
		}

		public override void LockFocus()
		{
			base.LockFocus();
			if (openGLContext.View != this)
				openGLContext.View = this;
		}

#endregion

#endregion

#region Protected

		protected NSViewController GetViewController()
		{
			NSResponder r = this;
			while (r != null) {
				var c = r as NSViewController;
				if (c != null)
					return c;
				r = r.NextResponder;
			}
			return null;
		}

#endregion

#region Private

		private bool SwapInterval { get; set; }

		private bool DisplaylinkSupported
		{
			get { return displayLinkSupported; }	
			set { displayLinkSupported = value; }
		}

		private NSWindowStyle GetStyleMask(WindowBorder border)
		{
			switch (border) {
				case WindowBorder.Resizable:
				return NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Titled | NSWindowStyle.Resizable;
				case WindowBorder.Fixed:
				return NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Titled;
				case WindowBorder.Hidden:
				return NSWindowStyle.Borderless;
			}
			return (NSWindowStyle)0;
		}

		private void UpdateWindowBorder(WindowBorder border)
		{
			AssertNonDisposed();
			var title = Title;
			Window.StyleMask = GetStyleMask(border);
			Title = title; // Title gets lost after going borderless
		}

		private void StartAnimation(double updatesPerSecond)
		{
			if (!animating) {
				if (displayLinkSupported) {
					if (displayLink != null && !displayLink.IsRunning)
						displayLink.Start();
				} else {
					var timeout = new TimeSpan((long)(((1.0 * TimeSpan.TicksPerSecond) / updatesPerSecond) + 0.5));
					if (SwapInterval) {
						animationTimer = NSTimer.CreateRepeatingScheduledTimer(timeout, delegate {
							NeedsDisplay = true;
						});
					} else {
						animationTimer = NSTimer.CreateRepeatingScheduledTimer(timeout, delegate {
							RenderScene();
						});
					}
					NSRunLoop.Current.AddTimer(animationTimer, NSRunLoopMode.Default);
					NSRunLoop.Current.AddTimer(animationTimer, NSRunLoopMode.EventTracking);
				}
			}

			animating = true;
		}

		private void DeAllocate()
		{
			Stop();
			displayLink = null;
		}

		void AssertNonDisposed()
		{
			if (disposed) {
				throw new ObjectDisposedException("");
			}
		}

		private void AssertContext()
		{
			if (openGLContext == null)
				throw new InvalidOperationException("Operation requires an OpenGLContext, which hasn't been created yet.");
		}

		private void RenderScene()
		{
			openGLContext.CGLContext.Lock();
			openGLContext.MakeCurrentContext();

			var curUpdateTime = DateTime.Now;
			if (prevUpdateTime.Ticks == 0) {
				prevUpdateTime = curUpdateTime;
			}
			var t = (curUpdateTime - prevUpdateTime).TotalSeconds;

			if (t <= 0)
				t = Double.Epsilon;

			OnUpdateFrame(EventArgs.Empty);
			prevUpdateTime = curUpdateTime;

			var curRenderTime = DateTime.Now;
			if (prevRenderTime.Ticks == 0) {
				prevRenderTime = curRenderTime;
			}
			t = (curRenderTime - prevRenderTime).TotalSeconds;

			if (t <= 0)
				t = Double.Epsilon;

			OnRenderFrame(EventArgs.Empty);
			prevRenderTime = curRenderTime;
			openGLContext.CGLContext.Unlock();
		}

		private void SetupDisplayLink()
		{
			if (displayLink != null)
				return;

			displayLink = new CVDisplayLink();
			displayLink.SetOutputCallback(DisplayLinkCallback);

			CGLContext cglContext = openGLContext.CGLContext;
			CGLPixelFormat cglPixelFormat = PixelFormat.CGLPixelFormat;
			displayLink.SetCurrentDisplay(cglContext, cglPixelFormat);
		}

		private CVReturn DisplayLinkCallback(CVDisplayLink displayLink, ref CVTimeStamp inNow, ref CVTimeStamp inOutputTime, CVOptionFlags flagsIn, ref CVOptionFlags flagsOut)
		{
			var result = CVReturn.Error;

			using (var pool = new NSAutoreleasePool()) {
				BeginInvokeOnMainThread(RenderScene);
				result = CVReturn.Success;
			}

			return result;
		}

#endregion

#region Virtual

#region Public

		public virtual string Title
		{
			get
			{
				AssertNonDisposed();
				if (Window != null)
					return Window.Title;
				else
					throw new NotSupportedException();
			}
			set
			{
				AssertNonDisposed();
				if (Window != null)
					Window.Title = value;
				else
					throw new NotSupportedException();
			}
		}

		public virtual bool Focused
		{
			get { return  base.Window.IsKeyWindow; }
		}

		public virtual bool Visible
		{
			get
			{
				AssertNonDisposed();
				return !base.Hidden;
			}
			set
			{
				AssertNonDisposed();
				if (base.Hidden != !value) {
					base.Hidden = !value;
					OnVisibleChanged(EventArgs.Empty);
				}
			}
		}

		public virtual WindowState WindowState
		{
			get
			{
				AssertNonDisposed();
				return windowState;
			}
			set
			{
				AssertNonDisposed();
				if (windowState != value) {
					windowState = value;
					OnWindowStateChanged(EventArgs.Empty);
				}
			}
		}

		public virtual WindowBorder WindowBorder
		{
			get
			{ 
				AssertNonDisposed();
				return windowBorder;
			}
			set
			{
				AssertNonDisposed();
				// Do not allow border changes during fullscreen mode.
				if (windowState == WindowState.Fullscreen || windowState == WindowState.Maximized || windowBorder == value)
					return;
				windowBorder = value;
				UpdateWindowBorder(windowBorder);
			}
		}

		public virtual void Close()
		{
			AssertNonDisposed();
			OnClosed(EventArgs.Empty);
		}

		public virtual void MakeCurrent()
		{
			AssertNonDisposed();
			AssertContext();
			openGLContext.MakeCurrentContext();
		}

		public virtual void SwapBuffers()
		{
			AssertNonDisposed();
			AssertContext();
			openGLContext.FlushBuffer();
		}

#endregion

#region Protected

		protected virtual void OnTitleChanged(EventArgs e)
		{
			var h = TitleChanged;
			if (h != null)
				h(this, EventArgs.Empty);
		}

		protected virtual void OnLoad(EventArgs e)
		{
			var h = Load;
			if (h != null)
				h(this, e);
		}

		protected virtual void OnUnload(EventArgs e)
		{
			var h = Unload;
			Stop();
			if (h != null)
				h(this, e);
		}

		protected virtual void OnUpdateFrame(EventArgs e)
		{
			var h = UpdateFrame;
			if (h != null)
				h(this,e);
		}

		protected virtual void OnRenderFrame(EventArgs e)
		{
			var h = RenderFrame;
			if (h != null)
				h(this,e);
		}

		protected virtual void OnKeyPressed(EventArgs e)
		{
			var h = KeyPress;
			if (h != null)
				h(this, e);
		}

		protected virtual void OnClosed(EventArgs e)
		{
			var h = Closed;
			if (h != null)
				h(this, e);
		}

		protected virtual void OnDisposed(EventArgs e)
		{
			var h = Disposed;
			if (h != null)
				h(this, e);
		}

		protected virtual void OnWindowStateChanged(EventArgs e)
		{
			var h = WindowStateChanged;
			if (h != null)
				h(this, EventArgs.Empty);
		}

		protected virtual void OnResize(object sender, EventArgs e)
		{
			var h = Resize;
			if (h != null)
				h(sender, e);
		}

		protected virtual void OnVisibleChanged(EventArgs e)
		{
			var h = VisibleChanged;
			if (h != null)
				h(this, EventArgs.Empty);
		}

#endregion

#endregion

#region Events

		public event EventHandler<EventArgs> UpdateFrame;
		public event EventHandler<EventArgs> RenderFrame;
		public event EventHandler<EventArgs> Resize;
		public event EventHandler<EventArgs> WindowStateChanged;
		public event EventHandler<EventArgs> KeyPress;
		public event EventHandler<EventArgs> Closed;
		public event EventHandler<EventArgs> Disposed;
		public event EventHandler<EventArgs> TitleChanged;
		public event EventHandler<EventArgs> VisibleChanged;
		public event EventHandler<EventArgs> Load;
		public event EventHandler<EventArgs> Unload;

#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposed)
				return;
			if (disposing) {
				DeAllocate();
				// DestroyFrameBuffer();
			}
			base.Dispose(disposing);
			disposed = true;
			if (disposing)
				OnDisposed(EventArgs.Empty);
		}

	}
}

#endif