using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
#if !UNITY
using System.Runtime.InteropServices;
#endif

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
		[ProtoEnum]
		Glow,
	}

	public struct Viewport
	{
		public int X;
		public int Y;
		public int Width;
		public int Height;

		public Viewport(IntVector2 origin, Size size)
		{
			X = origin.X;
			Y = origin.Y;
			Width = size.Width;
			Height = size.Height;
		}
	}

#if UNITY
	public static partial class Renderer
#else
	public static unsafe partial class Renderer
#endif
	{
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
			Vector2 size = new Vector2(0, fontHeight);
			float width = 0;
			// float scale = fontHeight / font.CharHeight;
			for (int i = 0; i < length; i++) {
				char ch = text[i + start];
				if (ch == '\n') {
					size.Y += fontHeight;
					width = 0;
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
				width += scale * (fontChar.ACWidths.X + kerning);
				width += scale * (fontChar.Width + fontChar.ACWidths.Y);
				size.X = Math.Max(size.X, width);
				prevChar = fontChar;
			}
			return size;
		}

		public static void DrawTextLine(Font font, Vector2 position, string text, Color4 color, float fontHeight, int start, int length)
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
				Vector2 size = new Vector2(scale * fontChar.Width, fontHeight);
				DrawSprite(font.Textures[fontChar.TextureIndex], color, position, size, fontChar.UV0, fontChar.UV1);
				position.X += scale * (fontChar.Width + fontChar.ACWidths.Y);
				prevChar = fontChar;
			}
		}

		public static void DrawSprite(ITexture texture, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1)
		{
			if (blending == Blending.Glow) {
				Blending = Blending.Default;
				DrawSpriteHelper(texture, color, position, size, uv0, uv1);
				Blending = Blending.Glow;
				DrawSpriteHelper(texture, color, position, size, uv0, uv1);
			} else {
				DrawSpriteHelper(texture, color, position, size, uv0, uv1);
			}
		}

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
			int j = currentIndex;
			currentIndex += 6;
			batchIndices[j++] = (ushort)(i + 0);
			batchIndices[j++] = (ushort)(i + 1);
			batchIndices[j++] = (ushort)(i + 2);
			batchIndices[j++] = (ushort)(i + 2);
			batchIndices[j++] = (ushort)(i + 1);
			batchIndices[j++] = (ushort)(i + 3);
			currentVertex += 4;
			batchVertices[i].Pos = transform.TransformVector(position.X, position.Y);
			batchVertices[i].Color = color;
			batchVertices[i].UV1 = uv0;
			i++;
			batchVertices[i].Pos = transform.TransformVector(position.X + size.X, position.Y);
			batchVertices[i].Color = color;
			batchVertices[i].UV1.X = uv1.X;
			batchVertices[i].UV1.Y = uv0.Y;
			i++;
			batchVertices[i].Pos = transform.TransformVector(position.X, position.Y + size.Y);
			batchVertices[i].Color = color;
			batchVertices[i].UV1.X = uv0.X;
			batchVertices[i].UV1.Y = uv1.Y;
			i++;
			batchVertices[i].Pos = transform.TransformVector(position.X + size.X, position.Y + size.Y);
			batchVertices[i].Color = color;
			batchVertices[i].UV1 = uv1;
		}

		public static void DrawTriangleFan(ITexture texture1, Vertex[] vertices, int numVertices)
		{
			DrawTriangleFan(texture1, null, vertices, numVertices);
		}

		public static void DrawTriangleFan(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			if (blending == Lime.Blending.Glow) {
				Blending = Lime.Blending.Default;
				DrawTriangleFanHelper(texture1, texture2, vertices, numVertices);
				Blending = Lime.Blending.Glow;
				DrawTriangleFanHelper(texture1, texture2, vertices, numVertices);
			} else {
				DrawTriangleFanHelper(texture1, texture2, vertices, numVertices);
			}
		}

		private static void DrawTriangleFanHelper(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			int baseVertex = DrawTrianglesHelper(texture1, texture2, vertices, numVertices);
			for (int i = 1; i <= numVertices - 2; i++) {
				batchIndices[currentIndex++] = (ushort)baseVertex;
				batchIndices[currentIndex++] = (ushort)(baseVertex + i);
				batchIndices[currentIndex++] = (ushort)(baseVertex + i + 1);
			}
		}

		public static void DrawTriangleStrip(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			if (blending == Lime.Blending.Glow) {
				Blending = Lime.Blending.Default;
				DrawTriangleStripHelper(texture1, texture2, vertices, numVertices);
				Blending = Lime.Blending.Glow;
				DrawTriangleStripHelper(texture1, texture2, vertices, numVertices);
			} else {
				DrawTriangleStripHelper(texture1, texture2, vertices, numVertices);
			}
		}

		private static void DrawTriangleStripHelper(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			int vertex = DrawTrianglesHelper(texture1, texture2, vertices, numVertices);
			for (int i = 0; i < numVertices - 2; i++) {
				batchIndices[currentIndex++] = (ushort)vertex;
				batchIndices[currentIndex++] = (ushort)(vertex + 1);
				batchIndices[currentIndex++] = (ushort)(vertex + 2);
				vertex++;
			}
		}

		private static int DrawTrianglesHelper(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
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
			Rectangle uvRect1 = (texture1 != null) ? texture1.UVRect : new Rectangle();
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
			return baseVertex;
		}
	}
}
