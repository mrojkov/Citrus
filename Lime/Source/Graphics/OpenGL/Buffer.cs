#if OPENGL
using System;
#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace Lime
{
	public class Buffer : IGLObject
	{
		private uint handle;
		private BufferTarget target;

		public bool Dynamic { get; private set; }
		public bool IsDisposed { get; private set; }

		internal Buffer(BufferTarget target, bool dynamic)
		{
			this.target = target;
			Dynamic = dynamic;
			GLObjectRegistry.Instance.Add(this);
		}

		~Buffer()
		{
			Discard();
		}

		internal uint GetHandle()
		{
			CreateIfRequired();
			return handle;
		}

		private void CreateIfRequired()
		{
			if (IsDisposed) {
				throw new ObjectDisposedException(GetType().Name);
			}
			if (handle == 0) {
				var handles = new uint[1];
				GL.GenBuffers(1, handles);
				PlatformRenderer.CheckErrors();
				handle = handles[0];
			}
		}

		public void SetData<T>(T[] data, int elementCount) where T : struct
		{
			CreateIfRequired();
			var elementSize = Toolbox.SizeOf<T>();
			var usageHint = Dynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;
			GL.BindBuffer(target, handle);
			PlatformRenderer.CheckErrors();
			GL.BufferData(target, new IntPtr(elementSize * elementCount), data, usageHint);
			PlatformRenderer.CheckErrors();
		}

		public void Dispose()
		{
			Discard();
			IsDisposed = true;
			GC.SuppressFinalize(this);
		}

		public void Discard()
		{
			if (handle != 0) {
				var handleCopy = handle;
				Window.Current.InvokeOnRendering(() => {
#if !MAC
					if (OpenTK.Graphics.GraphicsContext.CurrentContext == null)
						return;
#endif
					GL.DeleteBuffers(1, new uint[] { handleCopy });
					PlatformRenderer.CheckErrors();
				});
				handle = 0;
			}
		}
	}
}

#endif