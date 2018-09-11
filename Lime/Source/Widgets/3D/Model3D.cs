using System.Linq;
using Yuzu;

namespace Lime
{
	[TangerineRegisterNode(CanBeRoot = true, Order = 22)]
	[TangerineVisualHintGroup("/All/Nodes/3D")]
	public class Model3D : Node3D
	{
		[YuzuAfterDeserialization]
		public void AfterDeserialization()
		{
			RebuildSkeleton();
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
	}
}
