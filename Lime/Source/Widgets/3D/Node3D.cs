using System;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	[ProtoInclude(101, typeof(Mesh3D))]
	[ProtoInclude(102, typeof(Camera3D))]
	[ProtoInclude(103, typeof(WidgetAdapter3D))]
	[ProtoInclude(104, typeof(Spline3D))]
	public class Node3D : Node
	{
		private Vector3 scale;
		private Quaternion rotation;
		private Vector3 position;
		private bool visible;
		private Color4 color;

		protected Matrix44 globalTransform;
		protected bool globallyVisible;
		protected Color4 globalColor;

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

		[ProtoMember(5)]
		public Color4 Color
		{
			get { return color; }
			set
			{
				color = value;
				PropagateDirtyFlags(DirtyFlags.Color);
			}
		}

		public Matrix44 GlobalTransform { get { RecalcDirtyGlobals(); return globalTransform; } }
		public bool GloballyVisible { get { RecalcDirtyGlobals(); return globallyVisible; } }
		public Color4 GlobalColor { get { RecalcDirtyGlobals(); return globalColor; } }

		public bool IsMouseOver()
		{
			return WidgetContext.Current.NodeUnderCursor == this ||
				IsAncestorOf(WidgetContext.Current.NodeUnderCursor);
		}

		public Node3D()
		{
			AsModelNode = this;
			scale = Vector3.One;
			rotation = Quaternion.Identity;
			visible = true;
			color = Color4.White;
		}

		protected override void RecalcDirtyGlobalsUsingParents()
		{
			if (IsDirty(DirtyFlags.Transform)) {
				globalTransform = CalcLocalTransform();
				if (Parent != null && Parent.AsModelNode != null) {
					globalTransform *= Parent.AsModelNode.GlobalTransform;
				}
			}
			if (IsDirty(DirtyFlags.Visible | DirtyFlags.Color)) {
				globallyVisible = Visible;
				globalColor = color;
				if (Parent != null) {
					if (Parent.AsWidget != null) {
						globallyVisible &= Parent.AsWidget.GloballyVisible;
						globalColor *= Parent.AsWidget.GlobalColor;
					} else if (Parent.AsModelNode != null) {
						globallyVisible &= Parent.AsModelNode.GloballyVisible;
						globalColor *= Parent.AsModelNode.GlobalColor;
					}
				}
			}
		}

		public Matrix44 CalcLocalTransform()
		{
			return Matrix44.CreateScale(scale) * Matrix44.CreateRotation(rotation) * Matrix44.CreateTranslation(position);
		}

		public bool TrySetLocalTransform(Matrix44 transform)
		{
			if (transform.Decompose(out scale, out rotation, out position)) {
				PropagateDirtyFlags(DirtyFlags.Transform);
				return true;
			}
			return false;
		}

		public void SetGlobalTransform(Matrix44 transform)
		{
			if (Parent != null && Parent.AsModelNode != null) {
				transform *= Parent.AsModelNode.GlobalTransform.CalcInverted();
			}
			SetLocalTransform(transform);
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
			if (vp == null) {
				throw new Lime.Exception("Viewport isn't set");
			}
			return HitTest(vp.ScreenPointToRay(point));
		}

		public Viewport3D GetViewport()
		{
			Node p = Parent;
			while (p != null && !(p is Viewport3D)) {
				p = p.Parent;
			}
			return p as Viewport3D;
		}

		public override Node DeepCloneFast()
		{
			var clone = base.DeepCloneFast() as Node3D;
			clone.AsModelNode = clone;
			return clone;
		}

		public struct MeshHitTestResult
		{
			public Mesh3D Mesh;
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

		internal virtual bool PerformHitTest(Ray ray, float distanceToNearNode, out float distance)
		{
			distance = default(float);
			return false;
		}

		public Matrix44 CalcTransformInSpaceOf(Camera3D camera)
		{
			return GlobalTransform * camera.View;
		}

		public Matrix44 CalcTransformInSpaceOf(Node3D node)
		{
			return GlobalTransform * node.GlobalTransform.CalcInverted();
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (!GloballyVisible) {
				return;
			}
			if (Layer != 0) {
				var oldLayer = chain.SetCurrentLayer(Layer);
				for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
					node.AddToRenderChain(chain);
				}
				chain.SetCurrentLayer(oldLayer);
			} else {
				for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
					node.AddToRenderChain(chain);
				}
			}
		}
	}
}