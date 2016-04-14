namespace Lime
{
	public interface IHitProcessor
	{
		void PerformHitTest(Node node, IPresenter presenter);
	}

	public class DefaultHitProcessor : IHitProcessor
	{
		public static readonly DefaultHitProcessor Instance = new DefaultHitProcessor();

		public void PerformHitTest(Node node, IPresenter presenter)
		{
			if (presenter.PerformHitTest(node, Window.Current.Input.MousePosition)) {
				WidgetContext.Current.NodeUnderCursor = node;
			}
		}
	}

	public class NullHitProcessor : IHitProcessor
	{
		public static readonly NullHitProcessor Instance = new NullHitProcessor();

		public void PerformHitTest(Node node, IPresenter presenter) { }
	}
}
