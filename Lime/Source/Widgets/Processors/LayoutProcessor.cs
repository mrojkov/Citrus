namespace Lime
{
	public class LayoutProcessor : NodeProcessor
	{
		private LayoutManager layoutManager;

		public override void Start()
		{
			layoutManager = Manager.ServiceProvider.RequireService<LayoutManager>();
		}

		public override void Stop(NodeManager manager)
		{
			layoutManager = null;
		}

		public override void Update(float delta)
		{
			layoutManager.Layout();
		}
	}
}
