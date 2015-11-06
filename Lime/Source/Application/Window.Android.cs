#if ANDROID
using System;

#pragma warning disable 0067

namespace Lime
{
	public class Window : IWindow
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
		public Size ClientSize { get { return ToLimeSize(ActivityDelegate.Instance.GameView.Size); } set {} }
		public Size DecoratedSize { get { return ClientSize; } set {} }
		public ActivityDelegate ActivityDelegate { get { return ActivityDelegate; } }

		public event Action Activated;
		public event Action Deactivated;
		public event Func<bool> Closing;
		public event Action Closed;
		public event Action Moved;
		public event Action Resized;
		public event Action<float> Updating;
		public event Action Rendering;

		public float CalcFPS() { return fpsCounter.FPS; }
		public void Center() {}
		public void Close() {}

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
				if (Deactivated != null) {
					Deactivated();
				}
			};
			ActivityDelegate.Instance.Resumed += activity => {
				Active = true;
				if (Activated != null) {
					Activated();
				}
			};
			ActivityDelegate.Instance.GameView.Resize += (sender, e) => {
				if (Resized != null) {
					Resized();
				}
			};
			ActivityDelegate.Instance.GameView.RenderFrame += (sender, e) => {
				if (Rendering != null) {
					Rendering();
				}
				fpsCounter.Refresh();
			};
			ActivityDelegate.Instance.GameView.UpdateFrame += (sender, e) => {
				if (Updating != null) {
					Updating((float)e.Time);
				}
			};
		}

		private static Size ToLimeSize(System.Drawing.Size size)
		{
			return new Size(size.Width, size.Height);
		}
	}
}
#endif