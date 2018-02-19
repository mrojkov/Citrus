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
		Opaque,
		LcdTextFirstPass,
		LcdTextSecondPass,
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
	
	[Flags]
	public enum ColorMask
	{
		None = 0,
		Red = 1,
		Green = 2,
		Blue = 4,
		Alpha = 8,
		All = Red | Green | Blue | Alpha
	}

	[Flags]
	public enum ClearTarget
	{
		None = 0,
		ColorBuffer = 1,
		DepthBuffer = 2,
		StencilBuffer = 4,
		All = ColorBuffer | DepthBuffer | StencilBuffer
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
		private static ColorMask colorWriteEnabled = ColorMask.All;
		private static bool Transform2Active;
		private static CullMode cullMode;
		private static StencilParams stencilParams;

		public static Blending Blending;
		public static ShaderId Shader;
		public static ShaderProgram CustomShaderProgram;
		public static Matrix32 Transform1;
		public static int RenderCycle { get; private set; }
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
		
		public static StencilParams StencilParams
		{
			get { return stencilParams; }
			set
			{
				if (!stencilParams.Equals(value)) {
					stencilParams = value;
					MainRenderList.Flush();
					PlatformRenderer.SetStencilParams(value);
				}
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

		public static ColorMask ColorWriteEnabled
		{
			get { return colorWriteEnabled; }
			set
			{
				if (colorWriteEnabled != value) {
					Flush();
					colorWriteEnabled = value;
					PlatformRenderer.EnableColorWrite(value);
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

		public static Vector2 MeasureTextLine(string text, float fontHeight, float letterSpacing)
		{
			return MeasureTextLine(FontPool.Instance[null], text, fontHeight, 0, text.Length, letterSpacing);
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
		
		static SpriteList staticSpriteList = new SpriteList();
		
		public static void DrawTextLine(IFont font, Vector2 position, string text, Color4 color, float fontHeight, int start, int length, float letterSpacing)
		{
			DrawTextLine(font, position, text, color, fontHeight, start, length, letterSpacing, staticSpriteList);
			staticSpriteList.Render(Color4.White, Blending, Shader);
			staticSpriteList.Clear();
		}
		
		public static void DrawTextLine(
			IFont font, Vector2 position, string text, Color4 color, float fontHeight, int start, int length, float letterSpacing,
			SpriteList list, Action<int, Vector2, Vector2> onDrawChar = null, int tag = -1)
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
					onDrawChar?.Invoke(i, position, Vector2.Down * fontHeight);
					position.X = savedX;
					position.Y += fontHeight;
					prevChar = null;
					continue;
				} else if (ch == '\r') {
					continue;
				}
				FontChar fontChar = font.Chars.Get(ch, fontHeight);
				if (fontChar == FontChar.Null) {
					onDrawChar?.Invoke(i, position, Vector2.Down * fontHeight);
					continue;
				}
				float scale = fontChar.Height != 0.0f ? fontHeight / fontChar.Height : 0.0f;
				var xDelta = scale * (fontChar.ACWidths.X + fontChar.Kerning(prevChar) + letterSpacing);
				position.X += xDelta;
				var size = new Vector2(scale * fontChar.Width, fontHeight - fontChar.VerticalOffset);
				Vector2 roundPos;
				if (font.RoundCoordinates) {
					roundPos = new Vector2(position.X.Round(), position.Y.Round() + fontChar.VerticalOffset);
					onDrawChar?.Invoke(i, new Vector2((position.X - xDelta).Round(), position.Y.Round()), size);
				} else {
					roundPos = new Vector2(position.X, position.Y + fontChar.VerticalOffset);
					onDrawChar?.Invoke(i, new Vector2(position.X - xDelta, position.Y), size);
				}
				chars[j].FontChar = fontChar;
				chars[j].Position = roundPos;
				++j;
				position.X += scale * (fontChar.Width + fontChar.ACWidths.Y);
				prevChar = fontChar;
			}
			list.Add(font, color, fontHeight, chars, tag);
		}
		
		public static void DrawTriangleFan(Vertex[] vertices, int numVertices)
		{
			DrawTriangleFan(WidgetMaterial.Diffuse, vertices, numVertices);
		}
		
		public static void DrawTriangleFan(ITexture texture1, Vertex[] vertices, int numVertices)
		{
			DrawTriangleFan(texture1, null, vertices, numVertices);
		}
		
		public static void DrawTriangleFan(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			var material = WidgetMaterial.GetInstance(Blending, Shader, CustomShaderProgram, texture1, texture2);
			var batch = DrawTriangleFanHelper(material, vertices, numVertices);
			var vd = batch.VertexBuffer.Data;
			for (int i = 0; i < numVertices; i++) {
				var t = batch.LastVertex - numVertices + i;
				texture1?.TransformUVCoordinatesToAtlasSpace(ref vd[t].UV1);
				texture2?.TransformUVCoordinatesToAtlasSpace(ref vd[t].UV2);
			}
		}
		
		public static void DrawTriangleFan(IMaterial material, Vertex[] vertices, int numVertices)
		{
			DrawTriangleFanHelper(material, vertices, numVertices);
		}

		private static RenderBatch DrawTriangleFanHelper(IMaterial material, Vertex[] vertices, int numVertices)
		{
			var batch = DrawTrianglesHelper(material, vertices, numVertices);
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
			return batch;
		}
		
		public static void DrawTriangleStrip(Vertex[] vertices, int numVertices)
		{
			DrawTriangleStrip(WidgetMaterial.Diffuse, vertices, numVertices);
		}

		public static void DrawTriangleStrip(ITexture texture1, Vertex[] vertices, int numVertices)
		{
			DrawTriangleStrip(texture1, null, vertices, numVertices);
		}
		
		public static void DrawTriangleStrip(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			var material = WidgetMaterial.GetInstance(Blending, Shader, CustomShaderProgram, texture1, texture2);
			var batch = DrawTriangleStripHelper(material, vertices, numVertices);
			var vd = batch.VertexBuffer.Data;
			for (int i = 0; i < numVertices; i++) {
				var t = batch.LastVertex - numVertices + i;
				texture1?.TransformUVCoordinatesToAtlasSpace(ref vd[t].UV1);
				texture2?.TransformUVCoordinatesToAtlasSpace(ref vd[t].UV2);
			}
		}
		
		public static void DrawTriangleStrip(IMaterial material, Vertex[] vertices, int numVertices)
		{
			DrawTriangleStripHelper(material, vertices, numVertices);
		}
		
		private static RenderBatch DrawTriangleStripHelper(IMaterial material, Vertex[] vertices, int numVertices)
		{
			var batch = DrawTrianglesHelper(material, vertices, numVertices);
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
			return batch;
		}

		private static RenderBatch DrawTrianglesHelper(IMaterial material, Vertex[] vertices, int numVertices)
		{
			var batch = CurrentRenderList.GetBatch(material, numVertices, (numVertices - 2) * 3);
			var transform = GetEffectiveTransform();
			var vd = batch.VertexBuffer.Data;
			batch.VertexBuffer.Dirty = true;
			int j = batch.LastVertex;
			for (int i = 0; i < numVertices; i++, j++) {
				var v = vertices[i];
				v.Pos = transform * v.Pos;
				vd[j] = v;
			}
			return batch;
		}
		
		public static void DrawSprite(ITexture texture1, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1)
		{
			var material = WidgetMaterial.GetInstance(Blending, Shader, CustomShaderProgram, texture1, null);
			texture1?.TransformUVCoordinatesToAtlasSpace(ref uv0);
			texture1?.TransformUVCoordinatesToAtlasSpace(ref uv1);
			DrawSprite(material, color, position, size, uv0, uv1, Vector2.Zero, Vector2.Zero);
		}
		
		public static void DrawSprite(ITexture texture1, ITexture texture2, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1)
		{
			var material = WidgetMaterial.GetInstance(Blending, Shader, CustomShaderProgram, texture1, texture2);
			var uv0t2 = uv0;
			var uv1t2 = uv1;
			texture2?.TransformUVCoordinatesToAtlasSpace(ref uv0t2);
			texture2?.TransformUVCoordinatesToAtlasSpace(ref uv1t2);
			texture1?.TransformUVCoordinatesToAtlasSpace(ref uv0);
			texture1?.TransformUVCoordinatesToAtlasSpace(ref uv1);
			DrawSprite(material, color, position, size, uv0, uv1, uv0t2, uv1t2);
		}
	
		public static void DrawSprite(IMaterial material, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1)
		{
			DrawSprite(material, color, position, size, uv0, uv1, Vector2.Zero, Vector2.Zero);
		}

		public static void DrawSprite(IMaterial material, Color4 color, Vector2 position, Vector2 size, Vector2 uv0t1, Vector2 uv1t1, Vector2 uv0t2, Vector2 uv1t2)
		{
			var batch = CurrentRenderList.GetBatch(material, 4, 6);
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
			float x0ux = x0 * matrix.UX;
			float x0uy = x0 * matrix.UY;
			float y0vx = y0 * matrix.VX;
			float y0vy = y0 * matrix.VY;
			float x1ux = x1 * matrix.UX;
			float x1uy = x1 * matrix.UY;
			float y1vx = y1 * matrix.VX;
			float y1vy = y1 * matrix.VY;
			var vertices = batch.VertexBuffer.Data;
			vertices[v].Pos = new Vector2 { X = x0ux + y0vx + matrix.TX, Y = x0uy + y0vy + matrix.TY };
			vertices[v].Color = color;
			vertices[v].UV1 = uv0t1;
			vertices[v++].UV2 = uv0t2;
			vertices[v].Pos = new Vector2 { X = x1ux + y0vx + matrix.TX, Y = x1uy + y0vy + matrix.TY };
			vertices[v].Color = color;
			vertices[v].UV1 = new Vector2 { X = uv1t1.X, Y = uv0t1.Y };
			vertices[v++].UV2 = new Vector2 { X = uv1t2.X, Y = uv0t2.Y };
			vertices[v].Pos = new Vector2 { X = x0ux + y1vx + matrix.TX, Y = x0uy + y1vy + matrix.TY };
			vertices[v].Color = color;
			vertices[v].UV1 = new Vector2 { X = uv0t1.X, Y = uv1t1.Y };
			vertices[v++].UV2 = new Vector2 { X = uv0t2.X, Y = uv1t2.Y };
			vertices[v].Pos = new Vector2 { X = x1ux + y1vx + matrix.TX, Y = x1uy + y1vy + matrix.TY };
			vertices[v].Color = color;
			vertices[v].UV1 = uv1t1;
			vertices[v].UV2 = uv1t2;
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
				if (batchLength == 0 || batchLength < batchedSprites.Length && s.Material == batchedSprites[0].Material) {
					batchedSprites[batchLength++] = s;
					continue;
				}
				var material = batchedSprites[0].Material;
				var batch = CurrentRenderList.GetBatch(material, 4 * batchLength, 6 * batchLength);
				int v = batch.LastVertex;
				int i = batch.LastIndex;
				batch.VertexBuffer.Dirty = true;
				batch.IndexBuffer.Dirty = true;
				var indices = batch.IndexBuffer.Data;
				var vertices = batch.VertexBuffer.Data;
				for (int j = 0; j < batchLength; j++) {
					var sprite = batchedSprites[j];
					var effectiveColor = color * sprite.Color;
					indices[i++] = (ushort)(v + 1);
					indices[i++] = (ushort)v;
					indices[i++] = (ushort)(v + 2);
					indices[i++] = (ushort)(v + 2);
					indices[i++] = (ushort)(v + 3);
					indices[i++] = (ushort)(v + 1);
					float x0 = sprite.Position.X;
					float y0 = sprite.Position.Y;
					float x1 = sprite.Position.X + sprite.Size.X;
					float y1 = sprite.Position.Y + sprite.Size.Y;
					float x0ux = x0 * matrix.UX;
					float x0uy = x0 * matrix.UY;
					float y0vx = y0 * matrix.VX;
					float y0vy = y0 * matrix.VY;
					float x1ux = x1 * matrix.UX;
					float x1uy = x1 * matrix.UY;
					float y1vx = y1 * matrix.VX;
					float y1vy = y1 * matrix.VY;
					var uv0 = sprite.UV0;
					var uv1 = sprite.UV1;
					vertices[v].Pos = new Vector2 { X = x0ux + y0vx + matrix.TX, Y = x0uy + y0vy + matrix.TY };
					vertices[v].Color = effectiveColor;
					vertices[v++].UV1 = uv0;
					vertices[v].Pos = new Vector2 { X = x1ux + y0vx + matrix.TX, Y = x1uy + y0vy + matrix.TY };
					vertices[v].Color = effectiveColor;
					vertices[v++].UV1 = new Vector2 { X = uv1.X, Y = uv0.Y };
					vertices[v].Pos = new Vector2 { X = x0ux + y1vx + matrix.TX, Y = x0uy + y1vy + matrix.TY };
					vertices[v].Color = effectiveColor;
					vertices[v++].UV1 = new Vector2 { X = uv0.X, Y = uv1.Y };
					vertices[v].Pos = new Vector2 { X = x1ux + y1vx + matrix.TX, Y = x1uy + y1vy + matrix.TY };
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

		static Vertex[] staticVertices = new Vertex[4];

		public static void DrawLine(Vector2 a, Vector2 b, Color4 color, float thickness = 1, LineCap cap = LineCap.Butt)
		{
			if (cap == LineCap.Round) {
				throw new NotImplementedException();
			}
			var d = (b - a).Normalized * thickness * 0.5f;
			Vector2 n = GetVectorNormal(d);
			staticVertices[0] = new Vertex { Pos = a - n, Color = color };
			staticVertices[1] = new Vertex { Pos = b - n, Color = color };
			staticVertices[2] = new Vertex { Pos = b + n, Color = color };
			staticVertices[3] = new Vertex { Pos = a + n, Color = color };
			if (cap == LineCap.Square) {
				staticVertices[0].Pos -= d;
				staticVertices[1].Pos += d;
				staticVertices[2].Pos += d;
				staticVertices[3].Pos -= d;
			}
			DrawTriangleFan(WidgetMaterial.Diffuse, staticVertices, 4);
		}

		static Vector2 GetVectorNormal(Vector2 v)
		{
			return new Vector2(-v.Y, v.X);
		}

		public static void DrawRect(Vector2 a, Vector2 b, Color4 color)
		{
			staticVertices[0] = new Vertex { Pos = a, Color = color };
			staticVertices[1] = new Vertex { Pos = new Vector2(b.X, a.Y), Color = color };
			staticVertices[2] = new Vertex { Pos = b, Color = color };
			staticVertices[3] = new Vertex { Pos = new Vector2(a.X, b.Y), Color = color };
			DrawTriangleFan(WidgetMaterial.Diffuse, staticVertices, 4);
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
		public static void DrawQuadrangle(Quadrangle q, Color4 color)
		{
			for (int i = 0; i < 4; i++) {
				staticVertices[i] = new Vertex { Pos = q[i], Color = color };
			}
			DrawTriangleFan(staticVertices, 4);
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
			staticVertices[0] = new Vertex { Pos = a, Color = gradient.A };
			staticVertices[1] = new Vertex { Pos = new Vector2(b.X, a.Y), Color = gradient.A };
			staticVertices[2] = new Vertex { Pos = b, Color = gradient.B };
			staticVertices[3] = new Vertex { Pos = new Vector2(a.X, b.Y), Color = gradient.B };
			DrawTriangleFan(WidgetMaterial.Diffuse, staticVertices, 4);
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
			staticVertices[0] = new Vertex { Pos = a, Color = gradient.A };
			staticVertices[1] = new Vertex { Pos = new Vector2(b.X, a.Y), Color = gradient.B };
			staticVertices[2] = new Vertex { Pos = b, Color = gradient.B };
			staticVertices[3] = new Vertex { Pos = new Vector2(a.X, b.Y), Color = gradient.A };
			DrawTriangleFan(WidgetMaterial.Diffuse, staticVertices, 4);
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
			if (staticVertices.Length < numSegments + 1) {
				staticVertices = new Vertex[numSegments + 1];
			}
			staticVertices[0] = new Vertex { Pos = center, Color = innerColor };
			for (int i = 0; i < numSegments; i++) {
				staticVertices[i + 1].Pos = Vector2.CosSin(i * Mathf.TwoPi / (numSegments - 1)) * radius + center;
				staticVertices[i + 1].Color = outerColor;
			}
			DrawTriangleFan(staticVertices, numSegments + 1);
		}

		public static void DrawCircle(Vector2 center, float radius, int numSegments, Color4 color)
		{
			if (staticVertices.Length < numSegments + 1) {
				staticVertices = new Vertex[numSegments + 1];
			}
			var prevPos = Vector2.CosSin(0) * radius + center;
			for (int i = 0; i < numSegments; i++) {
				var pos = Vector2.CosSin(i * Mathf.TwoPi / (numSegments - 1)) * radius + center;
				DrawLine(prevPos, pos, color);
				prevPos = pos;
			}
		}

		public static void DrawRound(Vector2 center, float radius, int numSegments, Color4 color)
		{
			DrawRound(center, radius, numSegments, color, color);
		}

		public static void DrawDashedLine(ITexture texture, Vector2 a, Vector2 b, Color4 color, float size = 8)
		{
			var dir = (b - a).Normalized;
			var l = (b - a).Length;
			var n = GetVectorNormal(dir) * size / 2;
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
			DrawTriangleFan(WidgetMaterial.GetInstance(Blending.None, ShaderId.Diffuse, texture1: texture), vertices, vertices.Length);
		}
	}
}
