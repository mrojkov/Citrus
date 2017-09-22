using System.Linq;
using Yuzu;

namespace Lime
{
	public class Model3D : Node3D
	{
		[YuzuMember]
		public bool LightningEnabled
		{
			get
			{
				return lightningEnabled;
			}
			set
			{
				lightningEnabled = value;
				ResetMaterial();
			}
		}

		private bool lightningEnabled;

		[YuzuAfterDeserialization]
		public void AfterDeserialization()
		{
			RebuildSkeleton();
		}

		protected override void Awake()
		{
			base.Awake();
			ResetMaterial();
		}

		public override Node Clone()
		{
			var model = base.Clone() as Model3D;
			model.RebuildSkeleton();
			return model;
		}

		public void RebuildSkeleton()
		{
			var submeshes = Descendants
				.OfType<Mesh3D>()
				.SelectMany(m => m.Submeshes);
			foreach (var sm in submeshes) {
				sm.RebuildSkeleton(this);
			}
		}

		private void ResetMaterial()
		{
			foreach (var material in Descendants
						.OfType<Mesh3D>()
						.SelectMany(m => m.Submeshes)
						.Select((s) => s.Material)
						.OfType<IMaterialLightning>()) {
				material.ProcessLightning = lightningEnabled;
			}
		}
	}
}