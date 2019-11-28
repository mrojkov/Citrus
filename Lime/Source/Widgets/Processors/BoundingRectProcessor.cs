namespace Lime
{
	public class BoundingRectProcessor : NodeProcessor
	{
		public override void Update(float delta)
		{
			if (Widget.EnableViewCulling) {
				foreach (var node in Manager.RootNodes) {
					node.UpdateBoundingRect();
				}
			}
		}
	}
}
