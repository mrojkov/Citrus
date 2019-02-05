using System;

#if !iOS && !MAC && !ANDROID
using OpenTK.Graphics.ES20;
#endif

namespace Lime.Graphics.Platform.OpenGL
{
	internal class PlatformBuffer : IPlatformBuffer
	{
		internal int GLBuffer;
		internal BufferTarget GLTarget;
		internal BufferUsageHint GLUsage;

		public PlatformRenderContext Context { get; }
		public BufferType BufferType { get; }
		public int Size { get; }
		public bool Dynamic { get; }
		public bool Disposed { get; private set; }

		internal PlatformBuffer(PlatformRenderContext context, BufferType bufferType, int size, bool dynamic)
		{
			Context = context;
			BufferType = bufferType;
			Size = size;
			Dynamic = dynamic;
			Initialize();
		}

		public void Dispose()
		{
			if (GLBuffer != 0) {
				GL.DeleteBuffer(GLBuffer);
				GLHelper.CheckGLErrors();
				GLBuffer = 0;
			}
			Disposed = true;
		}

		private void Initialize()
		{
			GLTarget = GetGLBufferTarget(BufferType);
			GLUsage = Dynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;
			GLBuffer = GL.GenBuffer();
			GLHelper.CheckGLErrors();
			GL.BindBuffer(GLTarget, GLBuffer);
			GLHelper.CheckGLErrors();
			GL.BufferData(GLTarget, Size, IntPtr.Zero, GLUsage);
			GLHelper.CheckGLErrors();
			Context.InvalidateBufferBinding(BufferType);
		}

		public void SetData(int offset, IntPtr data, int size, BufferSetDataMode mode)
		{
			GL.BindBuffer(GLTarget, GLBuffer);
			GLHelper.CheckGLErrors();
			if (mode == BufferSetDataMode.Discard) {
				GL.BufferData(GLTarget, Size, IntPtr.Zero, GLUsage);
				GLHelper.CheckGLErrors();
			}
			GL.BufferSubData(GLTarget, new IntPtr(offset), size, data);
			GLHelper.CheckGLErrors();
			Context.InvalidateBufferBinding(BufferType);
		}

		private static BufferTarget GetGLBufferTarget(BufferType bufferType)
		{
			switch (bufferType) {
				case BufferType.Vertex:
					return BufferTarget.ArrayBuffer;
				case BufferType.Index:
					return BufferTarget.ElementArrayBuffer;
				default:
					throw new ArgumentException(nameof(bufferType));
			}
		}
	}
}