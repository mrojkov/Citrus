using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yuzu;

namespace Lime
{
	/// <summary>
	/// Ломаная линия
	/// </summary>
	[TangerineRegisterNode(Order = 29)]
	[TangerineAllowedChildrenTypes(typeof(PolylinePoint))]
	[TangerineVisualHintGroup("/All/Nodes/Splines")]
	public class Polyline : Widget
	{
		/// <summary>
		/// Толщина
		/// </summary>
		[YuzuMember]
		[TangerineKeyframeColor(30)]
		public float Thickness { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(31)]
		public bool StaticThickness { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(1)]
		public bool Closed { get; set; }

		public Polyline()
		{
			Presenter = DefaultPresenter.Instance;
			Thickness = 1f;
		}

		static Vector2 GetVectorNormal(Vector2 v)
		{
			return new Vector2(-v.Y, v.X).Normalized;
		}

		static List<Vector2> positions = new List<Vector2>();

		//public override void Render()
		//{
		//	var t = Matrix32.Identity;
		//	PrepareRendererState();
		//	if (StaticThickness) {
		//		Renderer.Transform1 = Matrix32.Identity;
		//		t = LocalToWorldTransform;
		//	}
		//	int n = Nodes.Count;
		//	positions.Clear();
		//	foreach (PolylinePoint point in Nodes) {
		//		positions.Add(StaticThickness ? point.TransformedPosition * t : point.TransformedPosition);
		//	}
		//	if (n >= 2) {
		//		if (!Closed || n == 2) {
		//			DrawHalfLeft(positions[0], positions[1]);
		//			DrawHalfRight(positions[n - 1], positions[n - 2]);
		//		} else {
		//			DrawPart(
		//				positions[n - 1],
		//				positions[0],
		//				positions[1]);
		//			DrawPart(
		//				positions[n - 2],
		//				positions[n - 1],
		//				positions[0]);
		//		}
		//		for (int i = 0; i < n - 2; i++) {
		//			DrawPart(
		//				positions[i],
		//				positions[i + 1],
		//				positions[i + 2]);
		//		}
		//	}
		//}

		private static Vector2 GetPosition(Node n)
		{
			return (n as PolylinePoint).TransformedPosition;
		}

		private void DrawHalfLeft(Vector2 p0, Vector2 p1)
		{
			DrawCap(p0, p1);
			DrawSegment(p0, (p0 + p1) / 2);
		}

		private void DrawHalfRight(Vector2 p0, Vector2 p1)
		{
			DrawCap(p0, p1);
			DrawSegment((p0 + p1) / 2, p0);
		}

		private void DrawPart(Vector2 p0, Vector2 p1, Vector2 p3)
		{
			p0 = (p0 + p1) / 2;
			p3 = (p1 + p3) / 2;
			DrawJoint(p0, p1, p3);
		}

		private void DrawJoint(Vector2 a, Vector2 b, Vector2 c)
		{
			Vector2 n1 = GetVectorNormal(b - a) * Thickness / 2;
			Vector2 n2 = GetVectorNormal(c - b) * Thickness / 2;
			if (Vector2.CrossProduct(a - b, c - b) > 0) {
				n1 = -n1;
				n2 = -n2;
			}
			Vector2 p;
			if (GeometryUtils.CalcLinesIntersection(a + n1, b + n1, b + n2, c + n2, out p)) {
				FillJointGap(p, b, n1, n2);
				DrawQuad(a - n1, b - n1, p, a + n1);
				DrawQuad(c - n2, b - n2, p, c + n2);
			} else {
				FillJointGap(b, b, n1, n2);
				DrawQuad(a - n1, b - n1, b + n1, a + n1);
				DrawQuad(c - n2, b - n2, b + n2, c + n2);
			}
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

	[TangerineRegisterNode(Order = 30)]
	[TangerineAllowedParentTypes(typeof(Polyline))]
	public class PolylinePoint : PointObject
	{
	}
}
