#if MAC || MONOMAC
using System;
using AppKit;
using CoreVideo;
using Foundation;
using OpenGL;
using System.ComponentModel;
using CoreGraphics;
using System.Collections.Generic;
using OpenTK;
using System.Drawing;

namespace Lime.Platform
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
		private OpenTK.WindowBorder windowBorder;

		private DateTime prevUpdateTime;
		private DateTime prevRenderTime;

		private NSEventModifierMask prevMask;

		private Lime.Input input;

		public NSGameView(Lime.Input input, CGRect frame, NSOpenGLContext shareContext, GraphicsMode mode)
			: base(frame)
		{
			this.input = input;
			// Avoid of double scaling on high DPI displays
			this.WantsBestResolutionOpenGLSurface = true;
			
			pixelFormat = SelectPixelFormat(mode, 1, 0);

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
			input.SetKeyState(Key.Mouse0, true);
			input.SetKeyState(Key.Touch0, true);
		}

		public override void RightMouseDown(NSEvent theEvent)
		{
			input.SetKeyState(Key.Mouse1, true);
		}

		public override void RightMouseUp(NSEvent theEvent)
		{
			input.SetKeyState(Key.Mouse1, false);
		}

		public override void MouseUp(NSEvent theEvent)
		{
			input.SetKeyState(Key.Mouse0, false);
			input.SetKeyState(Key.Touch0, false);
		}

		public override void MouseDragged(NSEvent theEvent)
		{
			RefreshMousePosition(theEvent);
		}

		public override void MouseMoved(NSEvent theEvent)
		{
			RefreshMousePosition(theEvent);
		}

		void RefreshMousePosition(NSEvent theEvent)
		{
			var point = new Point((int)theEvent.LocationInWindow.X, (int)Frame.Height - (int)theEvent.LocationInWindow.Y);
			var p = new Vector2(point.X, point.Y) * (float)Window.BackingScaleFactor;
			input.MousePosition = p * input.ScreenToWorldTransform;
		}

		public override void ScrollWheel(NSEvent theEvent)
		{
			// On Mac and Win we assume this as number of "lines" to scroll, not pixels to scroll
			var wheelDelta = Math.Sign(theEvent.ScrollingDeltaY);
			if (wheelDelta > 0) {
				if (!input.HasPendingKeyEvent(Key.MouseWheelUp)) {
					input.SetKeyState(Key.MouseWheelUp, true);
					input.SetKeyState(Key.MouseWheelUp, false);
					input.WheelScrollAmount = wheelDelta;
				} else {
					input.WheelScrollAmount += wheelDelta;
				}
			} else {
				if (!input.HasPendingKeyEvent(Key.MouseWheelDown)) {
					input.SetKeyState(Key.MouseWheelDown, true);
					input.SetKeyState(Key.MouseWheelDown, false);
					input.WheelScrollAmount = wheelDelta;
				} else {
					input.WheelScrollAmount += wheelDelta;
				}
			}
		}

		public override void KeyDown(NSEvent theEvent)
		{
			var key = (Key)MacKeyMap.GetKey((MacKeyCode)theEvent.KeyCode);
			input.SetKeyState(key, true);
			// There is no KeyUp event for regular key on Mac if Command key pressed, so we release it manualy in the same frame
			if ((theEvent.ModifierFlags & (NSEventModifierMask)(MacKeyModifiers.LWinFlag | MacKeyModifiers.RWinFlag)) != 0) {
				input.SetKeyState(key, false);
			}
			foreach(var c in theEvent.Characters) {
				const char backspaceCode = (char)127;
				bool isControl = Enum.IsDefined(typeof(NSFunctionKey), (ulong)c);
				if (!isControl && c != backspaceCode) {
					input.TextInput += c;
				} else if (c == backspaceCode) {
					// Imitation of original OpenTK backspace bug
					input.TextInput += '\b';
				}
			}
		}

		private readonly Dictionary<MacKeyModifiers, Key> modifiersToKeys = new Dictionary<MacKeyModifiers, Key> {
			{ MacKeyModifiers.LShiftFlag, Key.LShift },
			{ MacKeyModifiers.RShiftFlag, Key.RShift },
			{ MacKeyModifiers.LCtrlFlag, Key.LControl },
			{ MacKeyModifiers.RCtrlFlag, Key.RControl },
			{ MacKeyModifiers.LAltFlag, Key.LAlt },
			{ MacKeyModifiers.RAltFlag, Key.RAlt },
			{ MacKeyModifiers.LWinFlag, Key.LWin },
			{ MacKeyModifiers.RWinFlag, Key.RWin } 
		};

		public override void FlagsChanged(NSEvent theEvent)
		{
			foreach (var item in modifiersToKeys) {
				if (IsMaskHasFlag(theEvent.ModifierFlags, item.Key)) {
					input.SetKeyState(item.Value, true);
				} else if (IsMaskHasFlag(prevMask, item.Key)) {
					input.SetKeyState(item.Value, false);
				}
			}
			prevMask = theEvent.ModifierFlags;
		}

		private static bool IsMaskHasFlag(NSEventModifierMask mask, MacKeyModifiers flag)
		{
			return ((ulong)mask & (ulong)flag) == (ulong)flag;
		}

		public override void KeyUp(NSEvent theEvent)
		{
			var key = (Key)MacKeyMap.GetKey((MacKeyCode)theEvent.KeyCode);
			input.SetKeyState(key, false);
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

		private NSWindowStyle GetStyleMask(OpenTK.WindowBorder border)
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

		private void UpdateWindowBorder(OpenTK.WindowBorder border)
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

		public virtual OpenTK.WindowBorder WindowBorder
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