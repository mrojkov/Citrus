namespace Lime
{
	public interface IRenderChainBuilder
	{
		void AddToRenderChain(Node node, RenderChain chain);
		IRenderChainBuilder Clone();
	}

	public class DefaultRenderChainBuilder : IRenderChainBuilder
	{
		public static readonly DefaultRenderChainBuilder Instance = new DefaultRenderChainBuilder();

		public void AddToRenderChain(Node node, RenderChain chain)
		{
			node.AddToRenderChain(chain);
		}

		public IRenderChainBuilder Clone()
		{
			return Instance;
		}
	}
}
