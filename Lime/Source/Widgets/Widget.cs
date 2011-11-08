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
	public class Widget : Node
	{
		#region Properties

		public event EventHandler<UIEventArgs> LeftDown;
		public event EventHandler<UIEventArgs> LeftUp;
		public event EventHandler<UIEventArgs> Move;

        [ProtoMember(1)]
		public Vector2 Position { get; set; }

        [ProtoMember(2)]
		public Vector2 Size { get; set; }

        [ProtoMember(3)]
		public Vector2 Pivot { get; set; }

		[ProtoMember (4)]
		public Vector2 Scale { get; set; }
		
		private float rotation;
		private Vector2 sincos = new Vector2 (1, 0);

		[ProtoMember(5), DefaultValue (0)]
		public float Rotation { 
			get {
				return rotation;
			} 
			set {
				rotation = value;
				float a = Utils.DegreesToRadians * Rotation;
				sincos.X = (float)Math.Cos (a);
				sincos.Y = (float)Math.Sin (a);
			}
		}

        [ProtoMember(6)]
		public Color4 Color { get; set; }

        [ProtoMember(7)]
		public Anchors Anchors { get; set; }

        [ProtoMember(8)]
		public Blending Blending { get; set; }

        [ProtoMember(9), DefaultValue (true)]
		public bool Visible { get; set; }

        [ProtoMember(10)]
		public SkinningWeights SkinningWeights { get; set; }

        [ProtoMember(11)]
		public HitTestMethod HitTestMethod { get; set; }

        [ProtoMember(12)]
		public bool AcceptInput { get; set; }

		public bool Shown { get { return Visible && Color.A > 0; } }
		
		[ProtoMember(13)]
		public BoneArray BoneArray;

		public Matrix32 LocalMatrix {
			get {
				var u = new Vector2 (sincos.X * Scale.X, sincos.Y * Scale.X);
				var v = new Vector2 (-sincos.Y * Scale.Y, sincos.X * Scale.Y);
				Vector2 translation = Position;
				Vector2 center = Vector2.Scale (Size, Pivot);
				Matrix32 matrix;
				matrix.U = u;
				matrix.V = v;
				matrix.T.X = -(center.X * u.X) - center.Y * v.X + translation.X;
				matrix.T.Y = -(center.X * u.Y) - center.Y * v.Y + translation.Y;
				return matrix;
			}
		}

		protected bool renderedToTexture;
		private Matrix32 worldMatrix;
		private Color4 worldColor;
		private Blending worldBlending;
		private bool worldShown;

		public Matrix32 WorldMatrix { get { return worldMatrix; } }

		public Color4 WorldColor { get { return worldColor; } }

		public Blending WorldBlending { get { return worldBlending; } }

		public override bool WorldShown { get { return worldShown; } }
		
		#endregion
		#region Methods

		public void DispatchMouseDown (MouseButton button, Vector2 pointer)
		{
			DispatchUIEvent (new UIEventArgs { Pointer = pointer, Type = UIEventType.LeftDown });
		}

		public void DispatchMouseUp (MouseButton button, Vector2 pointer)
		{
			DispatchUIEvent (new UIEventArgs { Pointer = pointer, Type = UIEventType.LeftUp });
		}

		public void DispatchUIEvent (UIEventArgs e)
		{
			//if (AcceptInput)
			{
				switch (e.Type) {
				case UIEventType.LeftDown:
					if (LeftDown != null)
						LeftDown (this, e);
					break;
				case UIEventType.LeftUp:
					if (LeftUp != null)
						LeftUp (this, e);
					break;
				case UIEventType.Move:
					if (Move != null)
						Move (this, e);
					break;
				}
			}
			for (int i = 0; i < Nodes.Count; i++) {
				var w = Nodes [i].Widget;
				if (w != null)
					w.DispatchUIEvent (e);
			}
		}

		public Widget ()
		{
			Widget = this;
			Size = new Vector2 (100, 100);
			Color = Color4.White;
			Scale = Vector2.One;
			Visible = true;
			Blending = Blending.Default;
		}

		public void UpdateWorldProperties ()
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
		
		public override void Update (int delta)
		{
			UpdatedNodes++;
			UpdateWorldProperties (); 
			if (WorldShown) {
				if (Playing) {
					AdvanceAnimation (delta);
				}
				for (int i = Nodes.Count - 1; i >= 0; i--) {
					Nodes [i].Update (delta);
				}
				ApplyAnchors ();
			}
		}

		bool parentSizeIsDefined;
		Vector2 parentSize;

		void ApplyAnchors ()
		{
			if (Anchors != Anchors.None && Parent.Widget != null) {
				Vector2 currentSize = Parent.Widget.Size;
				if (parentSizeIsDefined && !parentSize.Equals (currentSize)) {
					// Apply anchors along X axis.
					if ((Anchors & Anchors.CenterH) != 0) {
						Position += new Vector2 ((currentSize.X - parentSize.X) / 2, 0);
					} else if ((Anchors & Anchors.Left) != 0 && (Anchors & Anchors.Right) != 0) {
						Size += new Vector2 (currentSize.X - parentSize.X, 0);
						Position += new Vector2 ((currentSize.X - parentSize.X) * Pivot.X, 0);
					} else if ((Anchors & Anchors.Right) != 0) {
						Position += new Vector2 (currentSize.X - parentSize.X, 0);
					}

					// Apply anchors along Y axis.
					if ((Anchors & Anchors.CenterV) != 0) {
						Position += new Vector2 (0, (currentSize.Y - parentSize.Y) / 2);
					} else if ((Anchors & Anchors.Top) != 0 && (Anchors & Anchors.Bottom) != 0) {
						Size += new Vector2 (0, currentSize.Y - parentSize.Y);
						Position += new Vector2 (0, (currentSize.Y - parentSize.Y) * Pivot.Y);
					} else if ((Anchors & Anchors.Bottom) != 0) {
						Position += new Vector2 (0, currentSize.Y - parentSize.Y);
					}
				}
				parentSizeIsDefined = true;
				parentSize = currentSize;
			}
		}

		public virtual bool HitTest (Vector2 point)
		{
			if (WorldShown) {
				if (HitTestMethod == HitTestMethod.BoundingRect) {
					Vector2 pt = WorldMatrix.CalcInversed ().TransformVector (point);
					Vector2 sz = Size;
					if (sz.X < 0) {
						pt.X = -pt.X;
						sz.X = -sz.X;
					}
					if (sz.Y < 0) {
						pt.Y = -pt.Y;
						sz.Y = -sz.Y;
					}
					return pt.X >= 0 && pt.Y >= 0 && pt.X < sz.X && pt.Y < sz.Y;
				} else if (HitTestMethod == HitTestMethod.Contents) {
					foreach (Node node in Nodes) {
						if (node.Widget != null && node.Widget.HitTest (point))
							return true;
					}
					return false;
				}
			}
			return false;
		}
		
		#endregion
	}
}
