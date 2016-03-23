using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class WidgetAdapter3D : Node3D, IRenderObject3D
	{
		private RenderChain renderChain = new RenderChain();

		public Widget Widget
		{
			get { return Nodes.Count != 0 ? Nodes[0].AsWidget : null; }
			set
			{
				if (value != null) {
					if (Nodes.Count != 0) {
						Nodes[0] = value;
					} else {
						Nodes.Push(value);
					}
				} else {
					Nodes.Clear();
				}
			}
		}

		public Vector3 Center
		{
			get { return (Vector3)Widget.Center * GlobalTransform; }
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (GloballyVisible && Widget != null) {
				chain.Add(this, Layer);
			}
		}

		public override void Render()
		{
			Widget.AddToRenderChain(renderChain);
			var oldZTestEnabled = Renderer.ZTestEnabled;
			var oldCullMode = Renderer.CullMode;
			var oldProj = Renderer.Projection;
			Renderer.ZTestEnabled = false;
			Renderer.CullMode = CullMode.None;
			Renderer.Projection = Matrix44.CreateScale(new Vector3(1, -1, 1)) * GlobalTransform * oldProj;
			renderChain.RenderAndClear();
			Renderer.Flush();
			Renderer.Projection = oldProj;
			Renderer.CullMode = oldCullMode;
			Renderer.ZTestEnabled = oldZTestEnabled;
		}
	}
}
