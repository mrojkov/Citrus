namespace Lime
{
	public class WidgetAdapter3D : Node3D, IRenderObject3D
	{
		private Widget widget;
		private RenderChain renderChain = new RenderChain();

		public bool BackFaceCullingEnabled { get; set; }

		public Widget Widget
		{
			get { return widget; }
			set
			{
				if (widget != value) {
					widget = value;
					if (Nodes.Count != 0) {
						Nodes[0] = widget;
					} else {
						Nodes.Add(widget);
					}
				}
			}
		}

		public Vector3 Center
		{
			get { return (Vector3)Widget.Center * GlobalTransform; }
		}

		public WidgetAdapter3D()
		{
			BackFaceCullingEnabled = true;
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (GloballyVisible && widget != null) {
				chain.Add(this, Layer);
			}
		}

		public override void Render()
		{
			widget.AddToRenderChain(renderChain);
			var oldZTestEnabled = Renderer.ZTestEnabled;
			var oldCullMode = Renderer.CullMode;
			var oldProj = Renderer.Projection;
			Renderer.ZTestEnabled = false;
			Renderer.CullMode = BackFaceCullingEnabled ? CullMode.None : CullMode.CullClockwise;
			Renderer.Projection = GlobalTransform * oldProj;
			renderChain.RenderAndClear();
			Renderer.Flush();
			Renderer.Projection = oldProj;
			Renderer.CullMode = oldCullMode;
			Renderer.ZTestEnabled = oldZTestEnabled;
		}
	}
}
