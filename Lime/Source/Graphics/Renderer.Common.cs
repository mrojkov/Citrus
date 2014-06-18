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
		[ProtoEnum]
		Modulate,
		[ProtoEnum]
		Burn,
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

		public class SpriteList : IDisposable
		{
			public class Item
			{
				public ITexture Texture;
				public Color4 Color;
				public Vector2 UV0;
				public Vector2 UV1;
				public Vector2 Position;
				public Vector2 Size;
				internal Item Next;

				public void Draw(Color4 color)
				{
					Renderer.DrawSprite(Texture, Color * color, Position, Size, UV0, UV1);
				}
			}

			static Item pool;
			Item head;
			Item tail;

			~SpriteList()
			{
				Dispose();
			}

			public Item Add()
			{
				var item = AllocateItem();
				if (head == null) {
					head = item;
					tail = item;
				} else {
					tail.Next = item;
					tail = item;
				}
				return item;
			}

			public Item Add(ITexture texture, Color4 color, Vector2 position, Vector2 size, Vector2 UV0, Vector2 UV1)
			{
				var item = Add();
				item.Texture = texture;
				item.Color = color;
				item.Position = position;
				item.Size = size;
				item.UV0 = UV0;
				item.UV1 = UV1;
				return item;
			}

			public void Render()
			{
				Render(Color4.White);
			}

			public void Render(Color4 color)
			{
				for (var item = head; item != null; item = item.Next) {
					item.Draw(color);
				}
			}

			public void Dispose()
			{
				if (head != null) {
					tail.Next = pool;
					pool = head;
					head = null;
					tail = null;
				}
			}

			private static Item AllocateItem()
			{
				if (pool == null) {
					return new Item();
				} else {
					var item = pool;
					pool = pool.Next;
					item.Next = null;
					return item;
				}
			}
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

		public static void DrawSprite(ITexture texture, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1)
		{
			if (blending == Blending.Glow) {
				Blending = Blending.Default;
				DrawSprite(texture, color, position, size, uv0, uv1);
				Blending = Blending.Glow;
			}
			SetTexture(texture, 0);
			SetTexture(null, 1);
			if (currentVertex >= MaxVertices - 4 || currentIndex >= MaxVertices * 4 - 6) {
				FlushSpriteBatch();
			}
			if (PremultipliedAlphaMode) {
				color = Color4.PremulAlpha(color);
			}
            if (texture != null) {
                texture.TransformUVCoordinatesToAtlasSpace(ref uv0, ref uv1);
            }
			int i = currentVertex;
			int j = currentIndex;
			var matrix = Transform2.IsIdentity() ? Transform1 : Transform1 * Transform2;
			currentIndex += 6;
			batchIndices[j++] = (ushort)(i + 0);
			batchIndices[j++] = (ushort)(i + 1);
			batchIndices[j++] = (ushort)(i + 2);
			batchIndices[j++] = (ushort)(i + 2);
			batchIndices[j++] = (ushort)(i + 1);
			batchIndices[j++] = (ushort)(i + 3);
			currentVertex += 4;
			float x0 = position.X;
			float y0 = position.Y;
			float x1 = position.X + size.X;
			float y1 = position.Y + size.Y;
			float x0ux = x0 * matrix.U.X;
			float x0uy = x0 * matrix.U.Y;
			float y0vx = y0 * matrix.V.X;
			float y0vy = y0 * matrix.V.Y;
			float x1ux = x1 * matrix.U.X;
			float x1uy = x1 * matrix.U.Y;
			float y1vx = y1 * matrix.V.X;
			float y1vy = y1 * matrix.V.Y;
			batchVertices[i].Pos.X = x0ux + y0vx + matrix.T.X;
			batchVertices[i].Pos.Y = x0uy + y0vy + matrix.T.Y;
			batchVertices[i].Color = color;
			batchVertices[i].UV1 = uv0;
			i++;
			batchVertices[i].Pos.X = x1ux + y0vx + matrix.T.X;
			batchVertices[i].Pos.Y = x1uy + y0vy + matrix.T.Y;
			batchVertices[i].Color = color;
			batchVertices[i].UV1.X = uv1.X;
			batchVertices[i].UV1.Y = uv0.Y;
			i++;
			batchVertices[i].Pos.X = x0ux + y1vx + matrix.T.X;
			batchVertices[i].Pos.Y = x0uy + y1vy + matrix.T.Y;
			batchVertices[i].Color = color;
			batchVertices[i].UV1.X = uv0.X;
			batchVertices[i].UV1.Y = uv1.Y;
			i++;
			batchVertices[i].Pos.X = x1ux + y1vx + matrix.T.X;
			batchVertices[i].Pos.Y = x1uy + y1vy + matrix.T.Y;
			batchVertices[i].Color = color;
			batchVertices[i].UV1 = uv1;
		}

		public static void DrawTriangleFan(ITexture texture, Vertex[] vertices, int numVertices)
		{
			DrawTriangleFan(texture, null, vertices, numVertices);
		}

		public static void DrawTriangleFan(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			if (blending == Lime.Blending.Glow) {
				Blending = Lime.Blending.Default;
				DrawTriangleFan(texture1, texture2, vertices, numVertices);
				Blending = Lime.Blending.Glow;
			}
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
				DrawTriangleStrip(texture1, texture2, vertices, numVertices);
				Blending = Lime.Blending.Glow;
			}
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
			SetTexture(texture1, 0);
			SetTexture(texture2, 1);
			if (currentIndex + (numVertices - 2) * 3 >= MaxVertices * 4 || currentVertex + numVertices >= MaxVertices) {
				FlushSpriteBatch();
			}
			if (numVertices < 3 || (numVertices - 2) * 3 > MaxVertices * 4) {
				throw new Lime.Exception("Wrong number of vertices");
			}
			Rectangle uvRect1 = (texture1 != null) ? texture1.AtlasUVRect : new Rectangle();
			Rectangle uvRect2 = (texture2 != null) ? texture2.AtlasUVRect : new Rectangle();
			int baseVertex = currentVertex;
			var transform = Transform2.IsIdentity() ? Transform1 : Transform1 * Transform2;
			for (int i = 0; i < numVertices; i++) {
				Vertex v = vertices[i];
				if (PremultipliedAlphaMode) {
					v.Color = Color4.PremulAlpha(v.Color);
				}
				v.Pos = transform * v.Pos;
				v.UV1 = uvRect1.A + uvRect1.Size * v.UV1;
				if (texture2 != null) {
					v.UV2 = uvRect2.A + uvRect2.Size * v.UV2;
				}
				batchVertices[currentVertex++] = v;
			}
			return baseVertex;
		}
	}
}
