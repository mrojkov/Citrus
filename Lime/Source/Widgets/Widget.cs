using System;
using Lime;
using ProtoBuf;
using System.ComponentModel;

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
	public class Widget : Node
	{
		#region Properties

		Vector2 position;
		[ProtoMember(1)]
		public Vector2 Position { get { return position; } set { position = value; } }
		public float X { get { return position.X; } set { position.X = value; } }
		public float Y { get { return position.Y; } set { position.Y = value; } }

		Vector2 size;
		[ProtoMember(2)]
		public Vector2 Size { get { return size; } set { size = value; } }
		public float Width { get { return size.X; } set { size.X = value; } }
		public float Height { get { return size.Y; } set { size.Y = value; } }

		[ProtoMember(3)]
		public Vector2 Pivot { get; set; }

		[ProtoMember(4)]
		public Vector2 Scale { get; set; }

		private float rotation;
		private Vector2 direction = new Vector2(1, 0);

		[ProtoMember(5), DefaultValue(0)]
		public float Rotation { 
			get {
				return rotation;
			}
			set {
				rotation = value;
				direction = MathLib.CosSin(MathLib.DegreesToRadians * value);
			}
		}

		[ProtoMember(6)]
		public Color4 Color { get; set; }

		[ProtoMember(7), DefaultValue(Anchors.None)]
		public Anchors Anchors { get; set; }

		[ProtoMember(8), DefaultValue(Blending.Default)]
		public Blending Blending { get; set; }

		[ProtoMember(9), DefaultValue(true)]
		public bool Visible { get; set; }

		[ProtoMember(10), DefaultValue(null)]
		public SkinningWeights SkinningWeights { get; set; }

		[ProtoMember(11), DefaultValue(HitTestMethod.BoundingRect)]
		public HitTestMethod HitTestMethod { get; set; }

		public bool Shown { get { return Visible && Color.A > 0; } }
		
		[ProtoMember(13)]
		public BoneArray BoneArray;

		public Matrix32 LocalMatrix {
			get {
				var u = new Vector2(direction.X * Scale.X, direction.Y * Scale.X);
				var v = new Vector2(-direction.Y * Scale.Y, direction.X * Scale.Y);
				Vector2 center = Size * Pivot;
				Matrix32 matrix;
				matrix.U = u;
				matrix.V = v;
				matrix.T.X = -(center.X * u.X) - center.Y * v.X + X;
				matrix.T.Y = -(center.X * u.Y) - center.Y * v.Y + Y;
				return matrix;
			}
		}

		protected bool renderedToTexture;
		
		protected Matrix32 worldMatrix;
		protected Color4 worldColor;
		protected Blending worldBlending;
		protected bool worldShown = true;

		public Matrix32 WorldMatrix { get { return worldMatrix; } }
		public Color4 WorldColor { get { return worldColor; } }
		public Blending WorldBlending { get { return worldBlending; } }
		public bool WorldShown { get { return worldShown; } }

		#endregion
		#region Methods

		public Widget()
		{
			Widget = this;
			Size = new Vector2(100, 100);
			Color = Color4.White;
			Scale = Vector2.One;
			Visible = true;
			Blending = Blending.Default;
		}

		public void Hide()
		{
			Visible = false;
		}

		public void Show()
		{
			Visible = true;
		}

		public void UpdateWorldProperties()
		{
			if (Parent != null) {
				var widget = Parent.Widget;
				if (widget != null && !widget.renderedToTexture) {
					worldMatrix = LocalMatrix * widget.WorldMatrix;
					worldColor = Color * widget.WorldColor;
					worldShown = Shown && widget.WorldShown;
					worldBlending = Blending == Blending.Default ? widget.WorldBlending : Blending;
					return;
				}
			}
			worldMatrix = LocalMatrix;
			worldColor = Color;
			worldShown = Shown;
			worldBlending = Blending;
		}
		
		public override void Update(int delta)
		{
			UpdatedNodes++;
			UpdateWorldProperties();
			if (worldShown) {
				if (Playing) {
					AdvanceAnimation(delta);
				}
				foreach (Node node in Nodes.AsArray) {
					node.Update(delta);
				}
				if (Anchors != Anchors.None && Parent.Widget != null) {
					ApplyAnchors();
				}
				if (RootFrame.Instance.ActiveWidget == this) {
					RootFrame.Instance.ActiveWidgetUpdated = true;
				}
			}
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (worldShown) {
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
		}

		Vector2? parentSize;

		void ApplyAnchors()
		{
			Vector2 s = Parent.Widget.Size;
			if (parentSize.HasValue && !parentSize.Value.Equals(s)) {
				// Apply anchors along X axis.
				if ((Anchors & Anchors.CenterH) != 0) {
					X += (s.X - parentSize.Value.X) / 2;
				} else if ((Anchors & Anchors.Left) != 0 && (Anchors & Anchors.Right) != 0) {
					Width += s.X - parentSize.Value.X;
					X += (s.X - parentSize.Value.X) * Pivot.X;
				} else if ((Anchors & Anchors.Right) != 0) {
					X += s.X - parentSize.Value.X;
				}
				// Apply anchors along Y axis.
				if ((Anchors & Anchors.CenterV) != 0) {
					Y += (s.Y - parentSize.Value.Y) / 2;
				} else if ((Anchors & Anchors.Top) != 0 && (Anchors & Anchors.Bottom) != 0) {
					Height += s.Y - parentSize.Value.Y;
					Y += (s.Y - parentSize.Value.Y) * Pivot.Y;
				} else if ((Anchors & Anchors.Bottom) != 0) {
					Y += s.Y - parentSize.Value.Y;
				}
			}
			parentSize = s;
		}

		public virtual bool HitTest(Vector2 point)
		{
			if (worldShown) {
				if (HitTestMethod == HitTestMethod.BoundingRect) {
					Vector2 p = worldMatrix.CalcInversed().TransformVector(point);
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
						if (node.Widget != null && node.Widget.HitTest(point))
							return true;
					}
					return false;
				}
			}
			return false;
		}

		#endregion
		#region Utils
		public static void Center(Widget widget)
		{
			if (widget.Parent == null) {
				throw new Lime.Exception("Parent must not be null");
			}
			widget.Position = widget.Parent.Widget.Size * 0.5f;
			widget.Pivot = Vector2.Half;
		}
		#endregion
	}
}
