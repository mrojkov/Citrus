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
		LeftRight = Left | Right,
		TopBottom = Top | Bottom,
		LeftRightTopBottom = Left | Right | Top | Bottom,
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
		private Blending blending;
		private Vector2 pivot;
		private Vector2 scale;
		private bool visible;

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
		public Vector2 Position
		{
			get { return position; }
			set
			{
				if (position.X != value.X || position.Y != value.Y) {
					position = value;
					if (GlobalValuesValid) InvalidateGlobalValues();
				}
			}
		}
		public float X 
		{ 
			get { return position.X; } 
			set {
				if (position.X != value) {
					position.X = value;
					if (GlobalValuesValid) InvalidateGlobalValues();
				}
			} 
		}
		public float Y
		{
			get { return position.Y; }
			set
			{
				if (position.Y != value) {
					position.Y = value;
					if (GlobalValuesValid) InvalidateGlobalValues();
				}
			}
		}

		[ProtoMember(2)]
		[TangerineProperty(7)]
		public Vector2 Size
		{
			get { return size; }
			set
			{
				if (value.X != size.X || value.Y != size.Y) {
					var sizeDelta = value - size;
					size = value;
					OnSizeChanged(sizeDelta);
					LayoutChildren(sizeDelta);
					if (GlobalValuesValid) InvalidateGlobalValues();
				}
			}
		}

		protected virtual void OnSizeChanged(Vector2 sizeDelta) { }

		public float Width { 
			get { return size.X; } 
			set { Size = new Vector2(value, Height); } 
		}

		public float Height { 
			get { return size.Y; } 
			set { Size = new Vector2(Width, value); } 
		}

		[ProtoMember(3)]
		[TangerineProperty(6)]
		public Vector2 Pivot 
		{ 
			get { return pivot; } 
			set 
			{
				if (pivot.X != value.X || pivot.Y != value.Y) {
					pivot = value;
					if (GlobalValuesValid) InvalidateGlobalValues();
				}
			} 
		}

		[ProtoMember(4)]
		[TangerineProperty(5)]
		public Vector2 Scale 
		{ 
			get { return scale; } 
			set 
			{
				if (scale.X != value.X || scale.Y != value.Y) {
					scale = value;
					if (GlobalValuesValid) InvalidateGlobalValues();
				}
			} 
		}

		[ProtoMember(5)]
		[TangerineProperty(3)]
		public float Rotation { 
			get { return rotation; }
			set 
			{
				if (rotation != value) {
					rotation = value;
					direction = Mathf.CosSin(Mathf.DegreesToRadians * value);
					if (GlobalValuesValid) InvalidateGlobalValues();
				}
			}
		}

		[ProtoMember(6)]
		[TangerineProperty(8)]
		public Color4 Color 
		{ 
			get { return color; } 
			set {
				if (color.ABGR != value.ABGR) {
					color = value;
					if (GlobalValuesValid) InvalidateGlobalValues();
				}
			} 
		}

		public float Opacity
		{
			get { return (float)color.A * (1 / 255f); }
			set 
			{
				var a = (byte)(value * 255f);
				if (color.A != a) {
					color.A = a;
					if (GlobalValuesValid) InvalidateGlobalValues();
				}
			}
		}

		[ProtoMember(7)]
		public Anchors Anchors { get; set; }

		[ProtoMember(8)]
		[TangerineProperty(9)]
		public Blending Blending 
		{ 
			get { return blending; } 
			set 
			{
				if (blending != value) {
					blending = value;
					if (GlobalValuesValid) InvalidateGlobalValues();
				}
			} 
		}

		[ProtoMember(9)]
		[TangerineProperty(2)]
		public bool Visible 
		{ 
			get { return visible; } 
			set 
			{
				if (visible != value) {
					visible = value;
					if (GlobalValuesValid) InvalidateGlobalValues();
				}
			}
		}

		[ProtoMember(10)]
		public SkinningWeights SkinningWeights { get; set; }

		[ProtoMember(11)]
		public HitTestMethod HitTestMethod { get; set; }
		
		[ProtoMember(12)]
		public uint HitTestMask { get; set; }

		[ProtoMember(13)]
		public BoneArray BoneArray;

		private Matrix32 localToWorldTransform;
		private Color4 globalColor;
		private Blending globalBlending;
		private bool globallyVisible;

		public Matrix32 LocalToWorldTransform
		{
			get { if (!GlobalValuesValid) RecalcGlobalValues(); return localToWorldTransform; }
		}

		public Color4 GlobalColor 
		{
			get { if (!GlobalValuesValid) RecalcGlobalValues(); return globalColor; }
		}
		
		public Blending GlobalBlending
		{
			get { if (!GlobalValuesValid) RecalcGlobalValues(); return globalBlending; }
		}

		public bool GloballyVisible 
		{
			get { if (!GlobalValuesValid) RecalcGlobalValues(); return globallyVisible; }
		}
		
		public Vector2 GlobalPosition { get { return LocalToWorldTransform * Vector2.Zero; } }
		public Vector2 GlobalCenter { get { return LocalToWorldTransform * (Size / 2); } }

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

		public override void Update(float delta)
		{
			delta *= AnimationSpeed;
			if (Updating != null) {
				Updating(delta);
			}
			if (GloballyVisible) {
				if (IsRunning) {
					AdvanceAnimation(delta);
				}
				SelfUpdate(delta);
				for (var node = Nodes.FirstOrNull(); node != null; ) {
					var next = node.NextSibling;
					node.Update(delta);
					node = next;
				} 
				SelfLateUpdate(delta);
				if (clicked != null) {
					HandleClick();
				}
			}
			if (Updated != null) {
				Updated(delta);
			}
		}

		private void HandleClick()
		{
			if (Input.WasMouseReleased() && IsMouseOver()) {
				clicked();
			}
		}

		protected override void RecalcGlobalValuesUsingParentsOnes()
		{
			if (IsRenderedToTexture()) {
				localToWorldTransform = Matrix32.Identity;
				globalColor = color;
				globalBlending = Lime.Blending.Default;
				globallyVisible = Visible && color.A != 0;
				return;
			}
			if (Parent != null) {
				var parentWidget = Parent.AsWidget;
				if (parentWidget != null) {
					var localToParent = CalcLocalToParentTransform();
					Matrix32.Multiply(ref localToParent, ref parentWidget.localToWorldTransform, out localToWorldTransform);
					globalColor = Color * parentWidget.GlobalColor;
					globalBlending = Blending == Blending.Default ? parentWidget.GlobalBlending : Blending;
					globallyVisible = (Visible && color.A != 0) && parentWidget.GloballyVisible;
					return;
				}
			}
			localToWorldTransform = CalcLocalToParentTransform();
			globalColor = color;
			globalBlending = Blending;
			globallyVisible = Visible && color.A != 0;
		}

		public Matrix32 CalcLocalToParentTransform()
		{
			Matrix32 matrix;
			var center = new Vector2 { X = Size.X * Pivot.X, Y = Size.Y * Pivot.Y };
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
				for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
					node.AddToRenderChain(chain);
				}
				chain.Add(this);
				chain.SetCurrentLayer(oldLayer);
			} else {
				for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
					node.AddToRenderChain(chain);
				}
				chain.Add(this);
			}
		}

		protected override void OnParentSizeChanged(Vector2 parentSizeDelta)
		{
			if (Anchors == Anchors.None || ParentWidget == null) {
				return;
			}
			Vector2 positionDelta;
			Vector2 sizeDelta;
			CalcXAndWidthDeltas(parentSizeDelta.X, out positionDelta.X, out sizeDelta.X);
			CalcYAndHeightDeltas(parentSizeDelta.Y, out positionDelta.Y, out sizeDelta.Y);
			ApplyPositionAndSizeDelta(positionDelta, sizeDelta);
		}

		private void CalcXAndWidthDeltas(float parentWidthDelta, out float xDelta, out float widthDelta)
		{
			xDelta = 0;
			widthDelta = 0;
			if ((Anchors & Anchors.CenterH) != 0) {
				xDelta = parentWidthDelta * 0.5f;
			} else if ((Anchors & Anchors.Left) != 0 && (Anchors & Anchors.Right) != 0) {
				widthDelta = parentWidthDelta;
				xDelta = parentWidthDelta * Pivot.X;
			} else if ((Anchors & Anchors.Right) != 0) {
				xDelta = parentWidthDelta;
			}
		}

		private void CalcYAndHeightDeltas(float parentHeightDelta, out float yDelta, out float heightDelta)
		{
			yDelta = 0;
			heightDelta = 0;
			if ((Anchors & Anchors.CenterV) != 0) {
				yDelta = parentHeightDelta * 0.5f;
			} else if ((Anchors & Anchors.Top) != 0 && (Anchors & Anchors.Bottom) != 0) {
				heightDelta = parentHeightDelta;
				yDelta = parentHeightDelta * Pivot.Y;
			} else if ((Anchors & Anchors.Bottom) != 0) {
				yDelta = parentHeightDelta;
			}
		}

		private void ApplyPositionAndSizeDelta(Vector2 positionDelta, Vector2 sizeDelta)
		{
			Position += positionDelta;
			Size += sizeDelta;
			if (Animators.Count > 0) {
				Animator<Vector2> animator;
				if (Animators.TryFind("Position", out animator)) {
					foreach (var key in animator.Keys) {
						key.Value += positionDelta;
					}
				}
				if (Animators.TryFind("Size", out animator)) {
					foreach (var key in animator.Keys) {
						key.Value += sizeDelta;
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
			foreach (var i in Nodes) {
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
				foreach (Node node in Nodes) {
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
			if (Parent != null) {
				return Parent.AsWidget.GetEffectiveClipperWidget();
			}
			return null;
		}

		#endregion
	}
}
