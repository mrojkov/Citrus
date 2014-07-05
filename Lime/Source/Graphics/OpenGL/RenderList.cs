using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
#if iOS
using OpenTK.Graphics.ES20;
#elif MAC
using MonoMac.OpenGL;
using PrimitiveType = MonoMac.OpenGL.BeginMode;
#elif WIN
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
#endif

namespace Lime
{
	/// <summary>
	/// This class contains an index array related to a single draw call.
	/// </summary>
	public unsafe class RenderBatch : IDisposable
	{
		public const int Capacity = 500;
		public Blending Blending;
		public ShaderId Shader;
		public ITexture Texture1;
		public ITexture Texture2;
		public int IndexCount;
		public VertexBuffer VertexBuffer;
		public ushort* Indices;
		private bool disposed;

		public RenderBatch()
		{
			Indices = (ushort*)Marshal.AllocHGlobal(sizeof(ushort) * Capacity);
		}

		~RenderBatch()
		{
			Dispose();
		}

		public void Clear()
		{
			Texture1 = Texture2 = null;
			IndexCount = 0;
			Blending = Lime.Blending.None;
			Shader = ShaderId.None;
			VertexBuffer = null;
		}

		public void Render()
		{
			PlatformRenderer.SetTexture(Texture1, 0);
			PlatformRenderer.SetTexture(Texture2, 1);
			PlatformRenderer.SetShader(Shader);
			GL.DrawElements(PrimitiveType.Triangles, IndexCount, DrawElementsType.UnsignedShort, (IntPtr)Indices);
			Renderer.DrawCalls++;
		}

		public void Dispose()
		{
			if (!disposed) {
				disposed = true;
				Marshal.FreeHGlobal((IntPtr)Indices);
				Indices = null;
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

	public unsafe class RenderList
	{
		public readonly List<RenderBatch> Batches = new List<RenderBatch>();
		public readonly List<VertexBuffer> Buffers = new List<VertexBuffer>();
		private VertexBuffer lastBuffer;
		private RenderBatch lastBatch;

		public bool IsEmpty { get { return lastBuffer == null; } }
		
		public RenderBatch RequestForBatch(ITexture texture1, ITexture texture2, Blending blending, ShaderId shader, int numVertices, int numIndices)
		{
			if (lastBuffer == null || lastBuffer.VertexCount + numVertices > VertexBuffer.Capacity) {
				lastBuffer = VertexBufferPool.Acquire();
				Buffers.Add(lastBuffer);
			} else if ((GetTextureHandle(lastBatch.Texture1) == GetTextureHandle(texture1)) &&
				(GetTextureHandle(lastBatch.Texture2) == GetTextureHandle(texture2)) &&
				lastBatch.IndexCount + numIndices <= RenderBatch.Capacity &&
				lastBatch.Blending == blending &&
				lastBatch.Shader == shader) 
			{
				return lastBatch;
			}
			lastBatch = RenderBatchPool.Acquire();
			lastBatch.VertexBuffer = lastBuffer;
			lastBatch.Texture1 = texture1;
			lastBatch.Texture2 = texture2;
			lastBatch.Blending = blending;
			lastBatch.Shader = shader;
			Batches.Add(lastBatch);
			return lastBatch;
		}

		private uint GetTextureHandle(ITexture texture)
		{
			return texture == null ? 0 : texture.GetHandle();
		}

		public void Render()
		{
			VertexBuffer buffer = null;
			foreach (var batch in Batches) {
				if (buffer != batch.VertexBuffer) {
					buffer = batch.VertexBuffer;
					buffer.Bind();
				}
				batch.Render();
			}
			PlatformRenderer.CheckErrors();
		}

		public void Clear()
		{
			if (lastBuffer == null) {
				return;
			}
			foreach (var i in Batches) {
				RenderBatchPool.Release(i);
			}
			Batches.Clear();
			foreach (var i in Buffers) {
				VertexBufferPool.Release(i);
			}
			Buffers.Clear();
			lastBuffer = null;
			lastBatch = null;
		}

		public void Flush()
		{
			if (lastBuffer != null) {
				Render();
				Clear();
			}
		}
	}
}
