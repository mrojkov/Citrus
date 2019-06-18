namespace Lime
{
	public class LayoutProcessor : NodeProcessor
	{
		private LayoutManager layoutManager;

		protected internal override void Start()
		{
			layoutManager = Manager.ServiceProvider.RequireService<LayoutManager>();
		}

		protected internal override void Stop()
		{
			layoutManager = null;
		}

		protected internal override void Update(float delta)
		{
			layoutManager.Layout();
		}
	}
}
