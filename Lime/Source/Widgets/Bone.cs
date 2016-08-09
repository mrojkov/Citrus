using ProtoBuf;
using Yuzu;

namespace Lime
{
	/// <summary>
	/// Хранит номер кости и силу влияния
	/// </summary>
	[ProtoContract]
	public struct BoneWeight
	{
		/// <summary>
		/// Номер кости
		/// </summary>
		[ProtoMember(1)]
		[YuzuMember]
		public int Index;

		/// <summary>
		/// Сила влияния кости
		/// </summary>
		[ProtoMember(2)]
		[YuzuMember]
		public float Weight;
	}

	/// <summary>
	/// Содержит информацию о степени влияния костей на точку DistortionMesh
	/// Поддерживается влияние 4 костей одновременно
	/// </summary>
	[ProtoContract]
	public class SkinningWeights
	{
		[ProtoMember(1)]
		[YuzuMember]
		public BoneWeight Bone0;

		[ProtoMember(2)]
		[YuzuMember]
		public BoneWeight Bone1;

		[ProtoMember(3)]
		[YuzuMember]
		public BoneWeight Bone2;

		[ProtoMember(4)]
		[YuzuMember]
		public BoneWeight Bone3;
	}

	/// <summary>
	/// Кость. Управляет движением точек DistortionMesh
	/// </summary>
	[ProtoContract]
	public class Bone : Node
	{
		/// <summary>
		/// Позиция в сцене
		/// </summary>
		[ProtoMember(1)]
		[YuzuMember]
		public Vector2 Position { get; set; }

		/// <summary>
		/// Угол поворота кости в градусах по часовой стрелке
		/// </summary>
		[ProtoMember(2)]
		[YuzuMember]
		public float Rotation { get; set; }

		/// <summary>
		/// Длина кости
		/// </summary>
		[ProtoMember(3)]
		[YuzuMember]
		public float Length { get; set; }

		/// <summary>
		/// Ограничитель обратной кинематики.
		/// Обратная кинематика не будет распространяться на кость-родитель и далее
		/// </summary>
		[ProtoMember(4)]
		[YuzuMember]
		public bool IKStopper { get; set; }

		/// <summary>
		/// Порядковый номер кости в сцене
		/// </summary>
		[ProtoMember(5)]
		[YuzuMember]
		public int Index { get; set; }

		/// <summary>
		/// Номер родительской кости
		/// </summary>
		[ProtoMember(6)]
		[YuzuMember]
		public int BaseIndex { get; set; }

		/// <summary>
		/// Область влияния, в которой кость оказывает максимальный эффект
		/// </summary>
		[ProtoMember(7)]
		[YuzuMember]
		public float EffectiveRadius { get; set; }

		/// <summary>
		/// Область влияния, в которой кость оказывает минимальный эффект
		/// </summary>
		[ProtoMember(8)]
		[YuzuMember]
		public float FadeoutZone { get; set; }

		[ProtoMember(9)]
		[YuzuMember]
		public Vector2 RefPosition { get; set; }

		[ProtoMember(10)]
		[YuzuMember]
		public float RefRotation { get; set; }

		[ProtoMember(11)]
		[YuzuMember]
		public float RefLength { get; set; }

		public Bone()
		{
			Length = 100;
			EffectiveRadius = 100;
			FadeoutZone = 50;
			IKStopper = true;
		}

		protected override void SelfLateUpdate(float delta)
		{
			if (Index > 0 && Parent != null) {
				BoneArray.Entry e;
				e.Joint = Position;
				e.Rotation = Rotation;
				e.Length = Length;
				if (BaseIndex > 0) {
					// Tie the bone to the parent bone.
					BoneArray.Entry b = Parent.AsWidget.BoneArray[BaseIndex];
					float l = ClipAboutZero(b.Length);
					Vector2 u = b.Tip - b.Joint;
					Vector2 v = new Vector2(-u.Y / l, u.X / l);
					e.Joint = b.Tip + u * Position.X + v * Position.Y;
					e.Rotation += b.Rotation;
				}
				// Get position of bone's tip.
				e.Tip = Vector2.RotateDegRough(new Vector2(e.Length, 0), e.Rotation) + e.Joint;
				if (RefLength != 0) {
					float relativeScaling = Length / ClipAboutZero(RefLength);
					// Calculating the matrix of relative transformation.
					Matrix32 m1, m2;
					m1 = Matrix32.Transformation(Vector2.Zero, Vector2.One, RefRotation * Mathf.DegToRad, RefPosition);
					m2 = Matrix32.Transformation(Vector2.Zero, new Vector2(relativeScaling, 1), e.Rotation * Mathf.DegToRad, e.Joint);
					e.RelativeTransform = m1.CalcInversed() * m2;
				} else
					e.RelativeTransform = Matrix32.Identity;
				Parent.AsWidget.BoneArray[Index] = e;
				Parent.PropagateDirtyFlags(DirtyFlags.Transform);
			}
		}

		static float ClipAboutZero(float value, float eps = 0.0001f)
		{
			if (value > -eps && value < eps)
				return eps < 0 ? -eps : eps;
			else
				return value;
		}
	}
}
