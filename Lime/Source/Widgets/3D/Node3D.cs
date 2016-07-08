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
			return WidgetContext.Current.NodeUnderMouse == this;
		}

		public Node3D()
		{
			AsNode3D = this;
			scale = Vector3.One;
			rotation = Quaternion.Identity;
			visible = true;
			color = Color4.White;
		}

		protected override void RecalcDirtyGlobalsUsingParents()
		{
			if (IsDirty(DirtyFlags.Transform)) {
				globalTransform = CalcLocalTransform();
				if (Parent != null && Parent.AsNode3D != null) {
					globalTransform *= Parent.AsNode3D.GlobalTransform;
				}
			}
			if (IsDirty(DirtyFlags.Visible | DirtyFlags.Color)) {
				globallyVisible = Visible;
				globalColor = color;
				if (Parent != null) {
					if (Parent.AsWidget != null) {
						globallyVisible &= Parent.AsWidget.GloballyVisible;
						globalColor *= Parent.AsWidget.GlobalColor;
					} else if (Parent.AsNode3D != null) {
						globallyVisible &= Parent.AsNode3D.GloballyVisible;
						globalColor *= Parent.AsNode3D.GlobalColor;
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
			if (Parent != null && Parent.AsNode3D != null) {
				transform *= Parent.AsNode3D.GlobalTransform.CalcInverted();
			}
			SetLocalTransform(transform);
		}

		public void SetLocalTransform(Matrix44 transform)
		{
			if (!TrySetLocalTransform(transform)) {
				throw new ArgumentException();
			}
		}

		public Viewport3D GetViewport()
		{
			Node p = Parent;
			while (p != null && !(p is Viewport3D)) {
				p = p.Parent;
			}
			return p as Viewport3D;
		}

		public override Node Clone()
		{
			var clone = base.Clone() as Node3D;
			clone.AsNode3D = clone;
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

		public override void AddToRenderChain(RenderChain chain)
		{
			if (!GloballyVisible) {
				return;
			}
			AddContentsToRenderChain(chain);
		}
	}
}