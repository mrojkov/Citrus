using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	/// <summary>
	/// Ломаная линия
	/// </summary>
	public class Polyline : Widget
	{
		/// <summary>
		/// Толщина
		/// </summary>
		public float Thickness;

		/// <summary>
		/// Точки, по которым строится линия
		/// </summary>
		public List<Vector2> Points = new List<Vector2>();

		public Polyline(float thickness = 20)
		{
			this.Thickness = thickness;
		}

		static Vector2 GetVectorNormal(Vector2 v)
		{
			return new Vector2(-v.Y, v.X).Normalized;
		}

		public override void Render()
		{
			Renderer.Blending = GlobalBlending;
			Renderer.Shader = GlobalShader;
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
			Vector2 n1 = GetVectorNormal(b - a) * Thickness / 2;
			Vector2 n2 = GetVectorNormal(c - b) * Thickness / 2;
			if (Vector2.CrossProduct(a - b, c - b) > 0) {
				n1 = -n1;
				n2 = -n2;
			}
			Vector2 p = b;
			GeometryUtils.CalcLinesIntersection(a + n1, b + n1, b + n2, c + n2, ref p);
			DrawQuad(a - n1, b - n1, p, a + n1);
			DrawQuad(c - n2, b - n2, p, c + n2);
			FillJointGap(p, b, n1, n2);
		}

		private void DrawQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
		{
			var v = new Vertex[4];
			v[0] = new Vertex { Pos = a, Color = GlobalColor };
			v[1] = new Vertex { Pos = b, Color = GlobalColor };
			v[2] = new Vertex { Pos = c, Color = GlobalColor };
			v[3] = new Vertex { Pos = d, Color = GlobalColor };
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
			var v = new Vertex[10];
			v[0] = new Vertex { Pos = p, Color = GlobalColor };
			float length = n2.Length;
			for (int i = 0; i < v.Length - 1; i++) {
				float t = (float)i / (v.Length - 2);
				p = -Vector2.CosSinRough(Mathf.Lerp(t, angle1, angle2) * Mathf.DegToRad) * length + b;
				v[i + 1] = new Vertex { Pos = p, Color = GlobalColor };
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
			var v = new Vertex[numPoints + 2];
			v[0] = new Vertex { Pos = center, Color = GlobalColor };
			for (int i = 0; i <= numPoints; i++) {
				float t = (float)i / numPoints;
				Vector2 p = Vector2.CosSinRough(Mathf.Lerp(t, angle1, angle2) * Mathf.DegToRad);
				v[i + 1] = new Vertex { Pos = center + p * Thickness / 2, Color = GlobalColor };
			}
			Renderer.DrawTriangleFan(null, null, v, v.Length);
		}

		private void DrawSegment(Vector2 a, Vector2 b)
		{
			Vector2 n = GetVectorNormal(b - a) * Thickness / 2;
			var v = new Vertex[4];
			v[0] = new Vertex { Pos = a - n, Color = GlobalColor };
			v[1] = new Vertex { Pos = b - n, Color = GlobalColor };
			v[2] = new Vertex { Pos = b + n, Color = GlobalColor };
			v[3] = new Vertex { Pos = a + n, Color = GlobalColor };
			Renderer.DrawTriangleFan(null, null, v, v.Length);
		}
	}
}
