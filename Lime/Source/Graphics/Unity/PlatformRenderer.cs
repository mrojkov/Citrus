#if UNITY
using System;
using ProtoBuf;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Lime
{
	// TODO: remove it
	public class ShaderProgram
	{
	}

	public static class PlatformRenderer
	{
		public static int RenderCycle = 1;

		public static bool PremulAlphaMode = true;
		
		public static int DrawCalls = 0;

//		static UnityEngine.Mesh mesh;
//		static ITexture[] textures; 
//		static Blending blending;

		static int currentVertex = 0;
		static int currentIndex = 0;

		public static Matrix32 Transform1 = Matrix32.Identity;
		public static Matrix32 Transform2 = Matrix32.Identity;

		static PlatformRenderer()
		{
//			batchVertices = new Vertex[MaxVertices];
//			batchIndices = new int[MaxVertices * 3]; 
//			blending = Blending.None;
//			mesh = new UnityEngine.Mesh();
		}

		public static void SetMaterial(ITexture texture1, ITexture texture2, ShaderId shader, Blending blending)
		{
			var m = MaterialFactory.CreateMaterial(blending, new ITexture[] { texture1, texture2 });
			m.SetPass(0);
		}
		// PlatformRenderer.SetMaterial(Texture1, Texture2, Shader, Blending);

//		public static void FlushSpriteBatch()
//		{
//			if (currentIndex > 0) {
//				var mat = MaterialFactory.CreateMaterial(blending, textures);
//				if (!mat.SetPass(0)) {
//					throw new Lime.Exception("Material.SetPass(0) failed");
//				}
//				UpdateMesh();
//				UnityEngine.Graphics.DrawMeshNow(mesh, UnityEngine.Matrix4x4.identity); 
//				currentIndex = currentVertex = 0;
//				DrawCalls++;
//			}
//		}
//
//		private static void UpdateMesh()
//		{
//			mesh.Clear(true);
//			mesh.MarkDynamic();
//			var colors32 = new UnityEngine.Color32[currentVertex];
//			var vertices = new UnityEngine.Vector3[currentVertex];
//			var uv1 = new UnityEngine.Vector2[currentVertex];
//			var uv2 = new UnityEngine.Vector2[currentVertex];
//			for (int i = 0; i < currentVertex; i++) {
//				Vertex v = batchVertices[i];
//				colors32[i].a = v.Color.A;
//				colors32[i].r = v.Color.R;
//				colors32[i].g = v.Color.G;
//				colors32[i].b = v.Color.B;
//				vertices[i].x = v.Pos.X;
//				vertices[i].y = v.Pos.Y;
//				uv1[i].x = v.UV1.X;
//				uv1[i].y = 1 - v.UV1.Y;
//				uv2[i].x = v.UV2.X;
//				uv2[i].y = 1 - v.UV2.Y;
//			}
//			mesh.vertices = vertices;
//			mesh.colors32 = colors32;
//			mesh.uv = uv1;
//			mesh.uv1 = uv2;
//			int[] indices = new int[currentIndex];
//			Array.Copy(batchIndices, indices, currentIndex);
//			mesh.triangles = indices;
//		}
		
		public static void BeginFrame()
		{
			// Texture2D.DeleteScheduledTextures();
			DrawCalls = 0;
			RenderCycle++;
		}
		
		public static void ClearRenderTarget(float r, float g, float b, float a)
		{
			UnityEngine.GL.Clear(false, true, new UnityEngine.Color(r, g, b, a));
		}

//		public static void SetTexture(ITexture texture, int stage)
//		{
//			if (textures[stage] != texture) {
//				FlushSpriteBatch();
//				textures[stage] = texture;
//			}
//		}
		
		public static void EndFrame()
		{
//			FlushSpriteBatch();
//			SetTexture(null, 0);
//			SetTexture(null, 1);
		}

		public static void SetOrthogonalProjection(Vector2 leftTop, Vector2 rightBottom)
		{
			SetOrthogonalProjection(leftTop.X, leftTop.Y, rightBottom.X, rightBottom.Y);
		}

		public static void SetOrthogonalProjection(float left, float top, float right, float bottom)
		{
			var mat = UnityEngine.Matrix4x4.Ortho(left, right, bottom, top, -50, 50);
			UnityEngine.GL.LoadProjectionMatrix(mat);
		}

		public static void SetViewport(WindowRect value)
		{
			UnityEngine.GL.Viewport(new UnityEngine.Rect(value.X, value.Y, value.Width, value.Height)); 
		}

		public static void SetProjectionMatrix(Matrix44 matrix)
		{
			var m = (UnityEngine.Matrix4x4)matrix;
			UnityEngine.GL.LoadProjectionMatrix(m);
		}


//		public static void PushProjectionMatrix()
//		{
//			UnityEngine.GL.PushMatrix();
//		}
//
//		public static void PopProjectionMatrix()
//		{
//			UnityEngine.GL.PopMatrix();
//		}

		public static void ResetShader()
		{
		}

		public static void SetScissorRectangle(WindowRect value)
		{
		}
		
		public static void EnableScissorTest(bool value)
		{
		}
	}
}
#endif