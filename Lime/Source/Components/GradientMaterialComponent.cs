using Yuzu;

namespace Lime
{
	[TangerineRegisterComponent]
	[AllowedComponentOwnerTypes(typeof(Image))]
	public class GradientMaterialComponent : NodeComponent
	{
		private ColorGradient gradient;
		private IMaterial oldCustomMaterial;
		private Blending blendng;
		private float angle;

		[YuzuMember]
		public float Angle
		{
			get => angle;
			set
			{
				if (angle != value) {
					angle = value;
					RewriteOrSetCustomMaterial();
				}
			}
		}

		[YuzuMember]
		public Blending Blending
		{
			get => blendng;
			set {
				if (blendng != value) {
					blendng = value;
					RewriteOrSetCustomMaterial();
				}
			}
		}

		[YuzuMember]
		public ColorGradient Gradient
		{
			get => gradient;
			set
			{
				if (gradient != value) {
					gradient = value;
					RewriteOrSetCustomMaterial();
				}
			}
		}

		public GradientMaterialComponent()
		{
			Gradient = new ColorGradient(Color4.White, Color4.Black);
			blendng = Blending.Inherited;
		}

		public override NodeComponent Clone()
		{
			var clone = (GradientMaterialComponent)base.Clone();
			clone.Owner = null;
			return clone;
		}

		protected override void OnOwnerChanged(Node oldOwner)
		{
			base.OnOwnerChanged(oldOwner);
			if (oldOwner != null) {
				oldOwner.Awoke -= OnAwake;
				((Image) oldOwner).CustomMaterial = oldCustomMaterial;
			}
			if (Owner != null) {
				Owner.Awoke += OnAwake;
				oldCustomMaterial = ((Image)Owner).CustomMaterial;
				RewriteOrSetCustomMaterial();
			}
		}

		private void OnAwake(Node node)
		{
			RewriteOrSetCustomMaterial();
		}

		private void RewriteOrSetCustomMaterial()
		{
			if (Owner != null) {
				var customMaterial = ((Image) Owner).CustomMaterial;
				var gradientMaterial = customMaterial is GradientMaterial material
					? material
					: new GradientMaterial();
				((Image) Owner).CustomMaterial = gradientMaterial;
				gradientMaterial.Gradient = gradient;
				gradientMaterial.Angle = angle;
				gradientMaterial.Blending = blendng;
			}
		}
	}
}
