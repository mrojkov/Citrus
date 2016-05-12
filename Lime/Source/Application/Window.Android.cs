#if ANDROID
using System;

using Android.Content.Res;

#pragma warning disable 0067

namespace Lime
{
	public class Window : CommonWindow, IWindow
	{
		private FPSCounter fpsCounter;

		public bool Active { get; private set; }
		public bool Fullscreen { get { return true; } set {} }
		public string Title { get; set; }
		public bool Visible { get { return true; } set {} }
		public Input Input { get { return ActivityDelegate.Instance.Input; } }
		public MouseCursor Cursor { get; set; }
		public WindowState State { get { return WindowState.Fullscreen; } set {} }
		public IntVector2 ClientPosition { get { return IntVector2.Zero; } set {} }
		public IntVector2 DecoratedPosition { get { return IntVector2.Zero; } set {} }
		public Size ClientSize
		{
			get { return ToLimeSize(ActivityDelegate.Instance.GameView.Size, PixelScale); } 
			set { }
		}
		public Size DecoratedSize { get { return ClientSize; } set {} }
		public Size MinimumDecoratedSize { get { return Size.Zero; } set {} }
		public Size MaximumDecoratedSize { get { return Size.Zero; } set {} }
		public ActivityDelegate ActivityDelegate { get { return ActivityDelegate; } }
		public float FPS { get { return fpsCounter.FPS; } }

		[Obsolete("Use FPS property instead", true)]
		public float CalcFPS() { return fpsCounter.FPS; }

		public float PixelScale
		{
			get; private set;
		}

		public void Center() {}
		public void Close() {}
		public void Invalidate() {}

		public Window(WindowOptions options)
		{
			if (Application.MainWindow != null) {
				throw new Lime.Exception("Attempt to set Application.MainWindow twice");
			}
			Application.MainWindow = this;
			Active = true;
			fpsCounter = new FPSCounter();
			ActivityDelegate.Instance.Paused += activity => {
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
			ActivityDelegate.Instance.GameView.RenderFrame += (sender, e) => {
				RaiseRendering();
				fpsCounter.Refresh();
			};
			ActivityDelegate.Instance.GameView.UpdateFrame += (sender, e) => {
				RaiseUpdating((float)e.Time);
			};

			PixelScale = Resources.System.DisplayMetrics.Density;
		}

		private static Size ToLimeSize(System.Drawing.Size size, float pixelScale)
		{
			return (Size)(new Vector2(size.Width, size.Height) / pixelScale);
		}
	}
}
#endif