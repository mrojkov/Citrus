
namespace Lime
{
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
			get { return GlobalTransform.TransformVector((Vector3)(Widget.Position * new Vector2(1, -1))); }
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

		internal protected override bool PartialHitTest(ref HitTestArgs args)
		{
			var plane = GetPlane();
			var ray = args.Ray;
			var distance = ray.Intersects(plane);
			if (distance.HasValue && distance <= args.Distance) {
				var oldPoint = args.Point;
				try {
					args.Point = (Vector2)GlobalTransform.CalcInverted().TransformVector(ray.Position + ray.Direction * distance.Value) * new Vector2(1, -1);
					Widget.AddToRenderChain(renderChain);
					if (renderChain.HitTest(ref args)) {
						args.Distance = distance.Value;
						return true;
					}
				} finally {
					args.Point = oldPoint;
					renderChain.Clear();
				}
			}
			return false;
		}
	}
}
