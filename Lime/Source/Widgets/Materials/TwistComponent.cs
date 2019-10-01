using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class TwistComponent : MaterialComponent<TwistMaterial>
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

		protected override void OnOwnerChanged(Node oldOwner)
		{
			base.OnOwnerChanged(oldOwner);
			if (Owner != null) {
				var image = (Image)Owner;
				CustomMaterial.UV0 = image.UV0;
				CustomMaterial.UV1 = image.UV1;
				image.Texture.TransformUVCoordinatesToAtlasSpace(ref CustomMaterial.UV0);
				image.Texture.TransformUVCoordinatesToAtlasSpace(ref CustomMaterial.UV1);
			}
		}
	}
}
