#if WIN || MAC || MONOMAC
using System;
using System.Collections.Generic;

namespace Lime
{
	public class DummyWindow : CommonWindow, IWindow
	{
		public bool Active { get; set; }
		public string Title { get; set; }
		public WindowState State { get; set; }
		public bool FixedSize { get; set; }
		public bool Fullscreen { get; set; }
		public Vector2 ClientPosition { get; set; }
		public Vector2 ClientSize { get; set; }
		public Vector2 DecoratedPosition { get; set; }
		public Vector2 DecoratedSize { get; set; }
		public Vector2 MinimumDecoratedSize { get; set; }
		public Vector2 MaximumDecoratedSize { get; set; }
		public bool Visible { get; set; }
		public WindowInput Input { get; set; } = new WindowInput(null);
		public bool AsyncRendering { get; } = false;
		public float FPS { get; set; }
		public IDisplay Display { get; }
		public float CalcFPS()
		{
			throw new NotImplementedException();
		}
		public void Center()
		{
			throw new NotImplementedException();
		}
		public MouseCursor Cursor { get; set; }
		public void Close()
		{
			throw new NotImplementedException();
		}
		public void Invalidate() {}
		public void Activate() {}
#if MAC
		public Platform.NSGameView NSGameView { get; set; }
#elif WIN
		public System.Windows.Forms.Form Form { get; set; }
#endif
		public float PixelScale { get; set; }

		public void ShowModal() {}

		public bool AllowDropFiles { get; set; }

#pragma warning disable CS0067
		public event Action<IEnumerable<string>> FilesDropped;
#pragma warning restore CS0067
		public void DragFiles(string[] filenames)
		{
			throw new NotImplementedException();
		}

		public void WaitForRendering()
		{
			throw new NotImplementedException();
		}

		public Vector2 LocalToDesktop(Vector2 localPosition)
		{
			throw new NotImplementedException();
		}

		public Vector2 DesktopToLocal(Vector2 desktopPosition)
		{
			throw new NotImplementedException();
		}

		public float UnclampedDelta
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}
}
#endif
