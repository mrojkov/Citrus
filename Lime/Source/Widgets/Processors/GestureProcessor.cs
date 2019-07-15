namespace Lime
{
	public class GestureProcessor : NodeProcessor
	{
		private WidgetContext widgetContext;

		protected internal override void Start()
		{
			widgetContext = Manager.ServiceProvider.RequireService<WidgetContext>();
		}

		protected internal override void Stop()
		{
			widgetContext = null;
		}

		protected internal override void Update(float delta)
		{
			widgetContext.GestureManager.Process();
		}
	}
}
