using Lime;

namespace Tangerine.UI
{
	public static class KeyframeFigure
	{
		private static readonly Vertex[] vertices = new Vertex[11];

		public static void Render(Vector2 a, Vector2 b, Color4 color, KeyFunction func)
		{
			var segmentWidth = b.X - a.X;
			var segmentHeight = b.Y - a.Y;
			switch (func) {
				case KeyFunction.Linear:
					vertices[0].Pos = new Vector2(a.X, b.Y - 0.5f);
					vertices[1].Pos = new Vector2(b.X, a.Y);
					vertices[2].Pos = new Vector2(b.X, b.Y - 0.5f);
					for (int i = 0; i < vertices.Length; i++) {
						vertices[i].Color = color;
					}
					Renderer.DrawTriangleFan(vertices, numVertices: 3);
					break;

				case KeyFunction.Steep:
					var leftSmallRectVertexA = new Vector2(a.X + 0.5f, a.Y + segmentHeight / 2);
					var leftSmallRectVertexB = new Vector2(a.X + segmentWidth / 2, b.Y - 0.5f);
					Renderer.DrawRect(leftSmallRectVertexA, leftSmallRectVertexB, color);
					var rightBigRectVertexA = new Vector2(a.X + segmentWidth / 2, a.Y + 0.5f);
					var rightBigRectVertexB = new Vector2(b.X, b.Y - 0.5f);
					Renderer.DrawRect(rightBigRectVertexA, rightBigRectVertexB, color);
					break;

				case KeyFunction.Spline:
					var numSegments = 5;
					var center = new Vector2(a.X, b.Y - 0.5f);
					vertices[0] = new Vertex { Pos = center, Color = color };
					for (int i = 0; i < numSegments; i++) {
						var r = Vector2.CosSin(i * Mathf.HalfPi / (numSegments - 1));
						vertices[i + 1].Pos = new Vector2(
							center.X + r.X * segmentWidth,
							center.Y - r.Y * segmentHeight);
						vertices[i + 1].Color = color;
					}
					Renderer.DrawTriangleFan(vertices, numSegments + 1);
					break;

				case KeyFunction.ClosedSpline:
					var circleCenter = new Vector2(a.X + segmentWidth / 2, a.Y + segmentHeight / 2);
					var circleRadius = 0f;
					if (segmentWidth < segmentHeight) {
						circleRadius = circleCenter.X - a.X - 0.5f;
					} else {
						circleRadius = circleCenter.Y - a.Y - 0.5f;
					}
					Renderer.DrawRound(circleCenter, circleRadius, numSegments: 6, color);
					break;
				default:
					throw new System.NotImplementedException("Unknown KeyFunction value");
			}
		}
	}
}
