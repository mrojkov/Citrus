#if OPENGL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES20;
#elif MAC
using OpenTK.Graphics.OpenGL;
#elif MONOMAC
using MonoMac.OpenGL;
#endif
using System.Runtime.InteropServices;

namespace Lime
{
	public class VertexBuffer : IDisposable, IGLObject
	{
		public readonly VertexAttribPointerType AttribType;
		public readonly int ComponentCount;
		public static int TotalVertexBuffers;
		private uint vboHandle;
		private readonly int stride;
		private int componentCount;
		private bool normalized;
		private bool disposed;
	
		public VertexBuffer(VertexAttribPointerType attribType, int componentCount, bool normalized = false)
		{
			AttribType = attribType;
			stride = GetAttributeSize(attribType) * componentCount;
			ComponentCount = componentCount;
			this.componentCount = componentCount;
			this.normalized = normalized;
			TotalVertexBuffers++;
			GLObjectRegistry.Instance.Add(this);
		}

		private static int GetAttributeSize(VertexAttribPointerType type)
		{
			switch (type) {
				case VertexAttribPointerType.Float:
					return 4;
				case VertexAttribPointerType.UnsignedByte:
					return 1;
				default:
					throw new ArgumentException();
			}
		}

		~VertexBuffer()
		{
			Dispose();
		}

		private void AllocateVBOHandle()
		{
			var t = new int[1];
			GL.GenBuffers(1, t);
			vboHandle = (uint)t[0];
		}

		public void Bind<T>(T[] vertices, int attribute, bool forceUpload) where T : struct
		{
			if (vboHandle == 0) {
				forceUpload = true;
				AllocateVBOHandle();
			}
			GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
			GL.EnableVertexAttribArray(attribute);
			GL.VertexAttribPointer(attribute, componentCount, AttribType, normalized, 0, (IntPtr)0);
			if (forceUpload) {
				GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(stride * vertices.Length), vertices, BufferUsageHint.DynamicDraw);
			}
			PlatformRenderer.CheckErrors();
		}

		public void Dispose()
		{
			if (!disposed) {
				TotalVertexBuffers--;
				Discard();
				disposed = true;
			}
			GC.SuppressFinalize(this);
		}

		public void Discard()
		{
			if (vboHandle != 0) {
				var capturedVboHandle = vboHandle;
				Application.InvokeOnMainThread(() => {
#if !MAC && !MONOMAC
					if (OpenTK.Graphics.GraphicsContext.CurrentContext == null)
						return;
#endif
					GL.DeleteBuffers(1, new uint[] { capturedVboHandle });
				});
				vboHandle = 0;
			}
		}
	}
}
#endif