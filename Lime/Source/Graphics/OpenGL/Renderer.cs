#if OPENGL
using System;
#if iOS
using OpenTK.Graphics.ES20;
#elif MAC
using MonoMac.OpenGL;
#elif WIN
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
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
		public static class Attributes
		{
			public const int Position = 0;
			public const int UV1 = 1;
			public const int UV2 = 2;
			public const int Color = 3;
		}

		[StructLayout(LayoutKind.Explicit, Size = 32)]
		public struct Vertex
		{
			[FieldOffset(0)]
			public Vector2 Pos;
			[FieldOffset(8)]
			public Vector2 UV1;
			[FieldOffset(16)]
			public Vector2 UV2;
			[FieldOffset(24)]
			public Color4 Color;
		}

		public static int RenderCycle { get; private set; }
		public static bool PremultipliedAlphaMode = true;
		public static int DefaultFramebuffer { get; private set; }
		
		const int MaxVertices = 1024;
		public static int DrawCalls = 0;

		private static readonly uint[] textures = new uint[2];

		private static ushort* batchIndices;
		private static Vertex* batchVertices;

		private static int currentVertex;
		private static int currentIndex;
		
		public static Matrix32 Transform1 = Matrix32.Identity;
		public static Matrix32 Transform2 = Matrix32.Identity;

		private static readonly Stack<Matrix44> projectionStack;
		private static WindowRect viewport;
		private static WindowRect scissorRectangle = new WindowRect();
		private static bool scissorTestEnabled = false;
		private static Blending blending = Blending.None;
		
		static Renderer()
		{
			projectionStack = new Stack<Matrix44>();
			projectionStack.Push(Matrix44.Identity);

			batchIndices = (ushort*)Marshal.AllocHGlobal(sizeof(ushort) * MaxVertices * 4);
			batchVertices = (Vertex*)Marshal.AllocHGlobal(sizeof(Vertex) * MaxVertices);
		}

		public static Matrix44 Projection
		{
			get { return projectionStack.Peek(); }
			set { SetProjection(value); }
		}

		public static void CheckErrors()
		{
#if DEBUG
			var errCode = GL.GetError();
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
		}
		
		public static void FlushSpriteBatch()
		{
			if (currentIndex > 0) {
				int numTextures = textures[1] != 0 ? 2 : (textures[0] != 0 ? 1 : 0);
				var currentProgram = ShaderPrograms.GetShaderProgram(blending, numTextures);
				currentProgram.Use();
				var matrix = projectionStack.Peek().ToFloatArray();
				GL.UniformMatrix4(currentProgram.ProjectionMatrixUniformId, 1, false, matrix);
				GL.DrawElements(PrimitiveType.Triangles, currentIndex, DrawElementsType.UnsignedShort, (IntPtr)batchIndices);
				CheckErrors();
				currentIndex = currentVertex = 0;
				DrawCalls++;
			}
		}

		public static int GetCurrentFramebuffer()
		{
			var currentFramebuffer = new int[1];
			GL.GetInteger(GetPName.FramebufferBinding, currentFramebuffer);
			return currentFramebuffer[0];
		}

		public static void BeginFrame()
		{
			DrawCalls = 0;
			RenderCycle++;
			DefaultFramebuffer = GetCurrentFramebuffer();
			SetDefaultViewport();
			Texture2D.DeleteScheduledTextures();
			ClearRenderTarget(0, 0, 0, 1);
			GL.Enable(EnableCap.Blend);
			blending = Blending.None;
			Blending = Blending.Default;
			SetupVertexAttribPointers();
			CheckErrors();
		}

		private static void SetupVertexAttribPointers()
		{
			GL.VertexAttribPointer(Attributes.Position, 2, VertexAttribPointerType.Float, false, sizeof(Vertex), (IntPtr)batchVertices);
			GL.EnableVertexAttribArray(Attributes.Position);
			GL.VertexAttribPointer(Attributes.UV1, 2, VertexAttribPointerType.Float, false, sizeof(Vertex), (IntPtr)batchVertices + 8);
			GL.EnableVertexAttribArray(Attributes.UV1);
			GL.VertexAttribPointer(Attributes.UV2, 2, VertexAttribPointerType.Float, false, sizeof(Vertex), (IntPtr)batchVertices + 16);
			GL.EnableVertexAttribArray(Attributes.UV2);
			GL.VertexAttribPointer(Attributes.Color, 4, VertexAttribPointerType.UnsignedByte, true, sizeof(Vertex), (IntPtr)batchVertices + 24);
			GL.EnableVertexAttribArray(Attributes.Color);
		}

		public static void ClearRenderTarget(float r, float g, float b, float a)
		{
			GL.ClearColor(r, g, b, a);
			GL.Clear(ClearBufferMask.ColorBufferBit);
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
			if (stage > 0) {
				GL.ActiveTexture(TextureUnit.Texture0 + stage);
				GL.BindTexture(TextureTarget.Texture2D, glTexNum);
				GL.ActiveTexture(TextureUnit.Texture0);
			} else {
				GL.BindTexture(TextureTarget.Texture2D, glTexNum);
			}
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
				Viewport = new WindowRect { 
					X = 0, Y = 0,
					Width = windowSize.Width, 
					Height = windowSize.Height 
				};
			}
		}

		public static WindowRect Viewport
		{
			get { return viewport; }
			set { SetViewport(value); }
		}
		
		private static void SetViewport(WindowRect value)
		{
			viewport = value;
			GL.Viewport(value.X, value.Y, value.Width, value.Height);
		}

		public static WindowRect ScissorRectangle
		{
			get { return scissorRectangle; }
			set { SetScissorRectangle(value); }
		}

		public static bool ScissorTestEnabled
		{
			get { return scissorTestEnabled; }
			set { SetScissorTestEnabled(value); }
		}

		public static void SetScissorRectangle(WindowRect value)
		{
			FlushSpriteBatch();
			scissorRectangle = value;
			GL.Scissor(value.X, value.Y, value.Width, value.Height);
		}

		public static void SetScissorTestEnabled(bool value)
		{
			FlushSpriteBatch();
			if (value) {
				GL.Enable(EnableCap.ScissorTest);
			} else {
				GL.Disable(EnableCap.ScissorTest);
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

		public static Blending Blending {
			set {
				if (value == blending)
					return;
				FlushSpriteBatch();
				ApplyBlending(value);
			}
		}

		private static void ApplyBlending(Blending value)
		{
			blending = value;
			switch (blending) {
				case Blending.Default:
				case Blending.Alpha:
					if (PremultipliedAlphaMode) {
						GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
					} else {
						GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
					}
					break;
				case Blending.Add:
				case Blending.Glow:
					GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
					break;
				case Blending.Silhuette:
					GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
					break;
				case Blending.Burn:
					GL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.OneMinusSrcAlpha);
					break;
				case Blending.Modulate:
					GL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.Zero);
					break;
			}
			CheckErrors();
		}

		private static void SetProjection(Matrix44 value)
		{
			projectionStack.Pop();
			projectionStack.Push(value);
		}
	}
}
#endif