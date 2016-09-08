using System;
using System.Collections.Generic;
using System.Diagnostics;
using Yuzu;

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

	public struct Transform2
	{
		public Vector2 Position;
		public Vector2 Scale;
		public float Rotation;
		public Vector2 U;
		public Vector2 V;

		public static Transform2 Lerp(float t, Transform2 a, Transform2 b)
		{
			return new Transform2 {
				Position = Mathf.Lerp(t, a.Position, b.Position),
				Scale = Mathf.Lerp(t, a.Scale, b.Scale),
				Rotation = Mathf.Lerp(t, a.Rotation, b.Rotation),
				U = Mathf.Lerp(t, a.U, b.U),
				V = Mathf.Lerp(t, a.V, b.V)
			};
		}
	}

	public struct WidgetHull
	{
		public Vector2 V1;
		public Vector2 V2;
		public Vector2 V3;
		public Vector2 V4;
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
	[DebuggerTypeProxy(typeof(WidgetDebugView))]
	public class Widget : Node
	{
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
		private Vector2 measuredMinSize;
		private Vector2 measuredMaxSize = Vector2.PositiveInfinity;
		private bool visible;
		private WidgetInput input;

		public static Widget Focused { get; private set; }

		public static Vector2 DefaultWidgetSize = new Vector2(100, 100);

		public Widget ParentWidget { get { return Parent != null ? Parent.AsWidget : null; } }

		public TabTraversable TabTravesable { get; set; }

		public KeyboardFocusScope FocusScope { get; set; }

		public ILayout Layout = AnchorLayout.Instance;

		/// <summary>
		/// Gets the layout-specific data.
		/// </summary>
		public LayoutCell LayoutCell { get; set; }

		public Vector2 EffectiveMinSize { get { return Vector2.Max(MinSize, MeasuredMinSize); } }
		public Vector2 EffectiveMaxSize { get { return Vector2.Min(MaxSize, MeasuredMaxSize); } }

		public Vector2 MeasuredMinSize
		{
			get { return measuredMinSize; }
			set {
				if (measuredMinSize != value) {
					measuredMinSize = value;
					InvalidateParentConstraintsAndArrangement();
				}
			}
		}

		public Vector2 MeasuredMaxSize
		{
			get { return measuredMaxSize; }
			set {
				if (measuredMaxSize != value) {
					measuredMaxSize = value;
					InvalidateParentConstraintsAndArrangement();
				}
			}
		}

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

		public float MinWidth
		{
			get { return MinSize.X; }
			set { MinSize = new Vector2(value, MinSize.Y); }
		}

		public float MinHeight
		{
			get { return MinSize.Y; }
			set { MinSize = new Vector2(MinSize.X, value); }
		}

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

		public float MaxWidth
		{
			get { return MaxSize.X; }
			set { MaxSize = new Vector2(value, MaxSize.Y); }
		}

		public float MaxHeight
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
			return Input.WasMouseReleased() && IsMouseOver();
		}

		private bool IsNumber(float x)
		{
			return !float.IsNaN(x) &&
					!float.IsInfinity(x);
		}

		/// <summary>
		/// Parent-relative position.
		/// </summary>
		[YuzuMember]
		[Tangerine(4)]
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

		[Tangerine(7)]
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
		/// SilenSize is needed to prevent unwanted propagation of `OnSizeChanged`
		/// while deserializing with Yuzu
		/// </summary>
		[YuzuMember("Size")]
		public Vector2 SilentSize { get { return size; } set { size = value; } }

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
		[YuzuMember]
		[Tangerine(6)]
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

		[YuzuMember]
		[Tangerine(5)]
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
		[YuzuMember]
		[Tangerine(3)]
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
		[YuzuMember]
		[Tangerine(9)]
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

		[YuzuMember]
		public Anchors Anchors { get; set; }

		[YuzuMember]
		[Tangerine(10)]
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

		[YuzuMember]
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

		[YuzuMember]
		[Tangerine(2)]
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

		[YuzuMember]
		public SkinningWeights SkinningWeights { get; set; }

		[YuzuMember]
		public HitTestMethod HitTestMethod { get; set; }

		[YuzuMember]
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
				if ((DirtyMask & (DirtyFlags.Visible | DirtyFlags.Color | DirtyFlags.TangerineFlags)) == 0) {
					return globallyVisible;
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

		public WidgetInput Input
		{
			get { return input ?? (input = new WidgetInput(this)); }
		}

		public bool HasInput() { return input != null; }

		/// <summary>
		/// Called before Update.
		/// </summary>
		public event UpdateHandler Updating;

		/// <summary>
		/// Called after Update.
		/// </summary>
		public event UpdateHandler Updated;

		public Widget()
		{
			Layout = AnchorLayout.Instance;
			AsWidget = this;
			Size = DefaultWidgetSize;
			Color = Color4.White;
			Scale = Vector2.One;
			Visible = true;
			Blending = Blending.Inherited;
			Shader = ShaderId.Inherited;
			direction = new Vector2(1, 0);
		}

		public bool IsFocused() { return Focused == this; }
		public void SetFocus() { SetFocus(this); }
		public void RevokeFocus()
		{
			if (IsFocused()) {
				var scope = KeyboardFocusScope.GetEnclosingScope(this);
				SetFocus(scope != null ? scope.Widget : null);
			}
		}

		internal static void SetFocus(Widget value)
		{
			if (Focused == value) {
				return;
			}
			if (value != null) {
				Application.SoftKeyboard.Show(true, value.Text);
			} else {
				Application.SoftKeyboard.Show(false, "");
			}
			Focused = value;
			foreach (var i in Application.Windows) {
				i.Invalidate();
			}
		}

		internal void InvalidateParentConstraintsAndArrangement()
		{
			if (ParentWidget != null) {
				ParentWidget.Layout.InvalidateConstraintsAndArrangement(ParentWidget);
			}
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
		/// Возвращает клон этого виджета. Используйте Clone() as Widget, т.к. он возвращает Node (базовый объект виджета)
		/// </summary>
		public override Node Clone()
		{
			var clone = base.Clone().AsWidget;
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
			if (input != null) {
				input.DispatchEvents();
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
			globalColor = Color;
			globalBlending = Blending;
			globalShader = Shader;
			globallyVisible = Visible && color.A != 0;
			if (Application.IsTangerine) {
				globallyVisible |= GetTangerineFlag(TangerineFlags.Shown);
				globallyVisible &= !GetTangerineFlag(TangerineFlags.Hidden | TangerineFlags.HiddenOnExposition);
			}
			localToWorldTransform = CalcLocalToParentTransform();
			if (Parent != null) {
				var parentWidget = Parent.AsWidget;
				var parentNode3D = Parent.AsNode3D;
				if (parentWidget != null) {
					localToWorldTransform *= parentWidget.localToWorldTransform;
					globalColor *= parentWidget.globalColor;
					globalBlending = Blending == Blending.Inherited ? parentWidget.globalBlending : Blending;
					globalShader = Shader == ShaderId.Inherited ? parentWidget.globalShader : Shader;
					globallyVisible &= parentWidget.globallyVisible;
				} else if (parentNode3D != null) {
					globalColor *= parentNode3D.GlobalColor;
					globallyVisible &= parentNode3D.GloballyVisible;
				}
			}
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
			if (GloballyVisible) {
				AddContentsToRenderChain(chain);
			}
		}

		public bool IsMouseOver()
		{
			var mouseOwner = WidgetInput.MouseCaptureStack.Top;
			return (mouseOwner == null || mouseOwner == this) && WidgetContext.Current.NodeUnderMouse == this;
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

		private static RenderChain renderChain = new RenderChain();

		/// <summary>
		/// Performs hit test only for this widget and its descendants. Returns true if the widget or one of its decendants contains the given point.
		/// This method doesn't take in account if one of the widget's ancestors overlaps the widget.
		/// </summary>
		public bool LocalHitTest(ref HitTestArgs args)
		{
			lock (renderChain) {
				var savedHitTestTarget = HitTestTarget;
				try {
					HitTestTarget = true;
					AddToRenderChain(renderChain);
					return renderChain.HitTest(ref args);
				} finally {
					renderChain.Clear();
					HitTestTarget = savedHitTestTarget;
				}
			}
		}

		/// <summary>
		/// Checks whether this widget or one of its descendents contains the given point.
		/// This method doesn't take in account if one of the widget's ancestors overlaps the widget.
		/// </summary>
		public bool LocalHitTest(Vector2 point)
		{
			var args = new HitTestArgs(point);
			return LocalHitTest(ref args);
		}

		/// <summary>
		/// Checks whether this widget contains the given point.
		/// </summary>
		internal protected override bool PartialHitTest(ref HitTestArgs args)
		{
			Node targetNode;
			for (targetNode = this; targetNode != null; targetNode = targetNode.Parent) {
				var method = targetNode.AsWidget != null ? targetNode.AsWidget.HitTestMethod : HitTestMethod.Contents;
				if (method == HitTestMethod.Skip || (targetNode != this && method == HitTestMethod.BoundingRect)) {
					return false;
				}
				if (targetNode.HitTestTarget) {
					break;
				}
			}
			if (targetNode == null || !IsPointInsideClipperWidget(ref args)) {
				return false;
			}
			if (
				HitTestMethod == HitTestMethod.BoundingRect && IsInsideBoundingRect(args.Point) ||
			    HitTestMethod == HitTestMethod.Contents && PartialHitTestByContents(ref args)
			) {
				args.Node = targetNode;
				return true;
			}
			return false;
		}

		internal protected virtual bool PartialHitTestByContents(ref HitTestArgs args)
		{
			return false;
		}

		internal bool IsInsideBoundingRect(Vector2 point)
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

		private bool IsPointInsideClipperWidget(ref HitTestArgs args)
		{
			return args.ClipperWidget == null || args.ClipperWidget.IsInsideBoundingRect(args.Point);
		}

		/// <summary>
		/// Renders widget's descendants to texture. If you want to render widget itself
		/// then add it to provided renderChain before calling RenderToTexture. Be sure
		/// not to do this inside Render() call.
		/// </summary>
		/// <param name="texture">Texture to render to. Must be RenderTexture or SerializableTexture</param>
		/// <param name="renderChain">Render queue (order relation of elements with tree structure and layers)</param>
		/// <param name="clearRenderTarget">Whether to clear texture before rendering or not.</param>
		public void RenderToTexture(ITexture texture, RenderChain renderChain, bool clearRenderTarget = true)
		{
			if (Width > 0 && Height > 0) {
				var scissorTest = Renderer.ScissorTestEnabled;
				if (scissorTest) {
					Renderer.ScissorTestEnabled = false;
				}
				texture.SetAsRenderTarget();
				var savedViewport = Renderer.Viewport;
				Renderer.Viewport = new WindowRect { X = 0, Y = 0, Width = texture.ImageSize.Width, Height = texture.ImageSize.Height };
				if (clearRenderTarget) {
					Renderer.Clear(0, 0, 0, 0);
				}
				Renderer.PushProjectionMatrix();
				Renderer.SetOrthogonalProjection(0, 0, Width, Height);
				for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
					node.AddToRenderChain(renderChain);
				}
				renderChain.RenderAndClear();
				texture.RestoreRenderTarget();
				Renderer.Viewport = savedViewport;
				Renderer.PopProjectionMatrix();
				if (scissorTest) {
					Renderer.ScissorTestEnabled = true;
				}
			}
		}

		/// <summary>
		/// Âîçâðàùàåò true, åñëè äâà âèäæåòà ïåðåñåêàþòñÿ. Ðåçóëüòàò íå çàâèñèò îò ñâîéñòâ HitTestMethod.
		/// Ïðîâåðÿåòñÿ òîëüêî ïåðåñå÷åíèå ãðàíèö âèäæåòîâ
		/// </summary>
		public static bool AreWidgetsIntersected(Widget a, Widget b)
		{
			Vector2[] rect = new Vector2[4] {
				new Vector2(0, 0), new Vector2(1, 0),
				new Vector2(1, 1), new Vector2(0, 1)
			};
			var sizes = new Vector2[2] { a.Size, b.Size };
			var matrices = new Matrix32[2] { a.LocalToWorldTransform, b.LocalToWorldTransform };
			var det = new float[2] { matrices[0].CalcDeterminant(), matrices[1].CalcDeterminant() };
			if (det[0] == 0 || det[1] == 0) {
				return false;
			}
			for (int k = 0; k < 2; k++)
				for (int i = 0; i < 4; i++) {
					var ptA = matrices[k] * (rect[i] * sizes[k]);
					var ptB = matrices[k] * (rect[(i + 1) % 4] * sizes[k]);
					var isOutside = true;
					for (int j = 0; j < 4; j++) {
						var pt = matrices[1 - k] * (rect[j] * sizes[1 - k]);
						isOutside = isOutside && GeometryUtils.CalcPointHalfPlane(pt, ptA, ptB) * det[k] < 0;
					}
					if (isOutside)
						return false;
				}
			return true;
		}

		public void CenterOnParent()
		{
			if (Parent == null) {
				throw new Lime.Exception("Parent must not be null");
			}
			Position = Parent.AsWidget.Size * 0.5f;
			Pivot = Vector2.Half;
		}

		public Matrix32 CalcTransitionToSpaceOf(Widget container)
		{
			Matrix32 mtx1 = container.LocalToWorldTransform.CalcInversed();
			Matrix32 mtx2 = LocalToWorldTransform;
			return mtx2 * mtx1;
		}

		public void CalcHullInSpaceOf(out WidgetHull hull, Widget container)
		{
			var transform = CalcTransformInSpaceOf(container);
			hull.V1 = transform.Position - transform.U * Size.X * Pivot.X - transform.V * Size.Y * Pivot.Y;
			hull.V2 = hull.V1 + transform.U * Size.X;
			hull.V3 = hull.V1 + transform.U * Size.X + transform.V * Size.Y;
			hull.V4 = hull.V1 + transform.V * Size.Y;
		}

		/// <summary>
		/// Âîçâðàùàåò îãðàíè÷èâàþùèé ïðÿìîóãîëüíèê ýòîãî âèäæåòà, åñëè áû îí íàõîäèëñÿ â ýòîé æå ýêðàííîé êîîðäèíàòå, íî â äðóãîì êîíòåéíåðå
		/// </summary>
		public Rectangle CalcAABBInSpaceOf(Widget container)
		{
			WidgetHull hull;
			CalcHullInSpaceOf(out hull, container);
			var aabb = new Rectangle(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue)
				.IncludingPoint(hull.V1)
				.IncludingPoint(hull.V2)
				.IncludingPoint(hull.V3)
				.IncludingPoint(hull.V4);
			return aabb;
		}

		/// <summary>
		/// Calculates AABB in the viewport space.
		/// AABB.A specifies the lower left corner of the rectangle.
		/// AABB.A specifies the upper right corner of the rectangle.
		/// </summary>
		public IntRectangle CalcAABBInViewportSpace()
		{
			var aabb = CalcAABBInSpaceOf(WidgetContext.Current.Root);
			// Get the projected AABB coordinates in the normalized OpenGL space
			Matrix44 proj = Renderer.Projection;
			aabb.A = proj.TransformVector(aabb.A);
			aabb.B = proj.TransformVector(aabb.B);
			// Transform to 0,0 - 1,1 coordinate space
			aabb.Left = (1 + aabb.Left) / 2;
			aabb.Right = (1 + aabb.Right) / 2;
			aabb.Top = (1 + aabb.Top) / 2;
			aabb.Bottom = (1 + aabb.Bottom) / 2;
			// Transform to vieport coordinates
			var viewport = Renderer.Viewport;
			var min = new Vector2(viewport.X, viewport.Y);
			var max = new Vector2(viewport.X + viewport.Width, viewport.Y + viewport.Height);
			return new IntRectangle {
				A = new IntVector2(
					Mathf.Lerp(aabb.Left, min.X, max.X).Round(),
					Mathf.Lerp(aabb.Bottom, min.Y, max.Y).Round()
				),
				B = new IntVector2(
					Mathf.Lerp(aabb.Right, min.X, max.X).Round(),
					Mathf.Lerp(aabb.Top, min.Y, max.Y).Round()
				)
			};
		}

		/// <summary>
		/// Calculates AABB in the window space.
		/// AABB.A specifies the upper left corner of the rectangle.
		/// AABB.A specifies the lower right corner of the rectangle.
		/// </summary>
		public Rectangle CalcAABBInWindowSpace()
		{
			var aabb = (Rectangle)CalcAABBInViewportSpace();
			var vpOrigin = (Vector2)Renderer.Viewport.Origin;
			aabb.A += vpOrigin;
			aabb.B += vpOrigin;
			var window = CommonWindow.Current;
			aabb.A /= window.PixelScale;
			aabb.B /= window.PixelScale;
			return new Rectangle {
				A = new Vector2(aabb.A.X, window.ClientSize.Y - aabb.B.Y),
				B = new Vector2(aabb.B.X, window.ClientSize.Y - aabb.A.Y),
			};
		}

		public Transform2 CalcTransformFromMatrix(Matrix32 matrix)
		{
			var v1 = new Vector2(1, 0);
			var v2 = new Vector2(0, 1);
			var v3 = new Vector2(0, 0);
			v1 = matrix.TransformVector(v1);
			v2 = matrix.TransformVector(v2);
			v3 = matrix.TransformVector(v3);
			v1 = v1 - v3;
			v2 = v2 - v3;
			Transform2 transform;
			transform.Position = matrix.TransformVector(Pivot * Size);
			transform.Scale = new Vector2(v1.Length, v2.Length);
			transform.Rotation = v1.Atan2Deg;
			transform.U = v1;
			transform.V = v2;
			return transform;
		}

		/// <summary>
		/// Âîçâðàùàåò èíôîðìàöèþ î òðàíñôîðìàöèè ýòîãî âèäæåòà, åñëè áû îí íàõîäèëñÿ â ýòîé æå ýêðàííîé êîîðäèíàòå, íî â äðóãîì êîíòåéíåðå
		/// </summary>
		public Transform2 CalcTransformInSpaceOf(Widget container)
		{
			Matrix32 matrix = CalcTransitionToSpaceOf(container);
			return CalcTransformFromMatrix(matrix);
		}

		/// <summary>
		/// Âîçâðàùàåò ïîçèöèþ ýòîãî âèäæåòà, åñëè áû îí íàõîäèëñÿ â ýòîé æå ýêðàííîé êîîðäèíàòå, íî â äðóãîì êîíòåéíåðå
		/// </summary>
		public Vector2 CalcPositionInSpaceOf(Widget container)
		{
			Matrix32 matrix = CalcTransitionToSpaceOf(container);
			return matrix.TransformVector(Pivot * Size);
		}

		/// <summary>
		/// Îòëàäî÷íûé ìåòîä, ïîçâîëÿþùèé óçíàòü ïðè÷èíû, ïî êîòîðûì âèäæåò íå îòðèñîâûâàåòñÿ
		/// </summary>
		public virtual IEnumerable<string> GetVisibilityIssues()
		{
			if (!DescendantOrThis(WidgetContext.Current.Root)) {
				yield return "The widget is not included to the world hierarchy";
			}
			if (!Visible) {
				yield return "The flag 'Visible' is not set";
			} else if (Opacity == 0) {
				yield return "It is fully transparent! Check up 'Opacity' property!";
			} else if (Opacity < 0.1f) {
				yield return "It is almost transparent! Check up 'Opacity' property!";
			} else if (!GloballyVisible) {
				yield return "One of its parent has 'Visible' flag not set";
			} else if (GlobalColor.A < 10) {
				yield return "One of its parent has 'Opacity' close to zero";
			}
			var basis = CalcTransformInSpaceOf(WidgetContext.Current.Root);
			if ((basis.Scale.X * Size.X).Abs() < 1 || (basis.Scale.Y * Size.Y).Abs() < 1) {
				yield return string.Format("The widget is probably too small");
			}
			bool passedHitTest = false;
			var hitTestLT = WidgetContext.Current.Root.Position;
			var hitTestRB = hitTestLT + WidgetContext.Current.Root.Size;
			for (float y = hitTestLT.Y; y < hitTestRB.Y && !passedHitTest; y++) {
				for (float x = hitTestLT.X; x < hitTestRB.X && !passedHitTest; x++) {
					var a = new HitTestArgs(new Vector2(x, y));
					if (PartialHitTest(ref a)) {
						passedHitTest = true;
					}
				}
			}
			if (!passedHitTest) {
				yield return string.Format("SelfHitTest() returns false in range [{0}] x [{1}].", hitTestLT, hitTestRB);
			}
			if (!(this is Image) && (this.Nodes.Count == 0)) {
				yield return "The widget doesn't contain any drawable node";
			}
		}

		public void PrepareRendererState()
		{
			Renderer.Transform1 = LocalToWorldTransform;
			Renderer.Blending = GlobalBlending;
			Renderer.Shader = GlobalShader;
		}

	}
}
