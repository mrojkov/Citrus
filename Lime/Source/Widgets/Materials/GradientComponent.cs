using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class GradientComponent : MaterialComponent<GradientMaterial>
	{
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

		protected override void OnOwnerChanged(Node oldOwner)
		{
			base.OnOwnerChanged(oldOwner);
			if (Owner != null) {
				CustomMaterial.BlendStateGetter = () => Owner.AsWidget.Blending.GetBlendState();
			}
		}
	}
}
