using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class GradientComponent : MaterialComponent<GradientMaterial>
	{
		private ITexture lastFilter;

		[YuzuMember]
		public float Angle
		{
			get => CustomMaterial.Angle;
			set => CustomMaterial.Angle = value;
		}

		[YuzuMember]
		public BlendMode BlendMode
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
			if (oldOwner != null) {
				var image = (Image)oldOwner;
				image.FilterTexture = lastFilter;
			}
			if (Owner != null) {
				var image = (Image)Owner;
				lastFilter = image.FilterTexture;
				image.FilterTexture = CustomMaterial.GradientTexture;
				CustomMaterial.BlendStateGetter = () => Owner.AsWidget.Blending.GetBlendState();
			}
		}
	}
}
