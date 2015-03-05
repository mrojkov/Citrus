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
			var m = MaterialFactory.CreateMaterial(blending, shader, new ITexture[] { texture1, texture2 });
			m.SetPass(0);
		}

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
		public static void BeginFrame()
		{
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

		public static void SetScissorRectangle(WindowRect value)
		{
		}
		
		public static void EnableScissorTest(bool value)
		{
		}
	}
}
#endif