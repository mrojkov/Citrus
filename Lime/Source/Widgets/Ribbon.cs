using System.Collections.Generic;

namespace Lime
{
	public class FunctionalRibbon : Ribbon
	{
		public int NumKnots;
		public delegate Vertex Function(float t);
		public Function Curve;
		public bool Looped;

		public FunctionalRibbon(Function curve, ITexture texture, float thickness = 10f, int numKnots = 50)
			: base (texture, thickness)
		{
			this.Curve = curve;
			this.NumKnots = numKnots;
		}

		protected override Vertex[] CreateVertexList()
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

	public class PolylineRibbon : Ribbon
	{
		public Vertex[] Vertices;

		public PolylineRibbon(ITexture texture, float thickness = 10f)
			: base (texture, thickness)
		{
		}

		public void SetVertices(Color4 color, params Vector2[] vertices)
		{
			Vertices = new Vertex[vertices.Length];
			float t = 0;
			int i = 0;
			foreach (var p in vertices) {
				Vertices[i++] = new Vertex { Position = p, Color = color, TextureU = t };
				t += 1.0f / vertices.Length;
			}
			GenerateTextureCoords();
		}

		public void SmoothOut(float smoothAmount, int numIterations)
		{
			if (Vertices.Length >= 4) {
				for (int i = 0; i < numIterations; i++) {
					Vertices = SmoothHelper(smoothAmount);
					smoothAmount *= 0.5f;
				}
				GenerateTextureCoords();
			}
		}

		private Vertex[] SmoothHelper(float smoothAmount)
		{
			List<Vertex> result = new List<Vertex>();
			result.Capacity = Vertices.Length * 2;
			for (int i = 0; i < Vertices.Length - 1; i++) {
				Vertex a = Vertices[i];
				Vertex b = Vertices[i + 1];
				float l = (b.Position - a.Position).Length;
				if (l > smoothAmount * 2) {
					Vector2 d = (b.Position - a.Position) / l;
					float t0 = smoothAmount / l;
					float t1 = 1 - t0;
					result.Add(Vertex.Lerp(t0, a, b));
					result.Add(Vertex.Lerp(t1, a, b));
				} else {
					result.Add(a);
				}
			}
			return result.ToArray();
		}

		public void GenerateTextureCoords()
		{
			if (Vertices.Length > 0) {
				float totalLength = 0;
				for (int i = 0; i < Vertices.Length; i++) {
					if (i > 0) {
						totalLength += (Vertices[i].Position - Vertices[i - 1].Position).Length;
					}
					Vertices[i].TextureU = totalLength;
				}
				for (int i = 0; i < Vertices.Length; i++) {
					Vertices[i].TextureU /= totalLength;
				}
			}
		}

		protected override Vertex[] CreateVertexList()
		{
			return Vertices;
		}
	}

	public abstract class Ribbon : Widget
	{
		public struct Vertex
		{
			public Vector2 Position;
			public float TextureU;
			public Color4 Color;

			public static Vertex Lerp(float t, Vertex a, Vertex b)
			{
				Vertex result;
				result.Position = Vector2.Lerp(t, a.Position, b.Position);
				result.Color = Color4.Lerp(t, a.Color, b.Color);
				result.TextureU = MathLib.Lerp(t, a.TextureU, b.TextureU);
				return result;
			}
		}
		
		public ITexture Texture;
		public float Thickness;

		public Ribbon(ITexture texture, float thickness = 10f)
		{
			this.Texture = texture;
			this.Thickness = thickness;
		}

		public override void Render()
		{
			Renderer.Blending = globalBlending;
			Renderer.Transform1 = globalMatrix;
			var vertices = CreateVertexList();
			if (vertices.Length >= 2) {
				var strip = CreateTriangleStrip(vertices);
				Renderer.DrawTriangleStrip(Texture, null, strip, strip.Length);
			}
		}

		private Renderer.Vertex[] CreateTriangleStrip(Vertex[] vertices)
		{
			var strip = new Renderer.Vertex[2 * (vertices.Length - 2)];
			int i = 0;
			for (int j = 1; j < vertices.Length - 1; j++) {
				Vertex v = vertices[j];
				Vector2 n = (vertices[j + 1].Position - vertices[j - 1].Position).Normal;
				n = n.Normalized;
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

		protected abstract Vertex[] CreateVertexList();
	}
}
