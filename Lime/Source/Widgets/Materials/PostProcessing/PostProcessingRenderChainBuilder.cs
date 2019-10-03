namespace Lime
{
	public class PostProcessingRenderChainBuilder : IRenderChainBuilder
	{
		public Widget Owner { get; set; }

		public void AddToRenderChain(RenderChain chain)
		{
			if (Owner == null || Owner.Width <= 0 || Owner.Height <= 0 || !Owner.GloballyVisible || !Owner.ClipRegionTest(chain.ClipRegion)) {
				return;
			}
			if (Owner.PostPresenter != null) {
				chain.Add(Owner, Owner.PostPresenter);
			}
			if (Owner.Presenter != null) {
				chain.Add(Owner, Owner.Presenter);
			}
		}
	}
}
