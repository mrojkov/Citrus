
namespace Lime
{
	[TangerineRegisterNode(CanBeRoot = true, Order = 24)]
	[TangerineNodeBuilder("BuildForTangerine")]
	[TangerineVisualHintGroup("/All/Nodes/3D")]
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

		private float CalcDistanceToCamera(Camera3D camera)
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

		protected internal override Lime.RenderObject GetRenderObject()
		{
			if (Widget == null) {
				return null;
			}
			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.World = Matrix44.CreateScale(new Vector3(1, -1, 1)) * GlobalTransform;
			ro.Opaque = false;
			ro.DistanceToCamera = CalcDistanceToCamera(Viewport.Camera);
			try {
				Widget.RenderChainBuilder?.AddToRenderChain(renderChain);
				ro.Objects.Clear();
				renderChain.GetRenderObjects(ro.Objects);
			} finally {
				renderChain.Clear();
			}
			return ro;
		}

		private class RenderObject : RenderObject3D
		{
			public Matrix44 World;
			public RenderObjectList Objects = new RenderObjectList();

			public override void Render()
			{
				Renderer.PushState(RenderState.World | RenderState.CullMode);
				Renderer.World = World;
				Renderer.CullMode = CullMode.None;
				Objects.Render();
				Renderer.Flush();
				Renderer.PopState();
			}
		}
	}
}
