using System;
using System.Collections.Generic;

namespace Lime
{
	public interface IRenderer
	{
		Blending Blending { get; set; }
		ShaderId Shader { get; set; }
		Matrix32 Transform1 { get; set; }
		int RenderCycle { get; }
		int PolyCount3d { get; }
		int DrawCalls { get; }
		ShaderParams GlobalShaderParams { get; }
		Matrix44 World { get; set; }
		Matrix44 View { get; set; }
		Matrix44 Projection { get; set; }
		Matrix44 WorldView { get; }
		Matrix44 ViewProjection { get; }
		Matrix44 WorldViewProjection { get; }
		Matrix32 Transform2 { get; set; }
		ScissorState ScissorState { get; set; }
		StencilState StencilState { get; set; }
		Viewport Viewport { get; set; }
		DepthState DepthState { get; set; }
		ColorWriteMask ColorWriteEnabled { get; set; }
		CullMode CullMode { get; set; }
		Color4 ColorFactor { get; set; }
		void SetOrthogonalProjection(Vector2 leftTop, Vector2 rightBottom);
		void SetOrthogonalProjection(float left, float top, float right, float bottom);
		void BeginFrame();
		void EndFrame();
		void Flush();
		void DrawTextLine(float x, float y, string text, float fontHeight, Color4 color, float letterSpacing);
		void DrawTextLine(Vector2 position, string text, float fontHeight, Color4 color, float letterSpacing);
		void DrawTextLine(IFont font, Vector2 position, string text, float fontHeight, Color4 color, float letterSpacing);
		Vector2 MeasureTextLine(string text, float fontHeight, float letterSpacing);
		Vector2 MeasureTextLine(IFont font, string text, float fontHeight, float letterSpacing);
		Vector2 MeasureTextLine(IFont font, string text, float fontHeight, int start, int length, float letterSpacing);
		void DrawTextLine(IFont font, Vector2 position, string text, Color4 color, float fontHeight, int start, int length, float letterSpacing);
		void DrawTextLine(
			IFont font, Vector2 position, string text, Color4 color, float fontHeight, int start, int length, float letterSpacing,
			SpriteList list, Action<int, Vector2, Vector2> onDrawChar = null, int tag = -1);
		void DrawTriangleFan(Vertex[] vertices, int numVertices);
		void DrawTriangleFan(ITexture texture1, Vertex[] vertices, int numVertices);
		void DrawTriangleFan(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices);
		RenderBatch<Vertex> DrawTriangleFan(ITexture texture1, ITexture texture2, IMaterial material, Vertex[] vertices, int numVertices);
		void DrawTriangleStrip(Vertex[] vertices, int numVertices);
		void DrawTriangleStrip(ITexture texture1, Vertex[] vertices, int numVertices);
		void DrawTriangleStrip(ITexture texture1, ITexture texture2, Vertex[] vertices, int numVertices);
		RenderBatch<Vertex> DrawTriangleStrip(ITexture texture1, ITexture texture2, IMaterial material, Vertex[] vertices, int numVertices);
		void DrawSprite(ITexture texture1, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1);
		void DrawSprite(ITexture texture1, ITexture texture2, Color4 color, Vector2 position, Vector2 size, Vector2 uv0, Vector2 uv1);
		void DrawSprite(ITexture texture1, ITexture texture2, Color4 color, Vector2 position, Vector2 size, Vector2 uv0t1, Vector2 uv1t1, Vector2 uv0t2, Vector2 uv1t2);
		void DrawSprite(ITexture texture1, ITexture texture2, IMaterial material, Color4 color, Vector2 position, Vector2 size, Vector2 uv0t1, Vector2 uv1t1, Vector2 uv0t2, Vector2 uv1t2);
		void Clear(Color4 color);
		void Clear(ClearOptions options);
		void Clear(ClearOptions options, Color4 color);
		void Clear(ClearOptions options, Color4 color, float depth, byte stencil);
		void DrawSpriteList(List<Sprite> spriteList, Color4 color);
		void DrawLine(float x0, float y0, float x1, float y1, Color4 color, float thickness = 1, LineCap cap = LineCap.Butt);
		void DrawLine(Vector2 a, Vector2 b, Color4 color, float thickness = 1, LineCap cap = LineCap.Butt);
		void DrawRect(Vector2 a, Vector2 b, Color4 color);
		void DrawRect(float x0, float y0, float x1, float y1, Color4 color);

		/// <summary>
		/// Draws the rectangle outline inscribed within the given bounds.
		/// </summary>
		void DrawRectOutline(Vector2 a, Vector2 b, Color4 color, float thickness = 1);

		/// <summary>
		/// Draws the quadrangle outline inscribed within the given bounds.
		/// </summary>
		void DrawQuadrangleOutline(Quadrangle q, Color4 color, float thickness = 1);

		/// <summary>
		/// Draws the quadrangle
		/// </summary>
		void DrawQuadrangle(Quadrangle q, Color4 color);

		/// <summary>
		/// Draws the rectangle outline inscribed within the given bounds.
		/// </summary>
		void DrawRectOutline(float x0, float y0, float x1, float y1, Color4 color, float thickness = 1);
		void DrawVerticalGradientRect(Vector2 a, Vector2 b, ColorGradient gradient);
		void DrawVerticalGradientRect(float x0, float y0, float x1, float y1, ColorGradient gradient);
		void DrawVerticalGradientRect(Vector2 a, Vector2 b, Color4 topColor, Color4 bottomColor);
		void DrawHorizontalGradientRect(Vector2 a, Vector2 b, ColorGradient gradient);
		void DrawHorizontalGradientRect(float x0, float y0, float x1, float y1, ColorGradient gradient);
		void DrawHorizontalGradientRect(Vector2 a, Vector2 b, Color4 topColor, Color4 bottomColor);
		void DrawVerticalGradientRect(float x0, float y0, float x1, float y1, Color4 topColor, Color4 bottomColor);
		void DrawRound(Vector2 center, float radius, int numSegments, Color4 innerColor, Color4 outerColor);
		void DrawCircle(Vector2 center, float radius, int numSegments, Color4 color);
		void DrawRound(Vector2 center, float radius, int numSegments, Color4 color);
		void DrawDashedLine(Vector2 a, Vector2 b, Color4 color, Vector2 dashSize);
		Matrix44 FixupWVP(Matrix44 projection);
	}
}
