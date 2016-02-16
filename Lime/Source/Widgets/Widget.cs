using System;
using Lime;
using ProtoBuf;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Lime
{
	/// <summary>
	/// Parent-relative layout.
	/// </summary>
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

	/// <summary>
	/// TODO: Translate
	/// Способы проверки столкновений
	/// </summary>
	public enum HitTestMethod
	{
		/// <summary>
		/// TODO: Translate
		/// Ограничивающий прямоугольник. Проверка без рекурсии. Грубо, но быстро
		/// </summary>
		BoundingRect,

		/// <summary>
		/// TODO: Translate
		/// Содержимое. Проверка с рекурсией. Проверка всех вложенных контейнеров.
		/// Картинки проверяются по маске. Точно, но медленно
		/// </summary>
		Contents,

		/// <summary>
		/// TODO: Translate
		/// Пропуск проверки столкновений
		/// </summary>
		Skip
	}

	/// <summary>
	/// Base class for any rendered object.
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
	[ProtoInclude(112, typeof(Movie))]
	[ProtoInclude(113, typeof(ModelViewport))]
	[DebuggerTypeProxy(typeof(WidgetDebugView))]
	public partial class Widget : Node
	{
		public const int EmptyHitTestMask = 0;
		public const int ControlsHitTestMask = 1;

		/// <summary>
		/// Minimum possible layer.
		/// </summary>
		public const int MinLayer = 0;

		/// <summary>
		/// Maximum possible layer.
		/// </summary>
		public const int MaxLayer = 99;

		private Vector2 position;
		private Vector2 size;
		private float rotation;
		private Vector2 direction;
		private Color4 color;
		private Action clicked;
		private Blending blending;
		private ShaderId shader;
		private Vector2 pivot;
		private Vector2 scale;
		private Vector2 minSize;
		private Vector2 maxSize = Vector2.PositiveInfinity;
		private bool visible;

		#region Properties

		public Widget ParentWidget { get { return Parent != null ? Parent.AsWidget : null; } }

		public ILayout Layout = AnchorLayout.Instance;

		/// <summary>
		/// Gets the layout-specific data. 
		/// </summary>
		public LayoutCell LayoutCell;

		/// <summary>
		/// The minimum widget size. For basic widgets could be provided by user or calculated automatically (e.g. for SimpleText).
		/// If the widget has non-fixed layout, the layout is responsible to manage MinSize property.
		/// </summary>
		public virtual Vector2 MinSize
		{
			get { return minSize; }
			set
			{
				if (minSize != value) {
					minSize = value;
					InvalidateParentConstraintsAndArrangement();
				}
			}
		}

		public virtual float MinWidth
		{
			get { return MinSize.X; }
			set { MinSize = new Vector2(value, MinSize.Y); }
		}

		public virtual float MinHeight
		{
			get { return MinSize.Y; }
			set { MinSize = new Vector2(MinSize.X, value); }
		}

		/// <summary>
		/// The maximum widget size. For basic widgets could be provided by user or calculated automatically (e.g. for SimpleText).
		/// If the widget has non-fixed layout, the layout is responsible to manage MaxSize property.
		/// </summary>
		public virtual Vector2 MaxSize
		{
			get { return maxSize; }
			set
			{
				if (maxSize != value) {
					maxSize = value;
					InvalidateParentConstraintsAndArrangement();
				}
			}
		}

		public virtual float MaxWidth
		{
			get { return MaxSize.X; }
			set { MaxSize = new Vector2(value, MaxSize.Y); }
		}

		public virtual float MaxHeight
		{
			get { return MaxSize.Y; }
			set { MaxSize = new Vector2(MaxSize.X, value); }
		}

		public Vector2 MinMaxSize
		{
			set { MinSize = MaxSize = value; }
		}

		public float MinMaxWidth
		{
			set { MinWidth = MaxWidth = value; }
		}

		public float MinMaxHeight
		{
			set { MinHeight = MaxHeight = value; }
		}

		/// <summary>
		/// TODO: Translate
		/// Используется только для виджетов, умеющих отображать текст. Если виджет не умеет отображать текст, возвращает null
		/// </summary>
		public virtual string Text
		{
			get { return null; }
			set { }
		}

		/// <summary>
		/// TODO: Translate
		/// Используется только для виджетов, умеющих отображать текстуры. Если виджет не умеет отображать текстуры, возвращает null
		/// </summary>
		public virtual ITexture Texture
		{
			get { return null; }
			set { }
		}

		internal protected virtual bool IsRenderedToTexture()
		{
			return false;
		}

		public virtual Action Clicked {
			get { return clicked; }
			set { clicked = value; }
		}

		public virtual bool WasClicked()
		{
			return Input.WasMouseReleased() && HitTest(Input.MousePosition);
		}

		private bool IsNumber(float x)
		{
			return !float.IsNaN(x) &&
					!float.IsInfinity(x);
		}

		/// <summary>
		/// Parent-relative position.
		/// </summary>
		[ProtoMember(1)]
		public Vector2 Position
		{
			get { return position; }
			set
			{
				System.Diagnostics.Debug.Assert(IsNumber(value.X));
				System.Diagnostics.Debug.Assert(IsNumber(value.Y));
				if (position.X != value.X || position.Y != value.Y) {
					position = value;
					PropagateDirtyFlags(DirtyFlags.Transform);
				}
			}
		}

		/// <summary>
		/// Parent-relative X position.
		/// </summary>
		public float X
		{
			get { return position.X; }
			set
			{
				System.Diagnostics.Debug.Assert(IsNumber(value));
				if (position.X != value) {
					position.X = value;
					PropagateDirtyFlags(DirtyFlags.Transform);
				}
			}
		}

		/// <summary>
		/// Parent-relative Y position.
		/// </summary>
		public float Y
		{
			get { return position.Y; }
			set
			{
				System.Diagnostics.Debug.Assert(IsNumber(value));
				if (position.Y != value) {
					position.Y = value;
					PropagateDirtyFlags(DirtyFlags.Transform);
				}
			}
		}

		[ProtoMember(2)]
		public Vector2 Size
		{
			get { return size; }
			set
			{
				System.Diagnostics.Debug.Assert(IsNumber(value.X));
				System.Diagnostics.Debug.Assert(IsNumber(value.Y));
				if (value.X != size.X || value.Y != size.Y) {
					var sizeDelta = value - size;
					size = value;
					OnSizeChanged(sizeDelta);
					PropagateDirtyFlags(DirtyFlags.Transform);
				}
			}
		}

		/// <summary>
		/// Gets or sets the widget padding. Padding defines the white space between the widget content and the widget border.
		/// The widget presenter and layout should respect the padding.
		/// </summary>
		public Thickness Padding;

		public Vector2 ContentPosition
		{
			get { return new Vector2(Padding.Left, Padding.Top); }
		}

		public Vector2 ContentSize
		{
			get { return new Vector2(Size.X - Padding.Left - Padding.Right, Size.Y - Padding.Top - Padding.Bottom); }
		}

		public float ContentWidth
		{
			get { return Size.X - Padding.Left - Padding.Right; }
		}

		public float ContentHeight
		{
			get { return Size.Y - Padding.Top - Padding.Bottom; }
		}

		/// <summary>
		/// Stops all tasks and calls Dispose of all descendants.
		/// </summary>
		public override void Dispose()
		{
			if (tasks != null) {
				tasks.Stop();
			}
			if (lateTasks != null) {
				lateTasks.Stop();
			}
			base.Dispose();
		}

		public void RefreshLayout()
		{
			OnSizeChanged(Vector2.Zero);
		}

		protected virtual void OnSizeChanged(Vector2 sizeDelta)
		{
			Layout.OnSizeChanged(this, sizeDelta);
		}

		public float Width {
			get { return size.X; }
			set {
				if (size.X != value)
					Size = new Vector2(value, Height);
			}
		}

		public float Height {
			get { return size.Y; }
			set {
				if (size.Y != value)
					Size = new Vector2(Width, value);
			}
		}

		/// <summary>
		/// Center point of rotation and scaling.
		/// (0, 0) is top-left corner, (1, 1) is bottom-right corner.
		/// </summary>
		[ProtoMember(3)]
		public Vector2 Pivot
		{
			get { return pivot; }
			set
			{
				System.Diagnostics.Debug.Assert(IsNumber(value.X));
				System.Diagnostics.Debug.Assert(IsNumber(value.Y));
				if (pivot.X != value.X || pivot.Y != value.Y) {
					pivot = value;
					PropagateDirtyFlags(DirtyFlags.Transform);
				}
			}
		}

		[ProtoMember(4)]
		public Vector2 Scale
		{
			get { return scale; }
			set
			{
				System.Diagnostics.Debug.Assert(IsNumber(value.X));
				System.Diagnostics.Debug.Assert(IsNumber(value.Y));
				if (scale.X != value.X || scale.Y != value.Y) {
					scale = value;
					PropagateDirtyFlags(DirtyFlags.Transform);
				}
			}
		}

		/// <summary>
		/// Counter-clockwise rotation of this widget.
		/// </summary>
		[ProtoMember(5)]
		public float Rotation {
			get { return rotation; }
			set
			{
				System.Diagnostics.Debug.Assert(IsNumber(value));
				if (rotation != value) {
					rotation = value;
					direction = Vector2.CosSinRough(Mathf.DegToRad * value);
					PropagateDirtyFlags(DirtyFlags.Transform);
				}
			}
		}

		/// <summary>
		/// Hue of this widget. Contents color will be multiplied by it on render.
		/// </summary>
		[ProtoMember(6)]
		public Color4 Color
		{
			get { return color; }
			set {
				if (color.ABGR != value.ABGR) {
					color = value;
					PropagateDirtyFlags(DirtyFlags.Color);
				}
			}
		}

		/// <summary>
		/// 0 - fully transparent. 1 - fully opaque.
		/// </summary>
		public float Opacity
		{
			get { return (float)color.A * (1 / 255f); }
			set
			{
				var a = (byte)(value * 255f);
				if (color.A != a) {
					color.A = a;
					PropagateDirtyFlags(DirtyFlags.Color);
				}
			}
		}

		[ProtoMember(7)]
		public Anchors Anchors { get; set; }

		[ProtoMember(8)]
		public Blending Blending
		{
			get { return blending; }
			set
			{
				if (blending != value) {
					blending = value;
					PropagateDirtyFlags(DirtyFlags.Color);
				}
			}
		}

		[ProtoMember(9)]
		public ShaderId Shader
		{
			get { return shader; }
			set
			{
				if (shader != value) {
					shader = value;
					PropagateDirtyFlags(DirtyFlags.Color);
				}
			}
		}

		[ProtoMember(10)]
		public bool Visible
		{
			get { return visible; }
			set
			{
				if (visible != value) {
					visible = value;
					PropagateDirtyFlags(DirtyFlags.Visible);
					InvalidateParentConstraintsAndArrangement();
				}
			}
		}

		[ProtoMember(11)]
		public SkinningWeights SkinningWeights { get; set; }

		[ProtoMember(12)]
		public HitTestMethod HitTestMethod { get; set; }

		[ProtoMember(13)]
		public uint HitTestMask { get; set; }

		[ProtoMember(14)]
		public BoneArray BoneArray;

		private Matrix32 localToWorldTransform;
		private Color4 globalColor;
		private Blending globalBlending;
		private ShaderId globalShader;
		private bool globallyVisible;

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		public Matrix32 LocalToWorldTransform
		{
			get { RecalcDirtyGlobals(); return localToWorldTransform; }
		}

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		public Color4 GlobalColor
		{
			get { RecalcDirtyGlobals(); return globalColor; }
		}

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		public Blending GlobalBlending
		{
			get { RecalcDirtyGlobals(); return globalBlending; }
		}

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		public ShaderId GlobalShader
		{
			get { RecalcDirtyGlobals(); return globalShader; }
		}

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		public bool GloballyVisible
		{
			get
			{
				if ((DirtyMask & (DirtyFlags.Visible | DirtyFlags.Color)) == 0) {
					return globallyVisible;
				}
				if (!visible || color.A == 0) {
					return false;
				}
				RecalcDirtyGlobals();
				return globallyVisible;
			}
		}

		/// <summary>
		/// Absolute position of this widget.
		/// </summary>
		public Vector2 GlobalPosition { get { return LocalToWorldTransform.T; } }

		/// <summary>
		/// Absolute position of center of this widget.
		/// </summary>
		public Vector2 GlobalCenter { get { return LocalToWorldTransform * (Size / 2); } }

		/// <summary>
		/// Parent-relative position of center of this widget.
		/// </summary>
		public Vector2 Center { get { return Position + (Vector2.Half - Pivot) * Size; } }

		private TaskList tasks;

		/// <summary>
		/// Tasks that are called before Update.
		/// </summary>
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

		/// <summary>
		/// Tasks that are called after Update.
		/// </summary>
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

		/// <summary>
		/// Called before Update.
		/// </summary>
		public event UpdateHandler Updating;

		/// <summary>
		/// Called after Update.
		/// </summary>
		public event UpdateHandler Updated;

		#endregion

		#region Methods

		public Widget()
		{
			Layout = AnchorLayout.Instance;
			AsWidget = this;
			Size = new Vector2(100, 100);
			Color = Color4.White;
			Scale = Vector2.One;
			Visible = true;
			Blending = Blending.Inherited;
			Shader = ShaderId.Inherited;
			direction = new Vector2(1, 0);
		}

		internal void InvalidateParentConstraintsAndArrangement()
		{
			if (ParentWidget != null) {
				ParentWidget.Layout.InvalidateConstraintsAndArrangement(ParentWidget);
			}
		}

		WidgetInput input;
		public WidgetInput Input
		{
			get { return input ?? (input = new WidgetInput(this)); }
		}

		public virtual Vector2 CalcContentSize()
		{
			return Size;
		}

		/// <summary>
		/// Searches for widget with provided path or id in this widget's descendants.
		/// Throws an exception if sought-for widget doesn't exist.
		/// <para>This function is thread safe.</para>
		/// </summary>
		/// <param name="path">Id or path of widget. Path can be incomplete
		/// (i.e. for path Root/Human/Head/Eye Human or Head can be ommited).</param>
		public Widget this[string path]
		{
			get { return Find<Widget>(path); }
		}

		/// <summary>
		/// Searches for widget with provided path or id in this widget's descendants.
		/// Throws an exception if sought-for widget doesn't exist.
		/// <para>This function is thread safe.</para>
		/// </summary>
		/// <param name="format">Id or path of widget. Path can be incomplete
		/// (i.e. for path Root/Human/Head/Eye Human or Head can be ommited).</param>
		public Widget this[string format, params object[] arg]
		{
			get { return Find<Widget>(string.Format(format, arg)); }
		}

		/// <summary>
		/// TODO: Translate
		/// Возвращает клон этого виджета. Используйте DeepCloneFast() as Widget, т.к. он возвращает Node (базовый объект виджета)
		/// </summary>
		public override Node DeepCloneFast()
		{
			var clone = base.DeepCloneFast().AsWidget;
			clone.input = null;
			clone.tasks = null;
			clone.lateTasks = null;
			return clone;
		}

		protected override void OnParentChanged(Node oldParent)
		{
			if (oldParent != null && oldParent.AsWidget != null) {
				var w = oldParent.AsWidget;
				w.Layout.InvalidateConstraintsAndArrangement(w);
			}
			InvalidateParentConstraintsAndArrangement();
		}

		/// <summary>
		/// TODO: Translate
		/// Обновляет состояние виджета (обновляет его анимации, генерирует события и. т.д.).
		/// Вызывает Update для всех дочерних виджетов. В нормальных условиях этот метод должен вызываться 1 раз за кадр.
		/// </summary>
		/// <param name="delta">Количество секунд, прошедшее с момента предыдущего вызова Update</param>
		public override void Update(float delta)
		{
			if (!Awoken) {
				Awake();
				Awoken = true;
			}
			delta *= AnimationSpeed;
			if (Updating != null) {
				Updating(delta);
			}
			if (GloballyVisible) {
				AdvanceAnimation(delta);
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

		/// <summary>
		/// Raises Updating event in respect of animation speed.
		/// </summary>
		/// <param name="delta">Time delta from last Update.</param>
		public void RaiseUpdating(float delta)
		{
			if (Updating != null) {
				Updating(delta * AnimationSpeed);
			}
		}

		/// <summary>
		/// Raises Updated event in respect of animation speed.
		/// </summary>
		/// <param name="delta">Time delta from last Update.</param>
		public void RaiseUpdated(float delta)
		{
			if (Updated != null) {
				Updated(delta * AnimationSpeed);
			}
		}

		private void HandleClick()
		{
			if (Input.WasMouseReleased() && IsMouseOver()) {
				if (Lime.Debug.BreakOnButtonClick) {
					Debugger.Break();
				}
				clicked();
			}
		}

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		protected override void RecalcDirtyGlobalsUsingParents()
		{
			// TODO: Optimize using DirtyMask
			if (IsRenderedToTexture()) {
				localToWorldTransform = Matrix32.Identity;
				globalColor = color;
				globalBlending = Blending.Inherited;
				globalShader = ShaderId.Inherited;
				globallyVisible = Visible && color.A != 0;
				return;
			}
			if (Parent != null) {
				var parentWidget = Parent.AsWidget;
				if (parentWidget != null) {
					var localToParent = CalcLocalToParentTransform();
					Matrix32.Multiply(ref localToParent, ref parentWidget.localToWorldTransform, out localToWorldTransform);
					globalColor = Color * parentWidget.globalColor;
					globalBlending = Blending == Blending.Inherited ? parentWidget.globalBlending : Blending;
					globalShader = Shader == ShaderId.Inherited ? parentWidget.globalShader : Shader;
					globallyVisible = (Visible && color.A != 0) && parentWidget.globallyVisible;
					return;
				}
			}
			localToWorldTransform = CalcLocalToParentTransform();
			globalColor = color;
			globalBlending = Blending;
			globalShader = Shader;
			globallyVisible = Visible && color.A != 0;
		}

		/// <summary>
		/// TODO: Add summary
		/// </summary>
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
			var translation = position;
			u.X = direction.X * scale.X;
			u.Y = direction.Y * scale.X;
			v.X = -direction.Y * scale.Y;
			v.Y = direction.X * scale.Y;
			if (SkinningWeights != null && Parent != null) {
				var a = Parent.AsWidget.BoneArray;
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

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		public override void StaticScale(float ratio, bool roundCoordinates)
		{
			if (Animators.Count > 0) {
				StaticScaleAnimationKeys(ratio, roundCoordinates);
			}
			StaticScalePositionAndSize(ratio, roundCoordinates);
			base.StaticScale(ratio, roundCoordinates);
		}

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		private void StaticScaleAnimationKeys(float ratio, bool roundCoordinates)
		{
			Animator<Vector2> posAnimator, sizeAnimator;
			if (Animators.TryFind("Position", out posAnimator)) {
				var savedPivot = pivot;
				var savedRotation = rotation;
				var savedScale = scale;
				var savedPosition = position;
				foreach (var k in posAnimator.Keys) {
					var savedSize = size;
					foreach (var a in Animators) {
						a.Apply(AnimationUtils.FramesToMsecs(k.Frame));
					}
					StaticScalePositionAndSize(ratio, roundCoordinates);
					k.Value = position;
					size = savedSize;
				}
				pivot = savedPivot;
				rotation = savedRotation;
				scale = savedScale;
				position = savedPosition;
			}
			Animators.TryFind("Size", out sizeAnimator);
			if (sizeAnimator != null) {
				sizeAnimator.Keys.ForEach(k => k.Value = RoundVectorIf(k.Value * ratio, roundCoordinates));
			}
		}

		private static Vector2 RoundVectorIf(Vector2 v, bool round)
		{
			return round ? new Vector2(v.X.Round(), v.Y.Round()) : v;
		}

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		private void StaticScalePositionAndSize(float ratio, bool round)
		{
			var p1 = CalcLocalToParentTransform() * Vector2.Zero;
			p1 = RoundVectorIf(p1 * ratio, round);
			size = RoundVectorIf(size * ratio, round);
			var p2 = CalcLocalToParentTransform() * Vector2.Zero;
			position += (p1 - p2);
		}

		/// <summary>
		/// Adds widget and all its descendants to render chain.
		/// </summary>
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
				chain.Add(this);
				chain.SetCurrentLayer(oldLayer);
			} else {
				for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
					node.AddToRenderChain(chain);
				}
				chain.Add(this);
			}
		}

		#endregion

		#region HitTest handling

		public bool IsMouseOver()
		{
			return Input.IsAcceptingMouse() && HitTest(Input.MousePosition);
		}

		/// <summary>
		/// Returns true if this widget contains provided point.
		/// </summary>
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
			WidgetContext.Current.Root.EnumerateHitTestTargets(targets, HitTestMask);
			var thisLayer = GetEffectiveLayer();
			var passedThis = false;
			foreach (var target in targets) {
				if (target == this) {
					passedThis = true;
					continue;
				}
				var targetLayer = target.GetEffectiveLayer();
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
			for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
				if (node.AsWidget != null) {
					node.AsWidget.EnumerateHitTestTargets(targets, mask);
				}
			}
		}

		protected virtual bool SelfHitTest(Vector2 point)
		{
			if (!GloballyVisible || !InsideClipRect(point)) {
				return false;
			}
			switch (HitTestMethod) {
				case HitTestMethod.BoundingRect:
					return HitTestBoundingRect(point);
				case HitTestMethod.Contents:
					return Nodes.Any(node => node.AsWidget != null && node.AsWidget.HitTest(point));
			}
			return false;
		}

		private bool HitTestBoundingRect(Vector2 point)
		{
			var position = LocalToWorldTransform.CalcInversed().TransformVector(point);
			var size = Size;
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
			return clipper == null || clipper.HitTestBoundingRect(point);
		}

		protected virtual Widget GetEffectiveClipperWidget()
		{
			return Parent != null ? Parent.AsWidget.GetEffectiveClipperWidget() : null;
		}

		protected internal override void PerformHitTest()
		{
			if (!HitTestTarget) {
				return;
			}
			if (SelfHitTest(Input.MousePosition)) {
				// TODO: Check Renderer.CurrentFrameBuffer == Renderer.DefaultFrameBuffer
				// TODO: Check Renderer.ScissorTestEnabled and ScissorRectangle
				WidgetContext.Current.NodeUnderCursor = this;
			}
		}
		#endregion
	}
}
