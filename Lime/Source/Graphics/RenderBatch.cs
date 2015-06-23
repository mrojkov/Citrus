using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if iOS || ANDROID
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace Lime
{
	public class RenderBatch
	{
		public const int VertexBufferCapacity = 400;
		public const int IndexBufferCapacity = 600;

		public Blending Blending;
		public Material Material;
		public ITexture Texture1;
		public ITexture Texture2;
		public int VertexCount;
		public int IndexCount;
		public Mesh Mesh;
		public bool OwnsMesh;

		public bool IsEmpty { get { return IndexCount == 0; } }

		public RenderBatch()
		{
			Mesh = new Mesh();
			Mesh.Allocate(VertexBufferCapacity, IndexBufferCapacity, Lime.Mesh.Attributes.VertexColorUV12);
		}

		public void Prepare(ITexture texture1, ITexture texture2, Blending blending, Material material, int vertexCount, int indexCount)
		{
			if (!IsEmpty) {
				if ((GetTextureHandle(Texture1) != GetTextureHandle(texture1)) ||
					(GetTextureHandle(Texture2) != GetTextureHandle(texture2)) ||
					Blending != blending ||
					Material != material ||
					VertexCount + vertexCount > Mesh.Vertices.Length ||
					IndexCount + indexCount > Mesh.Indices.Length)
				{
					Render();
					Clear();
				}
			}
			Texture1 = texture1;
			Texture2 = texture2;
			Blending = blending;
			Material = material;
		}

		private static uint GetTextureHandle(ITexture texture)
		{
			return texture == null ? 0 : texture.GetHandle();
		}

		public void Clear()
		{
			IndexCount = VertexCount = 0;
		}

		public void Render()
		{
			if (IsEmpty) {
				return;
			}
#if UNITY
			PlatformRenderer.SetMaterial(Texture1, Texture2, Shader, Blending);
#else
			Renderer.MaterialRenderer.SetMesh(Mesh, 0, IndexCount);
			Material.Texture1 = Texture1;
			Material.Texture2 = Texture2;
			Material.Blending = Blending;
			Material.Render(Renderer.MaterialRenderer);
#endif
			Renderer.DrawCalls++;
		}
	}
}