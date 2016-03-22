using System;
using System.Collections.Generic;

namespace Lime
{
	public class RenderBatch
	{
		public const int VertexBufferCapacity = 400;
		public const int IndexBufferCapacity = 600;

		public Blending Blending;
		public ShaderId Shader;
		public ShaderProgram CustomShaderProgram;
		public ITexture Texture1;
		public ITexture Texture2;
		public int LastVertex;
		public int StartIndex;
		public int LastIndex;
		public GeometryBuffer Geometry;
		public bool OwnsMesh;

		public RenderBatch()
		{
			Clear();
		}

		public void Clear()
		{
			Texture1 = Texture2 = null;
			Blending = Lime.Blending.None;
			Shader = ShaderId.None;
			CustomShaderProgram = null;
			StartIndex = LastIndex = LastVertex = 0;
			if (Geometry != null) {
				if (OwnsMesh) {
					GeometryBufferPool.Release(Geometry);
				}
				Geometry = null;
			}
			OwnsMesh = false;
		}

		public void Render()
		{
			PlatformRenderer.SetTexture(Texture1, 0);
			PlatformRenderer.SetTexture(Texture2, 1);
			PlatformRenderer.SetBlending(Blending);
			PlatformRenderer.SetShader(Shader, CustomShaderProgram);
			Geometry.Render(StartIndex, LastIndex - StartIndex);
			Renderer.DrawCalls++;
		}
	}

	static class RenderBatchPool
	{
		private static Stack<RenderBatch> items = new Stack<RenderBatch>();

		public static RenderBatch Acquire()
		{
			if (items.Count == 0) {
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

	static class GeometryBufferPool
	{
		private static Stack<GeometryBuffer> items = new Stack<GeometryBuffer>();

		public static GeometryBuffer Acquire()
		{
			if (items.Count == 0) {
				var mesh = new GeometryBuffer();
				mesh.Allocate(RenderBatch.VertexBufferCapacity, RenderBatch.IndexBufferCapacity, GeometryBuffer.Attributes.VertexColorUV12);
				return mesh;
			}
			return items.Pop();
		}

		public static void Release(GeometryBuffer item)
		{
			items.Push(item);
		}
	}

}