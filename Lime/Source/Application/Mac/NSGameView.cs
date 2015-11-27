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
		private bool swapInterval;
		private bool disposed;
		private bool animating;
		private bool displayLinkSupported;
		private NSEventModifierMask prevMask;
		private Lime.Input input;

		public event Action RenderFrame;

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

			swapInterval = true;
			displayLinkSupported = true;
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

		private void RefreshMousePosition(NSEvent theEvent)
		{
			var point = new Point((int)theEvent.LocationInWindow.X, (int)Frame.Height - (int)theEvent.LocationInWindow.Y);
			var p = new Vector2(point.X, point.Y) * (float)Window.BackingScaleFactor;
			input.MousePosition = p * input.ScreenToWorldTransform;
		}

		public override void ScrollWheel(NSEvent theEvent)
		{
			const int SizeOfLineInPixels = 40;
			var wheelDelta = theEvent.HasPreciseScrollingDeltas ? (float)theEvent.ScrollingDeltaY
				: (float)theEvent.ScrollingDeltaY * SizeOfLineInPixels;
			input.SetWheelScrollAmount(wheelDelta);
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

		private readonly static Dictionary<MacKeyModifiers, Key> modifiersToKeys = new Dictionary<MacKeyModifiers, Key> {
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

		public void UpdateGLContext()
		{
			openGLContext.CGLContext.Lock();
			openGLContext.Update();
			openGLContext.CGLContext.Unlock();					
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

		public void Run(double updatesPerSecond)
		{
			AssertNonDisposed();
			displayLinkSupported = false;
			openGLContext.SwapInterval = swapInterval;
			if (displayLinkSupported)
				SetupDisplayLink();
			StartAnimation(updatesPerSecond);
		}

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

		private void StartAnimation(double updatesPerSecond)
		{
			if (!animating) {
				if (displayLinkSupported) {
					if (displayLink != null && !displayLink.IsRunning)
						displayLink.Start();
				} else {
					var timeout = new TimeSpan((long)(((1.0 * TimeSpan.TicksPerSecond) / updatesPerSecond) + 0.5));
					if (swapInterval) {
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

		private void AssertNonDisposed()
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
			if (RenderFrame != null) {
				RenderFrame();
			}
			openGLContext.CGLContext.Unlock();
		}

		private void SetupDisplayLink()
		{
			if (displayLink != null)
				return;

			displayLink = new CVDisplayLink();
			displayLink.SetOutputCallback(DisplayLinkCallback);

			CGLContext cglContext = openGLContext.CGLContext;
			CGLPixelFormat cglPixelFormat = pixelFormat.CGLPixelFormat;
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

		protected override void Dispose(bool disposing)
		{
			if (disposed)
				return;
			if (disposing) {
				Stop();
				displayLink = null;
				// DestroyFrameBuffer();
			}
			base.Dispose(disposing);
			disposed = true;
		}
	}
}
#endif