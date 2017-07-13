#if OPENGL
using System;
#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif
using Yuzu;

namespace Lime
{
	internal interface IGLIndexBuffer
	{
		void BufferData();
	}

	public class IndexBuffer : IIndexBuffer, IGLIndexBuffer, IGLObject
	{
		private uint iboHandle;
		private bool disposed;

		public ushort[] Data { get; set; }
		public bool Dynamic { get; set; }
		public bool Dirty { get; set; }

		public IndexBuffer()
		{
			Dirty = true;
			GLObjectRegistry.Instance.Add(this);
		}

		public IndexBuffer(bool dynamic) : this()
		{
			Dynamic = dynamic;
		}

		~IndexBuffer()
		{
			Dispose();
		}

		private void AllocateHandle()
		{
			var t = new int[1];
			GL.GenBuffers(1, t);
			iboHandle = (uint)t[0];
		}

		void IGLIndexBuffer.BufferData()
		{
			if (iboHandle == 0) {
				AllocateHandle();
			}
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, iboHandle);
			if (Dirty) {
				Dirty = false;
				var usageHint = Dynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;
				GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(ushort) * Data.Length), Data, usageHint);
			}
		}

		public void Dispose()
		{
			if (!disposed) {
				Discard();
				disposed = true;
			}
			GC.SuppressFinalize(this);
		}

		public void Discard()
		{
			if (iboHandle != 0) {
				var capturedIboHandle = iboHandle;
				Application.InvokeOnMainThread(() => {
#if !MAC
					if (OpenTK.Graphics.GraphicsContext.CurrentContext == null)
						return;
#endif
					GL.DeleteBuffers(1, new uint[] { capturedIboHandle });
				});
				iboHandle = 0;
			}
		}
	}
}
#endif