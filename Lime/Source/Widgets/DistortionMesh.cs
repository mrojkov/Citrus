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
			if (GloballyVisible && ClipRegionTest(chain.ClipRegion)) {
				AddSelfAndChildrenToRenderChain(chain, Layer);
			}
		}

		protected internal override Lime.RenderObject GetRenderObject()
		{
			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.CaptureRenderState(this);
			ro.Texture = Texture;
			ro.Vertices.Clear();
			ro.Indices.Clear();
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
