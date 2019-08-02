using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	public class TwistComponent : MaterialComponent<TwistMaterial>
	{
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
				CustomMaterial.BlendingGetter = () => Owner.AsWidget.Blending;
			}
		}
	}
}
