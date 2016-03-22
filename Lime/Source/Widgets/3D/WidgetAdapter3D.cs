namespace Lime
{
	public class WidgetAdapter3D : Node3D, IRenderObject3D
	{
		private Widget widget;
		
		public bool DoubleSided { get; set; }

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
			DoubleSided = true;
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (GloballyVisible && widget != null) {
				chain.Add(this, Layer);
			}
		}

		public override void Render()
		{
			var oldCullMode = Renderer.CullMode;
			var oldProj = Renderer.Projection;
			Renderer.CullMode = DoubleSided ? CullMode.None : CullMode.CullClockwise;
			Renderer.Projection = GlobalTransform * oldProj;
			widget.PerformHitTest();
			widget.Render();
			Renderer.Flush();
			Renderer.Projection = oldProj;
			Renderer.CullMode = oldCullMode;
		}
	}
}
