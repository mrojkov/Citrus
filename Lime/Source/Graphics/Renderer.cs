using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
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
		Glow,
		[ProtoEnum]
		Modulate,
		[ProtoEnum]
		Burn,
	}

	public struct WindowRect
	{
		public int X;
		public int Y;
		public int Width;
		public int Height;

		public static explicit operator IntRectangle(WindowRect r)
		{
			return new IntRectangle(r.X, r.Y, r.X + r.Width, r.Y + r.Height);
		}
	}

	public static unsafe class Renderer
	{
		private static Matrix32 transform2 = Matrix32.Identity;
		private static Stack<Matrix44> projectionStack;
		private static WindowRect viewport;
		private static WindowRect scissorRectangle = new WindowRect();
		private static bool scissorTestEnabled = false;
		private static bool Transform2Active;

		public static Blending Blending;
		public static ShaderId Shader;
		public static Matrix32 Transform1;
		public static int RenderCycle { get; private set; }
		public static bool PremultipliedAlphaMode;
		public static int DrawCalls = 0;
		public static readonly RenderList MainRenderList = new RenderList();
		public static RenderList CurrentRenderList;

		public static Matrix32 Transform2
		{
			get { return transform2; }
			set
			{
				transform2 = value;
				Transform2Active = !value.IsIdentity();
			}
		}

		public static Matrix44 Projection
		{
			get { return projectionStack.Peek(); }
			set 
			{
				projectionStack.Pop();
				projectionStack.Push(value);
				PlatformRenderer.ResetShader();
			}
		}

		public static WindowRect ScissorRectangle
		{
			get { return scissorRectangle; }
			set 
			{
				MainRenderList.Flush();
				scissorRectangle = value;
				PlatformRenderer.SetScissorRectangle(value);
			}
		}

		public static bool ScissorTestEnabled
		{
			get { return scissorTestEnabled; }
			set 
			{
				MainRenderList.Flush();
				scissorTestEnabled = value;
				PlatformRenderer.EnableScissorTest(value);
			}
		}

		public static WindowRect Viewport
		{
			get { return viewport; }
			set
			{
				viewport = value;
				PlatformRenderer.SetViewport(value);
			}
		}

		public static void SetDefaultViewport()
		{
			if (Application.Instance != null) {
				var windowSize = Application.Instance.WindowSize;
				Viewport = new WindowRect {
					X = 0,
					Y = 0,
					Width = windowSize.Width,
					Height = windowSize.Height
				};
			}
		}

		public static void SetOrthogonalProjection(Vector2 leftTop, Vector2 rightBottom)
		{
			SetOrthogonalProjection(leftTop.X, leftTop.Y, rightBottom.X, rightBottom.Y);
		}

		public static void SetOrthogonalProjection(float left, float top, float right, float bottom)
		{
			Projection = Matrix44.CreateOrthographicOffCenter(left, right, bottom, top, 0, 1);
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

		static Renderer()
		{
			PremultipliedAlphaMode = true;
			projectionStack = new Stack<Matrix44>();
			projectionStack.Push(Matrix44.Identity);
		}

		public static void BeginFrame()
		{
			PlatformRenderer.BeginFrame();
			DrawCalls = 0;
			Blending = Blending.None;
			Shader = ShaderId.None;
			Transform1 = Matrix32.Identity;
			Transform2 = Matrix32.Identity;
			CurrentRenderList = MainRenderList;
			SetDefaultViewport();
			RenderCycle++;
		}

		public static void EndFrame()
		{
			Flush();
			PlatformRenderer.EndFrame();
		}

		public static void Flush()
		{
			MainRenderList.Flush();
		}

		public static void DrawTextLine(float x, float y, string text, float fontHeight = 20, uint abgr = 0xFFFFFFFF)
		{
			DrawTextLine(new Vector2(x, y), text, fontHeight, abgr);
		}

		public static void DrawTextLine(Vector2 position, string text, float fontHeight = 20, uint abgr = 0xFFFFFFFF)
		{
			DrawTextLine(FontPool.Instance[null], position, text, fontHeight, new Color4(abgr));
		}

		public static void DrawTextLine(Font font, Vector2 position, string text, float fontHeight, Color4 color)
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
			var size = new Vector2(0, fontHeight);
			float width = 0;
			// float scale = fontHeight / font.CharHeight;
			for (int i = 0; i < length; i++) {
				char ch = text[i + start];
				if (ch == '\n') {
					size.Y += fontHeight;
					width = 0;
					continue;
				}
				var fontChar = font.Chars[ch];
				if (fontChar == FontChar.Null) {
					continue;
				}
				float kerning = 0;
				if (prevChar != null && prevChar.KerningPairs != null) {
					foreach (var pair in prevChar.KerningPairs) {
						if (pair.Char == fontChar.Char) {
							kerning = pair.Kerning;
							break;
						}
					}
				}
				float scale = fontHeight / fontChar.Height;
				width += scale * (fontChar.ACWidths.X + kerning);
				width += scale * (fontChar.Width + fontChar.ACWidths.Y);
				size.X = Math.Max(size.X, width);
				prevChar = fontChar;
			}
			return size;
		}

		public static void DrawTextLine(Font font, Vector2 position, string text, Color4 color, float fontHeight, int start, int length)
		{
			using (var list = new SpriteList()) {
				DrawTextLine(list, font, position, text, color, fontHeight, start, length);
				list.Render();
			}
		}

		public static void DrawTextLine(SpriteList list, Font font, Vector2 position, string text, Color4 color, float fontHeight, int start, int length)
		{
			FontChar prevChar = null;
			float savedX = position.X;
			for (int i = 0; i < length; i++) {
				char ch = text[i + start];
				if (ch == '\n') {
					position.X = savedX;
					position.Y += fontHeight;
					continue;
				}
				FontChar fontChar = font.Chars[ch];
				if (fontChar == FontChar.Null) {
					continue;
				}
				float kerning = 0;
				if (prevChar != null && prevChar.KerningPairs != null) {
					foreach (var pair in prevChar.KerningPairs) {
						if (pair.Char == fontChar.Char) {
							kerning = pair.Kerning;
							break;
						}
					}
				}
				float scale = fontHeight / fontChar.Height;
				position.X += scale * (fontChar.ACWidths.X + kerning);
				var item = list.Add();
				item.Texture = font.Textures[fontChar.TextureIndex];
				item.Color = color;
				item.Position = position;
				item.Size = new Vector2(scale * fontChar.Width, fontHeight);
				item.UV0 = fontChar.UV0;
				item.UV1 = fontChar.UV1;
				position.X += scale * (fontChar.Width + fontChar.ACWidths.Y);
				prevChar = fontChar;
			}
		}

		public static void DrawTriangleFan(ITexture texture, Vertex[] vertices, int numVertices)
		{
			DrawTriangleFan(texture, null, vertices, numVertices);
		}

		public static void DrawTriangleFan(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			if (Blending == Blending.Glow) {
				Blending = Blending.Default;
				DrawTriangleFan(texture1, texture2, vertices, numVertices);
				Blending = Blending.Glow;
			}
			var batch = DrawTrianglesHelper(texture1, texture2, vertices, numVertices);
			var iptr = &batch.Indices[batch.IndexCount];
			var baseVertex = batch.VertexBuffer.VertexCount;
			for (int i = 1; i <= numVertices - 2; i++) {
				*iptr++ = (ushort)baseVertex;
				*iptr++ = (ushort)(baseVertex + i);
				*iptr++ = (ushort)(baseVertex + i + 1);
				batch.IndexCount += 3;
			}
			batch.VertexBuffer.VertexCount += numVertices;
		}

		public static void DrawTriangleStrip(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			if (Blending == Blending.Glow) {
				Blending = Blending.Default;
				DrawTriangleStrip(texture1, texture2, vertices, numVertices);
				Blending = Blending.Glow;
			}
			var batch = DrawTrianglesHelper(texture1, texture2, vertices, numVertices);
			var iptr = &batch.Indices[batch.IndexCount];
			var vertex = batch.VertexBuffer.VertexCount;
			for (int i = 0; i < numVertices - 2; i++) {
				*iptr++ = (ushort)vertex;
				*iptr++ = (ushort)(vertex + 1);
				*iptr++ = (ushort)(vertex + 2);
				vertex++;
				batch.IndexCount += 3;
			}
			batch.VertexBuffer.VertexCount += numVertices;
		}

		private static RenderBatch DrawTrianglesHelper(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			var batch = CurrentRenderList.RequestForBatch(texture1, texture2, Blending, Shader, numVertices, (numVertices - 2) * 3);
			Rectangle uvRect1 = (texture1 != null) ? texture1.AtlasUVRect : new Rectangle();
			Rectangle uvRect2 = (texture2 != null) ? texture2.AtlasUVRect : new Rectangle();
			var transform = GetEffectiveTransform();
			var buffer = batch.VertexBuffer;
			var vptr = &buffer.Vertices[buffer.VertexCount];
			for (int i = 0; i < numVertices; i++) {
				Vertex v = vertices[i];
				if (PremultipliedAlphaMode && v.Color.A != 255) {
					v.Color = Color4.PremulAlpha(v.Color);
				}
				v.Pos = transform * v.Pos;
				v.UV1 = uvRect1.A + uvRect1.Size * v.UV1;
				if (texture2 != null) {
					v.UV2 = uvRect2.A + uvRect2.Size * v.UV2;
				}
				*vptr++ = v;
			}
			return batch;
		}

		public static void DrawSprite(ITexture texture, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1)
		{
			if (Blending == Blending.Glow) {
				Blending = Blending.Default;
				DrawSprite(texture, color, position, size, uv0, uv1);
				Blending = Blending.Add;
			}
			var batch = CurrentRenderList.RequestForBatch(texture, null, Blending, Shader, 4, 6);
			if (Renderer.PremultipliedAlphaMode && color.A != 255) {
				color = Color4.PremulAlpha(color);
			}
			if (texture != null) {
				texture.TransformUVCoordinatesToAtlasSpace(ref uv0, ref uv1);
			}
			var buffer = batch.VertexBuffer;
			int i = buffer.VertexCount;
			Int32* ip = (Int32*)&batch.Indices[batch.IndexCount];
			Vertex* vp = &buffer.Vertices[i];
			batch.IndexCount += 6;
			buffer.VertexCount += 4;
			*ip++ = (i << 16) | (i + 1);
			*ip++ = ((i + 2) << 16) | (i + 2);
			*ip++ = ((i + 1) << 16) | (i + 3);
			float x0 = position.X;
			float y0 = position.Y;
			float x1 = position.X + size.X;
			float y1 = position.Y + size.Y;
			var matrix = GetEffectiveTransform();
			float x0ux = x0 * matrix.U.X;
			float x0uy = x0 * matrix.U.Y;
			float y0vx = y0 * matrix.V.X;
			float y0vy = y0 * matrix.V.Y;
			float x1ux = x1 * matrix.U.X;
			float x1uy = x1 * matrix.U.Y;
			float y1vx = y1 * matrix.V.X;
			float y1vy = y1 * matrix.V.Y;
			vp->Pos.X = x0ux + y0vx + matrix.T.X;
			vp->Pos.Y = x0uy + y0vy + matrix.T.Y;
			vp->Color = color;
			vp->UV1 = uv0;
			vp++;
			vp->Pos.X = x1ux + y0vx + matrix.T.X;
			vp->Pos.Y = x1uy + y0vy + matrix.T.Y;
			vp->Color = color;
			vp->UV1.X = uv1.X;
			vp->UV1.Y = uv0.Y;
			vp++;
			vp->Pos.X = x0ux + y1vx + matrix.T.X;
			vp->Pos.Y = x0uy + y1vy + matrix.T.Y;
			vp->Color = color;
			vp->UV1.X = uv0.X;
			vp->UV1.Y = uv1.Y;
			vp++;
			vp->Pos.X = x1ux + y1vx + matrix.T.X;
			vp->Pos.Y = x1uy + y1vy + matrix.T.Y;
			vp->Color = color;
			vp->UV1 = uv1;
		}

		private static Matrix32 GetEffectiveTransform()
		{
			if (Transform2Active) {
				return Transform1 * Transform2;
			} else {
				return Transform1;
			}
		}

		public static void ClearRenderTarget(float r, float g, float b, float a)
		{
			PlatformRenderer.ClearRenderTarget(r, g, b, a);
		}
	}
}
