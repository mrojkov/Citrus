using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if iOS
using OpenTK.Graphics.ES20;
#elif MAC
using MonoMac.OpenGL;
using PrimitiveType = MonoMac.OpenGL.BeginMode;
#elif WIN
using OpenTK.Graphics.OpenGL;
#endif

namespace Lime
{
	/// <summary>
	/// This class contains an index array related to a single draw call.
	/// </summary>
	public class RenderBatch : IDisposable
	{
		public Blending Blending;
		public ShaderId Shader;
		public ITexture Texture1;
		public ITexture Texture2;
		public VertexBuffer VertexBuffer;
		public IndexBuffer IndexBuffer;
		private bool disposed;

		private static IndexBuffer spriteOnlyIndexBuffer = CreateSpriteOnlyIndexBuffer();

		private unsafe static Lime.IndexBuffer CreateSpriteOnlyIndexBuffer()
		{
			var ib = new IndexBuffer((VertexBuffer.DefaultCapacity >> 2) * 6);
			ib.IndexCount = ib.Capacity;
			int v = 0;
			int c = ib.Capacity / 6;
			Int32* ip = (Int32*)ib.Indices;
			for (int i = 0; i < c; i++) {
				*ip++ = (v << 16) | (v + 1);
				*ip++ = ((v + 2) << 16) | (v + 2);
				*ip++ = ((v + 1) << 16) | (v + 3);
				v += 4;
			}
			return ib;
		}

		~RenderBatch()
		{
			Dispose();
		}

		public void Clear()
		{
			Texture1 = Texture2 = null;
			Blending = Lime.Blending.None;
			Shader = ShaderId.None;
			VertexBuffer = null;
			if (IndexBuffer != null) {
				IndexBufferPool.Release(IndexBuffer);
			}
		}

		public unsafe void Render()
		{
			PlatformRenderer.SetTexture(Texture1, 0);
			PlatformRenderer.SetTexture(Texture2, 1);
			PlatformRenderer.SetShader(Shader);
			PlatformRenderer.SetBlending(Blending);
			int offset = 0;
			if (VertexBuffer.SpritesOnly) {
				spriteOnlyIndexBuffer.Bind();
				offset = ((IndexBuffer.Indices[0] - 1) >> 2) * 6 * sizeof(ushort);
			} else {
				IndexBuffer.Bind();
			}
			GL.DrawElements(PrimitiveType.Triangles, IndexBuffer.IndexCount, DrawElementsType.UnsignedShort, (IntPtr)offset);
			Renderer.DrawCalls++;
		}

		public void Dispose()
		{
			if (!disposed) {
				disposed = true;
				Clear();
			}
			GC.SuppressFinalize(this);
		}
	}

	static class RenderBatchPool
	{
		private static Stack<RenderBatch> items = new Stack<RenderBatch>();

		public static RenderBatch Acquire()
		{
			int i = items.Count;
			if (i == 0) {
				return new RenderBatch();
			}
			return items.Pop();
		}

		public static void Release(RenderBatch item)
		{
			item.Clear();
			items.Push(item);
		}
	}
}