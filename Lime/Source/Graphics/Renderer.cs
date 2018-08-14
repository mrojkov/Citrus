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
	}

	public enum LineCap
	{
		Butt,
		Round,
		Square
	}

#pragma warning disable CS0660, CS0661
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

		public static bool operator ==(WindowRect lhs, WindowRect rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(WindowRect lhs, WindowRect rhs)
		{
			return !lhs.Equals(rhs);
		}

		public bool Equals(WindowRect other)
		{
			return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
		}
	}
#pragma warning restore CS0660, CS0661

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
		private static ShaderParamKey<Matrix44> projectionParamKey;
		private static ShaderParamKey<Vector4> colorFactorParamKey;

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
		private static Viewport viewport;
		private static ColorWriteMask colorWriteEnabled = ColorWriteMask.All;
		private static bool Transform2Active;
		private static CullMode cullMode;
		private static DepthState depthState;
		private static StencilState stencilState;
		private static ScissorState scissorState;
		private static Color4 colorFactor;

		public static Blending Blending { private get; set; }
		public static ShaderId Shader { private get; set; }
		public static Matrix32 Transform1 { private get; set; }
		public static int RenderCycle { get; private set; }
		public static int PolyCount3d = 0;
		public static readonly RenderList MainRenderList = new RenderList();
		public static RenderList CurrentRenderList;
#if ANDROID
		public static bool AmazonBindTextureWorkaround;
#endif

		public static int DrawCalls => PlatformRenderer.DrawCount;

		public static readonly ShaderParams GlobalShaderParams = new ShaderParams();

		public static Matrix44 World
		{
			get { return world; }
			set {
				world = value;
				worldViewDirty = worldViewProjDirty = true;
				FlushWVPMatrix();
			}
		}

		public static Matrix44 View
		{
			get { return view; }
			set {
				view = value;
				viewProjDirty = worldViewDirty = worldViewProjDirty = true;
				FlushWVPMatrix();
			}
		}

		public static Matrix44 Projection
		{
			get { return proj; }
			set {
				proj = value;
				viewProjDirty = worldViewProjDirty = true;
				Flush();
				FlushWVPMatrix();
			}
		}

		public static Matrix44 WorldView
		{
			get {
				if (worldViewDirty) {
					worldViewDirty = false;
					worldView = world * view;
				}
				return worldView;
			}
		}

		public static Matrix44 ViewProjection
		{
			get {
				if (viewProjDirty) {
					viewProjDirty = false;
					viewProj = view * proj;
				}
				return viewProj;
			}
		}

		public static Matrix44 WorldViewProjection
		{
			get {
				if (worldViewProjDirty) {
					worldViewProjDirty = false;
					worldViewProj = world * ViewProjection;
				}
				return worldViewProj;
			}
		}

		public static Matrix32 Transform2
		{
			private get { return transform2; }
			set {
				transform2 = value;
				Transform2Active = !value.IsIdentity();
			}
		}

		public static ScissorState ScissorState
		{
			private get { return scissorState; }
			set {
				MainRenderList.Flush();
				scissorState = value;
				PlatformRenderer.SetScissorState(value);
			}
		}

		public static StencilState StencilState
		{
			private get { return stencilState; }
			set {
				MainRenderList.Flush();
				stencilState = value;
				PlatformRenderer.SetStencilState(value);
			}
		}

		public static Viewport Viewport
		{
			private get { return viewport; }
			set {
				MainRenderList.Flush();
				viewport = value;
				PlatformRenderer.SetViewport(value);
			}
		}

		public static DepthState DepthState
		{
			private get { return depthState; }
			set {
				MainRenderList.Flush();
				depthState = value;
				PlatformRenderer.SetDepthState(depthState);
			}
		}

		public static ColorWriteMask ColorWriteEnabled
		{
			private get { return colorWriteEnabled; }
			set {
				if (colorWriteEnabled != value) {
					Flush();
					colorWriteEnabled = value;
					PlatformRenderer.SetColorWriteMask(value);
				}
			}
		}

		public static CullMode CullMode
		{
			private get { return cullMode; }
			set {
				if (cullMode != value) {
					cullMode = value;
					Flush();
					PlatformRenderer.SetCullMode(cullMode);
				}
			}
		}

		public static Color4 ColorFactor
		{
			get { return colorFactor; }
			set {
				if (colorFactor != value) {
					colorFactor = value;
					GlobalShaderParams.Set(colorFactorParamKey, colorFactor.ToVector4());
				}
			}
		}

		static Renderer()
		{
			projectionParamKey = GlobalShaderParams.GetParamKey<Matrix44>("matProjection");
			colorFactorParamKey = GlobalShaderParams.GetParamKey<Vector4>("colorFactor");
			PlatformRenderer.RenderTargetChanged += OnRenderTargetChanged;
			ColorFactor = Color4.White;
		}

		public static void SetOrthogonalProjection(Vector2 leftTop, Vector2 rightBottom)
		{
			SetOrthogonalProjection(leftTop.X, leftTop.Y, rightBottom.X, rightBottom.Y);
		}

		public static void SetOrthogonalProjection(float left, float top, float right, float bottom)
		{
			Projection = Matrix44.CreateOrthographicOffCenter(left, right, bottom, top, -50, 50);
		}

		public static void SetScissorState(ScissorState value, bool intersectWithCurrent = false)
		{
			if (intersectWithCurrent && scissorState.Enable && value.Enable) {
				value.Bounds = (WindowRect)IntRectangle.Intersect((IntRectangle)value.Bounds, (IntRectangle)scissorState.Bounds);
			}
			ScissorState = value;
		}

		public static void BeginFrame()
		{
			PlatformRenderer.BeginFrame();
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
			DrawTriangleFan(null, null, WidgetMaterial.Diffuse, vertices, numVertices);
		}

		public static void DrawTriangleFan(ITexture texture1, Vertex[] vertices, int numVertices)
		{
			DrawTriangleFan(texture1, null, vertices, numVertices);
		}

		public static void DrawTriangleFan(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			var material = GetMaterial(GetNumTextures(texture1, texture2));
			DrawTriangleFan(texture1, texture2, material, vertices, numVertices);
		}

		public static RenderBatch<Vertex> DrawTriangleFan(ITexture texture1, ITexture texture2, IMaterial material, Vertex[] vertices, int numVertices)
		{
			var batch = DrawTrianglesHelper(texture1, texture2, material, vertices, numVertices);
			var baseVertex = batch.LastVertex;
			int j = batch.LastIndex;
			var indices = batch.Mesh.Indices;
			for (int i = 1; i <= numVertices - 2; i++) {
				indices[j++] = (ushort)(baseVertex);
				indices[j++] = (ushort)(baseVertex + i);
				indices[j++] = (ushort)(baseVertex + i + 1);
				batch.LastIndex += 3;
			}
			batch.Mesh.DirtyFlags |= MeshDirtyFlags.Indices;
			batch.LastVertex += numVertices;
			return batch;
		}

		public static void DrawTriangleStrip(Vertex[] vertices, int numVertices)
		{
			DrawTriangleStrip(null, null, WidgetMaterial.Diffuse, vertices, numVertices);
		}

		public static void DrawTriangleStrip(ITexture texture1, Vertex[] vertices, int numVertices)
		{
			DrawTriangleStrip(texture1, null, vertices, numVertices);
		}

		public static void DrawTriangleStrip(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices)
		{
			var material = GetMaterial(GetNumTextures(texture1, texture2));
			DrawTriangleStrip(texture1, texture2, material, vertices, numVertices);
		}

		public static RenderBatch<Vertex> DrawTriangleStrip(ITexture texture1, ITexture texture2, IMaterial material, Vertex[] vertices, int numVertices)
		{
			var batch = DrawTrianglesHelper(texture1, texture2, material, vertices, numVertices);
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
			batch.LastVertex += numVertices;
			batch.Mesh.DirtyFlags |= MeshDirtyFlags.Indices;
			return batch;
		}

		private static RenderBatch<Vertex> DrawTrianglesHelper(ITexture texture1, ITexture texture2, IMaterial material, Vertex[] vertices, int numVertices)
		{
			var batch = CurrentRenderList.GetBatch<Vertex>(texture1, texture2, material, numVertices, (numVertices - 2) * 3);
			var transform = GetEffectiveTransform();
			var vd = batch.Mesh.Vertices;
			batch.Mesh.DirtyFlags |= MeshDirtyFlags.Vertices;
			int j = batch.LastVertex;
			for (int i = 0; i < numVertices; i++, j++) {
				var v = vertices[i];
				v.Pos = transform * v.Pos;
				texture1?.TransformUVCoordinatesToAtlasSpace(ref v.UV1);
				texture2?.TransformUVCoordinatesToAtlasSpace(ref v.UV2);
				vd[j] = v;
			}
			return batch;
		}

		public static void DrawSprite(ITexture texture1, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1)
		{
			DrawSprite(texture1, null, color, position, size, uv0, uv1, Vector2.Zero, Vector2.Zero);
		}

		public static void DrawSprite(ITexture texture1, ITexture texture2, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1)
		{
			DrawSprite(texture1, texture2, color, position, size, uv0, uv1, uv0, uv1);
		}

		public static void DrawSprite(ITexture texture1, ITexture texture2, Color4 color, Vector2 position, Vector2 size, Vector2 uv0t1, Vector2 uv1t1, Vector2 uv0t2, Vector2 uv1t2)
		{
			var material = GetMaterial(GetNumTextures(texture1, texture2));
			DrawSprite(texture1, texture2, material, color, position, size, uv0t1, uv1t1, uv0t2, uv1t2);
		}

		public static void DrawSprite(ITexture texture1, ITexture texture2, IMaterial material, Color4 color, Vector2 position, Vector2 size, Vector2 uv0t1, Vector2 uv1t1, Vector2 uv0t2, Vector2 uv1t2)
		{
			texture1?.TransformUVCoordinatesToAtlasSpace(ref uv0t1);
			texture1?.TransformUVCoordinatesToAtlasSpace(ref uv1t1);
			texture2?.TransformUVCoordinatesToAtlasSpace(ref uv0t2);
			texture2?.TransformUVCoordinatesToAtlasSpace(ref uv1t2);
			var batch = CurrentRenderList.GetBatch<Vertex>(texture1, texture2, material, 4, 6);
			batch.Mesh.DirtyFlags |= MeshDirtyFlags.VerticesIndices;
			int v = batch.LastVertex;
			int i = batch.LastIndex;
			batch.LastIndex += 6;
			batch.LastVertex += 4;
			var indices = batch.Mesh.Indices;
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
			var vertices = batch.Mesh.Vertices;
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

		public static void Clear(Color4 color)
		{
			Clear(ClearOptions.All, color);
		}

		public static void Clear(ClearOptions options)
		{
			Clear(options, Color4.Black);
		}

		public static void Clear(ClearOptions options, Color4 color)
		{
			PlatformRenderer.Clear(options, color);
		}

		public static void Clear(ClearOptions options, Color4 color, float depth, byte stencil)
		{
			PlatformRenderer.Clear(options, color, depth, stencil);
		}

		private static Sprite[] batchedSprites = new Sprite[20];
		private static Sprite sentinelSprite = new Sprite();

		public static void DrawSpriteList(List<Sprite> spriteList, Color4 color)
		{
			var matrix = GetEffectiveTransform();
			int batchLength = 0;
			var clipRect = scissorState.Enable ? CalcLocalScissorAABB(matrix) : new Rectangle();
			for (int t = 0; t <= spriteList.Count; t++) {
				var s = (t == spriteList.Count) ? sentinelSprite : spriteList[t];
				if (scissorState.Enable && s != sentinelSprite) {
					if (s.Position.X + s.Size.X < clipRect.A.X ||
						s.Position.X > clipRect.B.X ||
						s.Position.Y + s.Size.Y < clipRect.A.Y ||
						s.Position.Y > clipRect.B.Y) {
						continue;
					}
				}
				if (batchLength == 0 || batchLength < batchedSprites.Length && s.Texture == batchedSprites[0].Texture && s.Material == batchedSprites[0].Material) {
					batchedSprites[batchLength++] = s;
					continue;
				}
				var texture = batchedSprites[0].Texture;
				var material = batchedSprites[0].Material;
				var batch = CurrentRenderList.GetBatch<Vertex>(texture, null, material, 4 * batchLength, 6 * batchLength);
				int v = batch.LastVertex;
				int i = batch.LastIndex;
				batch.Mesh.DirtyFlags |= MeshDirtyFlags.VerticesIndices;
				var indices = batch.Mesh.Indices;
				var vertices = batch.Mesh.Vertices;
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
			var r = (Rectangle)(IntRectangle)scissorState.Bounds;
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
			DrawTriangleFan(null, null, WidgetMaterial.Diffuse, staticVertices, 4);
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
			DrawTriangleFan(null, null, WidgetMaterial.Diffuse, staticVertices, 4);
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
			FindMinMaxGradientPointColors(gradient, out Color4 colorA, out Color4 colorB);
			staticVertices[0] = new Vertex { Pos = a, Color = colorA };
			staticVertices[1] = new Vertex { Pos = new Vector2(b.X, a.Y), Color = colorA };
			staticVertices[2] = new Vertex { Pos = b, Color = colorB };
			staticVertices[3] = new Vertex { Pos = new Vector2(a.X, b.Y), Color = colorB };
			DrawTriangleFan(null, null, WidgetMaterial.Diffuse, staticVertices, 4);
		}

		public static void FindMinMaxGradientPointColors(ColorGradient gradient, out Color4 minPointColor, out Color4 maxPointColor)
		{
			var min = float.MaxValue;
			var max = float.MinValue;
			minPointColor = Color4.Zero;
			maxPointColor = Color4.Zero;
			foreach (var point in gradient) {
				if (point.Position < min) {
					minPointColor = point.Color;
					min = point.Position;
				}

				if (point.Position > max) {
					maxPointColor = point.Color;
					max = point.Position;
				}
			}
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
			FindMinMaxGradientPointColors(gradient, out Color4 colorA, out Color4 colorB);
			staticVertices[0] = new Vertex { Pos = a, Color = colorA };
			staticVertices[1] = new Vertex { Pos = new Vector2(b.X, a.Y), Color = colorB };
			staticVertices[2] = new Vertex { Pos = b, Color = colorB };
			staticVertices[3] = new Vertex { Pos = new Vector2(a.X, b.Y), Color = colorA };
			DrawTriangleFan(null, null, WidgetMaterial.Diffuse, staticVertices, 4);
		}

		public static void DrawHorizontalGradientRect(float x0, float y0, float x1, float y1, ColorGradient gradient)
		{
			DrawVerticalGradientRect(new Vector2(x0, y0), new Vector2(x1, y1), gradient);
		}

		public static void DrawHorizontalGradientRect(Vector2 a, Vector2 b, Color4 topColor, Color4 bottomColor)
		{
			DrawVerticalGradientRect(a, b, new ColorGradient(topColor, bottomColor));
		}

		public static void DrawVerticalGradientRect(float x0, float y0, float x1, float y1, Color4 topColor, Color4 bottomColor)
		{
			DrawVerticalGradientRect(new Vector2(x0, y0), new Vector2(x1, y1), topColor, bottomColor);
		}

		public static void DrawRound(Vector2 center, float radius, int numSegments, Color4 innerColor, Color4 outerColor)
		{
			if (staticVertices.Length < numSegments + 2) {
				staticVertices = new Vertex[numSegments + 2];
			}
			staticVertices[0] = new Vertex { Pos = center, Color = innerColor };
			for (int i = 0; i < numSegments + 1; i++) {
				staticVertices[i + 1].Pos = Vector2.CosSin(i * Mathf.TwoPi / numSegments) * radius + center;
				staticVertices[i + 1].Color = outerColor;
			}
			DrawTriangleFan(staticVertices, numSegments + 2);
		}

		public static void DrawCircle(Vector2 center, float radius, int numSegments, Color4 color)
		{
			if (staticVertices.Length < numSegments + 2) {
				staticVertices = new Vertex[numSegments + 2];
			}
			var prevPos = Vector2.CosSin(0) * radius + center;
			for (int i = 0; i < numSegments + 1; i++) {
				var pos = Vector2.CosSin(i * Mathf.TwoPi / numSegments) * radius + center;
				DrawLine(prevPos, pos, color);
				prevPos = pos;
			}
		}

		public static void DrawRound(Vector2 center, float radius, int numSegments, Color4 color)
		{
			DrawRound(center, radius, numSegments, color, color);
		}

		public static void DrawDashedLine(Vector2 a, Vector2 b, Color4 color, Vector2 dashSize)
		{
			const float feather = 1.0f;
			dashSize += Vector2.One * feather;
			var dir = (b - a).Normalized;
			var l = (b - a).Length;
			var n = GetVectorNormal(dir) * (dashSize.Y / 2);
			var uv2 = new Vector2(feather / dashSize.X, feather / dashSize.Y);
			Vertex[] vertices = {
				new Vertex {
					Pos = a - n,
					UV1 = new Vector2(0, 1),
					UV2 = uv2,
					Color = color,
				},
				new Vertex {
					Pos = a + n,
					UV1 = new Vector2(0, 0),
					UV2 = uv2,
					Color = color,
				},
				new Vertex {
					Pos = b + n,
					UV1 = new Vector2(l / dashSize.X, 0),
					UV2 = uv2,
					Color = color,
				},
				new Vertex {
					Pos = b - n,
					UV1 = new Vector2(l / dashSize.X, 1),
					UV2 = uv2,
					Color = color,
				}
			};
			DrawTriangleFan(null, null, DashedLineMaterial.Instance, vertices, vertices.Length);
		}

		private static int GetNumTextures(ITexture texture1, ITexture texture2)
		{
			return texture1 != null ? texture2 != null ? 2 : 1 : 0;
		}

		private static WidgetMaterial GetMaterial(int numTextures)
		{
			return WidgetMaterial.GetInstance(Blending, Shader, numTextures);
		}

		private static void OnRenderTargetChanged()
		{
			FlushWVPMatrix();
		}

		public static void MultiplyTransform1(Matrix32 transform)
		{
			Transform1 = transform * Transform1;
		}

		public static void MultiplyTransform2(Matrix32 transform)
		{
			Transform2 = transform * Transform2;
		}

		public static Matrix44 FixupWVP(Matrix44 projection)
		{
			if (PlatformRenderer.OffscreenRendering) {
				projection *= Matrix44.CreateScale(new Vector3(1, -1, 1));
			}
			return projection;
		}

		private static void FlushWVPMatrix()
		{
			GlobalShaderParams.Set(projectionParamKey, FixupWVP(WorldViewProjection));
		}

		private static Stack<State> stateStack = new Stack<State>();
		private static Stack<State> statePool = new Stack<State>();

		public static void PushState(RenderState mask)
		{
			var state = AcquireState();
			state.StateMask = mask;
			if ((mask & RenderState.Viewport) != 0) {
				state.Viewport = Viewport;
			}
			if ((mask & RenderState.Transform1) != 0) {
				state.Transform1 = Transform1;
			}
			if ((mask & RenderState.Transform2) != 0) {
				state.Transform2 = Transform2;
			}
			if ((mask & RenderState.Blending) != 0) {
				state.Blending = Blending;
			}
			if ((mask & RenderState.Shader) != 0) {
				state.Shader = Shader;
			}
			if ((mask & RenderState.ColorWriteEnabled) != 0) {
				state.ColorWriteEnabled = ColorWriteEnabled;
			}
			if ((mask & RenderState.CullMode) != 0) {
				state.CullMode = CullMode;
			}
			if ((mask & RenderState.DepthState) != 0) {
				state.DepthState = DepthState;
			}
			if ((mask & RenderState.ScissorState) != 0) {
				state.ScissorState = ScissorState;
			}
			if ((mask & RenderState.StencilState) != 0) {
				state.StencilState = StencilState;
			}
			if ((mask & RenderState.ColorFactor) != 0) {
				state.ColorFactor = ColorFactor;
			}
			if ((mask & RenderState.World) != 0) {
				state.World = World;
			}
			if ((mask & RenderState.View) != 0) {
				state.View = View;
			}
			if ((mask & RenderState.Projection) != 0) {
				state.Projection = Projection;
			}
			stateStack.Push(state);
		}

		public static void PopState()
		{
			var state = stateStack.Pop();
			var mask = state.StateMask;
			if ((mask & RenderState.Viewport) != 0) {
				Viewport = state.Viewport;
			}
			if ((mask & RenderState.Transform1) != 0) {
				Transform1 = state.Transform1;
			}
			if ((mask & RenderState.Transform2) != 0) {
				Transform2 = state.Transform2;
			}
			if ((mask & RenderState.Blending) != 0) {
				Blending = state.Blending;
			}
			if ((mask & RenderState.Shader) != 0) {
				Shader = state.Shader;
			}
			if ((mask & RenderState.ColorWriteEnabled) != 0) {
				ColorWriteEnabled = state.ColorWriteEnabled;
			}
			if ((mask & RenderState.CullMode) != 0) {
				CullMode = state.CullMode;
			}
			if ((mask & RenderState.DepthState) != 0) {
				DepthState = state.DepthState;
			}
			if ((mask & RenderState.ScissorState) != 0) {
				ScissorState = state.ScissorState;
			}
			if ((mask & RenderState.StencilState) != 0) {
				StencilState = state.StencilState;
			}
			if ((mask & RenderState.ColorFactor) != 0) {
				ColorFactor = state.ColorFactor;
			}
			if ((mask & RenderState.World) != 0) {
				World = state.World;
			}
			if ((mask & RenderState.View) != 0) {
				View = state.View;
			}
			if ((mask & RenderState.Projection) != 0) {
				Projection = state.Projection;
			}
			RecycleState(state);
		}

		private static State AcquireState()
		{
			if (statePool.Count > 0) {
				return statePool.Pop();
			} else {
				return new State();
			}
		}

		private static void RecycleState(State state)
		{
			statePool.Push(state);
		}

		private class State
		{
			public RenderState StateMask;
			public Viewport Viewport;
			public Matrix32 Transform1;
			public Matrix32 Transform2;
			public Blending Blending;
			public ShaderId Shader;
			public ColorWriteMask ColorWriteEnabled;
			public CullMode CullMode;
			public DepthState DepthState;
			public ScissorState ScissorState;
			public StencilState StencilState;
			public Color4 ColorFactor;
			public Matrix44 World;
			public Matrix44 View;
			public Matrix44 Projection;
		}
	}

	[Flags]
	public enum RenderState
	{
		None = 0,
		Viewport = 1 << 0,
		Transform1 = 1 << 1,
		Transform2 = 1 << 2,
		Blending = 1 << 3,
		Shader = 1 << 4,
		ColorWriteEnabled = 1 << 5,
		CullMode = 1 << 6,
		DepthState = 1 << 7,
		ScissorState = 1 << 8,
		StencilState = 1 << 9,
		ColorFactor = 1 << 10,
		World = 1 << 11,
		View = 1 << 12,
		Projection = 1 << 13
	}

	public class DashedLineMaterial : IMaterial
	{
		public int PassCount => 1;

		public static readonly DashedLineMaterial Instance = new DashedLineMaterial();

		private ShaderParams[] shaderParamsArray = new[] { Renderer.GlobalShaderParams };

		public void Apply(int pass)
		{
			PlatformRenderer.SetBlendState(Blending.Alpha.GetBlendState());
			PlatformRenderer.SetShaderParams(shaderParamsArray);
			PlatformRenderer.SetShaderProgram(ShaderPrograms.DashedLineShaderProgram.GetInstance());
		}

		public IMaterial Clone() => this;

		public void Invalidate() { }
	}
}
