namespace Lime
{
	public class GestureProcessor : NodeProcessor
	{
		private GestureManager gestureManager;

		protected internal override void Start()
		{
			gestureManager = Manager.ServiceProvider.RequireService<GestureManager>();
		}

		protected internal override void Stop()
		{
			gestureManager = null;
		}

		protected internal override void Update(float delta)
		{
			gestureManager.Process();
		}
	}
}
