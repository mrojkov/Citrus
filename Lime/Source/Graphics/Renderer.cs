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
	public enum HorizontalAlign
	{
        [ProtoEnum]
		/// <summary>
		/// Constant left.
		/// </summary>
		Left,
        [ProtoEnum]
		/// <summary>
		/// Constant center.
		/// </summary>
		Center,
        [ProtoEnum]
		/// <summary>
		/// Constant right.
		/// </summary>
		Right,
	}

    [ProtoContract]
	public enum VerticalAlign
	{
        [ProtoEnum]
		/// <summary>
		/// Constant top.
		/// </summary>
		Top,
        [ProtoEnum]
		/// <summary>
		/// Constant center.
		/// </summary>
		Center,
        [ProtoEnum]
		/// <summary>
		/// Constant bottom.
		/// </summary>
		Bottom,
	}

	public struct Viewport
	{
		/// <summary>
		/// The x.
		/// </summary>
		public int X;
		/// <summary>
		/// The y.
		/// </summary>
		public int Y;
		/// <summary>
		/// The width.
		/// </summary>
		public int Width;
		/// <summary>
		/// The height.
		/// </summary>
		public int Height;
	}

	public class Renderer
	{
		static readonly Renderer instance = new Renderer ();
		public bool PremulAlphaMode = true;
		
		const int MaxVertices = 128;
		public int DrawCalls = 0;

		[StructLayout(LayoutKind.Explicit, Size=32)]
		public struct Vertex
		{
			/// <summary>
			/// The position.
			/// </summary>
			[FieldOffset(0)]
			public Vector2 Pos;
			/// <summary>
			/// The color.
			/// </summary>
			[FieldOffset(8)]
			public Color4 Color;
			/// <summary>
			/// The U v1.
			/// </summary>
			[FieldOffset(12)]
			public Vector2 UV1;
			/// <summary>
			/// The U v2.
			/// </summary>
			[FieldOffset(20)]
			public Vector2 UV2;
		}

		uint [] textures = new uint [2];
		ushort [] batchIndices = new ushort [MaxVertices * 3];
		Vertex [] batchVertices = new Vertex [MaxVertices];
		uint batchVBO;

		int currentVertex = 0;
		int currentIndex = 0;
		
		public Matrix32 WorldMatrix = Matrix32.Identity;

		public static Renderer Instance {
			get  { return instance; }
		}
		
		public void CheckErrors ()
		{
#if DEBUG
#if GLES11
			All errCode = GL.GetError ();
			if (errCode == All.NoError)
				return;
			string errors = "";
			while (errCode != All.NoError) {
				if (errors != "")
					errors += ", ";
				errors += GL.GetString (errCode);
				errCode = GL.GetError ();
			}
#else
			ErrorCode errCode = GL.GetError ();
			if (errCode == ErrorCode.NoError)
				return;
			string errors = "";
			while (errCode != ErrorCode.NoError) {
				if (errors != "")
					errors += ", ";
				errors += errCode.ToString();
				errCode = GL.GetError ();
			}
#endif
			throw new Exception ("OpenGL errors have occurred: " + errors);
#endif
		}
		
		public void FlushSpriteBatch ()
		{
			if (currentIndex > 0) {
#if GLES11
				GL.BufferData (All.ArrayBuffer, (IntPtr)(32 * currentVertex), batchVertices, All.StaticDraw);
				GL.DrawElements (All.Triangles, currentIndex, All.UnsignedShort, batchIndices);
#else
				// Tell OpenGL to discard old VBO when done drawing it and reserve memory now for a new buffer.
				// without this, GL would wait until draw operations on old VBO are complete before writing to it
				GL.BufferData (BufferTarget.ArrayBuffer, (IntPtr)(32 * currentVertex), IntPtr.Zero, BufferUsageHint.StreamDraw);
				// Fill newly allocated buffer
				GL.BufferData (BufferTarget.ArrayBuffer, (IntPtr)(32 * currentVertex), batchVertices, BufferUsageHint.StreamDraw);
				GL.DrawElements (BeginMode.Triangles, currentIndex, DrawElementsType.UnsignedShort, batchIndices);
#endif
				CheckErrors ();
				currentIndex = currentVertex = 0;
				DrawCalls++;
			}
		}
		
		public void BeginFrame ()
		{
			PlainTexture.DeleteScheduledTextures ();
			DrawCalls = 0;
			TexturePool.Instance.AdvanceGameRenderCycle ();
#if GLES11
			GL.ClearColor (0.5f, 0.5f, 0.5f, 1.0f);
			GL.Clear ((uint)All.ColorBufferBit);
			GL.Enable (All.Texture2D);

			GL.GenBuffers (1, ref batchVBO);
			GL.BindBuffer (All.ArrayBuffer, batchVBO);
			// Set up vertex and color arrays
			GL.VertexPointer (2, All.Float, 32, (IntPtr)0);
			GL.EnableClientState (All.VertexArray);
			GL.ColorPointer (4, All.UnsignedByte, 32, (IntPtr)8);
			GL.EnableClientState (All.ColorArray);

			// Set up texture coordinate arrays
			GL.ClientActiveTexture (All.Texture1);
			GL.EnableClientState (All.TextureCoordArray);
			GL.TexCoordPointer (2, All.Float, 32, (IntPtr)20);
			GL.ClientActiveTexture (All.Texture0);
			GL.EnableClientState (All.TextureCoordArray);
			GL.TexCoordPointer (2, All.Float, 32, (IntPtr)12);
#else
			GL.ClearColor (0.5f, 0.5f, 0.5f, 1.0f);
			GL.Clear (ClearBufferMask.ColorBufferBit);
			GL.Enable (EnableCap.Texture2D);

			GL.GenBuffers (1, out batchVBO);
			// Since there's only 1 VBO in the app, might aswell setup here.
			GL.BindBuffer (BufferTarget.ArrayBuffer, batchVBO);
			// Set up vertex and color arrays
			GL.VertexPointer (2, VertexPointerType.Float, 32, 0);
			GL.EnableClientState (ArrayCap.VertexArray);
			GL.ColorPointer (4, ColorPointerType.UnsignedByte, 32, 8);
			GL.EnableClientState (ArrayCap.ColorArray);
			// Set up texture coordinate arrays
			GL.ClientActiveTexture (TextureUnit.Texture1);
			GL.EnableClientState (ArrayCap.TextureCoordArray);
			GL.TexCoordPointer (2, TexCoordPointerType.Float, 32, 20);
			GL.ClientActiveTexture (TextureUnit.Texture0);
			GL.EnableClientState (ArrayCap.TextureCoordArray);
			GL.TexCoordPointer (2, TexCoordPointerType.Float, 32, 12);
#endif
			Blending = Blending.Default;

			CheckErrors ();
		}
		
		public void SetTexture (ITexture texture, int stage)
		{
			uint handle = texture != null ? texture.GetHandle() : 0;
			SetTexture (handle, stage);
		}

		internal void SetTexture (uint glTexNum, int stage)
		{
			if (glTexNum == textures [stage])
				return;
			FlushSpriteBatch ();
#if GLES11
			if (stage > 0) {
				GL.ActiveTexture (All.Texture0 + stage);
				if (glTexNum > 0) {
					GL.Enable (All.Texture2D);
					GL.BindTexture (All.Texture2D, glTexNum);
				} else {
					GL.Disable (All.Texture2D);
				}
				GL.ActiveTexture (All.Texture0);
			} else {
				GL.BindTexture (All.Texture2D, glTexNum);
			}
#else
			if (stage > 0) {
				GL.ActiveTexture (TextureUnit.Texture0 + stage);
				if (glTexNum > 0) {
					GL.Enable (EnableCap.Texture2D);
					GL.BindTexture (TextureTarget.Texture2D, glTexNum);
				} else {
					GL.Disable (EnableCap.Texture2D);
				}
				GL.ActiveTexture (TextureUnit.Texture0);
			} else {
				GL.BindTexture (TextureTarget.Texture2D, glTexNum);
			}
#endif
			textures [stage] = glTexNum;
			CheckErrors ();
		}
		
		public void EndFrame ()
		{
			FlushSpriteBatch ();
			SetTexture (null, 0);
			SetTexture (null, 1);
		}

		public void SetOrthogonalProjection (float left, float top, float right, float bottom)
		{
#if GLES11
			GL.MatrixMode (All.Projection);
		
			GL.LoadIdentity ();
			GL.Ortho (left, right, bottom, top, -1, 1);

			GL.MatrixMode (All.Modelview);
#else
			GL.MatrixMode (MatrixMode.Projection);
		
			GL.LoadIdentity ();
			GL.Ortho (left, right, bottom, top, 0, 1);

			GL.MatrixMode (MatrixMode.Modelview);
#endif
		}

		Viewport viewport;

		public Viewport Viewport {
			get { return viewport; }
			set {
				viewport = value;
				GL.Viewport (value.X, value.Y, value.Width, value.Height);
			}
		}

		public void PushProjectionMatrix ()
		{
#if GLES11
			GL.MatrixMode (All.Projection);
			GL.PushMatrix ();
			GL.MatrixMode (All.Modelview);
#else
			GL.MatrixMode (MatrixMode.Projection);
			GL.PushMatrix ();
			GL.MatrixMode (MatrixMode.Modelview);
#endif
		}

		public void PopProjectionMatrix ()
		{
#if GLES11
			GL.MatrixMode (All.Projection);
			GL.PopMatrix ();
			GL.MatrixMode (All.Modelview);
#else
			GL.MatrixMode (MatrixMode.Projection);
			GL.PopMatrix ();
			GL.MatrixMode (MatrixMode.Modelview);
#endif
		}

		Blending blending = Blending.None;
		public Blending Blending {
			set {
				if (value == blending)
					return;
				FlushSpriteBatch ();
				blending = value;
#if GLES11
				GL.Enable (All.Blend);
				GL.TexEnv (All.TextureEnv, All.Src0Rgb, (int)All.Previous);
				GL.TexEnv (All.TextureEnv, All.Src0Alpha, (int)All.Previous);
				GL.TexEnv (All.TextureEnv, All.Src1Rgb, (int)All.Texture);
				GL.TexEnv (All.TextureEnv, All.Src1Alpha, (int)All.Texture);
				switch (value) {
				case Blending.Silhuette:
					GL.BlendFunc (All.SrcAlpha, All.OneMinusSrcAlpha);
					GL.TexEnv (All.TextureEnv, All.CombineRgb, (int)All.Replace);
					GL.TexEnv (All.TextureEnv, All.CombineAlpha, (int)All.Modulate);
					GL.TexEnv (All.TextureEnv, All.TextureEnvMode, (int)All.Combine);
					break;
				case Blending.Add:
					GL.BlendFunc (All.SrcAlpha, All.One);
					GL.TexEnv (All.TextureEnv, All.TextureEnvMode, (int)All.Modulate);
					break;
				case Blending.Alpha:
				case Blending.Default:
					if (PremulAlphaMode)
						GL.BlendFunc (All.One, All.OneMinusSrcAlpha);
					else
						GL.BlendFunc (All.SrcAlpha, All.OneMinusSrcAlpha);
					GL.TexEnv (All.TextureEnv, All.TextureEnvMode, (int)All.Modulate);
					break;
				}
#else
				GL.Enable (EnableCap.Blend);
				GL.TexEnv (TextureEnvTarget.TextureEnv, TextureEnvParameter.Source0Rgb, (int)TextureEnvModeSource.Previous);
				GL.TexEnv (TextureEnvTarget.TextureEnv, TextureEnvParameter.Src0Alpha, (int)TextureEnvModeSource.Previous);
				GL.TexEnv (TextureEnvTarget.TextureEnv, TextureEnvParameter.Src1Rgb, (int)TextureEnvModeSource.Texture);
				GL.TexEnv (TextureEnvTarget.TextureEnv, TextureEnvParameter.Src1Alpha, (int)TextureEnvModeSource.Texture);
				switch (value) {
				case Blending.Silhuette:
					GL.BlendFunc (BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
					GL.TexEnv (TextureEnvTarget.TextureEnv, TextureEnvParameter.CombineRgb, (int)TextureEnvModeCombine.Replace);
					GL.TexEnv (TextureEnvTarget.TextureEnv, TextureEnvParameter.CombineAlpha, (int)TextureEnvModeCombine.Modulate);
					GL.TexEnv (TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Combine);
					break;
				case Blending.Add:
					if (PremulAlphaMode)
						GL.BlendFunc (BlendingFactorSrc.One, BlendingFactorDest.One);
					else
						GL.BlendFunc (BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
					GL.TexEnv (TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
					break;
				case Blending.Alpha:
				case Blending.Default:
				default:
					if (PremulAlphaMode)
						GL.BlendFunc (BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
					else
						GL.BlendFunc (BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
					GL.TexEnv (TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
					break;
				}
#endif
				CheckErrors ();
			}
		}
		
		public void DrawSprite (ITexture texture, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1)
		{
			Rectangle textureRect = texture.UVRect;
			uv0 = textureRect.A + (textureRect.B - textureRect.A) * uv0;
			uv1 = textureRect.A + (textureRect.B - textureRect.A) * uv1;
			if (PremulAlphaMode) {
				color = Color4.PremulAlpha (color);
			}
			SetTexture (texture, 0);
			if (currentIndex + 6 >= batchIndices.Length || currentVertex + 4 >= batchVertices.Length) {
				FlushSpriteBatch ();
			}
			int i = currentVertex;
			currentVertex += 4;
			Vertex v = new Vertex ();
			v.Pos = WorldMatrix.TransformVector (position.X, position.Y);
			v.Color = color;
			v.UV1.X = uv0.X;
			v.UV1.Y = uv0.Y;
			batchVertices [i + 0] = v;
			v.Pos = WorldMatrix.TransformVector (position.X + size.X, position.Y);
			v.UV1.X = uv1.X;
			batchVertices [i + 1] = v;
			v.Pos = WorldMatrix.TransformVector (position.X + size.X, position.Y + size.Y);
			v.UV1.Y = uv1.Y;
			batchVertices [i + 3] = v;
			v.Pos = WorldMatrix.TransformVector (position.X, position.Y + size.Y);
			v.UV1.X = uv0.X;
			batchVertices [i + 2] = v;
			batchIndices [currentIndex++] = (ushort)(i + 0);
			batchIndices [currentIndex++] = (ushort)(i + 1);
			batchIndices [currentIndex++] = (ushort)(i + 2);
			batchIndices [currentIndex++] = (ushort)(i + 2);
			batchIndices [currentIndex++] = (ushort)(i + 1);
			batchIndices [currentIndex++] = (ushort)(i + 3);
		}
		
		public void DrawTriangleFan (ITexture texture, Vertex[] vertices, int numVertices)
		{
			SetTexture (texture, 0);
			if (currentIndex + (numVertices - 2) * 3 >= batchIndices.Length || currentVertex + numVertices >= batchVertices.Length) {
				FlushSpriteBatch ();
			}
			if (numVertices < 3 || (numVertices - 2) * 3 > batchIndices.Length) {
				throw new Lime.Exception ("Wrong number of vertices");
			}
			int baseVertex = currentVertex;
			Vector2 UV0 = texture.UVRect.A;
			Vector2 dUV = texture.UVRect.B - UV0;
			for (int i = 0; i < numVertices; i++) {
				Vertex v = vertices [i];
				if (PremulAlphaMode) {
					v.Color = Color4.PremulAlpha (v.Color);
				}
				v.Pos = WorldMatrix * v.Pos;
				v.UV1 = UV0 + dUV * v.UV1;
				batchVertices [currentVertex] = v;
				currentVertex++;
			}
			for (int i = 1; i <= numVertices - 2; i++) {
				batchIndices [currentIndex++] = (ushort)baseVertex;
				batchIndices [currentIndex++] = (ushort)(baseVertex + i);
				batchIndices [currentIndex++] = (ushort)(baseVertex + i + 1);
			}
		}

		public void DrawCombinedTriangleFan (ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			FlushSpriteBatch ();
			if (vertices.Length > batchVertices.Length) {
				throw new Lime.Exception ("Too many vertices");
			}
			SetTexture (texture1, 0);
			SetTexture (texture2, 1);
			Rectangle textureRect1 = texture1.UVRect;
			Rectangle textureRect2 = texture2.UVRect;
			for (int i = 0; i < numVertices; i++) {
				Vertex v = vertices [i];
				if (PremulAlphaMode) {
					v.Color = Color4.PremulAlpha (v.Color);
				}
				v.UV1 = textureRect1.A + (textureRect1.B - textureRect1.A) * v.UV1;
				v.UV2 = textureRect2.A + (textureRect2.B - textureRect2.A) * v.UV2;
				batchVertices [i] = v;
			}
#if GLES11
			GL.DrawArrays (All.TriangleFan, 0, numVertices);
#else
			GL.DrawArrays (BeginMode.TriangleFan, 0, numVertices);
#endif
			SetTexture (null, 1);
			DrawCalls++;
		}

		public Vector2 MeasureTextLine (Font font, string text, float fontHeight)
		{
			Vector2 size = new Vector2 (0, fontHeight);
			char prevChar = '\0';
			for (int i = 0; i < text.Length; i++) {
				FontChar fontChar = font.Chars [text [i]];
				float scale = fontHeight / fontChar.Size.Y;
				float delta = font.Pairs.Get (prevChar, fontChar.Char);
				size.X += scale * (fontChar.ACWidths.X + delta);
				size.X += scale * (fontChar.Size.X + fontChar.ACWidths.Y + delta);
				prevChar = fontChar.Char;
			}
			return size;
		}

		public void DrawTextLine (Font font, Vector2 position, string text, Color4 color, float fontHeight)
		{
			float savedX = position.X;
			char prevChar = '\0';
			for (int i = 0; i < text.Length; i++) {
				if (text [i] == '\n') {
					position.X = savedX;
					position.Y += fontHeight;
					continue;
				}
				FontChar fontChar = font.Chars [text [i]];
				float scale = fontHeight / fontChar.Size.Y;
				float delta = font.Pairs.Get (prevChar, fontChar.Char);
				position.X += scale * (fontChar.ACWidths.X + delta);
				DrawSprite (font.Texture, color, position, scale * fontChar.Size, fontChar.UV0, fontChar.UV1);
				position.X += scale * (fontChar.Size.X + fontChar.ACWidths.Y + delta);
				prevChar = fontChar.Char;
			}
		}

		public void DrawSprite2 (ITexture texture, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1)
		{
			Rectangle textureRect = texture.UVRect;
			uv0 = textureRect.A + (textureRect.B - textureRect.A) * uv0;
			uv1 = textureRect.A + (textureRect.B - textureRect.A) * uv1;
			if (PremulAlphaMode) {
				color = Color4.PremulAlpha (color);
			}
			SetTexture (texture, 0);
			if (currentIndex + 6 >= batchIndices.Length || currentVertex + 4 >= batchVertices.Length) {
				FlushSpriteBatch ();
			}
			int i = currentVertex;
			currentVertex += 4;
			batchVertices [i + 0] = new Vertex {
				Pos = WorldMatrix * new Vector2 (position.X, position.Y),
				Color = color,
				UV1 = new Vector2 (uv0.X, uv0.Y),
			};
			batchVertices [i + 1] = new Vertex {
				Pos = WorldMatrix * new Vector2 (position.X + size.X, position.Y),
				Color = color,
				UV1 = new Vector2 (uv1.X, uv0.Y),
			};
			batchVertices [i + 2] = new Vertex {
				Pos = WorldMatrix * new Vector2 (position.X, position.Y + size.Y),
				Color = color,
				UV1 = new Vector2 (uv0.X, uv1.Y),
			};
			batchVertices [i + 3] = new Vertex {
				Pos = WorldMatrix * new Vector2 (position.X + size.X, position.Y + size.Y),
				Color = color,
				UV1 = new Vector2 (uv1.X, uv1.Y),
			};
			batchIndices [currentIndex++] = (ushort)(i + 0);
			batchIndices [currentIndex++] = (ushort)(i + 1);
			batchIndices [currentIndex++] = (ushort)(i + 2);
			batchIndices [currentIndex++] = (ushort)(i + 2);
			batchIndices [currentIndex++] = (ushort)(i + 1);
			batchIndices [currentIndex++] = (ushort)(i + 3);
		}

		public void DrawTextLine2 (Font font, Vector2 position, string text, Color4 color, float fontHeight)
		{
			FontChar fontChar = font.Chars ['X'];
			float savedX = position.X;
			float scale = fontHeight / fontChar.Size.Y;
			char prevChar = '\0';
			for (int i = 0; i < text.Length; i++) {
				if (text [i] == '\n') {
					position.X = savedX;
					position.Y += fontHeight;
					continue;
				}
				//FontChar fontChar = font.Chars [text [i]];
				
				float delta = 0;//font.Pairs.Get (prevChar, fontChar.Char);
				position.X += scale * (fontChar.ACWidths.X + delta);
				DrawSprite (font.Texture, color, position, scale * fontChar.Size, fontChar.UV0, fontChar.UV1);
				position.X += scale * (fontChar.Size.X + fontChar.ACWidths.Y + delta);
				prevChar = fontChar.Char;
			}
		}
	}
}