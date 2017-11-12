using System;
using Yuzu;

namespace Lime
{
	[AllowedParentTypes(typeof(Node3D), typeof(Viewport3D))]
	[AllowedChildrenTypes(typeof(Node))]
	public class Node3D : Node, Viewport3D.IZSorterParams
	{
		private Vector3 scale;
		private Quaternion rotation;
		private Vector3 position;
		private bool visible;
		private Color4 color;

		protected Matrix44 globalTransform;
		protected bool globallyVisible;
		protected Color4 globalColor;
		protected Viewport3D viewport;

		[YuzuMember]
		[TangerineKeyframeColor(2)]
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

		[YuzuMember]
		[TangerineKeyframeColor(5)]
		public Vector3 Scale
		{
			get { return scale; }
			set
			{
				scale = value;
				PropagateDirtyFlags(DirtyFlags.Transform);
			}
		}

		[YuzuMember]
		[TangerineKeyframeColor(3)]
		public Quaternion Rotation
		{
			get { return rotation; }
			set
			{
				rotation = value;
				PropagateDirtyFlags(DirtyFlags.Transform);
			}
		}

		[YuzuMember]
		[TangerineKeyframeColor(4)]
		public Vector3 Position
		{
			get { return position; }
			set
			{
				position = value;
				PropagateDirtyFlags(DirtyFlags.Transform);
			}
		}

		[YuzuMember]
		[TangerineKeyframeColor(9)]
		public Color4 Color
		{
			get { return color; }
			set
			{
				color = value;
				PropagateDirtyFlags(DirtyFlags.Color);
			}
		}

		[YuzuMember]
		public bool Opaque { get; set; }

		public Matrix44 GlobalTransform
		{
			get
			{
				if (CleanDirtyFlags(DirtyFlags.Transform)) {
					RecalcGlobalTransform();
				}
				return globalTransform;
			}
		}

		protected virtual void RecalcGlobalTransform()
		{
			if (Parent?.AsNode3D != null) {
				globalTransform = CalcLocalTransform() * Parent.AsNode3D.GlobalTransform;
			} else {
				globalTransform = CalcLocalTransform();
			}
		}

		public bool GloballyVisible
		{
			get
			{
				if (CleanDirtyFlags(DirtyFlags.Visible)) {
					RecalcGloballyVisible();
				}
				return globallyVisible;
			}
		}

		private void RecalcGloballyVisible()
		{
			globallyVisible = Visible;
			if (Application.IsTangerine) {
				globallyVisible |= GetTangerineFlag(TangerineFlags.Shown);
				globallyVisible &= !GetTangerineFlag(TangerineFlags.Hidden | TangerineFlags.HiddenOnExposition);
			}
			if (Parent != null) {
				if (Parent.AsWidget != null) {
					globallyVisible &= Parent.AsWidget.GloballyVisible;
				} else if (Parent.AsNode3D != null) {
					globallyVisible &= Parent.AsNode3D.GloballyVisible;
				}
			}
		}

		public Color4 GlobalColor
		{
			get
			{
				if (CleanDirtyFlags(DirtyFlags.Color)) {
					RecalcGlobalColor();
				}
				return globalColor;
			}
		}

		private void RecalcGlobalColor()
		{
			globalColor = color;
			if (Parent != null) {
				if (Parent.AsWidget != null) {
					globalColor *= Parent.AsWidget.GlobalColor;
				} else if (Parent.AsNode3D != null) {
					globalColor *= Parent.AsNode3D.GlobalColor;
				}
			}
		}

		public Node3D()
		{
			AsNode3D = this;
			Opaque = true;
			scale = Vector3.One;
			rotation = Quaternion.Identity;
			visible = true;
			color = Color4.White;
		}

		protected override void Awake()
		{
			viewport = GetViewport();
		}

		public virtual float CalcDistanceToCamera(Camera3D camera)
		{
			return camera.View.TransformVector(GlobalTransform.Translation).Z;
		}

		public Matrix44 CalcLocalTransform()
		{
			return Matrix44.CreateScale(scale) * Matrix44.CreateRotation(rotation) * Matrix44.CreateTranslation(position);
		}

		public void SetGlobalTransform(Matrix44 transform)
		{
			if (Parent != null && Parent.AsNode3D != null) {
				transform *= Parent.AsNode3D.GlobalTransform.CalcInverted();
			}
			SetLocalTransform(transform);
		}

		public void SetLocalTransform(Matrix44 transform)
		{
			transform.Decompose(out scale, out rotation, out position);
			PropagateDirtyFlags(DirtyFlags.Transform);
		}

		public Viewport3D GetViewport()
		{
			Node p = Parent;
			while (p != null && !(p is Viewport3D)) {
				p = p.Parent;
			}
			return p as Viewport3D;
		}

		public Model3D FindModel()
		{
			var model = TryFindModel();
			if (model == null) {
				throw new Lime.Exception("Model for node '{0}' is not found", this);
			}
			return model;
		}

		public Model3D TryFindModel()
		{
			Node n = this;
			do {
				var model = n as Model3D;
				if (model != null) {
					return model;
				}
				n = n.Parent;
			} while (n != null);
			return null;
		}

		public override Node Clone()
		{
			var clone = base.Clone() as Node3D;
			clone.AsNode3D = clone;
			clone.Opaque = Opaque;
			return clone;
		}

		public Matrix44 CalcTransformInSpaceOf(Camera3D camera)
		{
			return GlobalTransform * camera.View;
		}

		public Matrix44 CalcTransformInSpaceOf(Node3D node)
		{
			return GlobalTransform * node.GlobalTransform.CalcInverted();
		}

		internal protected override void AddToRenderChain(RenderChain chain)
		{
			if (GloballyVisible) {
				AddChildrenToRenderChain(chain);
			}
		}

		protected override void OnParentChanged(Node oldParent)
		{
			base.OnParentChanged(oldParent);
			viewport = GetViewport();
		}
	}
}