using Yuzu;

namespace Lime
{
	[AllowedChildrenTypes(typeof(DistortionMeshPoint))]
	public class DistortionMesh : Widget
	{
		[YuzuMember]
		[TangerineStaticProperty]
		public int NumCols { get; set; }

		[YuzuMember]
		[TangerineStaticProperty]
		public int NumRows { get; set; }

		[YuzuMember]
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

		protected static Vertex[] polygon = new Vertex[6];
		protected static DistortionMeshPoint[] points = new DistortionMeshPoint[4];

		internal protected override void AddToRenderChain(RenderChain chain)
		{
			if (GloballyVisible && ClipRegionTest(chain.ClipRegion)) {
				AddSelfAndChildrenToRenderChain(chain);
			}
		}

		protected Vertex CalculateCenterVertex()
		{
			var v = new Vertex();
			v.UV1 = Vector2.Zero;
			v.Pos = Vector2.Zero;
			Vector2 colorAR, colorGB;
			colorAR = colorGB = Vector2.Zero;
			for (int t = 0; t < 4; t++) {
				v.UV1 += points[t].UV;
				v.Pos += points[t].TransformedPosition;
				colorAR.X += points[t].Color.A;
				colorAR.Y += points[t].Color.R;
				colorGB.X += points[t].Color.G;
				colorGB.Y += points[t].Color.B;
			}
			Vector2 k = new Vector2(0.25f, 0.25f);
			colorAR *= k;
			colorGB *= k;
			v.Color = new Color4((byte)colorAR.Y, (byte)colorGB.X, (byte)colorGB.Y, (byte)colorAR.X) * GlobalColor;
			v.UV1 *= k;
			v.Pos *= k;
			return v;
		}

		protected virtual void RenderTile()
		{
			polygon[0] = CalculateCenterVertex();
			for (int t = 0; t < 5; t++) {
				int w = t % 4;
				polygon[t + 1].Color = points[w].Color * GlobalColor;
				polygon[t + 1].UV1 = points[w].UV;
				polygon[t + 1].Pos = points[w].TransformedPosition;
			}
			Renderer.DrawTriangleFan(Texture, polygon, 6);
		}

		public override void Render()
		{
			Renderer.Blending = GlobalBlending;
			Renderer.Shader = GlobalShader;
			Renderer.Transform1 = LocalToWorldTransform;
			for (int i = 0; i < NumRows; ++i) {
				for (int j = 0; j < NumCols; ++j) {
					points[0] = GetPoint(i, j);
					points[1] = GetPoint(i + 1, j);
					points[2] = GetPoint(i + 1, j + 1);
					points[3] = GetPoint(i, j + 1);
					if (points[0] != null && points[1] != null && points[2] != null && points[3] != null) {
						RenderTile();
					}
				}
			}
		}
	}
}
