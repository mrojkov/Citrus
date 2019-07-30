using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class DissolveComponent : MaterialComponent<DissolveMaterial>
	{
		[YuzuMember]
		public Vector2 Range
		{
			get => CustomMaterial.Range;
			set => CustomMaterial.Range = value;
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

		protected override void OnOwnerChanged(Node oldOwner)
		{
			base.OnOwnerChanged(oldOwner);
			if (Owner != null) {
				CustomMaterial.BlendingGetter = () => Owner.AsWidget.Blending;
			}
		}
	}
}
