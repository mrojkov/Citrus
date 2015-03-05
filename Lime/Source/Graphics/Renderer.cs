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
		Inherited,
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
		[ProtoEnum]
		Darken
	}

	[ProtoContract]
	public enum ShaderId
	{
		[ProtoEnum]
		None,
		[ProtoEnum]
		Inherited,
		[ProtoEnum]
		Diffuse,
		[ProtoEnum]
		Silhuette,
		[ProtoEnum]
		InversedSilhuette,
		[ProtoEnum]
		Custom,
	}

	public struct WindowRect : IEquatable<WindowRect>
	{
		public int X;
		public int Y;
		public int Width;
		public int Height;

		public static explicit operator IntRectangle(WindowRect r)
		{
			return new IntRectangle(r.X, r.Y, r.X + r.Width, r.Y + r.Height);
		}

		public bool Equals(WindowRect other)
		{
			return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
		}
	}

	public struct Vertex
	{
		public Vector2 Pos;
		public Color4 Color;
		public Vector2 UV1;
		public Vector2 UV2;
	}

	public static class Renderer
	{
		private static Matrix32 transform2 = Matrix32.Identity;
		private static Stack<Matrix44> projectionStack;
		private static WindowRect viewport;
		private static WindowRect scissorRectangle = new WindowRect();
		private static bool scissorTestEnabled = false;
		private static bool Transform2Active;

		public static Blending Blending;
		public static ShaderId Shader;
		public static ShaderProgram CustomShaderProgram;
		public static Matrix32 Transform1;
		public static int RenderCycle { get; private set; }
		public static bool PremultipliedAlphaMode;
		public static int DrawCalls = 0;
		public static readonly RenderList MainRenderList = new RenderList();
		public static RenderList CurrentRenderList;
#if ANDROID
		public static bool AmazonBindTextureWorkaround;
#endif
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
				PlatformRenderer.SetProjectionMatrix(value);
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
			Projection = Matrix44.CreateOrthographicOffCenter(left, right, bottom, top, -50, 50);
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
#if !UNITY
			PremultipliedAlphaMode = true;
#endif
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

		public static void DrawTextLine(
			Font font, Vector2 position, string text, Color4 color, float fontHeight, int start, int length, SpriteList list = null,
			Action<int, Vector2, Vector2> onDrawChar = null)
		{
			FontChar prevChar = null;
			float savedX = position.X;
			for (int i = 0; i < length; i++) {
				char ch = text[i + start];
				if (ch == '\n') {
					if (onDrawChar != null) {
						onDrawChar(i, position, Vector2.Down * fontHeight);
					}
					position.X = savedX;
					position.Y += fontHeight;
					continue;
				}
				FontChar fontChar = font.Chars[ch];
				if (fontChar == FontChar.Null) {
					if (onDrawChar != null) {
						onDrawChar(i, position, Vector2.Down * fontHeight);
					}
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
				var texture = font.Textures[fontChar.TextureIndex];
				var size = new Vector2(scale * fontChar.Width, fontHeight);
				if (onDrawChar != null) {
					onDrawChar(i, position, size);
				}
				if (list == null) {
					DrawSprite(texture, color, position, size, fontChar.UV0, fontChar.UV1);
				} else {
					var item = new SpriteList.Item() {
						Texture = texture,
						Color = color,
						Position = position,
						Size = size,
						UV0 = fontChar.UV0,
						UV1 = fontChar.UV1
					};
					list.Add(item);
				}
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
				Blending = Blending.Alpha;
				DrawTriangleFan(texture1, texture2, vertices, numVertices);
				Blending = Blending.Glow;
			}
			if (Blending == Blending.Darken) {
				Blending = Blending.Alpha;
				DrawTriangleFan(texture1, texture2, vertices, numVertices);
				Blending = Blending.Darken;
			}
			var batch = DrawTrianglesHelper(texture1, texture2, vertices, numVertices);
			var baseVertex = batch.LastVertex;
			int j = batch.LastIndex;
			var indices = batch.Mesh.Indices;
			for (int i = 1; i <= numVertices - 2; i++) {
				indices[j++] = (ushort)(baseVertex);
				indices[j++] = (ushort)(baseVertex + i);
				indices[j++] = (ushort)(baseVertex + i + 1);
				batch.LastIndex += 3;
			}
			batch.Mesh.IndicesDirty = true;
			batch.LastVertex += numVertices;
		}

		public static void DrawTriangleStrip(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			if (Blending == Blending.Glow) {
				Blending = Blending.Alpha;
				DrawTriangleStrip(texture1, texture2, vertices, numVertices);
				Blending = Blending.Glow;
			}
			if (Blending == Blending.Darken) {
				Blending = Blending.Alpha;
				DrawTriangleStrip(texture1, texture2, vertices, numVertices);
				Blending = Blending.Darken;
			}
			var batch = DrawTrianglesHelper(texture1, texture2, vertices, numVertices);
			var vertex = batch.LastVertex;
			int j = batch.LastIndex;
			var indices = batch.Mesh.Indices;
			for (int i = 0; i < numVertices - 2; i++) {
				indices[j++] = (ushort)vertex;
				indices[j++] = (ushort)(vertex + 1);
				indices[j++] = (ushort)(vertex + 2);
				vertex++;
				batch.LastIndex += 3;
			}
			batch.Mesh.IndicesDirty = true;
			batch.LastVertex += numVertices;
		}

		private static RenderBatch DrawTrianglesHelper(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			var batch = CurrentRenderList.GetBatch(texture1, texture2, Blending, Shader, CustomShaderProgram, numVertices, (numVertices - 2) * 3);
			Rectangle uvRect1 = (texture1 != null) ? texture1.AtlasUVRect : new Rectangle();
			Rectangle uvRect2 = (texture2 != null) ? texture2.AtlasUVRect : new Rectangle();
			var transform = GetEffectiveTransform();
			var mesh = batch.Mesh;
			mesh.DirtyAttributes |= Mesh.Attributes.VertexColorUV12 | (texture2 != null ? Mesh.Attributes.UV2 : Mesh.Attributes.None);
			int j = batch.LastVertex;
			for (int i = 0; i < numVertices; i++) {
				var v = vertices[i];
				if (PremultipliedAlphaMode && v.Color.A != 255) {
					v.Color = Color4.PremulAlpha(v.Color);
				}
				mesh.Colors[j] = v.Color;
				mesh.Vertices[j] = transform * v.Pos;
				mesh.UV1[j] = uvRect1.A + uvRect1.Size * v.UV1;
				if (texture2 != null) {
					mesh.UV2[j] = uvRect2.A + uvRect2.Size * v.UV2;
				}
				j++;
			}
			return batch;
		}

		public static void DrawSprite(ITexture texture, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1)
		{
			if (Blending == Blending.Glow) {
				Blending = Blending.Alpha;
				DrawSprite(texture, color, position, size, uv0, uv1);
				Blending = Blending.Glow;
			}
			if (Blending == Blending.Darken) {
				Blending = Blending.Alpha;
				DrawSprite(texture, color, position, size, uv0, uv1);
				Blending = Blending.Darken;
			}
			var batch = CurrentRenderList.GetBatch(texture, null, Blending, Shader, CustomShaderProgram, 4, 6);
			if (Renderer.PremultipliedAlphaMode && color.A != 255) {
				color = Color4.PremulAlpha(color);
			}
			if (texture != null) {
				texture.TransformUVCoordinatesToAtlasSpace(ref uv0, ref uv1);
			}
			var mesh = batch.Mesh;
			mesh.DirtyAttributes |= Mesh.Attributes.VertexColorUV12;
			mesh.IndicesDirty = true;
			int bv = batch.LastVertex;
			int bi = batch.LastIndex;
			batch.LastIndex += 6;
			batch.LastVertex += 4;
			var indices = mesh.Indices;
			indices[bi++] = (ushort)(bv + 1);
			indices[bi++] = (ushort)bv;
			indices[bi++] = (ushort)(bv + 2);
			indices[bi++] = (ushort)(bv + 2);
			indices[bi++] = (ushort)(bv + 3);
			indices[bi++] = (ushort)(bv + 1);
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
			var v = mesh.Vertices;
			v[bv + 0] = new Vector2() { X = x0ux + y0vx + matrix.T.X, Y = x0uy + y0vy + matrix.T.Y };
			v[bv + 1] = new Vector2() { X = x1ux + y0vx + matrix.T.X, Y = x1uy + y0vy + matrix.T.Y };
			v[bv + 2] = new Vector2() { X = x0ux + y1vx + matrix.T.X, Y = x0uy + y1vy + matrix.T.Y };
			v[bv + 3] = new Vector2() { X = x1ux + y1vx + matrix.T.X, Y = x1uy + y1vy + matrix.T.Y };
			var c = mesh.Colors;
			c[bv + 0] = color;
			c[bv + 1] = color;
			c[bv + 2] = color;
			c[bv + 3] = color;
			var uv = mesh.UV1;
			uv[bv + 0] = uv0;
			uv[bv + 1] = new Vector2() { X = uv1.X, Y = uv0.Y };
			uv[bv + 2] = new Vector2() { X = uv0.X, Y = uv1.Y };
			uv[bv + 3] = uv1;
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
