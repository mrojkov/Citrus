using System.Linq;
using Yuzu;

namespace Lime
{
	public class Model3D : Node3D
	{
		[YuzuMember]
		[TangerineKeyframeColor(19)]
		public bool LightningEnabled
		{
			get
			{
				return lightningEnabled;
			}
			set
			{
				lightningEnabled = value;
				ResetLightning();
			}
		}

		[YuzuMember]
		[TangerineKeyframeColor(20)]
		public bool CastShadow
		{
			get
			{
				return castShadow;
			}
			set
			{
				castShadow = value;
				ResetLightning();
			}
		}

		[YuzuMember]
		[TangerineKeyframeColor(21)]
		public bool RecieveShadow
		{
			get
			{
				return recieveShadow;
			}
			set
			{
				recieveShadow = value;
				ResetLightning();
			}
		}

		private bool castShadow;
		private bool recieveShadow;
		private bool lightningEnabled;

		[YuzuAfterDeserialization]
		public void AfterDeserialization()
		{
			RebuildSkeleton();
		}

		protected override void Awake()
		{
			base.Awake();
			ResetLightning();
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

		private void ResetLightning()
		{
			foreach (var mesh in Descendants.OfType<Mesh3D>()) {
				mesh.ProcessLightning = lightningEnabled;
				mesh.RecieveShadow = recieveShadow;
				mesh.CastShadow = castShadow;
			}
		}
	}
}
