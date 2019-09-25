using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class AlphaIntensityComponent : MaterialComponent<AlphaIntensityMaterial>
	{
		[YuzuMember]
		public Blending Blending
		{
			get => CustomMaterial.Blending;
			set => CustomMaterial.Blending = value;
		}

		[YuzuMember]
		public float Radius
		{
			get => 1.0f - CustomMaterial.Radius;
			set => CustomMaterial.Radius = 1.0f - value;
		}

		[YuzuMember]
		public float Brightness
		{
			get => CustomMaterial.Brightness;
			set => CustomMaterial.Brightness = value;
		}

		[YuzuMember]
		public Color4 Color
		{
			get => Color4.FromFloats(
				CustomMaterial.Color.X,
				CustomMaterial.Color.Y,
				CustomMaterial.Color.Z,
				CustomMaterial.Color.W
			);
			set => CustomMaterial.Color = value.ToVector4();
		}

		[YuzuMember]
		public ITexture MaskTexture
		{
			get => CustomMaterial.MaskTexture;
			set => CustomMaterial.MaskTexture = value;
		}
	}
}
