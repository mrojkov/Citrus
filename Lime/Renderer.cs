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

namespace Lime
{
    [ProtoContract]
	public enum Blending
	{
		[ProtoEnum]
		/// <summary>
		/// Constant none.
		/// </summary>
		None,
		[ProtoEnum]
		/// <summary>
		/// Constant default.
		/// </summary>
		Default,
		[ProtoEnum]
		/// <summary>
		/// Constant alpha.
		/// </summary>
		Alpha,
		[ProtoEnum]
		/// <summary>
		/// Constant add.
		/// </summary>
		Add,
		[ProtoEnum]
		/// <summary>
		/// Constant silhuette.
		/// </summary>
		Silhuette
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
		public static int DrawCalls = 0;
		public const int BatchMaxSize = 128;
		private ushort[] batchIndices = new ushort [BatchMaxSize * 6];
		private Vector2[] batchVertices = new Vector2 [BatchMaxSize * 4];
		private Color4[] batchColors = new Color4 [BatchMaxSize * 4];
		private Vector2[] batchTexCoords0 = new Vector2 [BatchMaxSize * 4];
		private Vector2[] batchTexCoords1 = new Vector2 [BatchMaxSize * 4];
		private int batchSize = 0;
		
		public static Renderer Instance {
			get { return instance; }
		}
		
		private Renderer ()
		{
			WorldMatrix = Matrix32.Identity;		
			int baseIndex = 0;
			ushort baseVertex = 0;
			for (int i = 0; i < BatchMaxSize; i++, baseIndex += 6, baseVertex += 4) {
				batchIndices [baseIndex + 0] = (ushort)(baseVertex + 0);
				batchIndices [baseIndex + 1] = (ushort)(baseVertex + 1);
				batchIndices [baseIndex + 2] = (ushort)(baseVertex + 2);
				batchIndices [baseIndex + 3] = (ushort)(baseVertex + 2);
				batchIndices [baseIndex + 4] = (ushort)(baseVertex + 1);
				batchIndices [baseIndex + 5] = (ushort)(baseVertex + 3);
			}
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
			throw new RuntimeError ("OpenGL errors have occurred: " + errors);
#endif			
		}
		
		public void FlushSpriteBatch ()
		{
			if (batchSize > 0) {
#if GLES11				
				GL.DrawElements (All.Triangles, batchSize * 6, All.UnsignedShort, batchIndices);
				CheckErrors ();
				batchSize = 0;
#else
				GL.DrawElements (BeginMode.Triangles, batchSize * 6, DrawElementsType.UnsignedShort, batchIndices);
				CheckErrors ();
				batchSize = 0;
#endif
				DrawCalls++;
			}
		}
		
		public void BeginFrame ()
		{
			DrawCalls = 0;
			TexturePool.Instance.AdvanceGameRenderCycle ();
#if GLES11			
			GL.ClearColor (0.5f, 0.5f, 0.5f, 1.0f);
			GL.Clear ((uint)All.ColorBufferBit);
			GL.VertexPointer (2, All.Float, 0, batchVertices);
			GL.EnableClientState (All.VertexArray);				
			GL.ColorPointer (4, All.UnsignedByte, 0, batchColors);
			GL.EnableClientState (All.ColorArray);
			//GL.ActiveTexture (All.Texture1);
			//GL.EnableClientState (All.TextureCoordArray);
			//GL.TexCoordPointer (2, All.Float, 0, batchTexCoords1);
			GL.ActiveTexture (All.Texture0);
			GL.Enable (All.Texture2D);
			GL.EnableClientState (All.TextureCoordArray);
			GL.TexCoordPointer (2, All.Float, 0, batchTexCoords0);
			Blending = Blending.Default;
#else
			GL.ClearColor (0.5f, 0.5f, 0.5f, 1.0f);
			GL.Clear (ClearBufferMask.ColorBufferBit);
			CheckErrors ();
			GL.VertexPointer (2, VertexPointerType.Float, 0, batchVertices);
			GL.EnableClientState (ArrayCap.VertexArray);				
			GL.ColorPointer (4, ColorPointerType.UnsignedByte, 0, batchColors);
			GL.EnableClientState (ArrayCap.ColorArray);
			CheckErrors ();
				//GL.ActiveTexture (All.Texture1);
			//GL.EnableClientState (All.TextureCoordArray);
			//GL.TexCoordPointer (2, All.Float, 0, batchTexCoords1);
			GL.ActiveTexture (TextureUnit.Texture0);
			GL.Enable (EnableCap.Texture2D);
			GL.EnableClientState (ArrayCap.TextureCoordArray);
			GL.TexCoordPointer (2, TexCoordPointerType.Float, 0, batchTexCoords0);
			CheckErrors ();
			Blending = Blending.Default;
#endif
			CheckErrors ();
		}
		
		private uint [] textures = new uint [2];
		
		private void SetTexture (ITexture texture, int stage)
		{
			uint handle = texture != null ? texture.GetHandle() : 0;
			if (handle != textures [stage])
			{
				BindTexture (handle, stage);
				textures [stage] = handle;
			}
		}

		private void BindTexture (uint glTexNum, int stage)
		{
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
			textures [stage] = 0;
			CheckErrors ();
		}
		
		public void EndFrame ()
		{
			SetTexture (null, 0);
			SetTexture (null, 1);
		}
		
		private void FlushSpriteBatchIfFull ()
		{
			if (batchSize >= BatchMaxSize)
				FlushSpriteBatch ();
		}
		
		public Matrix32 WorldMatrix {
			get; set;
		}

		/// <summary>
		/// Sets the orthogonal projection.
		/// </summary>
		/// <param name='left'>
		/// Left.
		/// </param>
		/// <param name='top'>
		/// Top.
		/// </param>
		/// <param name='right'>
		/// Right.
		/// </param>
		/// <param name='bottom'>
		/// Bottom.
		/// </param>
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

		/// <summary>
		/// Gets or sets the viewport.
		/// </summary>
		/// <value>
		/// The viewport.
		/// </value>
		public Viewport Viewport {
			get { return viewport; }
			set {
				viewport = value;
				GL.Viewport (value.X, value.Y, value.Width, value.Height);
			}
		}

		/// <summary>
		/// Pushs the projection matrix.
		/// </summary>
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

		/// <summary>
		/// Pops the projection matrix.
		/// </summary>
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

		private Blending blending = Blending.None;

		/// <summary>
		/// Sets the blending.
		/// </summary>
		/// <value>
		/// The blending.
		/// </value>
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
					GL.BlendFunc (BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
					GL.TexEnv (TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
					break;
				case Blending.Alpha:
				case Blending.Default:
					GL.BlendFunc (BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
					GL.TexEnv (TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
					break;
				}
#endif
				CheckErrors ();
			}
		}
		
		/// <summary>
		/// Renders a rectangular textured polygon.
		/// </summary>
		/// <param name="texture">Texture object.</param>
		/// <param name="color">Sprite modulation color.</param>
		/// <param name="position">Left top corner of the sprite.</param>
		/// <param name="size">Size of the sprite.</param>
		/// <param name="uv0">Left top of texture coordinates.</param>
		/// <param name="uv1">Right bottom of texture coordinates.</param>
		/// <summary>
		/// Draws the sprite.
		/// </summary>
		/// <param name='texture'>
		/// Texture.
		/// </param>
		/// <param name='color'>
		/// Color.
		/// </param>
		/// <param name='position'>
		/// Position.
		/// </param>
		/// <param name='size'>
		/// Size.
		/// </param>
		/// <param name='uv0'>
		/// Uv0.
		/// </param>
		/// <param name='uv1'>
		/// Uv1.
		/// </param>
		public void DrawSprite (ITexture texture, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1)
		{
			Rectangle textureRect = texture.UVRect;
			uv0 = textureRect.A + (textureRect.B - textureRect.A) * uv0;
			uv1 = textureRect.A + (textureRect.B - textureRect.A) * uv1;
		
			SetTexture (texture, 0);
			FlushSpriteBatchIfFull ();
			int i = batchSize++ * 4;
			batchColors [i + 0] = color;
			batchVertices [i + 0] = WorldMatrix.TransformVector (new Vector2 (position.X, position.Y));
			batchTexCoords0 [i + 0] = new Vector2 (uv0.X, uv0.Y);

			batchColors [i + 1] = color;
			batchVertices [i + 1] = WorldMatrix.TransformVector (new Vector2 (position.X + size.X, position.Y));
			batchTexCoords0 [i + 1] = new Vector2 (uv1.X, uv0.Y);
	
			batchColors [i + 2] = color;
			batchVertices [i + 2] = WorldMatrix.TransformVector (new Vector2 (position.X, position.Y + size.Y));
			batchTexCoords0 [i + 2] = new Vector2 (uv0.X, uv1.Y);

			batchColors [i + 3] = color;
			batchVertices [i + 3] = WorldMatrix.TransformVector (new Vector2 (position.X + size.X, position.Y + size.Y));
			batchTexCoords0 [i + 3] = new Vector2 (uv1.X, uv1.Y);
		}

		public struct Vertex
		{
			/// <summary>
			/// The position.
			/// </summary>
			public Vector2 Pos;
			/// <summary>
			/// The color.
			/// </summary>
			public Color4 Color;
			/// <summary>
			/// The U v1.
			/// </summary>
			public Vector2 UV1;
			/// <summary>
			/// The U v2.
			/// </summary>
			public Vector2 UV2;
		}

		/// <summary>
		/// Draws the triangle fan.
		/// </summary>
		/// <param name='texture'>
		/// Texture.
		/// </param>
		/// <param name='vertices'>
		/// Vertices.
		/// </param>
		/// <param name='numVertices'>
		/// Number vertices.
		/// </param>
		public void DrawTriangleFan (ITexture texture, Vertex[] vertices, int numVertices)
		{
//			DrawCalls++;
//			GL.ActiveTexture (TextureUnit.Texture0);
//			GL.Enable (EnableCap.Texture2D);
//			GL.BindTexture (TextureTarget.Texture2D, texture.GetHandle ());
//			GL.Begin (BeginMode.TriangleFan);
//			for (int i = 0; i < numVertices; i++) {
//				var v = vertices [i];
//				GL.Color4 (v.Color.R, v.Color.G, v.Color.B, v.Color.A);
//				GL.TexCoord2 (v.UV1.X, v.UV1.Y);
//				GL.Vertex2 (v.Pos.X, v.Pos.Y);
//			}
//			GL.End ();
		}

		/// <summary>
		/// Draws the combined triangle fan.
		/// </summary>
		/// <param name='texture1'>
		/// Texture1.
		/// </param>
		/// <param name='texture2'>
		/// Texture2.
		/// </param>
		/// <param name='vertices'>
		/// Vertices.
		/// </param>
		/// <param name='numVertices'>
		/// Number vertices.
		/// </param>
		public void DrawCombinedTriangleFan (ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
//			DrawCalls++;
//			GL.Enable (EnableCap.Texture2D);
//			GL.BindTexture (TextureTarget.Texture2D, texture1.GetHandle ());
//			GL.ActiveTexture (TextureUnit.Texture1);
//			GL.Enable (EnableCap.Texture2D);
//			GL.BindTexture (TextureTarget.Texture2D, texture2.GetHandle ());
//			GL.Begin (BeginMode.TriangleFan);
//			for (int i = 0; i < numVertices; i++) {
//				var v = vertices [i];
//				GL.Color4 (v.Color.R, v.Color.G, v.Color.B, v.Color.A);
//				GL.MultiTexCoord2 (TextureUnit.Texture0, v.UV1.X, v.UV1.Y);
//				GL.MultiTexCoord2 (TextureUnit.Texture1, v.UV2.X, v.UV2.Y);
//				GL.Vertex2 (v.Pos.X, v.Pos.Y);
//			}
//			GL.End ();
//			GL.ActiveTexture (TextureUnit.Texture1);
//			GL.Disable (EnableCap.Texture2D);
//			GL.ActiveTexture (TextureUnit.Texture0);
		}

		/// <summary>
		/// Measures the text line.
		/// </summary>
		/// <returns>
		/// The text line.
		/// </returns>
		/// <param name='font'>
		/// Font.
		/// </param>
		/// <param name='text'>
		/// Text.
		/// </param>
		/// <param name='fontHeight'>
		/// Font height.
		/// </param>
		public Vector2 MeasureTextLine (Font font, string text, float fontHeight)
		{
			Vector2 size = new Vector2 (0, fontHeight);
			uint prevCharCode = 0;
			for (int i = 0; i < text.Length; i++) {
				FontChar fontChar = font.Chars [text [i]];
				float scale = fontHeight / fontChar.Size.Y;
				float delta = font.Pairs.Get (prevCharCode, fontChar.Code);
				size.X += scale * (fontChar.ACWidths.X + delta);
				size.X += scale * (fontChar.Size.X + fontChar.ACWidths.Y + delta);
				prevCharCode = fontChar.Code;
			}
			return size;
		}

		/// <summary>
		/// Draws the text line.
		/// </summary>
		/// <param name='font'>
		/// Font.
		/// </param>
		/// <param name='position'>
		/// Position.
		/// </param>
		/// <param name='text'>
		/// Text.
		/// </param>
		/// <param name='color'>
		/// Color.
		/// </param>
		/// <param name='fontHeight'>
		/// Font height.
		/// </param>
		public void DrawTextLine (Font font, Vector2 position, string text, Color4 color, float fontHeight)
		{
			float savedX = position.X;
			uint prevCharCode = 0;
			var invTextureSize = new Vector2 (1.0f / font.Texture.ImageSize.Width, 1.0f / font.Texture.ImageSize.Height);
			for (int i = 0; i < text.Length; i++) {
				if (text [i] == '\n') {
					position.X = savedX;
					position.Y += fontHeight;
					continue;
				}
				FontChar fontChar = font.Chars [text [i]];
				float scale = fontHeight / fontChar.Size.Y;
				float delta = font.Pairs.Get (prevCharCode, fontChar.Code);
				Vector2 uv0 = Vector2.Scale (fontChar.Position, invTextureSize);
				Vector2 uv1 = Vector2.Scale (fontChar.Position + fontChar.Size, invTextureSize);
				position.X += scale * (fontChar.ACWidths.X + delta);
				DrawSprite (font.Texture, color, position, scale * fontChar.Size, uv0, uv1);
				position.X += scale * (fontChar.Size.X + fontChar.ACWidths.Y + delta);
				prevCharCode = fontChar.Code;
			}
		}
	}
}