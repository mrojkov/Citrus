using System;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	[ProtoInclude(101, typeof(ModelMesh))]
	[ProtoInclude(102, typeof(ModelCamera))]
	public class ModelNode : Node
	{
		private Vector3 scale;
		private Quaternion rotation;
		private Vector3 position;
		private bool visible;

		protected Matrix44 globalTransform;
		protected bool globallyVisible;

		[ProtoMember(1)]
		public bool Visible
		{
			get { return visible; }
			set
			{
				if (visible != value) {
					visible = value;
					PropagateDirtyFlags(DirtyFlags.Visible);
				}
			}
		}

		[ProtoMember(2)]
		public Vector3 Scale
		{
			get { return scale; }
			set
			{
				scale = value;
				PropagateDirtyFlags(DirtyFlags.Transform);
			}
		}

		[ProtoMember(3)]
		public Quaternion Rotation
		{
			get { return rotation; }
			set
			{
				rotation = value;
				PropagateDirtyFlags(DirtyFlags.Transform);
			}
		}

		[ProtoMember(4)]
		public Vector3 Position
		{
			get { return position; }
			set
			{
				position = value;
				PropagateDirtyFlags(DirtyFlags.Transform);
			}
		}

		public Matrix44 GlobalTransform { get { RecalcDirtyGlobals(); return globalTransform; } }
		public bool GloballyVisible { get { RecalcDirtyGlobals(); return globallyVisible; } }

		public ModelNode()
		{
			AsModelNode = this;
			Visible = true;
		}

		protected override void RecalcDirtyGlobalsUsingParents()
		{
			if (IsDirty(DirtyFlags.Transform)) {
				if (Parent.AsModelNode != null) {
					globalTransform = CalcLocalTransform() * Parent.AsModelNode.GlobalTransform;
				} else {
					globalTransform = CalcLocalTransform();
				}
			}
			if (IsDirty(DirtyFlags.Visible)) {
				globallyVisible = Visible;
				if (Parent.AsWidget != null) {
					globallyVisible &= Parent.AsWidget.GloballyVisible;
				} else if (Parent.AsModelNode != null) {
					globallyVisible &= Parent.AsModelNode.GloballyVisible;
				}
			}
		}

		public Matrix44 CalcLocalTransform()
		{
			return Matrix44.CreateScale(scale) * Matrix44.CreateRotation(rotation) * Matrix44.CreateTranslation(position);
		}

		public bool TrySetLocalTransform(Matrix44 transform)
		{
			return transform.Decompose(out scale, out rotation, out position);
		}

		public void SetLocalTransform(Matrix44 transform)
		{
			if (!TrySetLocalTransform(transform)) {
				throw new ArgumentException();
			}
		}

		public MeshHitTestResult HitTest(Vector2 point)
		{
			var vp = GetViewport();
			if (vp == null || vp.Camera == null) {
				throw new Lime.Exception("Viewport or camera isn't set");
			}
			// Get the point coordinates in the normalized screen space <-1, 1>
			var camera = vp.Camera;
			point = vp.LocalToWorldTransform.CalcInversed().TransformVector(point);
			point = (point / vp.Size - Vector2.Half) * 2;
			// Calc the ray direction
			var direction = new Vector3(point.X, -point.Y, -1);
			direction = camera.Projection.CalcInverted().ProjectVector(direction);
			// Rotate the direction according to the camera orientation
			var cameraTransform = camera.GlobalTransform;
			cameraTransform.Translation = Vector3.Zero; // Discard the camera position
			direction = cameraTransform.TransformVector(direction);
			var ray = new Ray() {
				Direction = direction.Normalized,
				Position = Vector3.Zero * camera.GlobalTransform
			};
			return HitTest(ray);
		}

		public ModelViewport GetViewport()
		{
			Node p = Parent;
			while (p != null && !(p is ModelViewport)) {
				p = p.Parent;
			}
			return p as ModelViewport;
		}

		public struct MeshHitTestResult
		{
			public ModelMesh Mesh;
			public float Distance;

			public static readonly MeshHitTestResult Default = new MeshHitTestResult { Distance = float.MaxValue };
		}

		public virtual MeshHitTestResult HitTest(Ray ray)
		{
			var result = MeshHitTestResult.Default;
			if (!GloballyVisible)
				return result;
			foreach (var node in Nodes) {
				if (node.AsModelNode != null) {
					var r = node.AsModelNode.HitTest(ray);
					if (r.Distance < result.Distance) {
						result = r;
					}
				}
			}
			return result;
		}

		protected internal override void PerformHitTest()
		{
			if (!HitTestTarget) {
				return;
			}
			var r = HitTest(Window.Current.Input.MousePosition);
			if (r.Distance < WidgetContext.Current.DistanceToNodeUnderCursor) {
				// TODO: Check Renderer.CurrentFrameBuffer == Renderer.DefaultFrameBuffer
				// TODO: Check Renderer.ScissorTestEnabled and ScissorRectangle
				WidgetContext.Current.DistanceToNodeUnderCursor = r.Distance;
				WidgetContext.Current.NodeUnderCursor = this;
			}
		}
	}
}