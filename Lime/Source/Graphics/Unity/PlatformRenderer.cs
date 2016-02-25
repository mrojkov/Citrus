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

		static int currentVertex = 0;
		static int currentIndex = 0;

		public static Matrix32 Transform1 = Matrix32.Identity;
		public static Matrix32 Transform2 = Matrix32.Identity;

		private static UnityEngine.Matrix4x4 projection;
		private static WindowRect viewport;
		private static WindowRect scissorRect;
		private static bool scissorTest;
		private static bool viewportOrProjectionChanged;

		internal static Blending Blending;
		internal static ShaderId Shader;
		internal static ShaderProgram CustomShader;
		internal static ITexture[] Textures = new ITexture[2];

		public static void SetTexture(ITexture texture, int stage)
		{
			Textures[stage] = texture;
		}

		public static void SetShader(ShaderId shader, ShaderProgram customShader = null)
		{
			Shader = shader;
			CustomShader = customShader;
		}

		public static void SetBlending(Blending blending)
		{
			Blending = blending;
		}

		public static void BeginFrame()
		{
			DrawCalls = 0;
			RenderCycle++;
		}
		
		public static void ClearRenderTarget(float r, float g, float b, float a)
		{
			UnityEngine.GL.Clear(false, true, new UnityEngine.Color(r, g, b, a));
		}

		public static void SetViewport(WindowRect value)
		{
			viewport = value;
			viewportOrProjectionChanged = true;
		}

		public static void SetProjectionMatrix(Matrix44 matrix)
		{
			projection = (UnityEngine.Matrix4x4)matrix;
			viewportOrProjectionChanged = true;
			UnityEngine.GL.LoadProjectionMatrix(projection);
		}

		public static void SetScissorRectangle(WindowRect value)
		{
			scissorRect = value;
			viewportOrProjectionChanged = true;
		}

		public static void EnableScissorTest(bool value)
		{
			scissorTest = value;
			viewportOrProjectionChanged = true;
		}

		public static void EnableZTest(bool value)
		{
			
		}

		public static void EnableZWrite(bool value)
		{
			
		}

		public static void SetViewportAndProject()
		{
			if (!viewportOrProjectionChanged) {
				return;
			}
			viewportOrProjectionChanged = false;
			if (scissorTest) {
				// TODO: handle non-fullscreen viewport
				UnityEngine.GL.Viewport(new UnityEngine.Rect(scissorRect.X, scissorRect.Y, scissorRect.Width, scissorRect.Height));
				var w = (float)viewport.Width;
				var h = (float)viewport.Height;
				var r = new Rectangle {
					Left = scissorRect.X / w,
					Top = scissorRect.Y / h,
					Right = (scissorRect.X + scissorRect.Width) / w,
					Bottom = (scissorRect.Y + scissorRect.Height) / h,
				};
				var m1 = UnityEngine.Matrix4x4.TRS(new UnityEngine.Vector3(r.Left, r.Top, 0), 
					UnityEngine.Quaternion.identity, new UnityEngine.Vector3(r.Width, r.Height, 1));
				var m2 = UnityEngine.Matrix4x4.TRS(new UnityEngine.Vector3((1 / r.Width - 1), (1 / r.Height - 1), 0), 
					UnityEngine.Quaternion.identity, new UnityEngine.Vector3(1 / r.Width, 1 / r.Height, 1));
				var m3 = UnityEngine.Matrix4x4.TRS(new UnityEngine.Vector3(-r.Left * 2 / r.Width, -r.Top * 2 / r.Height, 0), 
					UnityEngine.Quaternion.identity, UnityEngine.Vector3.one);
				UnityEngine.GL.LoadProjectionMatrix(m3 * m2 * projection);
			} else {
				UnityEngine.GL.Viewport(new UnityEngine.Rect(viewport.X, viewport.Y, viewport.Width, viewport.Height));
				UnityEngine.GL.LoadProjectionMatrix(projection);
			}
		}
	}
}
#endif