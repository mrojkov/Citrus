using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class Polyline : Widget
	{
		public float Thickness;
		public List<Vector2> Points = new List<Vector2>();

		public Polyline(float thickness = 20)
		{
			this.Thickness = thickness;
		}

		public override void Render()
		{
			Renderer.Blending = GlobalBlending;
			Renderer.Transform1 = LocalToWorldTransform;
			int n = Points.Count;
			if (n >= 2) {
				bool closed = Points[0] == Points[n - 1];
				if (!closed) {
					DrawCap(Points[0], Points[1]);
					DrawSegment(Points[0], (Points[0] + Points[1]) / 2);
				}
				if (closed) {
					Vector2 a = Points[n - 2];
					Vector2 b = Points[0];
					Vector2 c = Points[1];
					a = (a + b) / 2;
					c = (b + c) / 2;
					DrawJoint(a, b, c);
				}
				for (int i = 0; i < n - 2; i++) {
					Vector2 a = Points[i];
					Vector2 b = Points[i + 1];
					Vector2 c = Points[i + 2];
					a = (a + b) / 2;
					c = (b + c) / 2;
					DrawJoint(a, b, c);
				}
				if (!closed) {
					DrawCap(Points[n - 1], Points[n - 2]);
					DrawSegment((Points[n - 1] + Points[n - 2]) / 2, Points[n - 1]);
				}
			}
		}

		private void DrawJoint(Vector2 a, Vector2 b, Vector2 c)
		{
			Vector2 n1 = (b - a).Normal * Thickness / 2;
			Vector2 n2 = (c - b).Normal * Thickness / 2;
			if (Vector2.CrossProduct(a - b, c - b) > 0) {
				n1 = -n1;
				n2 = -n2;
			}
			Vector2 p = b;
			Geometry.CalcLinesIntersection(a + n1, b + n1, b + n2, c + n2, ref p);
			DrawQuad(a - n1, b - n1, p, a + n1);
			DrawQuad(c - n2, b - n2, p, c + n2);
			FillJointGap(p, b, n1, n2);
		}

		private void DrawQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
		{
			var v = new Renderer.Vertex[4];
			v[0] = new Renderer.Vertex { Pos = a, Color = GlobalColor };
			v[1] = new Renderer.Vertex { Pos = b, Color = GlobalColor };
			v[2] = new Renderer.Vertex { Pos = c, Color = GlobalColor };
			v[3] = new Renderer.Vertex { Pos = d, Color = GlobalColor };
			Renderer.DrawTriangleFan(null, null, v, v.Length);
		}

		private void FillJointGap(Vector2 p, Vector2 b, Vector2 n1, Vector2 n2)
		{
			float angle1 = n1.Atan2Deg;
			float angle2 = n2.Atan2Deg;
			if (angle1 > angle2) {
				Toolbox.Swap(ref angle1, ref angle2);
			}
			if (angle2 - angle1 > 180) {
				angle2 -= 360;
			}
			var v = new Renderer.Vertex[10];
			v[0] = new Renderer.Vertex { Pos = p, Color = GlobalColor };
			float length = n2.Length;
			for (int i = 0; i < v.Length - 1; i++) {
				float t = (float)i / (v.Length - 2);
				p = -Vector2.HeadingDeg(Mathf.Lerp(t, angle1, angle2)) * length + b;
				v[i + 1] = new Renderer.Vertex { Pos = p, Color = GlobalColor };
			}
			Renderer.DrawTriangleFan(null, null, v, v.Length);
		}

		private void DrawCap(Vector2 a, Vector2 b)
		{
			float angle = (a - b).Atan2Deg;
			DrawRoundPart(a, angle - 90, angle + 90);
		}

		private void DrawRoundPart(Vector2 center, float angle1, float angle2)
		{
			const int numPoints = 10;
			var v = new Renderer.Vertex[numPoints + 2];
			v[0] = new Renderer.Vertex { Pos = center, Color = GlobalColor };
			for (int i = 0; i <= numPoints; i++) {
				float t = (float)i / numPoints;
				Vector2 p = Vector2.HeadingDeg(Mathf.Lerp(t, angle1, angle2));
				v[i + 1] = new Renderer.Vertex { Pos = center + p * Thickness / 2, Color = GlobalColor };
			}
			Renderer.DrawTriangleFan(null, null, v, v.Length);
		}

		private void DrawSegment(Vector2 a, Vector2 b)
		{
			Vector2 n = (b - a).Normal * Thickness / 2;
			var v = new Renderer.Vertex[4];
			v[0] = new Renderer.Vertex { Pos = a - n, Color = GlobalColor };
			v[1] = new Renderer.Vertex { Pos = b - n, Color = GlobalColor };
			v[2] = new Renderer.Vertex { Pos = b + n, Color = GlobalColor };
			v[3] = new Renderer.Vertex { Pos = a + n, Color = GlobalColor };
			Renderer.DrawTriangleFan(null, null, v, v.Length);
		}
	}
}
