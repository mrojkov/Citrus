#if OPENGL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if iOS || ANDROID
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif
using System.Runtime.InteropServices;

namespace Lime
{
	public unsafe class IndexBuffer : IDisposable, IGLObject
	{
		public const int DefaultCapacity = 400;
		public static int TotalIndexBuffers;
		private uint iboHandle;
		private bool disposed;

		public IndexBuffer()
		{
			TotalIndexBuffers++;
			GLObjectRegistry.Instance.Add(this);
		}

		~IndexBuffer()
		{
			Dispose();
		}

		private void AllocateIBOHandle()
		{
			var t = new int[1];
			GL.GenBuffers(1, t);
			iboHandle = (uint)t[0];
		}

		public void Bind(ushort[] indices, bool forceUpload)
		{
			if (iboHandle == 0) {
				forceUpload = true;
				AllocateIBOHandle();
			}
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, iboHandle);
			if (forceUpload) {
				GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(short) * indices.Length), indices, BufferUsageHint.DynamicDraw);
			}
		}

		public void Dispose()
		{
			if (!disposed) {
				TotalIndexBuffers--;
				Discard();
				disposed = true;
			}
			GC.SuppressFinalize(this);
		}

		public void Discard()
		{
			if (iboHandle != 0) {
				Application.InvokeOnMainThread(() => {
					if (OpenTK.Graphics.GraphicsContext.CurrentContext != null) {
						GL.DeleteBuffers(1, new uint[] { iboHandle });
					}
				});
				iboHandle = 0;
			}
		}
	}
}
#endif