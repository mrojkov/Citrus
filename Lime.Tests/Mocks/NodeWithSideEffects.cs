using System;

namespace Lime.Tests.Mocks
{
	class NodeWithSideEffects: Node
	{
		public override void AddToRenderChain(RenderChain chain)
		{
			base.AddToRenderChain(chain);
			chain.Add(this, Layer);
		}
	}
}
