using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class HSLComponent : MaterialComponent<ColorCorrectionMaterial>
	{
		[YuzuMember]
		public float Hue
		{
			get => CustomMaterial.HSL.X * 360.0f;
			set {
				var hsl = CustomMaterial.HSL;
				hsl.X = value / 360.0f;
				CustomMaterial.HSL = hsl;
			}
		}

		[YuzuMember]
		public float Saturation
		{
			get => CustomMaterial.HSL.Y * 100.0f;
			set {
				var hsl = CustomMaterial.HSL;
				hsl.Y = value / 100.0f;
				CustomMaterial.HSL = hsl;
			}
		}

		[YuzuMember]
		public float Lightness
		{
			get => CustomMaterial.HSL.Z * 100.0f;
			set {
				var hsl = CustomMaterial.HSL;
				hsl.Z = value / 100.0f;
				CustomMaterial.HSL = hsl;
			}
		}
	}
}
