using Lime;
using ProtoBuf;

namespace Lemon
{
	[ProtoContract]
    public class DistortionMesh : Widget
    {
        [ProtoMember(1)]
        public int NumCols { get; set; }

        [ProtoMember(2)]
        public int NumRows { get; set; }

        [ProtoMember(3)]
        public PersistentTexture Texture { get; set; }

        DistortionMeshPoint GetPoint(int row, int col)  
        {
            int i = row * (NumCols + 1) + col;
            if (i < 0 || i > Nodes.Count)
                return null;
            return Nodes[i] as DistortionMeshPoint;
        }

        static Renderer.Vertex[] quad = new Renderer.Vertex[4];
        static DistortionMeshPoint[] points = new DistortionMeshPoint[4];

        public override void Render()
        {
            Renderer.Instance.Blending = WorldBlending;
            Renderer.Instance.WorldMatrix = WorldMatrix;
            for (int i = 0; i < NumRows; ++i)
            {
                for (int j = 0; j < NumCols; ++j)
                {
                    points[0] = GetPoint(i, j);
                    points[1] = GetPoint(i, j + 1);
                    points[2] = GetPoint(i + 1, j + 1);
                    points[3] = GetPoint(i + 1, j);
                    if (points[0] != null && points[1] != null && points[2] != null && points[3] != null)
                    {
                        for (int t = 0; t < 4; t++)
                        {
                            quad[t].Color = points[t].Color * WorldColor;
                            quad[t].UV1 = points[t].UV;
                            quad[t].Pos = points[t].TransformedPosition;
                        }
                        Renderer.Instance.DrawTriangleFan(Texture, quad, 4);
                    }
                }
            }
        }
    }
}
