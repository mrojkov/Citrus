using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if iOS || ANDROID
using OpenTK.Graphics.ES20;
#elif MAC
using MonoMac.OpenGL;
using PrimitiveType = MonoMac.OpenGL.BeginMode;
#elif WIN
using OpenTK.Graphics.OpenGL;
#endif

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
		public Mesh Mesh;
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
			if (Mesh != null) {
				if (OwnsMesh) {
					MeshesForBatchingPool.Release(Mesh);
				}
				Mesh = null;
			}
			OwnsMesh = false;
		}

		public void Render()
		{
#if UNITY
			PlatformRenderer.SetMaterial(Texture1, Texture2, Shader, Blending);
#else
			PlatformRenderer.SetTexture(Texture1, 0);
			PlatformRenderer.SetTexture(Texture2, 1);
			PlatformRenderer.SetShader(Shader, CustomShaderProgram);
			PlatformRenderer.SetBlending(Blending);
#endif
			Mesh.Render(StartIndex, LastIndex - StartIndex);
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

	static class MeshesForBatchingPool
	{
		private static Stack<Mesh> items = new Stack<Mesh>();

		public static Mesh Acquire()
		{
			if (items.Count == 0) {
				var mesh = new Mesh();
				mesh.Allocate(RenderBatch.VertexBufferCapacity, RenderBatch.IndexBufferCapacity, Mesh.Attributes.VertexColorUV12);
				return mesh;
			}
			return items.Pop();
		}

		public static void Release(Mesh item)
		{
			items.Push(item);
		}
	}

}