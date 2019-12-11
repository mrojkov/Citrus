using System;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	[TangerineRegisterNode(Order = 14)]
	[TangerineAllowedChildrenTypes(typeof(DistortionMeshPoint))]
	[TangerineVisualHintGroup("/All/Nodes/Images", "Distortion Mesh")]
	public class DistortionMesh : Widget
	{
		[YuzuMember]
		[TangerineStaticProperty]
		public int NumCols { get; set; }

		[YuzuMember]
		[TangerineStaticProperty]
		public int NumRows { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(4)]
		public override ITexture Texture { get; set; }

		public DistortionMesh()
		{
			Presenter = DefaultPresenter.Instance;
			NumCols = 2;
			NumRows = 2;
			Texture = new SerializableTexture();
		}

		public DistortionMeshPoint GetPoint(int row, int col)
		{
			if (row < 0 || col < 0 || row > NumRows || col > NumCols)
				return null;
			int i = row * (NumCols + 1) + col;
			return Nodes[i] as DistortionMeshPoint;
		}

		protected static DistortionMeshPoint[] points = new DistortionMeshPoint[4];

		public override void AddToRenderChain(RenderChain chain)
		{
			if (GloballyVisible) {
				// Need to recalculate points transformed positions in order to refresh widget's BoundingRect.
				for (var n = FirstChild; n != null; n = n.NextSibling) {
					((DistortionMeshPoint) n).RecalcTransformedPositionIfNeeded();
				}
				if (ClipRegionTest(chain.ClipRegion)) {
					AddSelfAndChildrenToRenderChain(chain, Layer);
				}
			}
		}

		protected internal override bool PartialHitTestByContents(ref HitTestArgs args)
		{
			Vector2 localPoint = LocalToWorldTransform.CalcInversed().TransformVector(args.Point);
			Vector2 size = Size;
			if (size.X < 0) {
				localPoint.X = -localPoint.X;
				size.X = -size.X;
			}
			if (size.Y < 0) {
				localPoint.Y = -localPoint.Y;
				size.Y = -size.Y;
			}
			for (int i = 0; i < (NumRows + 1) * NumCols; i++) {
				if ((i + 1) % (NumCols + 1) == 0) {
					continue;
				}

				var n1 = (DistortionMeshPoint)Nodes[i];
				var n2 = (DistortionMeshPoint)Nodes[i + NumCols + 1];
				var n3 = (DistortionMeshPoint)Nodes[i + NumCols + 2];
				var n4 = (DistortionMeshPoint)Nodes[i + 1];
				var v1 = (n1.TransformedPosition, n1.UV);
				var v2 = (n2.TransformedPosition, n2.UV);
				var v3 = (n3.TransformedPosition, n3.UV);
				var v4 = (n4.TransformedPosition, n4.UV);
				var center = (
					(v1.TransformedPosition + v2.TransformedPosition + v3.TransformedPosition + v4.TransformedPosition) * .25f,
					(v1.UV + v2.UV + v3.UV + v4.UV) * .25f
				);
				if (
					HitTest(localPoint, v1, center, v2) || HitTest(localPoint, v2, center, v3) ||
					HitTest(localPoint, v3, center, v4) || HitTest(localPoint, v4, center, v1)
				) {
					return true;
				}
			}
			return false;
		}

		private bool HitTest(Vector2 point, (Vector2 Position, Vector2 UV) v1, (Vector2 Position, Vector2 UV) v2, (Vector2 Position, Vector2 UV) v3)
		{
			if (
				TryCalculateBarycentricCoordinates(point, v1.Position, v2.Position, v3.Position, out var w1, out var w2, out var w3) &&
				w1 >= 0 && w2 >= 0 && w1 + w2 < 1
			) {
				var pointUV = w1 * v1.UV + w2 * v2.UV + w3 * v3.UV;
				return !Texture.IsTransparentPixel(
					(int)(Texture.ImageSize.Width * pointUV.X), (int)(Texture.ImageSize.Height * pointUV.Y));
			}
			return false;
		}

		private static bool TryCalculateBarycentricCoordinates(Vector2 point, Vector2 v1, Vector2 v2, Vector2 v3,
			out float w1, out float w2, out float w3)
		{
			w1 = w2 = w3 = 0;
			var det = ((v2.Y - v3.Y) * (v1.X - v3.X) + (v3.X - v2.X) * (v1.Y - v3.Y));
			if (Math.Abs(det) < float.Epsilon) {
				return false;
			}
			w1 = ((v2.Y - v3.Y) * (point.X - v3.X) + (v3.X - v2.X) * (point.Y - v3.Y)) / det;
			w2 = ((v3.Y - v1.Y) * (point.X - v3.X) + (v1.X - v3.X) * (point.Y - v3.Y)) / det;
			w3 = 1 - w1 - w2;
			return true;
		}

		protected internal override Lime.RenderObject GetRenderObject()
		{
			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.CaptureRenderState(this);
			ro.Texture = Texture;
			for (var n = FirstChild; n != null; n = n.NextSibling) {
				var p = (DistortionMeshPoint)n;
				ro.Vertices.Add(new Vertex {
					Pos = p.TransformedPosition,
					Color = p.Color * GlobalColor,
					UV1 = p.UV
				});
			}
			for (int i = 0; i < NumRows; ++i) {
				for (int j = 0; j < NumCols; ++j) {
					var t = i * (NumCols + 1) + j;
					ro.Indices.Add(t);
					ro.Indices.Add(t + NumCols + 1);
					ro.Indices.Add(t + NumCols + 2);
					ro.Indices.Add(t + 1);
				}
			}
			return ro;
		}

		private class RenderObject : WidgetRenderObject
		{
			private static Vertex[] polygon = new Vertex[6];

			public readonly List<Vertex> Vertices = new List<Vertex>();
			public readonly List<int> Indices = new List<int>();
			public ITexture Texture;

			protected override void OnRelease()
			{
				Texture = null;
				Vertices.Clear();
				Indices.Clear();
			}

			public override void Render()
			{
				PrepareRenderState();
				for (int i = 0; i < Indices.Count; i += 4) {
					polygon[0] = CalculateCenterVertex(i);
					for (int t = 0; t < 5; t++) {
						polygon[t + 1] = Vertices[Indices[i + (t % 4)]];
					}
					Renderer.DrawTriangleFan(Texture, polygon, 6);
				}
			}

			protected Vertex CalculateCenterVertex(int index)
			{
				var v = new Vertex();
				Vector2 colorAR, colorGB;
				colorAR = colorGB = Vector2.Zero;
				for (int t = index; t < index + 4; t++) {
					var p = Vertices[Indices[t]];
					v.UV1 += p.UV1;
					v.Pos += p.Pos;
					colorAR.X += p.Color.A;
					colorAR.Y += p.Color.R;
					colorGB.X += p.Color.G;
					colorGB.Y += p.Color.B;
				}
				Vector2 k = new Vector2(0.25f, 0.25f);
				colorAR *= k;
				colorGB *= k;
				v.Color = new Color4((byte)colorAR.Y, (byte)colorGB.X, (byte)colorGB.Y, (byte)colorAR.X);
				v.UV1 *= k;
				v.Pos *= k;
				return v;
			}
		}
	}
}
