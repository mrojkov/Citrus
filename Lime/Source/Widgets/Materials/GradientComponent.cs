using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class GradientComponent : MaterialComponent<GradientMaterial>
	{
		[YuzuMember]
		public Blending Blending
		{
			get => CustomMaterial.Blending;
			set => CustomMaterial.Blending = value;
		}

		[YuzuMember]
		public float Angle
		{
			get => CustomMaterial.Angle;
			set => CustomMaterial.Angle = value;
		}

		[YuzuMember]
		public GradientMaterialBlendMode BlendMode
		{
			get => CustomMaterial.BlendMode;
			set => CustomMaterial.BlendMode = value;
		}

		[YuzuMember]
		public ColorGradient Gradient
		{
			get => CustomMaterial.Gradient;
			set => CustomMaterial.Gradient = value;
		}
	}
}
