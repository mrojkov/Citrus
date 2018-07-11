
namespace Lime
{
	[TangerineRegisterNode(CanBeRoot = true, Order = 24)]
	[TangerineNodeBuilder("BuildForTangerine")]
	public class WidgetAdapter3D : Node3D
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

		public WidgetAdapter3D()
		{
			Presenter = DefaultPresenter.Instance;
			Opaque = false;
		}

		private void BuildForTangerine()
		{
			var root = new Frame {
				Id = "Root",
				Pivot = Vector2.Half,
				Size = Vector2.One
			};
			Nodes.Add(root);
		}

		public override float CalcDistanceToCamera(Camera3D camera)
		{
			if (Widget == null) {
				return 0f;
			}
			var p = GlobalTransform.TransformVector((Vector3)(Widget.Position * new Vector2(1, -1)));
			return camera.View.TransformVector(p).Z;
		}

		public Plane GetPlane()
		{
			return new Plane(new Vector3(0, 0, 1), 0).Transform(GlobalTransform);
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (GloballyVisible) {
				AddSelfToRenderChain(chain, Layer);
			}
		}

		public override void Render()
		{
			if (Widget == null) {
				return;
			}
			Widget.RenderChainBuilder?.AddToRenderChain(renderChain);
			var oldCullMode = Renderer.CullMode;
			var oldWorld = Renderer.World;
			Renderer.CullMode = CullMode.None;
			Renderer.World = Matrix44.CreateScale(new Vector3(1, -1, 1)) * GlobalTransform;
			renderChain.RenderAndClear();
			Renderer.Flush();
			Renderer.World = oldWorld;
			Renderer.CullMode = oldCullMode;
		}

		internal protected override bool PartialHitTest(ref HitTestArgs args)
		{
			if (Widget == null) {
				return false;
			}
			var plane = GetPlane();
			var ray = args.Ray;
			var distance = ray.Intersects(plane);
			if (distance.HasValue && distance <= args.Distance) {
				var oldPoint = args.Point;
				try {
					args.Point = (Vector2)GlobalTransformInverse.TransformVector(ray.Position + ray.Direction * distance.Value) * new Vector2(1, -1);
					Widget.RenderChainBuilder?.AddToRenderChain(renderChain);
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
