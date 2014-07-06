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
}