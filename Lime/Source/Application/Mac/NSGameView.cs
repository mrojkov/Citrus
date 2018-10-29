#if MAC || MONOMAC
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
#if MAC
using AppKit;
using CoreGraphics;
using CoreVideo;
using Foundation;
using OpenGL;
#else
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.CoreVideo;
using MonoMac.Foundation;
using MonoMac.OpenGL;
#endif
using OpenTK;

namespace Lime.Platform
{
	public class NSGameView : NSView
	{
		private static NSOpenGLContext openGLContext;
		private NSOpenGLPixelFormat pixelFormat;
		private NSTimer animationTimer;
		private NSTrackingArea trackingArea;
		private bool swapInterval;
		private bool disposed;
		private NSEventModifierMask prevMask;
		private Lime.Input input;
		private bool running;
		private bool allowDropFiles;

		public event Action Update;
		public event Action RenderFrame;
		public event Action InputChanged;
		public event Action<IEnumerable<string>> FilesDropped;
		public event Action DidMouseEnter;
		public event Action DidMouseExit;

		private static NSGameView lastEntered;
		private static NSGameView firstExited;
		private static NSGameView capturedByMouseView;

		public bool AllowDropFiles
		{
			get { return allowDropFiles; }
			set
			{
				if (value && !allowDropFiles) {
					RegisterForDraggedTypes(new string[] { NSPasteboard.NSFilenamesType });
				}
				if (!value && allowDropFiles) {
					UnregisterDraggedTypes();
				}
				allowDropFiles = value;
			}
		}

		public NSGameView(Lime.Input input, CGRect frame, GraphicsMode mode)
			: base(frame)
		{
			this.input = input;
			this.Hidden = true;
			// Avoid of double scaling on high DPI displays
			this.WantsBestResolutionOpenGLSurface = true;
			pixelFormat = SelectPixelFormat(mode, 1, 0);
			if (pixelFormat == null) {
				throw new InvalidOperationException(string.Format("Failed to contruct NSOpenGLPixelFormat for GraphicsMode {0}", mode));
			}
			if (openGLContext == null) {
				openGLContext = new NSOpenGLContext(pixelFormat, null);
			}
			if (openGLContext == null) {
				throw new InvalidOperationException(string.Format("Failed to construct NSOpenGLContext {0}", mode));
			}
			openGLContext.MakeCurrentContext();
			swapInterval = true;
		}

		public override void MouseEntered(NSEvent theEvent)
		{
			if (capturedByMouseView == null) {
				DidMouseEnter?.Invoke();
			} else {
				lastEntered = this;
			}
		}

		public override void MouseExited(NSEvent theEvent)
		{
			if (capturedByMouseView == null) {
				DidMouseExit?.Invoke();
			} else if (firstExited == null) {
				firstExited = this;
			}
		}

		public override bool AcceptsFirstMouse(NSEvent theEvent)
		{
			return true;
		}

		public override void UpdateTrackingAreas()
		{
			if (trackingArea != null) {
				RemoveTrackingArea(trackingArea);
				trackingArea.Dispose();
			}
			var options = NSTrackingAreaOptions.ActiveInActiveApp | NSTrackingAreaOptions.MouseEnteredAndExited;
			trackingArea = new NSTrackingArea(Bounds, options, this, null);
			AddTrackingArea(trackingArea);
		}

		public override void MouseDown(NSEvent theEvent)
		{
			input.SetKeyState(Key.Mouse0, true);
			input.SetKeyState(Key.Touch0, true);
			capturedByMouseView = this;
			if (theEvent.ClickCount == 2) {
				input.SetKeyState(Key.Mouse0DoubleClick, true);
			}
		}

		public override void RightMouseDown(NSEvent theEvent)
		{
			input.SetKeyState(Key.Mouse1, true);
			if (theEvent.ClickCount == 2) {
				input.SetKeyState(Key.Mouse1DoubleClick, true);
			}
		}

		public override void OtherMouseDown(NSEvent theEvent)
		{
			input.SetKeyState(Key.Mouse2, true);
		}

		public override void RightMouseUp(NSEvent theEvent)
		{
			input.SetKeyState(Key.Mouse1, false);
			if (theEvent.ClickCount == 2) {
				input.SetKeyState(Key.Mouse1DoubleClick, false);
			}
		}

		public override void MouseUp(NSEvent theEvent)
		{
			input.SetKeyState(Key.Mouse0, false);
			input.SetKeyState(Key.Touch0, false);
			capturedByMouseView = null;
			firstExited?.DidMouseExit?.Invoke();
			lastEntered?.DidMouseEnter?.Invoke();
			firstExited = null;
			lastEntered = null;
			if (theEvent.ClickCount == 2) {
				input.SetKeyState(Key.Mouse0DoubleClick, false);
			}
		}

		public override void OtherMouseUp(NSEvent theEvent)
		{
			input.SetKeyState(Key.Mouse2, false);
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
			if (!theEvent.IsARepeat) {
				input.SetKeyState(key, true);
				if ((input.GetModifiers() & Modifiers.Win) != 0) {
					// There is no KeyUp event if the Command key is pressed down, so unpress the key immediately.
					input.SetKeyState(key, false);
				}
			}
			if ((input.GetModifiers() & (Modifiers.Control | Modifiers.Alt | Modifiers.Win)) != 0) {
				return;
			}
			foreach (var c in theEvent.Characters) {
				// Imitation of original OpenTK backspace bug.
				if (c == (char)127) {
					input.TextInput += '\b';
					continue;
				}
				if (Enum.IsDefined(typeof(NSFunctionKey), (ulong)c)) {
					continue;
				}
				input.TextInput += c;
			}
		}

		private readonly static Dictionary<MacKeyModifiers, Key> modifiersToKeys = new Dictionary<MacKeyModifiers, Key> {
			{ MacKeyModifiers.LShiftFlag, Key.Shift },
			{ MacKeyModifiers.RShiftFlag, Key.Shift },
			{ MacKeyModifiers.LCtrlFlag, Key.Control },
			{ MacKeyModifiers.RCtrlFlag, Key.Control },
			{ MacKeyModifiers.LAltFlag, Key.Alt },
			{ MacKeyModifiers.RAltFlag, Key.Alt },
			{ MacKeyModifiers.LWinFlag, Key.Win },
			{ MacKeyModifiers.RWinFlag, Key.Win },
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
			openGLContext.View = this;
			openGLContext.Update();
		}
	
		public void Stop()
		{
			if (running) {
				animationTimer.Invalidate();
				animationTimer = null;
			}
			running = false;
		}

		public void Run(double updatesPerSecond, bool modal)
		{
			AssertNonDisposed();
			openGLContext.SwapInterval = swapInterval;
			StartAnimation(updatesPerSecond, modal);
		}

		public override bool AcceptsFirstResponder()
		{
			// We want this view to be able to receive key events
			return true;
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

		private void StartAnimation(double updatesPerSecond, bool modal)
		{
			if (!running) {
				var timeout = new TimeSpan((long)(((1.0 * TimeSpan.TicksPerSecond) / updatesPerSecond) + 0.5));
				animationTimer = NSTimer.CreateRepeatingScheduledTimer(timeout, delegate {
					OnUpdate();
					if (!running) {
						// The window could be closed on update
						return;
					}
					OnRender();
				});
				NSRunLoop.Current.AddTimer(animationTimer, modal ? NSRunLoopMode.ModalPanel : NSRunLoopMode.Default);
				NSRunLoop.Current.AddTimer(animationTimer, NSRunLoopMode.EventTracking);
			}
			running = true;
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

		private void OnUpdate()
		{
			if (Update != null) {
				Update();
			}
		}

		public void MakeCurrent()
		{
			openGLContext.View = this;
		}

		private void OnRender()
		{
			if (RenderFrame != null) {
				RenderFrame();
			}
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
				// DestroyFrameBuffer();
			}
			base.Dispose(disposing);
			disposed = true;
		}

		private void RaiseInputChanged()
		{
			if (InputChanged != null) {
				InputChanged();
			}
		}

		public override bool PerformDragOperation(NSDraggingInfo sender)
		{
#if !MONOMAC
			if (base.PrepareForDragOperation (sender)) {
				var nsFiles = ((NSArray)sender.DraggingPasteboard.GetPropertyListForType(NSPasteboard.NSFilenamesType));
				var files = new List<string>();
				for (uint i = 0; i < nsFiles.Count; i++) {
					files.Add((nsFiles.GetItem<NSString>(i).ToString()));
				}
				FilesDropped?.Invoke(files);
				return true;
			}
#endif
			return false;
		}

		public override NSDragOperation DraggingEntered(NSDraggingInfo sender) => NSDragOperation.Generic;
	}
}
#endif