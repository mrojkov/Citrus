using Lime;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class DistortionMesh : Widget
	{
		[ProtoMember(1)]
		public int NumCols { get; set; }

		[ProtoMember(2)]
		public int NumRows { get; set; }

		[ProtoMember(3)]
		public SerializableTexture Texture { get; set; }
		
		public DistortionMesh()
		{
			NumCols = 2;
			NumRows = 2;
			Texture = new SerializableTexture();
		}
		
		DistortionMeshPoint GetPoint(int row, int col)
		{
			int i = row * (NumCols + 1) + col;
			if (i < 0 || i >= Nodes.Count)
				return null;
			return Nodes[i] as DistortionMeshPoint;
		}

		static Renderer.Vertex[] polygon = new Renderer.Vertex[6];
		static DistortionMeshPoint[] points = new DistortionMeshPoint[4];
		
		Renderer.Vertex CalculateCenterVertex()
		{
			var v = new Renderer.Vertex();
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
			Vector2 divisor = new Vector2(0.25f, 0.25f);
			colorAR *= divisor;
			colorGB *= divisor;
			v.Color = new Color4((byte)colorAR.Y, (byte)colorGB.X, (byte)colorGB.Y, (byte)colorAR.X) * WorldColor;
			v.UV1 *= divisor;
			v.Pos *= divisor;
			return v;
		}
		
		void RenderTile()
		{
			polygon[0] = CalculateCenterVertex();
			for (int t = 0; t < 5; t++) {
				int w = t % 4;
				polygon[t + 1].Color = points[w].Color * WorldColor;
				polygon[t + 1].UV1 = points[w].UV;
				polygon[t + 1].Pos = points[w].TransformedPosition;
			}
			Renderer.DrawTriangleFan(Texture, polygon, 6);
		}
		
		public override void Render()
		{
			Renderer.Blending = WorldBlending;
			Renderer.WorldMatrix = WorldMatrix;
			for (int i = 0; i < NumRows; ++i) {
				for (int j = 0; j < NumCols; ++j) {
					points[0] = GetPoint(i, j);
					points[1] = GetPoint(i, j + 1);
					points[2] = GetPoint(i + 1, j + 1);
					points[3] = GetPoint(i + 1, j);
					if (points[0] != null && points[1] != null && points[2] != null && points[3] != null) {
						RenderTile();
					}
				}
			}
		}
	}
}
