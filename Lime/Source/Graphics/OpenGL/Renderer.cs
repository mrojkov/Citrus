#if OPENGL || GLES11
using System;
#if iOS
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
#elif MAC
using MonoMac.OpenGL;
using OGL = MonoMac.OpenGL.GL;
#elif WIN
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OGL = OpenTK.Graphics.OpenGL.GL;
#endif
using ProtoBuf;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Lime
{
	public struct WindowRect
	{
		public int X;
		public int Y;
		public int Width;
		public int Height;
	}

	public unsafe static partial class Renderer
	{
		[StructLayout(LayoutKind.Explicit, Size = 32)]
		public struct Vertex
		{
			[FieldOffset(0)]
			public Vector2 Pos;
			[FieldOffset(8)]
			public Color4 Color;
			[FieldOffset(12)]
			public Vector2 UV1;
			[FieldOffset(20)]
			public Vector2 UV2;
		}

		public static int RenderCycle = 1;

		public static bool PremulAlphaMode = true;
		
		const int MaxVertices = 1024;
		public static int DrawCalls = 0;

		static readonly uint[] textures = new uint[2];

		static ushort* batchIndices;
		static Vertex* batchVertices;
		
		static int currentVertex;
		static int currentIndex;
		
		public static Matrix32 Transform1 = Matrix32.Identity;
		public static Matrix32 Transform2 = Matrix32.Identity;

		static readonly Stack<Matrix44> projectionStack;

		static Renderer()
		{
			projectionStack = new Stack<Matrix44>();
			projectionStack.Push(Matrix44.Identity);
		}

		public static Matrix44 Projection
		{
			get { return projectionStack.Peek(); }
			set { SetProjection(value); }
		}

		public static void CheckErrors()
		{
#if DEBUG
#if GLES11
			All errCode = GL.GetError();
			if (errCode == All.NoError)
				return;
			string errors = "";
			while (errCode != All.NoError) {
				if (errors != "")
					errors += ", ";
				errors += errCode.ToString();// GL.GetString(errCode);
				errCode = GL.GetError();
			}
			throw new Exception("OpenGL error(s): " + errors);
#elif OPENGL
			ErrorCode errCode = GL.GetError();
			if (errCode == ErrorCode.NoError)
				return;
			string errors = "";
			while (errCode != ErrorCode.NoError) {
				if (errors != "")
					errors += ", ";
				errors += errCode.ToString();
				errCode = GL.GetError();
			}
			throw new Exception("OpenGL error(s): " + errors);
#endif
#endif
		}
		
		public static void FlushSpriteBatch()
		{
			if (currentIndex > 0) {
#if GLES11
				GL.DrawElements(All.Triangles, currentIndex, All.UnsignedShort, (IntPtr)batchIndices);
#elif OPENGL
				OGL.DrawElements(BeginMode.Triangles, currentIndex, DrawElementsType.UnsignedShort, (IntPtr)batchIndices);
#endif
				CheckErrors();
				currentIndex = currentVertex = 0;
				DrawCalls++;
			}
		}
		
		public static void BeginFrame()
		{
			SetDefaultViewport();
			if (batchIndices == null) {
				batchIndices = (ushort*)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(ushort) * MaxVertices * 4);
				batchVertices = (Vertex*)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(Vertex) * MaxVertices);
			}
			Texture2D.DeleteScheduledTextures();
			DrawCalls = 0;
			RenderCycle++;
			ClearRenderTarget(0, 0, 0, 1);
#if GLES11
			GL.Enable(All.Texture2D);

			// Set up vertex and color arrays
			GL.VertexPointer(2, All.Float, 32, (IntPtr)batchVertices);
			GL.EnableClientState(All.VertexArray);
			GL.ColorPointer(4, All.UnsignedByte, 32, (IntPtr)((uint)batchVertices + 8));
			GL.EnableClientState(All.ColorArray);

			// Set up texture coordinate arrays
			GL.ClientActiveTexture(All.Texture1);
			GL.EnableClientState(All.TextureCoordArray);
			GL.TexCoordPointer(2, All.Float, 32, (IntPtr)((uint)batchVertices + 20));
			GL.ClientActiveTexture(All.Texture0);
			GL.EnableClientState(All.TextureCoordArray);
			GL.TexCoordPointer(2, All.Float, 32, (IntPtr)((uint)batchVertices + 12));
			
#elif OPENGL
			OGL.Enable(EnableCap.Texture2D);

			// Set up vertex and color arrays
			OGL.VertexPointer(2, VertexPointerType.Float, 32, (IntPtr)batchVertices);
			OGL.EnableClientState(ArrayCap.VertexArray);
			OGL.ColorPointer(4, ColorPointerType.UnsignedByte, 32, (IntPtr)((uint)batchVertices + 8));
			OGL.EnableClientState(ArrayCap.ColorArray);
			// Set up texture coordinate arrays
			OGL.ClientActiveTexture(TextureUnit.Texture1);
			OGL.EnableClientState(ArrayCap.TextureCoordArray);
			OGL.TexCoordPointer(2, TexCoordPointerType.Float, 32, (IntPtr)((uint)batchVertices + 20));
			OGL.ClientActiveTexture(TextureUnit.Texture0);
			OGL.EnableClientState(ArrayCap.TextureCoordArray);
			OGL.TexCoordPointer(2, TexCoordPointerType.Float, 32, (IntPtr)((uint)batchVertices + 12));
#endif
			blending = Blending.None;
			Blending = Blending.Default;

			CheckErrors();
		}

		public static void ClearRenderTarget(float r, float g, float b, float a)
		{
#if GLES11
			GL.ClearColor(r, g, b, a);
			GL.Clear((uint)All.ColorBufferBit);
#elif OPENGL
			OGL.ClearColor(r, g, b, a);
			OGL.Clear(ClearBufferMask.ColorBufferBit);
#endif
		}

		public static void SetTexture(ITexture texture, int stage)
		{
			uint handle = texture != null ? texture.GetHandle() : 0;
			SetTexture(handle, stage);
		}

		internal static void SetTexture(uint glTexNum, int stage)
		{
			if (glTexNum == textures[stage])
				return;
			FlushSpriteBatch();
#if GLES11
			if (stage > 0) {
				GL.ActiveTexture(All.Texture0 + stage);
				if (glTexNum > 0) {
					GL.Enable(All.Texture2D);
					GL.BindTexture(All.Texture2D, glTexNum);
				} else {
					GL.Disable(All.Texture2D);
				}
				GL.ActiveTexture(All.Texture0);
			} else {
				GL.BindTexture(All.Texture2D, glTexNum);
			}
#elif OPENGL
			if (stage > 0) {
				OGL.ActiveTexture(TextureUnit.Texture0 + stage);
				if (glTexNum > 0) {
					OGL.Enable(EnableCap.Texture2D);
					OGL.BindTexture(TextureTarget.Texture2D, glTexNum);
				} else {
					OGL.Disable(EnableCap.Texture2D);
				}
				OGL.ActiveTexture(TextureUnit.Texture0);
			} else {
				OGL.BindTexture(TextureTarget.Texture2D, glTexNum);
			}
#endif
			textures[stage] = glTexNum;
			CheckErrors();
		}
		
		public static void EndFrame()
		{
			FlushSpriteBatch();
			SetTexture(null, 0);
			SetTexture(null, 1);
		}

		public static void SetOrthogonalProjection(Vector2 leftTop, Vector2 rightBottom)
		{
			SetOrthogonalProjection(leftTop.X, leftTop.Y, rightBottom.X, rightBottom.Y);
		}

		public static void SetOrthogonalProjection(float left, float top, float right, float bottom)
		{
			Projection = Matrix44.CreateOrthographicOffCenter(left, right, bottom, top, 0, 1);
		}

		public static void SetDefaultViewport()
		{
			if (Application.Instance != null) {
				var windowSize = Application.Instance.WindowSize;
#if iOS
				if (GameView.Instance.IsRetinaDisplay) {
					windowSize.Width *= 2;
					windowSize.Height *= 2;
				}
#endif
				Viewport = new WindowRect { 
					X = 0, Y = 0, 
					Width = windowSize.Width, 
					Height = windowSize.Height 
				};
			}
		}

		static WindowRect viewport;
		public static WindowRect Viewport
		{
			get { return viewport; }
			set { SetViewport(value); }
		}

		private static void SetViewport(WindowRect value)
		{
			viewport = value;
#if GLES11
			GL.Viewport(value.X, value.Y, value.Width, value.Height);
#elif OPENGL
			OGL.Viewport(value.X, value.Y, value.Width, value.Height);
#endif
		}

		static WindowRect scissorRectangle = new WindowRect();
		public static WindowRect ScissorRectangle
		{
			get { return scissorRectangle; }
			set { SetScissorRectangle(value); }
		}

		static bool scissorTestEnabled = false;
		public static bool ScissorTestEnabled
		{
			get { return scissorTestEnabled; }
			set { SetScissorTestEnabled(value); }
		}

		public static void SetScissorRectangle(WindowRect value)
		{
			FlushSpriteBatch();
#if GLES11
			GL.Scissor(value.X, value.Y, value.Width, value.Height);
#else
			OGL.Scissor(value.X, value.Y, value.Width, value.Height);
#endif
		}

		public static void SetScissorTestEnabled(bool value)
		{
			FlushSpriteBatch();
			if (value) {
#if GLES11
				GL.Enable(All.ScissorTest);
#else
				OGL.Enable(EnableCap.ScissorTest);
#endif
			} else {
#if GLES11
				GL.Disable(All.ScissorTest);
#else
				OGL.Disable(EnableCap.ScissorTest);
#endif
			}
		}

		public static void PushProjectionMatrix()
		{
			projectionStack.Push(projectionStack.Peek());
		}

		public static void PopProjectionMatrix()
		{
			projectionStack.Pop();
			Projection = projectionStack.Peek();
		}

		static Blending blending = Blending.None;
		public static Blending Blending {
			set {
				if (value == blending)
					return;
				FlushSpriteBatch();
				blending = value;
#if GLES11
				GL.Enable(All.Blend);
				GL.TexEnv(All.TextureEnv, All.Src0Rgb, (int)All.Previous);
				GL.TexEnv(All.TextureEnv, All.Src0Alpha, (int)All.Previous);
				GL.TexEnv(All.TextureEnv, All.Src1Rgb, (int)All.Texture);
				GL.TexEnv(All.TextureEnv, All.Src1Alpha, (int)All.Texture);
				switch(value) {
				case Blending.Silhuette:
					GL.BlendFunc(All.SrcAlpha, All.OneMinusSrcAlpha);
					GL.TexEnv(All.TextureEnv, All.CombineRgb, (int)All.Replace);
					GL.TexEnv(All.TextureEnv, All.CombineAlpha, (int)All.Modulate);
					GL.TexEnv(All.TextureEnv, All.TextureEnvMode, (int)All.Combine);
					break;
				case Blending.Burn:
					GL.BlendFunc(All.DstColor, All.OneMinusSrcAlpha);
					break;
				case Blending.Add:
				case Blending.Glow:
					GL.BlendFunc(All.SrcAlpha, All.One);
					GL.TexEnv(All.TextureEnv, All.TextureEnvMode, (int)All.Modulate);
					break;
				case Blending.Alpha:
				case Blending.Default:
					if (PremulAlphaMode)
						GL.BlendFunc(All.One, All.OneMinusSrcAlpha);
					else
						GL.BlendFunc(All.SrcAlpha, All.OneMinusSrcAlpha);
					GL.TexEnv(All.TextureEnv, All.TextureEnvMode, (int)All.Modulate);
					break;
				case Blending.Modulate:
					GL.BlendFunc(All.DstColor, All.Zero);
					GL.TexEnv(All.TextureEnv, All.TextureEnvMode, (int)All.Modulate);
					break;
				}
#elif OPENGL
				OGL.Enable(EnableCap.Blend);
				OGL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Source0Rgb, (int)TextureEnvModeSource.Previous);
				OGL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src0Alpha, (int)TextureEnvModeSource.Previous);
				OGL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src1Rgb, (int)TextureEnvModeSource.Texture);
				OGL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src1Alpha, (int)TextureEnvModeSource.Texture);
				switch(value) {
				case Blending.Alpha:
				case Blending.Default:
				default:
					if (PremulAlphaMode) {
						OGL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
					} else {
						OGL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
					}
					OGL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
					break;
				case Blending.Silhuette:
					OGL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
					OGL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.CombineRgb, (int)TextureEnvModeCombine.Replace);
					OGL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.CombineAlpha, (int)TextureEnvModeCombine.Modulate);
					OGL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Combine);
					break;
				case Blending.Burn:
					OGL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.OneMinusSrcAlpha);
					break;
				case Blending.Add:
				case Blending.Glow:
					if (PremulAlphaMode) {
						OGL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
					} else {
						OGL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
					}
					OGL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
					break;
				case Blending.Modulate:
					OGL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.Zero);
					OGL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
					break;
				}
#endif
				CheckErrors();
			}
		}

		private static void SetProjection(Matrix44 value)
		{
			projectionStack.Pop();
			projectionStack.Push(value);
#if GLES11
			GL.MatrixMode(All.Projection);
			GL.LoadMatrix(value.ToFloatArray());
			GL.MatrixMode(All.Modelview);
#else
			OGL.MatrixMode(MatrixMode.Projection);
			OGL.LoadMatrix(value.ToFloatArray());
			OGL.MatrixMode(MatrixMode.Modelview);
#endif

		}

#if X
		// Highly optimized version for OpenGL
		public static void DrawSpriteHelper(ITexture texture, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1)
		{
			var transform = Transform1 * Transform2;
			SetTexture(texture, 0);
			SetTexture(null, 1);
			if (currentVertex >= MaxVertices - 4 || currentIndex >= MaxVertices * 4 - 6) {
				FlushSpriteBatch();
			}
			if (PremulAlphaMode) {
				color = Color4.PremulAlpha(color);
			}
			Rectangle textureRect = texture.UVRect;
			uv0 = textureRect.A + textureRect.Size * uv0;
			uv1 = textureRect.A + textureRect.Size * uv1;
			int i = currentVertex;
			currentVertex += 4;
			Vertex* vp = &batchVertices[i];
			vp->Pos = transform.TransformVector(position.X, position.Y);
			vp->Color = color;
			vp->UV1 = uv0;
			vp++;
			vp->Pos = transform.TransformVector(position.X + size.X, position.Y);
			vp->Color = color;
			vp->UV1.X = uv1.X;
			vp->UV1.Y = uv0.Y;
			vp++;
			vp->Pos = transform.TransformVector(position.X, position.Y + size.Y);
			vp->Color = color;
			vp->UV1.X = uv0.X;
			vp->UV1.Y = uv1.Y;
			vp++;
			vp->Pos = transform.TransformVector(position.X + size.X, position.Y + size.Y);
			vp->Color = color;
			vp->UV1 = uv1;
			int j = currentIndex;
			currentIndex += 6;
			ushort* ip = &batchIndices[j];
			*ip++ = (ushort)(i + 0);
			*ip++ = (ushort)(i + 1);
			*ip++ = (ushort)(i + 2);
			*ip++ = (ushort)(i + 2);
			*ip++ = (ushort)(i + 1);
			*ip++ = (ushort)(i + 3);
		}
#endif
	}
}
#endif