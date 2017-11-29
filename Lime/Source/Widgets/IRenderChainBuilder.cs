namespace Lime
{
	public interface IRenderChainBuilder
	{
		void AddToRenderChain(RenderChain chain);
		IRenderChainBuilder Clone(Node newOwner);
	}
}