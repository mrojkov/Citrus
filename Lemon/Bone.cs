using Lime;
using ProtoBuf;

namespace Lemon
{
	[ProtoContract]
    public struct BoneWeight
    {
		[ProtoMember(1)]
        public int Index;
		[ProtoMember(2)]
        public float Weight;
    }
	 
	[ProtoContract]
	public class SkinningWeights
    {
		[ProtoMember(1)]
        public BoneWeight Bone0;
		
		[ProtoMember(2)]
        public BoneWeight Bone1;
		
		[ProtoMember(3)]
        public BoneWeight Bone2;
		
		[ProtoMember(4)]
        public BoneWeight Bone3;
    }

	[ProtoContract]
    public class Bone : Node
    {
        [ProtoMember(1)]
        public Vector2 Position { get; set; }

        [ProtoMember(2)]
        public float Rotation { get; set; }

        [ProtoMember(3)]
        public float Length { get; set; }

        [ProtoMember(4)]
        public bool IKStopper { get; set; }

        [ProtoMember(5)]
        public int Index { get; set; }

        [ProtoMember(6)]
        public int BaseIndex { get; set; }

        [ProtoMember(7)]
        public float EffectiveRadius { get; set; }

        [ProtoMember(8)]
        public float FadeoutZone { get; set; }

        [ProtoMember(9)]
        public Vector2 RefPosition { get; set; }

        [ProtoMember(10)]
        public float RefRotation { get; set; }

        [ProtoMember(11)]
        public float RefLength { get; set; }

        public Bone()
        {
            Length = 100;
            EffectiveRadius = 100;
            FadeoutZone = 50;
            IKStopper = true;
        }

        public override void Update(int delta)
        {
            base.Update(delta);
            if (Index > 0 && Parent != null)
            {
                BoneArray.Entry e;
                e.Joint = Position;
                e.Rotation = Rotation;
                e.Length = Length;
                if (BaseIndex > 0)
                {
                    // Tie the bone to the parent bone.
                    BoneArray.Entry b = Parent.Widget.BoneArray[BaseIndex];
                    float l = Utils.ClipAboutZero(b.Length);
                    Vector2 u = b.Tip - b.Joint;
                    Vector2 v = new Vector2(-u.Y / l, u.X / l);
                    e.Joint = b.Tip + u * Position.X + v * Position.Y;
                    e.Rotation += b.Rotation;
                }
                // Get position of bone's tip.
                e.Tip = Vector2.Rotate(new Vector2(e.Length, 0), e.Rotation * Utils.DegreesToRadians) + e.Joint;
                if (RefLength != 0)
                {
                    float relativeScaling = Length / Utils.ClipAboutZero(RefLength);
                    // Calculating the matrix of relative transformation.
                    Matrix32 m1, m2;
                    m1 = Matrix32.Transformation(Vector2.Zero, Vector2.One, RefRotation * Utils.DegreesToRadians, RefPosition);
                    m2 = Matrix32.Transformation(Vector2.Zero, new Vector2(relativeScaling, 1), e.Rotation * Utils.DegreesToRadians, e.Joint);
                    e.RelativeTransform = m1.CalcInversed() * m2;
                }
                else
                    e.RelativeTransform = Matrix32.Identity;
                Parent.Widget.BoneArray[Index] = e;
            }
        }
    }
}
