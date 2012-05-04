using System;
#if iOS
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
#elif MAC
using MonoMac.OpenGL;
#else
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
#endif
using ProtoBuf;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Lime
{
	[ProtoContract]
	public enum Blending
	{
		[ProtoEnum]
		None,
		[ProtoEnum]
		Default,
		[ProtoEnum]
		Alpha,
		[ProtoEnum]
		Add,
		[ProtoEnum]
		Silhuette,
	}

	[ProtoContract]
	public enum HAlignment
	{
		[ProtoEnum]
		Left,
		[ProtoEnum]
		Center,
		[ProtoEnum]
		Right,
	}

	[ProtoContract]
	public enum VAlignment
	{
		[ProtoEnum]
		Top,
		[ProtoEnum]
		Center,
		[ProtoEnum]
		Bottom,
	}

	public struct Viewport
	{
		public int X;
		public int Y;
		public int Width;
		public int Height;
	}

	public unsafe static class Renderer
	{
		public static int RenderCycle = 1;

		public static bool PremulAlphaMode = true;
		
		const int MaxVertices = 1024;
		public static int DrawCalls = 0;

		[StructLayout(LayoutKind.Explicit, Size=32)]
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

		static uint[] textures = new uint[2];
		static ushort* batchIndices;
		static Vertex* batchVertices;

		static int currentVertex = 0;
		static int currentIndex = 0;
		
		public static Matrix32 Transform1 = Matrix32.Identity;
		public static Matrix32 Transform2 = Matrix32.Identity;

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
				errors += GL.GetString(errCode);
				errCode = GL.GetError();
			}
#else
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
#endif
			throw new Exception("OpenGL error(s): " + errors);
#endif
		}
		
		public static void FlushSpriteBatch()
		{
			if (currentIndex > 0) {
#if GLES11
				GL.DrawElements(All.Triangles, currentIndex, All.UnsignedShort, (IntPtr)batchIndices);
#else
				GL.DrawElements(BeginMode.Triangles, currentIndex, DrawElementsType.UnsignedShort, (IntPtr)batchIndices);
#endif
				CheckErrors();
				currentIndex = currentVertex = 0;
				DrawCalls++;
			}
		}
		
		public static void BeginFrame()
		{
			if (batchIndices == null) {
				batchIndices = (ushort*)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(ushort) * MaxVertices * 4);
				batchVertices = (Vertex*)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(Vertex) * MaxVertices);
			}
			PlainTexture.DeleteScheduledTextures();
			DrawCalls = 0;
			RenderCycle++;
#if GLES11
			GL.ClearColor(0, 0, 0, 1);
			GL.Clear((uint)All.ColorBufferBit);
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
			
#else
			GL.ClearColor(0, 0, 0, 1);
			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.Enable(EnableCap.Texture2D);

			// Set up vertex and color arrays
			GL.VertexPointer(2, VertexPointerType.Float, 32, (IntPtr)batchVertices);
			GL.EnableClientState(ArrayCap.VertexArray);
			GL.ColorPointer(4, ColorPointerType.UnsignedByte, 32, (IntPtr)((uint)batchVertices + 8));
			GL.EnableClientState(ArrayCap.ColorArray);
			// Set up texture coordinate arrays
			GL.ClientActiveTexture(TextureUnit.Texture1);
			GL.EnableClientState(ArrayCap.TextureCoordArray);
			GL.TexCoordPointer(2, TexCoordPointerType.Float, 32, (IntPtr)((uint)batchVertices + 20));
			GL.ClientActiveTexture(TextureUnit.Texture0);
			GL.EnableClientState(ArrayCap.TextureCoordArray);
			GL.TexCoordPointer(2, TexCoordPointerType.Float, 32, (IntPtr)((uint)batchVertices + 12));
#endif
			Blending = Blending.Default;

			CheckErrors();
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
#else
			if (stage > 0) {
				GL.ActiveTexture(TextureUnit.Texture0 + stage);
				if (glTexNum > 0) {
					GL.Enable(EnableCap.Texture2D);
					GL.BindTexture(TextureTarget.Texture2D, glTexNum);
				} else {
					GL.Disable(EnableCap.Texture2D);
				}
				GL.ActiveTexture(TextureUnit.Texture0);
			} else {
				GL.BindTexture(TextureTarget.Texture2D, glTexNum);
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

		public static void SetOrthogonalProjection(float left, float top, float right, float bottom)
		{
#if GLES11
			GL.MatrixMode(All.Projection);
		
			GL.LoadIdentity();
			GL.Ortho(left, right, bottom, top, -1, 1);

			GL.MatrixMode(All.Modelview);
#else
			GL.MatrixMode(MatrixMode.Projection);
		
			GL.LoadIdentity();
			GL.Ortho(left, right, bottom, top, 0, 1);

			GL.MatrixMode(MatrixMode.Modelview);
#endif
		}

		static Viewport viewport;

		public static Viewport Viewport {
			get { return viewport; }
			set {
				viewport = value;
				GL.Viewport(value.X, value.Y, value.Width, value.Height);
			}
		}

		public static void PushProjectionMatrix()
		{
#if GLES11
			GL.MatrixMode(All.Projection);
			GL.PushMatrix();
			GL.MatrixMode(All.Modelview);
#else
			GL.MatrixMode(MatrixMode.Projection);
			GL.PushMatrix();
			GL.MatrixMode(MatrixMode.Modelview);
#endif
		}

		public static void PopProjectionMatrix()
		{
#if GLES11
			GL.MatrixMode(All.Projection);
			GL.PopMatrix();
			GL.MatrixMode(All.Modelview);
#else
			GL.MatrixMode(MatrixMode.Projection);
			GL.PopMatrix();
			GL.MatrixMode(MatrixMode.Modelview);
#endif
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
				case Blending.Add:
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
				}
#else
				GL.Enable(EnableCap.Blend);
				GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Source0Rgb, (int)TextureEnvModeSource.Previous);
				GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src0Alpha, (int)TextureEnvModeSource.Previous);
				GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src1Rgb, (int)TextureEnvModeSource.Texture);
				GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src1Alpha, (int)TextureEnvModeSource.Texture);
				switch(value) {
				case Blending.Silhuette:
					GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
					GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.CombineRgb, (int)TextureEnvModeCombine.Replace);
					GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.CombineAlpha, (int)TextureEnvModeCombine.Modulate);
					GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Combine);
					break;
				case Blending.Add:
					if (PremulAlphaMode)
						GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
					else
						GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
					GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
					break;
				case Blending.Alpha:
				case Blending.Default:
				default:
					if (PremulAlphaMode)
						GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
					else
						GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
					GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
					break;
				}
#endif
				CheckErrors();
			}
		}
		
		public static void DrawSprite(ITexture texture, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1)
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

		public static void DrawTriangleFan(ITexture texture1, Vertex[] vertices, int numVertices)
		{
			DrawTriangleFan(texture1, null, vertices, numVertices);
		}

		public static void DrawTriangleFan(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			var transform = Transform1 * Transform2;
			SetTexture(texture1, 0);
			SetTexture(texture2, 1);
			if (currentIndex + (numVertices - 2) * 3 >= MaxVertices * 4 || currentVertex + numVertices >= MaxVertices) {
				FlushSpriteBatch();
			}
			if (numVertices < 3 || (numVertices - 2) * 3 > MaxVertices * 4) {
				throw new Lime.Exception("Wrong number of vertices");
			}
			Rectangle uvRect1 = texture1.UVRect;
			Rectangle uvRect2 = (texture2 != null) ? texture2.UVRect : new Rectangle();
			int baseVertex = currentVertex;
			for (int i = 0; i < numVertices; i++) {
				Vertex v = vertices[i];
				if (PremulAlphaMode) {
					v.Color = Color4.PremulAlpha(v.Color);
				}
				v.Pos = transform * v.Pos;
				v.UV1 = uvRect1.A + uvRect1.Size * v.UV1;
				if (texture2 != null)
					v.UV2 = uvRect2.A + uvRect2.Size * v.UV2;
				batchVertices[currentVertex++] = v;
			}
			for (int i = 1; i <= numVertices - 2; i++) {
				batchIndices[currentIndex++] = (ushort)baseVertex;
				batchIndices[currentIndex++] = (ushort)(baseVertex + i);
				batchIndices[currentIndex++] = (ushort)(baseVertex + i + 1);
			}
		}

		public static void DrawTextLine(Font font, Vector2 position, string text, Color4 color, float fontHeight)
		{
			DrawTextLine(font, position, text, color, fontHeight, 0, text.Length);
		}

		public static Vector2 MeasureTextLine(Font font, string text, float fontHeight)
		{
			return MeasureTextLine(font, text, fontHeight, 0, text.Length);
		}

		public static Vector2 MeasureTextLine(Font font, string text, float fontHeight, int start, int length)
		{
			FontChar prevChar = null;
			Vector2 size = new Vector2(0, fontHeight);
			float width = 0;
			float scale = fontHeight / font.CharHeight;
			for (int i = 0; i < length; i++) {
				char ch = text[i + start];
				if (ch == '\n') {
					size.Y += fontHeight;
					width = 0;
					continue;
				}
				FontChar fontChar = font.Chars[ch];
				float kerning = 0;
				if (prevChar != null && prevChar.KerningPairs != null) {
					foreach (var pair in prevChar.KerningPairs) {
						if (pair.Char == fontChar.Char) {
							kerning = pair.Kerning;
							break;
						}
					}
				}
				width += scale * (fontChar.ACWidths.X + kerning);
				width += scale * (fontChar.Width + fontChar.ACWidths.Y + kerning);
				size.X = Math.Max(size.X, width);
				prevChar = fontChar;
			}
			return size;
		}

		public static void DrawTextLine(Font font, Vector2 position, string text, Color4 color, float fontHeight, int start, int length)
		{
			FontChar prevChar = null;
			float savedX = position.X;
			float scale = fontHeight / font.CharHeight;
			for (int i = 0; i < length; i++) {
				char ch = text[i + start];
				if (ch == '\n') {
					position.X = savedX;
					position.Y += fontHeight;
					continue;
				}
				FontChar fontChar = font.Chars[ch];
				float kerning = 0;
				if (prevChar != null && prevChar.KerningPairs != null) {
					foreach (var pair in prevChar.KerningPairs) {
						if (pair.Char == fontChar.Char) {
							kerning = pair.Kerning;
							break;
						}
					}
				}
				position.X += scale * (fontChar.ACWidths.X + kerning);
				Vector2 size = new Vector2(scale * fontChar.Width, fontHeight);
				DrawSprite(font.Texture, color, position, size, fontChar.UV0, fontChar.UV1);
				position.X += scale * (fontChar.Width + fontChar.ACWidths.Y + kerning);
				prevChar = fontChar;
			}
		}
	}
}