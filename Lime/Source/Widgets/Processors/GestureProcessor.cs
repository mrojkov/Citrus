namespace Lime
{
	public class GestureProcessor : NodeProcessor
	{
		private WidgetContext widgetContext;

		public override void Start()
		{
			widgetContext = Manager.ServiceProvider.RequireService<WidgetContext>();
		}

		public override void Stop(NodeManager manager)
		{
			widgetContext = null;
		}

		public override void Update(float delta)
		{
			widgetContext.GestureManager.Update(delta);
		}
	}
}
