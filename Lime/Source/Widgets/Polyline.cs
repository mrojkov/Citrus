using System.Collections.Generic;

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



		protected internal override Lime.RenderObject GetRenderObject()

		{

			var ro = RenderObjectPool<RenderObject>.Acquire();

			ro.CaptureRenderState(this);

			var t = Matrix32.Identity;

			if (StaticThickness) {

				t = ro.LocalToWorldTransform;

				ro.LocalToWorldTransform = Matrix32.Identity;

			}

			ro.Vertices.Clear();

			foreach (PolylinePoint point in Nodes) {

				ro.Vertices.Add(StaticThickness ? point.TransformedPosition * t : point.TransformedPosition);

			}

			ro.Color = GlobalColor;

			ro.Thickness = Thickness;

			ro.Closed = Closed;

			return ro;

		}



		private class RenderObject : Lime.WidgetRenderObject

		{

			public List<Vector2> Vertices = new List<Vector2>();

			public bool Closed;

			public float Thickness;

			public Color4 Color;



			protected override void OnRelease()

			{

				Vertices.Clear();

			}



			public override void Render()

			{

				PrepareRenderState();

				if (Vertices.Count >= 2) {

					if (!Closed || Vertices.Count == 2) {

						DrawHalfLeft(Vertices[0], Vertices[1]);

						DrawHalfRight(Vertices[Vertices.Count - 1], Vertices[Vertices.Count - 2]);

					}

					else {

						DrawPart(

							Vertices[Vertices.Count - 1],

							Vertices[0],

							Vertices[1]);

						DrawPart(

							Vertices[Vertices.Count - 2],

							Vertices[Vertices.Count - 1],

							Vertices[0]);

					}

					for (int i = 0; i < Vertices.Count - 2; i++) {

						DrawPart(

							Vertices[i],

							Vertices[i + 1],

							Vertices[i + 2]);

					}

				}

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

				if (GeometryUtils.CalcLinesIntersection(a + n1, b + n1, b + n2, c + n2, out var p)) {

					FillJointGap(p, b, n1, n2);

					DrawQuad(a - n1, b - n1, p, a + n1);

					DrawQuad(c - n2, b - n2, p, c + n2);

				}

				else {

					FillJointGap(b, b, n1, n2);

					DrawQuad(a - n1, b - n1, b + n1, a + n1);

					DrawQuad(c - n2, b - n2, b + n2, c + n2);

				}

			}



			private void DrawQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d)

			{

				var v = new Vertex[4];

				v[0] = new Vertex { Pos = a, Color = Color };

				v[1] = new Vertex { Pos = b, Color = Color };

				v[2] = new Vertex { Pos = c, Color = Color };

				v[3] = new Vertex { Pos = d, Color = Color };

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

				v[0] = new Vertex { Pos = p, Color = Color };

				float length = n2.Length;

				for (int i = 0; i < v.Length - 1; i++) {

					float t = (float)i / (v.Length - 2);

					p = -Vector2.CosSinRough(Mathf.Lerp(t, angle1, angle2) * Mathf.DegToRad) * length + b;

					v[i + 1] = new Vertex { Pos = p, Color = Color };

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

				v[0] = new Vertex { Pos = center, Color = Color };

				for (int i = 0; i <= numPoints; i++) {

					float t = (float)i / numPoints;

					Vector2 p = Vector2.CosSinRough(Mathf.Lerp(t, angle1, angle2) * Mathf.DegToRad);

					v[i + 1] = new Vertex { Pos = center + p * Thickness / 2, Color = Color };

				}

				Renderer.DrawTriangleFan(null, null, v, v.Length);

			}



			private void DrawSegment(Vector2 a, Vector2 b)

			{

				Vector2 n = GetVectorNormal(b - a) * Thickness / 2;

				var v = new Vertex[4];

				v[0] = new Vertex { Pos = a - n, Color = Color };

				v[1] = new Vertex { Pos = b - n, Color = Color };

				v[2] = new Vertex { Pos = b + n, Color = Color };

				v[3] = new Vertex { Pos = a + n, Color = Color };

				Renderer.DrawTriangleFan(null, null, v, v.Length);

			}

		}

	}



	[TangerineRegisterNode(Order = 30)]

	[TangerineAllowedParentTypes(typeof(Polyline))]

	public class PolylinePoint : PointObject

	{

	}

}

