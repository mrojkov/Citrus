using System.Collections.Generic;

namespace Lime
{
	public class Ribbon : Widget
	{
		public struct Vertex
		{
			public Vector2 Position;
			public float TextureU;
			public Color4 Color;
		}

		public delegate Vertex Function(float t);
		public Function Curve;
		public ITexture Texture1;
		public ITexture Texture2;
		public float Thickness;
		public bool Looped;
		public int NumKnots;

		public Ribbon(Function curve, ITexture texture1, float thickness = 10f, int numKnots = 50)
		{
			this.Curve = curve;
			this.Texture1 = texture1;
			this.Thickness = thickness;
			this.NumKnots = numKnots;
		}

		public override void Render()
		{
			Renderer.Blending = globalBlending;
			Renderer.Transform1 = globalMatrix;
			var vertices = CreateVertexList();
			if (vertices.Length >= 2) {
				Renderer.Vertex[] strip = CreateTriangleStrip(vertices);
				Renderer.DrawTriangleStrip(Texture1, Texture2, strip, strip.Length);
			}
		}

		private Renderer.Vertex[] CreateTriangleStrip(Vertex[] vertices)
		{
			var strip = new Renderer.Vertex[2 * (vertices.Length - 2)];
			int i = 0;
			for (int j = 1; j < vertices.Length - 1; j++) {
				Vertex v = vertices[j];
				Vector2 n = (vertices[j + 1].Position - vertices[j - 1].Position).Normalized.Normal;
				strip[i++] = new Renderer.Vertex {
					Color = v.Color * globalColor,
					Pos = v.Position - n * Thickness * 0.5f,
					UV1 = new Vector2(v.TextureU, 0)
				};
				strip[i++] = new Renderer.Vertex {
					Color = v.Color * globalColor,
					Pos = v.Position + n * Thickness * 0.5f,
					UV1 = new Vector2(v.TextureU, 1)
				};
			}
			return strip;
		}

		private Vertex[] CreateVertexList()
		{
			Vertex[] vertices = new Vertex [NumKnots + 3];
			float dt = 1f / NumKnots;
			if (!Looped) {
				vertices[0] = Curve(-dt);
				vertices[NumKnots + 2] = Curve(dt + 1);
			} else {
				vertices[0] = Curve(1 - dt);
				vertices[NumKnots + 2] = Curve(dt);
			}
			for (int i = 0; i <= NumKnots; i++) {
				vertices[i + 1] = Curve((float)i / NumKnots);
			}
			return vertices;
		}
	}
}
