#if ANDROID
using System;
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
		public Input Input { get { return ActivityDelegate.Instance.Input; } }
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

		private Vector2 lastDesktopMousePosition = new Vector2(-1, -1);
		private Vector2 calculatedMousePosition = new Vector2(-1, -1);

		public Vector2 MousePosition
		{
			get {
				if (lastDesktopMousePosition != Input.DesktopMousePosition) {
					lastDesktopMousePosition = Input.DesktopMousePosition;
					calculatedMousePosition = lastDesktopMousePosition * MousePositionTransform / PixelScale;
				}
				return calculatedMousePosition;
			}
		}

		public Vector2 LocalToDesktop(Vector2 localPosition)
		{
			return localPosition * MousePositionTransform.CalcInversed() * PixelScale;
		}

		private Matrix32 mousePositionTransform = Matrix32.Identity;
		public Matrix32 MousePositionTransform
		{
			get { return mousePositionTransform; }
			set {
				mousePositionTransform = value;
				calculatedTouchPositions = new Vector2?[Input.MaxTouches];
				lastDesktopMousePosition = new Vector2(-1, -1);
			}
		}

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
				UnclampedDelta = (float)e.Time;
				var delta = Math.Min(UnclampedDelta, Application.MaxDelta);
				RaiseUpdating(delta);
			};

			PixelScale = Resources.System.DisplayMetrics.Density;
		}

		private Vector2?[] calculatedTouchPositions = new Vector2?[Input.MaxTouches];
		public Vector2 GetTouchPosition(int index)
		{
			if (calculatedTouchPositions[index] == null) {
				calculatedTouchPositions[index] = Input.GetDesktopTouchPosition(index) * MousePositionTransform / PixelScale;
			}
			return calculatedTouchPositions[index].Value;
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
