using System;
using Lime;
using ProtoBuf;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

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
		CenterV = 32,
		LeftAndRight = Left | Right,
		TopAndBottom = Top | Bottom,
		Center = CenterH | CenterV,
	}

	public enum HitTestMethod
	{
		BoundingRect,
		Contents,
		Skip
	}

	/// <summary>
	/// The Widget class is the base class of all 2D drawable and user interface objects.
	/// The widget is the atom of the user interface: it receives mouse, keyboard and other
	/// events from the input system, and paints a representation of itself on the screen.
	/// Every widget is rectangular, and they are sorted in a Z-order. 
	/// </summary>
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
	[DebuggerTypeProxy(typeof(WidgetDebugView))]
	[TangerineClass]
	public partial class Widget : Node
	{
		public const int EmptyHitTestMask = 0;
		public const int ControlsHitTestMask = 1;
		public const int MinLayer = 0;
		public const int MaxLayer = 99;

		private Vector2 position;
		private Vector2 size;
		private float rotation;
		private Vector2 direction;
		private Color4 color;
		private Action clicked;
		public Vector2 ParentSize;

		#region Properties

		public Widget ParentWidget { get { return Parent != null ? Parent.AsWidget : null; } }

		public virtual string Text 
		{ 
			get { return null; }
			set { }
		}

		public virtual ITexture Texture
		{
			get { return null; }
			set { }
		}

		internal virtual bool IsRenderedToTexture() { return false; }

		public virtual Action Clicked {
			get { return clicked; }
			set { clicked = value; }
		}

		public virtual bool WasClicked()
		{
			return Input.WasMouseReleased() && HitTest(Input.MousePosition);
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
		
		[ProtoMember(12)]
		public uint HitTestMask { get; set; }

		[ProtoMember(13)]
		public BoneArray BoneArray;

		protected Matrix32 localToWorldTransform;
		public Matrix32 LocalToWorldTransform { get { return localToWorldTransform; } }
		public Color4 GlobalColor { get; protected set; }
		public Blending GlobalBlending { get; protected set; }
		public bool GloballyVisible { get; protected set; }
		public Vector2 GlobalPosition { get { return localToWorldTransform * Vector2.Zero; } }
		public Vector2 GlobalCenter { get { return localToWorldTransform * (Size / 2); } }

		private TaskList tasks;
		public TaskList Tasks
		{
			get
			{
				if (tasks == null) {
					tasks = new TaskList();
					Updating += tasks.Update;
				}
				return tasks;
			}
		}

		private TaskList lateTasks;
		public TaskList LateTasks
		{
			get
			{
				if (lateTasks == null) {
					lateTasks = new TaskList();
					Updated += lateTasks.Update;
				}
				return lateTasks;
			}
		}

		public event UpdateHandler Updating;
		public event UpdateHandler Updated;

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
			direction = new Vector2(1, 0);
		}

		WidgetInput input;
		public WidgetInput Input
		{
			get {
				if (input == null) {
					input = new WidgetInput(this);
				}
				return input;
			}
		}

		[ProtoAfterDeserialization]
		public void AfterDeserialization()
		{
			ParentSize = Size; 	// This assignment sets the parentSize for a top parentless widget
			foreach (var node in Nodes.AsArray) {
				if (node.AsWidget != null) {
					node.AsWidget.ParentSize = Size;
				}
			}
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

		public override Node DeepCloneFast()
		{
			var clone = base.DeepCloneFast();
			clone.AsWidget.input = null;
			(clone as Widget).tasks = null;
			(clone as Widget).lateTasks = null;
			return clone;
		}

		public override void Update(int delta)
		{
			if (AnimationSpeed != 1) {
				delta = ScaleDeltaWithAnimationSpeed(delta);
			}
			if (Updating != null) {
				Updating(delta * 0.001f);
			}
			if (Anchors != Anchors.None && ParentWidget != null) {
				ApplyAnchors();
			}
			RecalcGlobalMatrixAndColorHelper();
			if (GloballyVisible) {
				if (IsRunning) {
					AdvanceAnimation(delta);
				}
				SelfUpdate(delta);
				foreach (Node node in Nodes.AsArray) {
					node.Update(delta);
				} 
				SelfLateUpdate(delta);
				if (clicked != null) {
					HandleClick();
				}
			}
			if (Updated != null) {
				Updated(delta * 0.001f);
			}
		}

		private void HandleClick()
		{
			if (Input.WasMouseReleased() && IsMouseOver()) {
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
			if (IsRenderedToTexture()) {
				localToWorldTransform = Matrix32.Identity;
				GlobalColor = color;
				GlobalBlending = Lime.Blending.Default;
				GloballyVisible = Visible && color.A != 0;
				return;
			}
			if (Parent != null) {
				var parentWidget = Parent.AsWidget;
				if (parentWidget != null) {
					var localToParent = CalcLocalToParentTransform();
					Matrix32.Multiply(ref localToParent, ref parentWidget.localToWorldTransform, out localToWorldTransform);
					GlobalColor = Color * parentWidget.GlobalColor;
					GlobalBlending = Blending == Blending.Default ? parentWidget.GlobalBlending : Blending;
					GloballyVisible = (Visible && color.A != 0) && parentWidget.GloballyVisible;
					return;
				}
			}
			localToWorldTransform = CalcLocalToParentTransform();
			GlobalColor = color;
			GlobalBlending = Blending;
			GloballyVisible = Visible && color.A != 0;
		}

		public Matrix32 CalcLocalToParentTransform()
		{
			Matrix32 matrix;
			Vector2 center = new Vector2 { X = Size.X * Pivot.X, Y = Size.Y * Pivot.Y };
			Vector2 scale = Scale;
			if (rotation == 0 && SkinningWeights == null) {
				matrix.U.X = scale.X;
				matrix.U.Y = 0;
				matrix.V.X = 0;
				matrix.V.Y = scale.Y;
				matrix.T.X = position.X - center.X * scale.X;
				matrix.T.Y = position.Y - center.Y * scale.Y;
				return matrix;
			}
			Vector2 u, v;
			Vector2 translation = position;
			u.X = direction.X * scale.X;
			u.Y = direction.Y * scale.X;
			v.X = -direction.Y * scale.Y;
			v.Y = direction.X * scale.Y;
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
				int oldLayer = chain.SetCurrentLayer(Layer);
				foreach (Node node in Nodes.AsArray) {
					node.AddToRenderChain(chain);
				}
				chain.Add(this);
				chain.SetCurrentLayer(oldLayer);
			} else {
				foreach (Node node in Nodes.AsArray) {
					node.AddToRenderChain(chain);
				}
				chain.Add(this);
			}
		}

		private void ApplyAnchors()
		{
			var actualParentSize = Parent.AsWidget.Size;
			if (!ParentSize.Equals(actualParentSize)) {
				Vector2 deltaPosition;
				Vector2 deltaSize;
				CalcAnchorChangesAlongXAxis(out deltaPosition.X, out deltaSize.X);
				CalcAnchorChangesAlongYAxis(out deltaPosition.Y, out deltaSize.Y);
				ApplyAnchorChanges(deltaPosition, deltaSize);
			}
			ParentSize = actualParentSize;
		}

		private void CalcAnchorChangesAlongXAxis(out float deltaX, out float deltaWidth)
		{
			deltaX = 0;
			deltaWidth = 0;
			Vector2 s = Parent.AsWidget.Size;
			if ((Anchors & Anchors.CenterH) != 0) {
				deltaX = (s.X - ParentSize.X) / 2;
			} else if ((Anchors & Anchors.Left) != 0 && (Anchors & Anchors.Right) != 0) {
				deltaWidth = s.X - ParentSize.X;
				deltaX = (s.X - ParentSize.X) * Pivot.X;
			} else if ((Anchors & Anchors.Right) != 0) {
				deltaX = s.X - ParentSize.X;
			}
		}

		private void CalcAnchorChangesAlongYAxis(out float deltaY, out float deltaHeight)
		{
			deltaY = 0;
			deltaHeight = 0;
			Vector2 s = Parent.AsWidget.Size;
			if ((Anchors & Anchors.CenterV) != 0) {
				deltaY = (s.Y - ParentSize.Y) / 2;
			} else if ((Anchors & Anchors.Top) != 0 && (Anchors & Anchors.Bottom) != 0) {
				deltaHeight = s.Y - ParentSize.Y;
				deltaY = (s.Y - ParentSize.Y) * Pivot.Y;
			} else if ((Anchors & Anchors.Bottom) != 0) {
				deltaY = s.Y - ParentSize.Y;
			}
		}

		private void ApplyAnchorChanges(Vector2 deltaPosition, Vector2 deltaSize)
		{
			Position += deltaPosition;
			Size += deltaSize;
			if (Animators.Count > 0) {
				Animator<Vector2> animator;
				if (Animators.TryFind("Position", out animator)) {
					foreach (var key in animator.Keys) {
						key.Value += deltaPosition;
					}
				}
				if (Animators.TryFind("Size", out animator)) {
					foreach (var key in animator.Keys) {
						key.Value += deltaSize;
					}
				}
			}
		}

		#endregion

		#region HitTest handling

		public bool IsMouseOver()
		{
			return Input.IsAcceptingMouse() && HitTest(Input.MousePosition);
		}

		public bool HitTest(Vector2 point)
		{
			return SelfHitTest(point) && !ObscuredByOtherHitTestTargets(point);
		}

		private bool ObscuredByOtherHitTestTargets(Vector2 point)
		{
			if (HitTestMask == 0) {
				return false;
			}
			var targets = new List<Widget>();
			World.Instance.AsWidget.EnumerateHitTestTargets(targets, HitTestMask);
			int thisLayer = GetEffectiveLayer();
			bool passedThis = false;
			foreach (var target in targets) {
				if (target == this) {
					passedThis = true;
					continue;
				}
				int targetLayer = target.GetEffectiveLayer();
				if (targetLayer < thisLayer) {
					continue;
				}
				if (targetLayer == thisLayer && passedThis) {
					continue;
				}
				if (target.SelfHitTest(point)) {
					return true;
				}
			}
			return false;
		}

		public int GetEffectiveLayer()
		{
			for (Node node = this; node != null; node = node.Parent) {
				if (node.AsWidget.Layer != 0) {
					return node.AsWidget.Layer;
				}
			}
			return 0;
		}

		private void EnumerateHitTestTargets(List<Widget> targets, uint mask)
		{
			if (!GloballyVisible) {
				return;
			}
			if ((HitTestMask & mask) != 0) {
				targets.Add(this);
			}
			foreach (var i in Nodes.AsArray) {
				if (i.AsWidget != null) {
					i.AsWidget.EnumerateHitTestTargets(targets, mask);
				}
			}
		}

		protected virtual bool SelfHitTest(Vector2 point)
		{
			if (!GloballyVisible || !InsideClipRect(point)) {
				return false;
			}
			if (HitTestMethod == HitTestMethod.BoundingRect) {
				return HitTestBoundingRect(point);
			} else if (HitTestMethod == HitTestMethod.Contents) {
				foreach (Node node in Nodes.AsArray) {
					if (node.AsWidget != null && node.AsWidget.HitTest(point))
						return true;
				}
				return false;
			}
			return false;
		}

		private bool HitTestBoundingRect(Vector2 point)
		{
			Vector2 position = LocalToWorldTransform.CalcInversed().TransformVector(point);
			Vector2 size = Size;
			if (size.X < 0) {
				position.X = -position.X;
				size.X = -size.X;
			}
			if (size.Y < 0) {
				position.Y = -position.Y;
				size.Y = -size.Y;
			}
			return position.X >= 0 && position.Y >= 0 && position.X < size.X && position.Y < size.Y;
		}

		protected bool InsideClipRect(Vector2 point)
		{
			var clipper = GetEffectiveClipperWidget();
			if (clipper != null) {
				return clipper.HitTestBoundingRect(point);
			}
			return true;
		}

		protected virtual Widget GetEffectiveClipperWidget()
		{
			if (Parent != null && Parent.AsWidget != null) {
				return Parent.AsWidget.GetEffectiveClipperWidget();
			}
			return null;
		}

		#endregion
	}
}
