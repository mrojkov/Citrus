using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Lime
{
	public enum Blending
	{
		None,
		Inherited,
		Alpha,
		Add,
		Glow,
		Modulate,
		Burn,
		Darken,
		Opaque
	}

	public enum ShaderId
	{
		None,
		Inherited,
		Diffuse,
		Silhuette,
		InversedSilhuette,

		[TangerineIgnore]
		Custom,
	}

	public enum LineCap
	{
		Butt,
		Round,
		Square
	}

	public enum CullMode
	{
		None,
		CullClockwise,
		CullCounterClockwise
	}

	public enum ClearTarget
	{
		None = 0,
		ColorBuffer = 1 << 0,
		DepthBuffer = 1 << 1,
		All = ColorBuffer | DepthBuffer
	}

	public struct WindowRect : IEquatable<WindowRect>
	{
		public int X;
		public int Y;
		public int Width;
		public int Height;

		public IntVector2 Origin
		{
			get { return new IntVector2(X, Y); }
			set { X = value.X; Y = value.Y; }
		}

		public IntVector2 Size
		{
			get { return new IntVector2(Width, Height); }
			set { Width = value.X; Height = value.Y; }
		}

		public static explicit operator IntRectangle(WindowRect r)
		{
			return new IntRectangle(r.X, r.Y, r.X + r.Width, r.Y + r.Height);
		}

		public bool Equals(WindowRect other)
		{
			return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 32)]
	public struct Vertex
	{
		public Vector2 Pos;
		public Color4 Color;
		public Vector2 UV1;
		public Vector2 UV2;
	}

	public static class Renderer
	{
		private static Matrix44 world = Matrix44.Identity;
		private static Matrix44 view = Matrix44.Identity;
		private static Matrix44 proj = Matrix44.Identity;
		private static Matrix44 worldView = Matrix44.Identity;
		private static Matrix44 worldViewProj = Matrix44.Identity;
		private static Matrix44 viewProj = Matrix44.Identity;

		private static bool worldViewDirty = true;
		private static bool worldViewProjDirty = true;
		private static bool viewProjDirty = true;

		private static Matrix32 transform2 = Matrix32.Identity;
		private static WindowRect viewport;
		private static WindowRect scissorRectangle = new WindowRect();
		private static bool scissorTestEnabled = false;
		private static bool zTestEnabled = false;
		private static bool zWriteEnabled = true;
		private static bool Transform2Active;
		private static CullMode cullMode;

		public static Blending Blending;
		public static ShaderId Shader;
		public static ShaderProgram CustomShaderProgram;
		public static Matrix32 Transform1;
		public static int RenderCycle { get; private set; }
		public static bool PremultipliedAlphaMode;
		public static int DrawCalls = 0;
		public static int PolyCount3d = 0;
		public static readonly RenderList MainRenderList = new RenderList();
		public static RenderList CurrentRenderList;
#if ANDROID
		public static bool AmazonBindTextureWorkaround;
#endif
		public static Matrix44 World
		{
			get { return world; }
			set
			{
				world = value;
				worldViewDirty = worldViewProjDirty = true;
				PlatformRenderer.InvalidateShaderProgram();
			}
		}

		public static Matrix44 View
		{
			get { return view; }
			set
			{
				view = value;
				viewProjDirty = worldViewDirty = worldViewProjDirty = true;
				PlatformRenderer.InvalidateShaderProgram();
			}
		}

		public static Matrix44 Projection
		{
			get { return proj; }
			set
			{
				proj = value;
				viewProjDirty = worldViewProjDirty = true;
				PlatformRenderer.InvalidateShaderProgram();
			}
		}

		public static Matrix44 WorldView
		{
			get
			{
				if (worldViewDirty) {
					worldViewDirty = false;
					worldView = world * view;
				}
				return worldView;
			}
		}

		public static Matrix44 ViewProjection
		{
			get
			{
				if (viewProjDirty) {
					viewProjDirty = false;
					viewProj = view * proj;
				}
				return viewProj;
			}
		}

		public static Matrix44 WorldViewProjection
		{
			get
			{
				if (worldViewProjDirty) {
					worldViewProjDirty = false;
					worldViewProj = world * ViewProjection;
				}
				return worldViewProj;
			}
		}

		public static Matrix32 Transform2
		{
			get { return transform2; }
			set
			{
				transform2 = value;
				Transform2Active = !value.IsIdentity();
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

		public static bool ZTestEnabled
		{
			get { return zTestEnabled; }
			set
			{
				if (zTestEnabled != value) {
					Flush();
					zTestEnabled = value;
					PlatformRenderer.EnableZTest(value);
				}
			}
		}

		public static bool ZWriteEnabled
		{
			get { return zWriteEnabled; }
			set
			{
				if (zWriteEnabled != value) {
					Flush();
					zWriteEnabled = value;
					PlatformRenderer.EnableZWrite(value);
				}
			}
		}

		public static CullMode CullMode
		{
			get { return cullMode; }
			set
			{
				if (cullMode != value) {
					cullMode = value;
					Flush();
					PlatformRenderer.SetCullMode(cullMode);
				}
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

		public static void BeginFrame()
		{
			PlatformRenderer.BeginFrame();
			DrawCalls = 0;
			PolyCount3d = 0;
			Blending = Blending.None;
			Shader = ShaderId.None;
			CullMode = CullMode.None;
			Transform1 = Matrix32.Identity;
			Transform2 = Matrix32.Identity;
			World = Matrix44.Identity;
			View = Matrix44.Identity;
			CurrentRenderList = MainRenderList;
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

		public static void DrawTextLine(float x, float y, string text, float fontHeight, Color4 color, float letterSpacing)
		{
			DrawTextLine(new Vector2(x, y), text, fontHeight, color, letterSpacing);
		}

		public static void DrawTextLine(Vector2 position, string text, float fontHeight, Color4 color, float letterSpacing)
		{
			DrawTextLine(FontPool.Instance[null], position, text, fontHeight, color, letterSpacing);
		}

		public static void DrawTextLine(IFont font, Vector2 position, string text, float fontHeight, Color4 color, float letterSpacing)
		{
			DrawTextLine(font, position, text, color, fontHeight, 0, text.Length, letterSpacing);
		}

		public static Vector2 MeasureTextLine(IFont font, string text, float fontHeight, float letterSpacing)
		{
			return MeasureTextLine(font, text, fontHeight, 0, text.Length, letterSpacing);
		}

		public static Vector2 MeasureTextLine(IFont font, string text, float fontHeight, int start, int length, float letterSpacing)
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
					prevChar = null;
					continue;
				}
				var fontChar = font.Chars.Get(ch, fontHeight);
				if (fontChar == FontChar.Null) {
					continue;
				}
				float scale = fontChar.Height != 0.0f ? fontHeight / fontChar.Height : 0.0f;
				width += scale * (fontChar.ACWidths.X + fontChar.Kerning(prevChar));
				width += scale * (fontChar.Width + fontChar.ACWidths.Y);
				width += scale * letterSpacing;
				size.X = Math.Max(size.X, width);
				prevChar = fontChar;
			}
			return size;
		}

		public static void DrawTextLine(
			IFont font, Vector2 position, string text, Color4 color, float fontHeight, int start, int length, float letterSpacing,
			SpriteList list = null, Action<int, Vector2, Vector2> onDrawChar = null, int tag = -1)
		{
			int j = 0;
			if (list != null) {
				for (int i = 0; i < length; i++) {
					char ch = text[i + start];
					if (ch != '\n' && ch != '\r' && font.Chars.Get(ch, fontHeight) != FontChar.Null)
						++j;
				}
			}
			// Use array instead of list to reduce memory consumption.
			var chars = new SpriteList.CharDef[j];
			j = 0;

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
					prevChar = null;
					continue;
				} else if (ch == '\r') {
					continue;
				}
				FontChar fontChar = font.Chars.Get(ch, fontHeight);
				if (fontChar == FontChar.Null) {
					if (onDrawChar != null) {
						onDrawChar(i, position, Vector2.Down * fontHeight);
					}
					continue;
				}
				float scale = fontChar.Height != 0.0f ? fontHeight / fontChar.Height : 0.0f;
				var xDelta = scale * (fontChar.ACWidths.X + fontChar.Kerning(prevChar) + letterSpacing);
				position.X += xDelta;
				var size = new Vector2(scale * fontChar.Width, fontHeight - fontChar.VerticalOffset);
				Vector2 roundPos;
				if (font.RoundCoordinates) {
					roundPos = new Vector2(position.X.Round(), position.Y.Round() + fontChar.VerticalOffset);
					if (onDrawChar != null) {
						onDrawChar(i, new Vector2((position.X - xDelta).Round(), position.Y.Round()), size);
					}
				} else {
					roundPos = new Vector2(position.X, position.Y + fontChar.VerticalOffset);
					if (onDrawChar != null) {
						onDrawChar(i, new Vector2(position.X - xDelta, position.Y), size);
					}
				}
				if (list == null) {
					DrawSprite(fontChar.Texture, color, roundPos, size, fontChar.UV0, fontChar.UV1);
				} else {
					chars[j].FontChar = fontChar;
					chars[j].Position = roundPos;
					++j;
				}
				position.X += scale * (fontChar.Width + fontChar.ACWidths.Y);
				prevChar = fontChar;
			}
			if (list != null)
				list.Add(font, color, fontHeight, chars, tag);
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
			var indices = batch.IndexBuffer.Data;
			for (int i = 1; i <= numVertices - 2; i++) {
				indices[j++] = (ushort)(baseVertex);
				indices[j++] = (ushort)(baseVertex + i);
				indices[j++] = (ushort)(baseVertex + i + 1);
				batch.LastIndex += 3;
			}
			batch.IndexBuffer.Dirty = true;
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
			var indices = batch.IndexBuffer.Data;
			for (int i = 0; i < numVertices - 2; i++) {
				indices[j++] = (ushort)vertex;
				indices[j++] = (ushort)(vertex + 1);
				indices[j++] = (ushort)(vertex + 2);
				vertex++;
				batch.LastIndex += 3;
			}
			batch.IndexBuffer.Dirty = true;
			batch.LastVertex += numVertices;
		}

		private static RenderBatch DrawTrianglesHelper(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			var batch = CurrentRenderList.GetBatch(texture1, texture2, Blending, Shader, CustomShaderProgram, numVertices, (numVertices - 2) * 3);
			var transform = GetEffectiveTransform();
			var vd = batch.VertexBuffer.Data;
			batch.VertexBuffer.Dirty = true;
			int j = batch.LastVertex;
			for (int i = 0; i < numVertices; i++) {
				var v = vertices[i];
				if (PremultipliedAlphaMode && v.Color.A != 255) {
					v.Color = Color4.PremulAlpha(v.Color);
				}
				vd[j].Color = v.Color;
				vd[j].Pos = transform * v.Pos;
				if (texture1 != null) {
					var uv1 = v.UV1;
					texture1.TransformUVCoordinatesToAtlasSpace(ref uv1);
					vd[j].UV1 = uv1;
				}
				if (texture2 != null) {
					var uv2 = v.UV2;
					texture2.TransformUVCoordinatesToAtlasSpace(ref uv2);
					vd[j].UV2 = uv2;
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
			if (PremultipliedAlphaMode && color.A != 255) {
				color = Color4.PremulAlpha(color);
			}
			if (texture != null) {
				texture.TransformUVCoordinatesToAtlasSpace(ref uv0);
				texture.TransformUVCoordinatesToAtlasSpace(ref uv1);
			}
			batch.VertexBuffer.Dirty = true;
			batch.IndexBuffer.Dirty = true;
			int v = batch.LastVertex;
			int i = batch.LastIndex;
			batch.LastIndex += 6;
			batch.LastVertex += 4;
			var indices = batch.IndexBuffer.Data;
			indices[i++] = (ushort)(v + 1);
			indices[i++] = (ushort)v;
			indices[i++] = (ushort)(v + 2);
			indices[i++] = (ushort)(v + 2);
			indices[i++] = (ushort)(v + 3);
			indices[i++] = (ushort)(v + 1);
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
			var vertices = batch.VertexBuffer.Data;
			vertices[v].Pos = new Vector2 { X = x0ux + y0vx + matrix.T.X, Y = x0uy + y0vy + matrix.T.Y };
			vertices[v].Color = color;
			vertices[v++].UV1 = uv0;
			vertices[v].Pos = new Vector2 { X = x1ux + y0vx + matrix.T.X, Y = x1uy + y0vy + matrix.T.Y };
			vertices[v].Color = color;
			vertices[v++].UV1 = new Vector2 { X = uv1.X, Y = uv0.Y };
			vertices[v].Pos = new Vector2 { X = x0ux + y1vx + matrix.T.X, Y = x0uy + y1vy + matrix.T.Y };
			vertices[v].Color = color;
			vertices[v++].UV1 = new Vector2 { X = uv0.X, Y = uv1.Y };
			vertices[v].Pos = new Vector2 { X = x1ux + y1vx + matrix.T.X, Y = x1uy + y1vy + matrix.T.Y };
			vertices[v].Color = color;
			vertices[v].UV1 = uv1;
		}

		private static Matrix32 GetEffectiveTransform()
		{
			if (Transform2Active) {
				return Transform1 * Transform2;
			} else {
				return Transform1;
			}
		}

		public static void Clear(float r, float g, float b, float a)
		{
			Clear(ClearTarget.All, r, g, b, a);
		}

		public static void Clear(ClearTarget targets)
		{
			Clear(targets, 0, 0, 0, 0);
		}

		public static void Clear(ClearTarget targets, float r, float g, float b, float a)
		{
			PlatformRenderer.Clear(targets, r, g, b, a);
		}

		private static Sprite[] batchedSprites = new Sprite[20];
		private static Sprite sentinelSprite = new Sprite();

		public static void DrawSpriteList(List<Sprite> spriteList, Color4 color)
		{
			if (Blending == Blending.Glow) {
				Blending = Blending.Alpha;
				DrawSpriteList(spriteList, color);
				Blending = Blending.Glow;
			}
			if (Blending == Blending.Darken) {
				Blending = Blending.Alpha;
				DrawSpriteList(spriteList, color);
				Blending = Blending.Darken;
			}
			var matrix = GetEffectiveTransform();
			int batchLength = 0;
			var clipRect = scissorTestEnabled ? CalcLocalScissorAABB(matrix) : new Rectangle();
			for (int t = 0; t <= spriteList.Count; t++) {
				var s = (t == spriteList.Count) ? sentinelSprite : spriteList[t];
				if (scissorTestEnabled && s != sentinelSprite) {
					if (s.Position.X + s.Size.X < clipRect.A.X ||
						s.Position.X > clipRect.B.X ||
						s.Position.Y + s.Size.Y < clipRect.A.Y ||
						s.Position.Y > clipRect.B.Y) {
						continue;
					}
				}
				if (
					batchLength == 0 ||
					batchLength < batchedSprites.Length &&
					s.Texture1 == batchedSprites[0].Texture1 &&
					s.Texture2 == batchedSprites[0].Texture2 &&
					s.ShaderProgram == batchedSprites[0].ShaderProgram
				) {
					batchedSprites[batchLength++] = s;
					continue;
				}
				bool customShader = batchedSprites[0].ShaderProgram != null;
				var batch = CurrentRenderList.GetBatch(
					batchedSprites[0].Texture1, batchedSprites[0].Texture2, Blending, customShader ? ShaderId.Custom : Shader,
					customShader ? batchedSprites[0].ShaderProgram : CustomShaderProgram, 4 * batchLength, 6 * batchLength);
				int v = batch.LastVertex;
				int i = batch.LastIndex;
				batch.VertexBuffer.Dirty = true;
				batch.IndexBuffer.Dirty = true;
				var indices = batch.IndexBuffer.Data;
				var vertices = batch.VertexBuffer.Data;
				var uvRect = batchedSprites[0].Texture1.AtlasUVRect;
				for (int j = 0; j < batchLength; j++) {
					var sprite = batchedSprites[j];
					var effectiveColor = color * sprite.Color;
					if (Renderer.PremultipliedAlphaMode && color.A != 255) {
						effectiveColor = Color4.PremulAlpha(effectiveColor);
					}
					indices[i++] = (ushort)(v + 1);
					indices[i++] = (ushort)v;
					indices[i++] = (ushort)(v + 2);
					indices[i++] = (ushort)(v + 2);
					indices[i++] = (ushort)(v + 3);
					indices[i++] = (ushort)(v + 1);
					var uv0 = sprite.UV0;
					var uv1 = sprite.UV1;
					uv0.X = uvRect.A.X + (uvRect.B.X - uvRect.A.X) * uv0.X;
					uv0.Y = uvRect.A.Y + (uvRect.B.Y - uvRect.A.Y) * uv0.Y;
					uv1.X = uvRect.A.X + (uvRect.B.X - uvRect.A.X) * uv1.X;
					uv1.Y = uvRect.A.Y + (uvRect.B.Y - uvRect.A.Y) * uv1.Y;
					float x0 = sprite.Position.X;
					float y0 = sprite.Position.Y;
					float x1 = sprite.Position.X + sprite.Size.X;
					float y1 = sprite.Position.Y + sprite.Size.Y;
					float x0ux = x0 * matrix.U.X;
					float x0uy = x0 * matrix.U.Y;
					float y0vx = y0 * matrix.V.X;
					float y0vy = y0 * matrix.V.Y;
					float x1ux = x1 * matrix.U.X;
					float x1uy = x1 * matrix.U.Y;
					float y1vx = y1 * matrix.V.X;
					float y1vy = y1 * matrix.V.Y;
					vertices[v].Pos = new Vector2 { X = x0ux + y0vx + matrix.T.X, Y = x0uy + y0vy + matrix.T.Y };
					vertices[v].Color = effectiveColor;
					vertices[v++].UV1 = uv0;
					vertices[v].Pos = new Vector2 { X = x1ux + y0vx + matrix.T.X, Y = x1uy + y0vy + matrix.T.Y };
					vertices[v].Color = effectiveColor;
					vertices[v++].UV1 = new Vector2 { X = uv1.X, Y = uv0.Y };
					vertices[v].Pos = new Vector2 { X = x0ux + y1vx + matrix.T.X, Y = x0uy + y1vy + matrix.T.Y };
					vertices[v].Color = effectiveColor;
					vertices[v++].UV1 = new Vector2 { X = uv0.X, Y = uv1.Y };
					vertices[v].Pos = new Vector2 { X = x1ux + y1vx + matrix.T.X, Y = x1uy + y1vy + matrix.T.Y };
					vertices[v].Color = effectiveColor;
					vertices[v++].UV1 = uv1;
				}
				batch.LastIndex = i;
				batch.LastVertex = v;
				batchLength = 1;
				batchedSprites[0] = s;
			}
		}

		private static Rectangle CalcLocalScissorAABB(Matrix32 transform)
		{
			// Get the scissor rectangle in 0,0 - 1,1 coordinate space
			var vp = new Rectangle {
				A = new Vector2(viewport.X, viewport.Y),
				B = new Vector2(viewport.X + viewport.Width, viewport.Y + viewport.Height)
			};
			var r = (Rectangle)(IntRectangle)scissorRectangle;
			var scissorRect = new Rectangle {
				A = (r.A - vp.A) / vp.Size,
				B = (r.B - vp.A) / vp.Size
			};
			// Transform it to the normalized OpenGL space
			scissorRect.A = scissorRect.A * 2 - Vector2.One;
			scissorRect.B = scissorRect.B * 2 - Vector2.One;
			// Get the unprojected coordinates
			var invProjection = Projection.CalcInverted();
			var v0 = invProjection.ProjectVector(scissorRect.A);
			var v1 = invProjection.ProjectVector(new Vector2(scissorRect.B.X, scissorRect.A.Y));
			var v2 = invProjection.ProjectVector(scissorRect.B);
			var v3 = invProjection.ProjectVector(new Vector2(scissorRect.A.X, scissorRect.B.Y));
			// Get coordinates in the widget space
			var invTransform = transform.CalcInversed();
			v0 = invTransform.TransformVector(v0);
			v1 = invTransform.TransformVector(v1);
			v2 = invTransform.TransformVector(v2);
			v3 = invTransform.TransformVector(v3);
			var aabb = new Rectangle { A = v0, B = v0 }.
				IncludingPoint(v1).
				IncludingPoint(v2).
				IncludingPoint(v3);
			return aabb;
		}

		public static void DrawLine(float x0, float y0, float x1, float y1, Color4 color, float thickness = 1, LineCap cap = LineCap.Butt)
		{
			DrawLine(new Vector2(x0, y0), new Vector2(x1, y1), color, thickness, cap);
		}

		static Vertex[] v = new Vertex[4];

		public static void DrawLine(Vector2 a, Vector2 b, Color4 color, float thickness = 1, LineCap cap = LineCap.Butt)
		{
			if (cap == LineCap.Round) {
				throw new NotImplementedException();
			}
			var d = (b - a).Normalized * thickness * 0.5f;
			Vector2 n = GetVectorNormal(d);
			v[0] = new Vertex { Pos = a - n, Color = color };
			v[1] = new Vertex { Pos = b - n, Color = color };
			v[2] = new Vertex { Pos = b + n, Color = color };
			v[3] = new Vertex { Pos = a + n, Color = color };
			if (cap == LineCap.Square) {
				v[0].Pos -= d;
				v[1].Pos += d;
				v[2].Pos += d;
				v[3].Pos -= d;
			}
			Renderer.DrawTriangleFan(null, null, v, 4);
		}

		static Vector2 GetVectorNormal(Vector2 v)
		{
			return new Vector2(-v.Y, v.X);
		}

		public static void DrawRect(Vector2 a, Vector2 b, Color4 color)
		{
			v[0] = new Vertex { Pos = a, Color = color };
			v[1] = new Vertex { Pos = new Vector2(b.X, a.Y), Color = color };
			v[2] = new Vertex { Pos = b, Color = color };
			v[3] = new Vertex { Pos = new Vector2(a.X, b.Y), Color = color };
			Renderer.DrawTriangleFan(null, null, v, 4);
		}

		public static void DrawRect(float x0, float y0, float x1, float y1, Color4 color)
		{
			DrawRect(new Vector2(x0, y0), new Vector2(x1, y1), color);
		}

		/// <summary>
		/// Draws the rectangle outline inscribed within the given bounds.
		/// </summary>
		public static void DrawRectOutline(Vector2 a, Vector2 b, Color4 color, float thickness = 1)
		{
			float t;
			if (b.X < a.X) {
				t = b.X;
				b.X = a.X;
				a.X = t;
			}
			if (b.Y < a.Y) {
				t = b.Y;
				b.Y = a.Y;
				a.Y = t;
			}
			var d = new Vector2(thickness / 2, thickness / 2);
			a += d;
			b -= d;
			DrawLine(a.X, a.Y, b.X, a.Y, color, thickness, LineCap.Square);
			DrawLine(b.X, a.Y, b.X, b.Y, color, thickness, LineCap.Square);
			DrawLine(b.X, b.Y, a.X, b.Y, color, thickness, LineCap.Square);
			DrawLine(a.X, b.Y, a.X, a.Y, color, thickness, LineCap.Square);
		}

		/// <summary>
		/// Draws the quadrangle outline inscribed within the given bounds.
		/// </summary>
		public static void DrawQuadrangleOutline(Quadrangle q, Color4 color, float thickness = 1)
		{
			for (int i = 0; i < 4; i++) {
				DrawLine(q[i].X, q[i].Y, q[(i + 1) % 4].X, q[(i + 1) % 4].Y, color, thickness, LineCap.Square);
			}
		}

		/// <summary>
		/// Draws the quadrangle
		/// </summary>
		public static void DrawQuadrangle(Quadrangle q, Color4 color, float thickness = 1)
		{
			for (int i = 0; i < 4; i++) {
				v[i] = new Vertex { Pos = q[i], Color = color };
			}
			Renderer.DrawTriangleFan(null, null, v, 4);
		}

		/// <summary>
		/// Draws the rectangle outline inscribed within the given bounds.
		/// </summary>
		public static void DrawRectOutline(float x0, float y0, float x1, float y1, Color4 color, float thickness = 1)
		{
			DrawRectOutline(new Vector2(x0, y0), new Vector2(x1, y1), color, thickness);
		}

		public static void DrawVerticalGradientRect(Vector2 a, Vector2 b, ColorGradient gradient)
		{
			v[0] = new Vertex { Pos = a, Color = gradient.A };
			v[1] = new Vertex { Pos = new Vector2(b.X, a.Y), Color = gradient.A };
			v[2] = new Vertex { Pos = b, Color = gradient.B };
			v[3] = new Vertex { Pos = new Vector2(a.X, b.Y), Color = gradient.B };
			Renderer.DrawTriangleFan(null, null, v, 4);
		}

		public static void DrawVerticalGradientRect(float x0, float y0, float x1, float y1, ColorGradient gradient)
		{
			DrawVerticalGradientRect(new Vector2(x0, y0), new Vector2(x1, y1), gradient);
		}

		public static void DrawVerticalGradientRect(Vector2 a, Vector2 b, Color4 topColor, Color4 bottomColor)
		{
			DrawVerticalGradientRect(a, b, new ColorGradient(topColor, bottomColor));
		}

		public static void DrawHorizontalGradientRect(Vector2 a, Vector2 b, ColorGradient gradient)
		{
			v[0] = new Vertex { Pos = a, Color = gradient.A };
			v[1] = new Vertex { Pos = new Vector2(b.X, a.Y), Color = gradient.B };
			v[2] = new Vertex { Pos = b, Color = gradient.B };
			v[3] = new Vertex { Pos = new Vector2(a.X, b.Y), Color = gradient.A };
			Renderer.DrawTriangleFan(null, null, v, 4);
		}

		public static void DrawHorizontalGradientRect(float x0, float y0, float x1, float y1, ColorGradient gradient)
		{
			DrawVerticalGradientRect(new Vector2(x0, y0), new Vector2(x1, y1), gradient);
		}

		public static void DrawHorizontalGradientRect(Vector2 a, Vector2 b, Color4 topColor, Color4 bottomColor)
		{
			DrawVerticalGradientRect(a, b, new ColorGradient(topColor, bottomColor));
		}

		public static Matrix44 FixupWVP(Matrix44 projection)
		{
			return PlatformRenderer.FixupWVP(projection);
		}

		public static void DrawVerticalGradientRect(float x0, float y0, float x1, float y1, Color4 topColor, Color4 bottomColor)
		{
			DrawVerticalGradientRect(new Vector2(x0, y0), new Vector2(x1, y1), topColor, bottomColor);
		}

		public static void DrawRound(Vector2 center, float radius, int numSegments, Color4 innerColor, Color4 outerColor)
		{
			if (v.Length < numSegments + 1) {
				v = new Vertex[numSegments + 1];
			}
			v[0] = new Vertex { Pos = center, Color = innerColor };
			for (int i = 0; i < numSegments; i++) {
				v[i + 1].Pos = Vector2.CosSin(i * Mathf.TwoPi / (numSegments - 1)) * radius + center;
				v[i + 1].Color = outerColor;
			}
			DrawTriangleFan(null, v, numSegments + 1);
		}

		public static void DrawRound(Vector2 center, float radius, int numSegments, Color4 color)
		{
			DrawRound(center, radius, numSegments, color, color);
		}

		public static void DrawDashedLine(ITexture texture, Vector2 a, Vector2 b, Color4 color, float size = 8)
		{
			var dir = (b - a).Normalized;
			var l = (b - a).Length;
			var n = new Vector2(-dir.Y, dir.X) * size / 2;
			Vertex[] vertices = {
				new Vertex {
					Pos = a - n,
					UV1 = Vector2.Zero,
					Color = color,
				},
				new Vertex {
					Pos = a + n,
					UV1 = new Vector2(0, 1),
					Color = color,
				},
				new Vertex {
					Pos = b + n,
					UV1 = new Vector2(l / size, 1),
					Color = color,
				},
				new Vertex {
					Pos = b - n,
					UV1 = new Vector2(l / size, 0),
					Color = color,
				}
			};
			Renderer.DrawTriangleFan(texture, vertices, vertices.Length);
		}
	}
}
