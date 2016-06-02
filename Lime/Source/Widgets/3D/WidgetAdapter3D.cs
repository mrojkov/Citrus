using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class WidgetAdapter3D : Node3D, IRenderObject3D
	{
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
			get { return GlobalTransform.TransformVector((Vector3)(Widget.GlobalCenter * new Vector2(1, -1))); }
		}

		public Plane GetPlane()
		{
			return new Plane(new Vector3(0, 0, 1), 0).Transform(GlobalTransform);
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (GloballyVisible && Widget != null) {
				AddSelfToRenderChain(chain);
			}
		}

		public override void Render()
		{
			var chain = new RenderChain(CreateHitProcessor());
			Widget.AddToRenderChain(chain);
			var oldZTestEnabled = Renderer.ZTestEnabled;
			var oldCullMode = Renderer.CullMode;
			var oldProj = Renderer.Projection;
			Renderer.ZTestEnabled = false;
			Renderer.CullMode = CullMode.None;
			Renderer.Projection = Matrix44.CreateScale(new Vector3(1, -1, 1)) * GlobalTransform * oldProj;
			chain.RenderAndClear();
			Renderer.Flush();
			Renderer.Projection = oldProj;
			Renderer.CullMode = oldCullMode;
			Renderer.ZTestEnabled = oldZTestEnabled;
		}

		private IHitProcessor CreateHitProcessor()
		{
			var plane = GetPlane();
			var ray = WidgetContext.Current.CursorRay;
			var distance = ray.Intersects(plane);
			if (distance != null && distance <= WidgetContext.Current.DistanceToNodeUnderCursor) {
				var mousePosition = (Vector2)GlobalTransform.CalcInverted().TransformVector(ray.Position + ray.Direction * distance.Value) * new Vector2(1, -1);
				return new HitProcessor(mousePosition, distance.Value);
			}
			return NullHitProcessor.Instance;
		}

		private class HitProcessor : IHitProcessor
		{
			private Vector2 mousePosition;
			private float distance;

			public HitProcessor(Vector2 mousePosition, float distance)
			{
				this.mousePosition = mousePosition;
				this.distance = distance;
			}

			public void PerformHitTest(Node node, IPresenter presenter)
			{
				if (presenter.PerformHitTest(node, mousePosition)) {
					WidgetContext.Current.NodeUnderCursor = node;
					WidgetContext.Current.DistanceToNodeUnderCursor = distance;
				}
			}
		}
	}
}
