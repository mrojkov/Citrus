using System;
using Lime;
using ProtoBuf;
using System.Collections.Generic;

namespace Lime
{
	[Flags]
	public enum Anchors
	{
		None,
		Left = 1,
		Right = 2,
		Top = 4,
		Bottom = 8,
		CenterH = 16,
		CenterV = 32
	}

	public enum HitTestMethod
	{
		BoundingRect,
		Contents,
		Skip
	}

	[ProtoContract]
	[ProtoInclude(100, typeof(Frame))]
	[ProtoInclude(101, typeof(Image))]
	[ProtoInclude(102, typeof(SimpleText))]
	[ProtoInclude(103, typeof(ParticleEmitter))]
	[ProtoInclude(104, typeof(DistortionMesh))]
	[ProtoInclude(105, typeof(Spline))]
	[ProtoInclude(106, typeof(ParticlesMagnet))]
	[ProtoInclude(107, typeof(NineGrid))]
	[ProtoInclude(108, typeof(Button))]
	[ProtoInclude(109, typeof(Slider))]
	[ProtoInclude(110, typeof(RichText))]
	[ProtoInclude(111, typeof(TextBox))]
	[ProtoInclude(112, typeof(Movie))]
	[TangerineClass]
	public partial class Widget : Node
	{
		private Vector2 position;
		private Vector2 size;
		private float rotation;
		private Vector2 direction = new Vector2(1, 0);
		private Color4 color;
		private Action clicked;
		private Vector2? parentSize;

		protected bool RenderedToTexture;

		#region Properties

		public virtual string Text { 
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public virtual Action Clicked {
			get { return clicked; }
			set { clicked = value; }
		}

		public virtual bool WasClicked()
		{
			return Input.WasMouseReleased() && HitTest(Input.MousePosition);
		}

		public virtual ITexture Texture
		{
			get { return null; }
			set { throw new NotImplementedException(); }
		}

		[ProtoMember(1)]
		[TangerineProperty(4)]
		public Vector2 Position { get { return position; } set { position = value; } }
		public float X { get { return position.X; } set { position.X = value; } }
		public float Y { get { return position.Y; } set { position.Y = value; } }

		[ProtoMember(2)]
		[TangerineProperty(7)]
		public Vector2 Size { get { return size; } set { size = value; } }
		public float Width { get { return size.X; } set { size.X = value; } }
		public float Height { get { return size.Y; } set { size.Y = value; } }

		[ProtoMember(3)]
		[TangerineProperty(6)]
		public Vector2 Pivot { get; set; }

		[ProtoMember(4)]
		[TangerineProperty(5)]
		public Vector2 Scale { get; set; }

		[ProtoMember(5)]
		[TangerineProperty(3)]
		public float Rotation { 
			get { return rotation; }
			set {
				rotation = value;
				direction = Mathf.CosSin(Mathf.DegreesToRadians * value);
			}
		}

		[ProtoMember(6)]
		[TangerineProperty(8)]
		public Color4 Color { get { return color; } set { color = value; } }

		public float Opacity { 
			get { return (float)color.A * (1 / 255f); }
			set { color.A = (byte)(value * 255f); }
		}

		[ProtoMember(7)]
		public Anchors Anchors { get; set; }

		[ProtoMember(8)]
		[TangerineProperty(9)]
		public Blending Blending { get; set; }

		[ProtoMember(9)]
		[TangerineProperty(2)]
		public bool Visible { get; set; }

		[ProtoMember(10)]
		public SkinningWeights SkinningWeights { get; set; }

		[ProtoMember(11)]
		public HitTestMethod HitTestMethod { get; set; }

		[ProtoMember(13)]
		public BoneArray BoneArray;

		public Matrix32 LocalToWorldTransform { get; protected set; }
		public Color4 GlobalColor { get; protected set; }
		public Blending GlobalBlending { get; protected set; }
		public bool GloballyVisible { get; protected set; }
		public Vector2 GlobalPosition { get { return LocalToWorldTransform * Vector2.Zero; } }
		public Vector2 GlobalCenter { get { return LocalToWorldTransform * (Size / 2); } }

		#endregion
		#region Methods

		public Widget()
		{
			AsWidget = this;
			Size = new Vector2(100, 100);
			Color = Color4.White;
			Scale = Vector2.One;
			Visible = true;
			Blending = Blending.Default;
		}

		public virtual Vector2 CalcContentSize()
		{
			return Size;
		}

		public Widget this[string id]
		{
			get { return Find<Widget>(id); }
		}

		public Widget this[string format, object arg]
		{
			get { return Find<Widget>(string.Format(format, arg)); }
		}

		public void MakeInvisible()
		{
			Visible = false;
		}

		public void MakeVisible()
		{
			Visible = true;
		}

		public override void Update(int delta)
		{
			if (Anchors != Anchors.None && Parent != null && Parent.AsWidget != null) {
				ApplyAnchors();
			}
			RecalcGlobalMatrixAndColorHelper();
			if (!GloballyVisible) {
				return;
			}
			if (IsRunning) {
				AdvanceAnimation(delta);
			}
			UpdateChildren(delta);
			if (clicked != null) {
				HandleClick();
			}
		}

		private void HandleClick()
		{
			if (Input.WasMouseReleased() && HitTest(Input.MousePosition)) {
				clicked();
			}
		}

		public void RecalcGlobalMatrixAndColor()
		{
			if (Parent != null) {
				Parent.AsWidget.RecalcGlobalMatrixAndColor();
			}
			RecalcGlobalMatrixAndColorHelper();
		}

		private void RecalcGlobalMatrixAndColorHelper()
		{
			if (Parent != null) {
				var parentWidget = Parent.AsWidget;
				if (parentWidget != null && !parentWidget.RenderedToTexture) {
					LocalToWorldTransform = CalcLocalToParentTransform() * parentWidget.LocalToWorldTransform;
					GlobalColor = Color * parentWidget.GlobalColor;
					GlobalBlending = Blending == Blending.Default ? parentWidget.GlobalBlending : Blending;
					GloballyVisible = (Visible && color.A != 0) && parentWidget.GloballyVisible;
					return;
				}
			}
			LocalToWorldTransform = CalcLocalToParentTransform();
			GlobalColor = color;
			GlobalBlending = Blending;
			GloballyVisible = Visible && color.A != 0;
		}

		public Matrix32 CalcLocalToParentTransform()
		{
			Vector2 u, v;
			Vector2 translation = position;
			Vector2 center = Size * Pivot;
			Vector2 scale = Scale;
			u.X = direction.X * scale.X;
			u.Y = direction.Y * scale.X;
			v.X = -direction.Y * scale.Y;
			v.Y = direction.X * scale.Y;
			Matrix32 matrix;
			if (SkinningWeights != null && Parent != null && Parent.AsWidget != null) {
				BoneArray a = Parent.AsWidget.BoneArray;
				translation = a.ApplySkinningToVector(position, SkinningWeights);
				u = a.ApplySkinningToVector(u + position, SkinningWeights) - translation;
				v = a.ApplySkinningToVector(v + position, SkinningWeights) - translation;
			}
			matrix.U = u;
			matrix.V = v;
			matrix.T.X = -(center.X * u.X) - center.Y * v.X + translation.X;
			matrix.T.Y = -(center.X * u.Y) - center.Y * v.Y + translation.Y;
			return matrix;
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (!GloballyVisible) {
				return;
			}
			if (Layer != 0) {
				int oldLayer = chain.SetLayer(Layer);
				foreach (Node node in Nodes.AsArray) {
					node.AddToRenderChain(chain);
				}
				chain.Add(this);
				chain.SetLayer(oldLayer);
			} else {
				foreach (Node node in Nodes.AsArray) {
					node.AddToRenderChain(chain);
				}
				chain.Add(this);
			}
		}

		internal override void OnParentChanged()
		{
			if (Parent != null && Parent.AsWidget != null) {
				parentSize = Parent.AsWidget.Size;
			}
		}

		private void ApplyAnchors()
		{
			Vector2 s = Parent.AsWidget.Size;
			if (parentSize.HasValue && !parentSize.Value.Equals(s)) {
				ApplyAnchorsAlongXAxis();
				ApplyAnchorsAlongYAxis();
			}
			parentSize = s;
		}

		private void ApplyAnchorsAlongYAxis()
		{
			Vector2 s = Parent.AsWidget.Size;
			if ((Anchors & Anchors.CenterV) != 0) {
				Y += (s.Y - parentSize.Value.Y) / 2;
			} else if ((Anchors & Anchors.Top) != 0 && (Anchors & Anchors.Bottom) != 0) {
				Height += s.Y - parentSize.Value.Y;
				Y += (s.Y - parentSize.Value.Y) * Pivot.Y;
			} else if ((Anchors & Anchors.Bottom) != 0) {
				Y += s.Y - parentSize.Value.Y;
			}
		}

		private void ApplyAnchorsAlongXAxis()
		{
			Vector2 s = Parent.AsWidget.Size;
			if ((Anchors & Anchors.CenterH) != 0) {
				X += (s.X - parentSize.Value.X) / 2;
			} else if ((Anchors & Anchors.Left) != 0 && (Anchors & Anchors.Right) != 0) {
				Width += s.X - parentSize.Value.X;
				X += (s.X - parentSize.Value.X) * Pivot.X;
			} else if ((Anchors & Anchors.Right) != 0) {
				X += s.X - parentSize.Value.X;
			}
		}

		public virtual bool HitTest(Vector2 point)
		{
			if (!GloballyVisible) {
				return false;
			}
			if (HitTestMethod == HitTestMethod.BoundingRect) {
				Vector2 p = LocalToWorldTransform.CalcInversed().TransformVector(point);
				Vector2 s = Size;
				if (s.X < 0) {
					p.X = -p.X;
					s.X = -s.X;
				}
				if (s.Y < 0) {
					p.Y = -p.Y;
					s.Y = -s.Y;
				}
				return p.X >= 0 && p.Y >= 0 && p.X < s.X && p.Y < s.Y;
			} else if (HitTestMethod == HitTestMethod.Contents) {
				foreach (Node node in Nodes.AsArray) {
					if (node.AsWidget != null && node.AsWidget.HitTest(point))
						return true;
				}
				return false;
			}
			return false;
		}

		#endregion
	}
}
