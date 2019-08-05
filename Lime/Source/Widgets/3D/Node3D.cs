using System;
using Yuzu;

namespace Lime
{
	[TangerineRegisterNode(CanBeRoot = true, Order = 23)]
	[TangerineAllowedParentTypes(typeof(Node3D), typeof(Viewport3D))]
	[TangerineAllowedChildrenTypes(typeof(Node))]
	[TangerineVisualHintGroup("/All/Nodes/Containers")]
	public class Node3D : Node
	{
		private Vector3 scale;
		private Quaternion rotation;
		private Vector3 position;
		private bool visible;
		private bool freezeInvisible = true;
		private Color4 color;
		private Viewport3D viewport;

		protected Matrix44 localTransform;
		protected Matrix44 globalTransform;
		protected Matrix44 globalTransformInverse;
		protected bool globallyVisible;
		protected bool globallyFreezeInvisible;
		protected Color4 globalColor;

		public Viewport3D Viewport
		{
			get
			{
				if (viewport == null) {
					var p = Parent;
					while (p != null && !(p is Viewport3D)) {
						p = p.Parent;
					}
					viewport = p as Viewport3D;
				}
				return viewport;
			}
		}

		[YuzuMember]
		[TangerineKeyframeColor(19)]
		public bool Visible
		{
			get { return visible; }
			set
			{
				if (visible != value) {
					visible = value;
					PropagateDirtyFlags(DirtyFlags.Visible | DirtyFlags.Frozen);
					Manager?.OnFilterChanged(this);
				}
			}
		}

		public bool FreezeInvisible
		{
			get { return freezeInvisible; }
			set
			{
				if (freezeInvisible != value) {
					freezeInvisible = value;
					PropagateDirtyFlags(DirtyFlags.FreezeInvisible);
					Manager?.OnFilterChanged(this);
				}
			}
		}

		[YuzuMember]
		[TangerineKeyframeColor(24)]
		public Vector3 Scale
		{
			get { return scale; }
			set
			{
				scale = value;
				OnTransformChanged();
			}
		}

		[YuzuMember]
		[TangerineKeyframeColor(4)]
		public Quaternion Rotation
		{
			get { return rotation; }
			set
			{
				rotation = value;
				OnTransformChanged();
			}
		}

		[YuzuMember]
		[TangerineKeyframeColor(25)]
#if TANGERINE
		[TangerineIgnoreIf(nameof(ShouldInspectPosition))]
#endif // TANGERINE
		public Vector3 Position
		{
			get { return position; }
			set
			{
				position = value;
				OnTransformChanged();
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
		[TangerineKeyframeColor(21)]
		public bool Opaque { get; set; }

		public Matrix44 LocalTransform
		{
			get
			{
				if (CleanDirtyFlags(DirtyFlags.LocalTransform)) {
					RecalcLocalTransform();
				}
				return localTransform;
			}
		}

		private void RecalcLocalTransform()
		{
			localTransform =
				Matrix44.CreateScale(scale) *
				Matrix44.CreateRotation(rotation) *
				Matrix44.CreateTranslation(position);
		}

		public Matrix44 GlobalTransform
		{
			get
			{
				if (CleanDirtyFlags(DirtyFlags.GlobalTransform)) {
					RecalcGlobalTransform();
				}
				return globalTransform;
			}
		}

		private void RecalcGlobalTransform()
		{
			globalTransform = LocalTransform;
			if (Parent?.AsNode3D != null) {
				globalTransform *= Parent.AsNode3D.GlobalTransform;
			}
		}

		public Matrix44 GlobalTransformInverse
		{
			get
			{
				if (CleanDirtyFlags(DirtyFlags.GlobalTransformInverse)) {
					RecalcGlobalTransformInverse();
				}
				return globalTransformInverse;
			}
		}

		private void RecalcGlobalTransformInverse()
		{
			globalTransformInverse = GlobalTransform.CalcInverted();
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
#if TANGERINE
			globallyVisible |= GetTangerineFlag(TangerineFlags.Shown);
			globallyVisible &= !GetTangerineFlag(TangerineFlags.Hidden | TangerineFlags.HiddenOnExposition);
#endif // TANGERINE
			if (Parent != null) {
				if (Parent.AsWidget != null) {
					globallyVisible &= Parent.AsWidget.GloballyVisible;
				} else if (Parent.AsNode3D != null) {
					globallyVisible &= Parent.AsNode3D.GloballyVisible;
				}
			}
		}

		public bool GloballyFreezeInvisible
		{
			get
			{
				if (CleanDirtyFlags(DirtyFlags.FreezeInvisible)) {
					RecalcGloballyFreezeInvisible();
				}
				return globallyFreezeInvisible;
			}
		}

		private void RecalcGloballyFreezeInvisible()
		{
			globallyFreezeInvisible = FreezeInvisible;
			if (Parent != null) {
				if (Parent.AsWidget != null) {
					globallyFreezeInvisible |= Parent.AsWidget.GloballyFreezeInvisible;
				} else if (Parent.AsNode3D != null) {
					globallyFreezeInvisible |= Parent.AsNode3D.GloballyFreezeInvisible;
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

		protected override void RecalcGloballyFrozen()
		{
			base.RecalcGloballyFrozen();
			globallyFrozen |= GloballyFreezeInvisible && !GloballyVisible;
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

		public void SetGlobalTransform(Matrix44 transform)
		{
			if (Parent?.AsNode3D != null) {
				SetLocalTransform(transform * Parent.AsNode3D.GlobalTransformInverse);
			} else {
				SetLocalTransform(transform);
			}
			globalTransform = transform;
			DirtyMask &= ~DirtyFlags.GlobalTransform;
		}

		public void SetGlobalTransformInverse(Matrix44 transform)
		{
			SetGlobalTransform(transform.CalcInverted());
			globalTransformInverse = transform;
			DirtyMask &= ~DirtyFlags.GlobalTransformInverse;
		}

		public void SetLocalTransform(Matrix44 transform)
		{
			localTransform = transform;
			localTransform.Decompose(out scale, out rotation, out position);
			PropagateDirtyFlags(
				DirtyFlags.GlobalTransform |
				DirtyFlags.GlobalTransformInverse);
			DirtyMask &= ~DirtyFlags.LocalTransform;
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

		protected override Node CloneInternal()
		{
			var clone = base.CloneInternal().AsNode3D;
			clone.Opaque = Opaque;
			clone.viewport = null;
			return clone;
		}

		public Matrix44 CalcTransformInSpaceOf(Node3D node)
		{
			return GlobalTransform * node.GlobalTransformInverse;
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (GloballyVisible) {
				AddSelfAndChildrenToRenderChain(chain, Layer);
			}
		}

		protected override void OnParentChanged(Node oldParent)
		{
			base.OnParentChanged(oldParent);
			viewport = null;
		}

		private void OnTransformChanged()
		{
			DirtyMask |= DirtyFlags.LocalTransform;
			PropagateDirtyFlags(
				DirtyFlags.GlobalTransform |
				DirtyFlags.GlobalTransformInverse);
		}
	}
}
