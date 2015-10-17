using System;
using Lime;
using ProtoBuf;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Lime
{
	/// <summary>
	/// TODO: Translate
	/// Якоря. Задают привязку границ виджета к границам его родителя
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
	/// TODO: Translate
	/// Виджет. Базовый класс всех визуальных 2D объектов и интерфейса пользователя.
	/// Атомарный объект пользовательского интерфейса. Подписан на события мыши, клавиатуры
	/// и других устройств ввода. Представляет собой прямоугольный контейнер, содержащий
	/// любые объекты сцены, в том числе и визуальные
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
	[ProtoInclude(113, typeof(ModelViewport))]
	[DebuggerTypeProxy(typeof(WidgetDebugView))]
	public partial class Widget : Node
	{
		// TODO: This will be removed
		public const int EmptyHitTestMask = 0;
		public const int ControlsHitTestMask = 1;

		/// <summary>
		/// TODO: Translate
		/// Минимально возможный номер слоя (свойство Layer)
		/// </summary>
		public const int MinLayer = 0;

		/// <summary>
		/// TODO: Translate
		/// Максимально возможный номер слоя (свойство Layer)
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
		private bool visible;

		#region Properties

		public Widget ParentWidget { get { return Parent != null ? Parent.AsWidget : null; } }

		public Layout Layout = AnchorLayout.Instance;

		/// <summary>
		/// Keeps layout-specific data, used by parent widget's Layout. 
		/// E.g: TableLayoutCell object, if parent widget has TableLayout.
		/// </summary>
		public object LayoutCell;

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

		/// <summary>
		/// TODO: Translate
		/// Действие, генерируемое, когда на виджет кликнули или нажали пальцем (для сенсорного экрана)
		/// </summary>
		public virtual Action Clicked {
			get { return clicked; }
			set { clicked = value; }
		}

		/// <summary>
		/// TODO: Translate
		/// Возвращает true, если на виджет кликнули или нажали пальцем (для сенсорного экрана)
		/// </summary>
		public virtual bool WasClicked()
		{
			return Input.WasMouseReleased() && HitTest(Input.MousePosition);
		}

		private void AssertIsNumber(float x)
		{
			if (float.IsNaN(x) || float.IsInfinity(x))
				throw new ArithmeticException();
		}

		/// <summary>
		/// TODO: Translate
		/// Позиция виджета в контейнере его родителя
		/// </summary>
		[ProtoMember(1)]
		public Vector2 Position
		{
			get { return position; }
			set
			{
				// TODO: Check through Debug.Assert
#if LIME_CHECK_FLOATS
				AssertIsNumber(value.X);
				AssertIsNumber(value.Y);
#endif
				if (position.X != value.X || position.Y != value.Y) {
					position = value;
					PropagateDirtyFlags(DirtyFlags.Transform);
				}
			}
		}

		/// <summary>
		/// TODO: Translate
		/// Позиция X виджета в контейнере его родителя (аналогично Position.X)
		/// </summary>
		public float X 
		{ 
			get { return position.X; } 
			set 
			{
#if LIME_CHECK_FLOATS
				AssertIsNumber(value);
#endif
				if (position.X != value) {
					position.X = value;
					PropagateDirtyFlags(DirtyFlags.Transform);
				}
			} 
		}

		/// <summary>
		/// TODO: Translate
		/// Позиция Y виджета в контейнере его родителя (аналогично Position.Y)
		/// </summary>
		public float Y
		{
			get { return position.Y; }
			set
			{
#if LIME_CHECK_FLOATS
				AssertIsNumber(value);
#endif
				if (position.Y != value) {
					position.Y = value;
					PropagateDirtyFlags(DirtyFlags.Transform);
				}
			}
		}

		/// <summary>
		/// TODO: Translate
		/// Определяет размеры его контейнера, по которому проверяются столкновения. Не влияет на визуальный размер
		/// Для изменения визуального размера используйте Scale
		/// </summary>
		[ProtoMember(2)]
		public Vector2 Size
		{
			get { return size; }
			set
			{
#if LIME_CHECK_FLOATS
				AssertIsNumber(value.X);
				AssertIsNumber(value.Y);
#endif
				if (value.X != size.X || value.Y != size.Y) {
					var sizeDelta = value - size;
					size = value;
					OnSizeChanged(sizeDelta);
					PropagateDirtyFlags(DirtyFlags.Transform);
				}
			}
		}

		/// <summary>
		/// TODO: Translate
		/// Останавливает все таски этого виджета (Tasks). Вызывает Dispose для всех виджетов,
		/// вложенных в этот виджет
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

		/// <summary>
		/// TODO: Translate
		/// Ширина (см свойство Size)
		/// </summary>
		public float Width { 
			get { return size.X; }
			set {
				if (size.X != value)
					Size = new Vector2(value, Height);
			} 
		}

		/// <summary>
		/// TODO: Translate
		/// Высота (см свойство Size)
		/// </summary>
		public float Height {
			get { return size.Y; } 
			set {
				if (size.Y != value)
					Size = new Vector2(Width, value);
			} 
		}

		/// <summary>
		/// TODO: Translate
		/// Точка опоры. Определяет точку поворота и масштабирования.
		/// [0, 0] - верхний левый угол виджета, [1, 1] - правый нижний
		/// </summary>
		[ProtoMember(3)]
		public Vector2 Pivot 
		{ 
			get { return pivot; } 
			set 
			{
#if LIME_CHECK_FLOATS
				AssertIsNumber(value.X);
				AssertIsNumber(value.Y);
#endif
				if (pivot.X != value.X || pivot.Y != value.Y) {
					pivot = value;
					PropagateDirtyFlags(DirtyFlags.Transform);
				}
			} 
		}

		/// <summary>
		/// TODO: Translate
		/// Масштаб (от 0 до 1)
		/// </summary>
		[ProtoMember(4)]
		public Vector2 Scale 
		{ 
			get { return scale; } 
			set 
			{
#if LIME_CHECK_FLOATS
				AssertIsNumber(value.X);
				AssertIsNumber(value.Y);
#endif
				if (scale.X != value.X || scale.Y != value.Y) {
					scale = value;
					PropagateDirtyFlags(DirtyFlags.Transform);
				}
			} 
		}

		/// <summary>
		/// TODO: Translate
		/// Угол поворота в градусах против часовой стрелки
		/// </summary>
		[ProtoMember(5)]
		public float Rotation { 
			get { return rotation; }
			set 
			{
#if LIME_CHECK_FLOATS
				AssertIsNumber(value);
#endif
				if (rotation != value) {
					rotation = value;
					direction = Mathf.CosSin(Mathf.DegreesToRadians * value);
					PropagateDirtyFlags(DirtyFlags.Transform);
				}
			}
		}

		/// <summary>
		/// TODO: Translate
		/// Оттенок. При отрисовке цвет текстуры умножается на этот цвет.
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
		/// TODO: Translate
		/// Непрозрачность (0 - 1)
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

		/// <summary>
		/// TODO: Translate
		/// Якоря. Задают привязку границ виджета к границам его родителя
		/// </summary>
		[ProtoMember(7)]
		public Anchors Anchors { get; set; }

		/// <summary>
		/// TODO: Translate
		/// Задает способ смешивания при отрисовке
		/// </summary>
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

		/// <summary>
		/// TODO: Translate
		/// Идентификатор шейдера, который будет использоваться при отрисовке виджета
		/// </summary>
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
				}
			}
		}

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		[ProtoMember(11)]
		public SkinningWeights SkinningWeights { get; set; }

		/// <summary>
		/// TODO: Translate
		/// Способ проверки столкновений
		/// </summary>
		[ProtoMember(12)]
		public HitTestMethod HitTestMethod { get; set; }
		
		// TODO: Will be removed
		[ProtoMember(13)]
		public uint HitTestMask { get; set; }

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		[ProtoMember(14)]
		public BoneArray BoneArray;

		private Matrix32 localToWorldTransform;
		private Color4 globalColor;
		private Blending globalBlending;
		private ShaderId globalShader;
		private bool globallyVisible;

		public Matrix32 LocalToWorldTransform
		{
			get { RecalcDirtyGlobals(); return localToWorldTransform; }
		}

		public Color4 GlobalColor 
		{
			get { RecalcDirtyGlobals(); return globalColor; }
		}
		
		public Blending GlobalBlending
		{
			get { RecalcDirtyGlobals(); return globalBlending; }
		}

		public ShaderId GlobalShader
		{
			get { RecalcDirtyGlobals(); return globalShader; }
		}

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
		
		public Vector2 GlobalPosition { get { return LocalToWorldTransform.T; } }

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		public Vector2 GlobalCenter { get { return LocalToWorldTransform * (Size / 2); } }

		/// <summary>
		/// TODO: Add summary
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
		/// TODO: Translate
		/// Генерируется в начале процедуры обновления виджета (вызова метода Update)
		/// </summary>
		public event UpdateHandler Updating;

		/// <summary>
		/// TODO: Translate
		/// Генерируется в конце процедуры обновления виджета (вызова метода Update)
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

		WidgetInput input;
		public WidgetInput Input
		{
			get { return input ?? (input = new WidgetInput(this)); }
		}

		/// <summary>
		/// TODO: Translate
		/// Возвращает размер содержимого, находящегося в контейнере этого виджета
		/// </summary>
		public virtual Vector2 CalcContentSize()
		{
			return Size;
		}

		/// <summary>
		/// TODO: Translate
		/// Ищет виджет, находящийся в контейнере этого виджета (рекурсивно).
		/// Если виджета с таким Id нет, генерирует исключение
		/// </summary>
		/// <param name="id">Id искомого виджета</param>
		public Widget this[string id]
		{
			get { return Find<Widget>(id); }
		}

		/// <summary>
		/// TODO: Translate
		/// Ищет виджет, находящийся в контейнере этого виджета (рекурсивно).
		/// Если виджета с таким Id нет, генерирует исключение
		/// </summary>
		/// <param name="id">Id искомого виджета</param>
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

		/// <summary>
		/// TODO: Translate
		/// Обновляет состояние виджета (обновляет его анимации, генерирует события и. т.д.).
		/// Вызывает Update для всех дочерних виджетов. В нормальных условиях этот метод должен вызываться 1 раз за кадр.
		/// </summary>
		/// <param name="delta">Количество секунд, прошедшее с момента предыдущего вызова Update</param>
		public override void Update(float delta)
		{
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
		/// TODO: Translate
		/// Генерирует событие Updating
		/// </summary>
		/// <param name="delta">Количество секунд, прошедшее с момента предыдущего вызова Update</param>
		public void RaiseUpdating(float delta)
		{
			if (Updating != null) {
				Updating(delta * AnimationSpeed);
			}
		}

		/// <summary>
		/// TODO: Translate
		/// Генерирует событие Updated
		/// </summary>
		/// <param name="delta">Количество секунд, прошедшее с момента предыдущего вызова Update</param>
		public void RaiseUpdated(float delta)
		{
			if (Updated != null) {
				Updated(delta * AnimationSpeed);
			}
		}
		
		/// <summary>
		/// TODO: Add summary
		/// </summary>
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
				var geometryProperties = new string[] { "Position", "Size", "Pivot", "Rotation", "Scale" };
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

		/// <summary>
		/// TODO: Add summary
		/// </summary>
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
		/// TODO: Translate
		/// Добавляет виджет и все его дочерние виджеты в очередь отрисовки
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

		/// <summary>
		/// TODO: Translate
		/// Возвращает true, если курсор мыши попадает в виджет
		/// </summary>
		public bool IsMouseOver()
		{
			return Input.IsAcceptingMouse() && HitTest(Input.MousePosition);
		}

		/// <summary>
		/// TODO: Translate
		/// Возвращает true, если точка попадает в виджет
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
			World.Instance.AsWidget.EnumerateHitTestTargets(targets, HitTestMask);
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
				World.Instance.NodeUnderCursor = this;
			}
		}

		#endregion
	}
}
