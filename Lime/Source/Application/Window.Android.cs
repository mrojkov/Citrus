#if ANDROID
using System;
using System.Threading;
using System.Collections.Generic;

using Android.Content.Res;
using Android.Runtime;
using Android.Views;
using AndroidApp = Android.App.Application;
using AndroidContext = Android.Content.Context;

#pragma warning disable 0067

namespace Lime
{
	public class Window : CommonWindow, IWindow
	{
		private Thread renderThread;
		private ManualResetEvent renderReady = new ManualResetEvent(false);
		private ManualResetEvent renderCompleted = new ManualResetEvent(true);
		private bool requestForRedraw;

		private static readonly IWindowManager WindowManager =
			AndroidApp.Context.GetSystemService(AndroidContext.WindowService).JavaCast<IWindowManager>();

		// This line only suppresses warning: "Window.Current: a name can be simplified".
		public new static IWindow Current => CommonWindow.Current;

		private readonly Display display = new Display(WindowManager.DefaultDisplay);
		private readonly FPSCounter fpsCounter;

		public bool Active { get; private set; }
		public bool Fullscreen { get { return true; } set {} }
		public string Title { get; set; }
		public bool Visible { get { return true; } set {} }
		public WindowInput Input { get; private set; }
		public bool AsyncRendering { get; private set; }
		public MouseCursor Cursor { get; set; }
		public WindowState State { get { return WindowState.Fullscreen; } set {} }
		public bool FixedSize { get { return true; } set {} }
		public Vector2 ClientPosition { get { return Vector2.Zero; } set {} }
		public Vector2 DecoratedPosition { get { return Vector2.Zero; } set {} }
		public Vector2 ClientSize
		{
			get { return ToLimeSize(ActivityDelegate.Instance.GameView.Size, PixelScale); }
			set { }
		}
		public Vector2 DecoratedSize { get { return ClientSize; } set {} }
		public Vector2 MinimumDecoratedSize { get { return Vector2.Zero; } set {} }
		public Vector2 MaximumDecoratedSize { get { return Vector2.Zero; } set {} }
		public ActivityDelegate ActivityDelegate { get { return ActivityDelegate; } }
		public float UnclampedDelta { get; private set; }
		public float FPS { get { return fpsCounter.FPS; } }

		[Obsolete("Use FPS property instead", true)]
		public float CalcFPS() { return fpsCounter.FPS; }

		public bool AllowDropFiles { get { return false; } set {} }

		public event Action<IEnumerable<string>> FilesDropped;

		public void DragFiles(string[] filenames)
		{
			throw new NotImplementedException();
		}

		public float PixelScale
		{
			get; private set;
		}

		public Vector2 LocalToDesktop(Vector2 localPosition)
		{
			return localPosition * PixelScale;
		}

		public Vector2 DesktopToLocal(Vector2 desktopPosition)
		{
			return desktopPosition / PixelScale;
		}

		public Window(WindowOptions options)
		{
			if (Application.MainWindow != null) {
				throw new Lime.Exception("Attempt to set Application.MainWindow twice");
			}
			Application.MainWindow = this;
			Input = new WindowInput(this);
			Active = true;
			AsyncRendering = options.AsyncRendering;
			fpsCounter = new FPSCounter();
			ActivityDelegate.Instance.Paused += activity => {
				WaitForRendering();
				Active = false;
				RaiseDeactivated();
			};
			ActivityDelegate.Instance.Resumed += activity => {
				Active = true;
				RaiseActivated();
			};
			ActivityDelegate.Instance.GameView.Resize += (sender, e) => {
				RaiseResized(((ResizeEventArgs)e).DeviceRotated);
			};
			ActivityDelegate.Instance.GameView.SurfaceCreating += () => requestForRedraw = true;
			ActivityDelegate.Instance.GameView.SurfaceDestroing += WaitForRender;
			PixelScale = Resources.System.DisplayMetrics.Density;

			if (AsyncRendering) {
				renderThread = new Thread(RenderLoop);
				renderThread.IsBackground = true;
				renderThread.Start();
			}

			Application.WindowUnderMouse = this;

			var ccb = new ChoreographerCallback();
			ccb.OnFrame += OnFrame;
			Choreographer.Instance.PostFrameCallback(ccb);
		}
		
		UpdateState updateState;
		
		void OnFrame(long frameTimeNanos)
		{
			var _continue = true;
			while (_continue) {
				switch (updateState) {
					case UpdateState.Update:
						_continue = HandleUpdateState_Update(frameTimeNanos);
						break;
					case UpdateState.WaitForRender:
						_continue = HandleUpdateState_WaitForRender();
						break;
					case UpdateState.RenderAsync:
						_continue = HandleUpdateState_RenderAsync();
						break;
					case UpdateState.RenderSync:
						_continue = HandleUpdateState_RenderSync();
						break;
				}
			}
		}
		
		long prevFrameTime = Java.Lang.JavaSystem.NanoTime();
		
		bool HandleUpdateState_Update(long frameTimeNanos)
		{
			var delta = (float)((frameTimeNanos - prevFrameTime) / 1000000000d);
			var gw = ActivityDelegate.Instance.GameView;
			prevFrameTime = frameTimeNanos;
			if ((Active && gw.IsSurfaceCreated) || requestForRedraw) {
				fpsCounter.Refresh();
				gw.ProcessTextInput();
				Update(delta);
				updateState = AsyncRendering ? UpdateState.WaitForRender : UpdateState.RenderSync;
				return true;
			} else {
				return false;
			}
		}
		
		bool HandleUpdateState_WaitForRender()
		{
			if (renderCompleted.WaitOne(100)) {
				if ((Active && ActivityDelegate.Instance.GameView.IsSurfaceCreated) || requestForRedraw) {
					renderCompleted.Reset();
					updateState = UpdateState.RenderAsync;
					return true;
				} else {
					updateState = UpdateState.Update;
					return false;
				}
			}
			return false;
		}
		
		bool HandleUpdateState_RenderAsync()
		{
			RaiseSync();
			ActivityDelegate.Instance.GameView.UnbindContext();
			renderReady.Set();
			requestForRedraw = false;
			updateState = UpdateState.Update;
			return false;
		}
		
		bool HandleUpdateState_RenderSync()
		{
			RaiseSync();
			Render();
			requestForRedraw = false;
			updateState = UpdateState.Update;
			return false;
		}
		
		enum UpdateState
		{
			Update,
			WaitForRender,
			RenderAsync,
			RenderSync
		}

		class ChoreographerCallback : Java.Lang.Object, Choreographer.IFrameCallback
		{
			public event Action<long> OnFrame;

			public void DoFrame(long frameTimeNanos)
			{
				Choreographer.Instance.PostFrameCallback(this);
				OnFrame?.Invoke(frameTimeNanos);
			}
		}

		public void WaitForRendering()
		{
			if (AsyncRendering) {
				renderCompleted.WaitOne();
			}
		}

		private void RenderLoop()
		{
			while (true) {
				renderReady.WaitOne();
				renderReady.Reset();
				Render();
				renderCompleted.Set();
			}
		}

		private void Update(float delta)
		{
			UnclampedDelta = delta;
			delta = Math.Min(UnclampedDelta, Application.MaxDelta);
			Input.ProcessPendingKeyEvents(delta);
			base.RaiseUpdating(delta);
			AudioSystem.Update();
			Input.CopyKeysState();
		}

		private void Render()
		{
			var gw = ActivityDelegate.Instance.GameView;
			gw.MakeCurrent();
			base.RaiseRendering();
			gw.SwapBuffers();
			gw.UnbindContext();
		}

		public void Center() {}
		public void Close() {}
		public void Invalidate() {}
		public void ShowModal() {}
		public void Activate() {}

		/// <summary>
		/// Gets the default display device.
		/// </summary>
		public IDisplay Display
		{
			get { return display; }
		}

		private static Vector2 ToLimeSize(System.Drawing.Size size, float pixelScale)
		{
			return new Vector2(size.Width, size.Height) / pixelScale;
		}
	}
}
#endif
