using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;
using System.Linq;
using ProtoBuf;
using System.ComponentModel;
using System.Diagnostics;

namespace Lime
{
	public delegate void UpdateHandler(float delta);

	/// <summary>
	/// Базовый класс для всех объектов сцены. Представляет собой контейнер, в который может быть добавлен любой объект
	/// </summary>
	[ProtoContract]
	[ProtoInclude(101, typeof(Widget))]
	[ProtoInclude(102, typeof(ParticleModifier))]
	[ProtoInclude(103, typeof(ImageCombiner))]
	[ProtoInclude(104, typeof(PointObject))]
	[ProtoInclude(105, typeof(Bone))]
	[ProtoInclude(106, typeof(Audio))]
	[ProtoInclude(107, typeof(SplineGear))]
	[ProtoInclude(108, typeof(TextStyle))]
	[ProtoInclude(109, typeof(LinearLayout))]
	[ProtoInclude(110, typeof(ModelNode))]
	[DebuggerTypeProxy(typeof(NodeDebugView))]
	public class Node : IDisposable
	{
		protected internal enum DirtyFlags
		{
			None			= 0,
			Visible			= 1 << 0,
			Color			= 1 << 1,
			Transform		= 1 << 2,
			All				= ~None
		}

		/// <summary>
		/// Генерируется при завершении анимации (например достигнут маркер типа Stop). После генерации сбрасывается в null.
		/// </summary>
		public event Action AnimationStopped
		{
			add { DefaultAnimation.Stopped += value; }
			remove { DefaultAnimation.Stopped -= value; }
		}

		internal protected bool GlobalValuesValid;

		#region properties
#if WIN
		public Guid Guid { get; set; }
#endif
	
		/// <summary>
		/// Название виджета, назначенное ему в HotStudio. Может быть null
		/// </summary>
		[ProtoMember(1)]
		public string Id { get; set; }

		[ProtoMember(2)]
		public string ContentsPath { get; set; }

		[Trigger]
		public string Trigger { get; set; }

		/// <summary>
		/// Родительский объект (в его контейнере находится этот объект)
		/// </summary>
		public Node Parent { get; internal set; }

		/// <summary>
		/// Преобразует этот объект в объект типа Widget. Если это не Widget, возвращает null (аналогично return this as Widget)
		/// </summary>
		public Widget AsWidget { get; internal set; }

		public ModelNode AsModelNode { get; internal set; }

		internal Node NextToRender;

		public Node NextSibling;

		/// <summary>
		/// Позволяет вручную задать Z-order (порядок отрисовки). 0 - по умолчанию, 1 - самый нижний слой, 99 - самый верхний слой
		/// </summary>
		public int Layer { get; set; }

		/// <summary>
		/// Аниматоры, изменяющие анимированные параметры объекта в время обновления
		/// </summary>
		[ProtoMember(5)]
		public AnimatorCollection Animators;

		/// <summary>
		/// Список дочерних объектов (объектов, находящихся в контейнере этого объекта)
		/// </summary>
		[ProtoMember(6)]
		public NodeList Nodes;

		/// <summary>
		/// Маркеры анимации
		/// </summary>
		public MarkerCollection Markers { get { return DefaultAnimation.Markers; } }

		/// <summary>
		/// Возвращает true, если проигрывается анимация
		/// </summary>
		public bool IsRunning { get { return DefaultAnimation.IsRunning; } set { DefaultAnimation.IsRunning = value; } }

		/// <summary>
		/// Возвращает true, если в данный момент анимация не проигрывается (например анимация не была запущена или доигралась до конца)
		/// </summary>
		public bool IsStopped {
			get { return !IsRunning; } 
			set { IsRunning = !value; } 
		}

		/// <summary>
		/// Время текущего кадра анимации (в миллисекундах)
		/// </summary>
		public int AnimationTime
		{
			get { return DefaultAnimation.Time; }
			set
			{
				DefaultAnimation.Time = value;
				DefaultAnimation.ApplyAnimators(this, invokeTriggers: false);
			}
		}

		public Animation DefaultAnimation
		{
			get
			{
				if (Animations.Count == 0) {
					Animations.Add(new Animation());
				}
				return Animations[0];
			}
		}
		
		/// <summary>
		/// Пользовательские данные (аналогично UserData)
		/// Если сцена загружена из внешнего файла, то здесь будет храниться путь к этой сцене (внешние сцены)
		/// Отличие от UserData в том, что Tag можно задавать из HotStudio, а UserData нет
		/// </summary>
		[ProtoMember(11)]
		public string Tag { get; set; }

		[ProtoMember(13)]
		public AnimationList Animations;

		/// <summary>
		/// Название текущей анимации. Название берется из названия маркера Play.
		/// Если анимация остановилась, это свойство не изменится
		/// </summary>
		public string CurrentAnimation
		{
			get { return DefaultAnimation.RunningMarkerId; }
			set { DefaultAnimation.RunningMarkerId = value; }
		}

		/// <summary>
		/// Множитель скорости анимации
		/// </summary>
		public float AnimationSpeed { get; set; }

		/// <summary>
		/// Пользовательские данные (аналогично Tag)
		/// </summary>
		public object UserData { get; set; }

		protected DirtyFlags DirtyMask;
		
		protected bool IsDirty(DirtyFlags mask) { return (DirtyMask & mask) != 0; }

		#endregion
		#region Methods

		public static int CreatedCount = 0;
		public static int FinalizedCount = 0;

		public Node()
		{
			AnimationSpeed = 1;
			Animators = new AnimatorCollection(this);
			Animations = new AnimationList();
			Nodes = new NodeList(this);
			++CreatedCount;
		}

		public virtual void Dispose()
		{
			for (var n = Nodes.FirstOrNull(); n != null; n = n.NextSibling) {
				n.Dispose();
			}
			Nodes.Clear();
		}

		protected internal void PropagateDirtyFlags(DirtyFlags mask = DirtyFlags.All)
		{
			if ((DirtyMask & mask) == mask)
				return;
			DirtyMask |= mask;
			for (var n = Nodes.FirstOrNull(); n != null; n = n.NextSibling) {
				if ((n.DirtyMask & mask) != mask) {
					n.PropagateDirtyFlags(mask);
				}
			}
		}

		protected void RecalcDirtyGlobals()
		{
			if (DirtyMask == DirtyFlags.None) {
				return;
			}
			if (Parent != null && Parent.DirtyMask != DirtyFlags.None) {
				Parent.RecalcDirtyGlobals();
			}
			RecalcDirtyGlobalsUsingParents();
			DirtyMask = DirtyFlags.None;
		}

		protected virtual void RecalcDirtyGlobalsUsingParents() { }

#if LIME_COUNT_NODES
		~Node() {
			++FinalizedCount;
		}
#endif

		/// <summary>
		/// Возвращает самый верхний объект в иерархии этого объекта
		/// </summary>
		public Node GetRoot()
		{
			Node node = this;
			while (node.Parent != null) {
				node = node.Parent;
			}
			return node;
		}

		public bool ChildOf(Node node)
		{
			for (Node n = Parent; n != null; n = n.Parent) {
				if (n == node)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Запускает анимацию и возвращает true. Если такой анимации нет, возвращает false
		/// </summary>
		/// <param name="markerId">Название маркера анимации</param>
		public bool TryRunAnimation(string markerId, string animationId = null)
		{
			return Animations.TryRun(animationId, markerId);
		}

		/// <summary>
		/// Запускает анимацию. Если такой анимации нет, генерирует исключение
		/// </summary>
		/// <param name="markerId">Название маркера анимации</param>
		/// <exception cref="Lime.Exception"/>
		public void RunAnimation(string markerId, string animationId = null)
		{
			Animations.Run(animationId, markerId);
		}

		/// <summary>
		/// Текущий кадр анимации
		/// </summary>
		public int AnimationFrame {
			get { return AnimationUtils.MsecsToFrames(AnimationTime); }
			set { AnimationTime = AnimationUtils.FramesToMsecs(value); }
		}

		/// <summary>
		/// Медленное, но безопасное клонирование. Эта функция основана на сериализации protobuf-net
		/// </summary>
		public Node DeepCloneSafe()
		{
			return Serialization.DeepClone<Node>(this);
		}

		/// <summary>
		/// Медленное, но безопасное клонирование. Эта функция основана на сериализации protobuf-net
		/// </summary>
		public T DeepCloneSafe<T>() where T : Node
		{
			return DeepCloneSafe() as T;
		}

		/// <summary>
		/// Используйте для быстрого клонирования с сохранением иерархии для неанимированных объектов.
		/// Функция основана на MemberwiseClone().
		/// Свойства Animators, Markers, SkinningWeights будут общими у клона и оригинала
		/// </summary>
		public virtual Node DeepCloneFast()
		{
			var clone = (Node)MemberwiseClone();
			++CreatedCount;
			clone.Parent = null;
			clone.NextSibling = null;
			clone.AsWidget = clone as Widget;
			clone.GlobalValuesValid = false;
			clone.Animations = Animations.Clone();
			clone.Animators = AnimatorCollection.SharedClone(clone, Animators);
			clone.Nodes = Nodes.DeepCloneFast(clone);
			return clone;
		}

		/// <summary>
		/// Используйте для быстрого клонирования с сохранением иерархии для неанимированных объектов.
		/// Функция основана на MemberwiseClone().
		/// Свойства Animators, Markers, SkinningWeights будут общими у клона и оригинала
		/// </summary>
		public T DeepCloneFast<T>() where T : Node
		{
			return (T)DeepCloneFast();
		}

		public override string ToString()
		{
			return string.Format("{0}, \"{1}\", {2}", GetType().Name, Id ?? "", GetHierarchyPath());
		}

		private string GetHierarchyPath()
		{
			string r = string.IsNullOrEmpty(Id) ? String.Format("[{0}]", GetType().Name) : Id;
			if (!string.IsNullOrEmpty(Tag)) {
				r += string.Format(" ({0})", Tag);
			}
			if (Parent != null) {
				r = Parent.GetHierarchyPath() + "/" + r;
			}
			return r;
		}

		/// <summary>
		/// Удаляет этот объект из контейнера родителя. Если Parent == null, сгенерирует исключение
		/// </summary>
		/// <exception cref="Lime.Exception"/>
		public void Unlink()
		{
			if (Parent != null) {
				Parent.Nodes.Remove(this);
			}
		}

		/// <summary>
		/// Вызывает Unlink и Dispose
		/// Если Parent == null, сгенерирует исключение
		/// </summary>
		/// <exception cref="Lime.Exception"/>
		public void UnlinkAndDispose()
		{
			Unlink();
			Dispose();
		}

		/// <summary>
		/// Обновляет состояние объекта (обновляет его анимации, генерирует события и. т.д.).
		/// Вызывает Update для всех дочерних виджетов. В нормальных условиях этот метод должен вызываться 1 раз за кадр.
		/// </summary>
		/// <param name="delta">Количество секунд, прошедшее с момента предыдущего вызова Update</param>
		public virtual void Update(float delta)
		{
			delta *= AnimationSpeed;
			AdvanceAnimation(delta);
			SelfUpdate(delta);
			for (var node = Nodes.FirstOrNull(); node != null; ) {
				var next = node.NextSibling;
				node.Update(delta);
				node = next;
			}
			SelfLateUpdate(delta);
		}

		protected virtual void SelfUpdate(float delta) { }

		protected virtual void SelfLateUpdate(float delta) { }

		public virtual void Render() {}

		/// <summary>
		/// Добавляет виджет и все его дочерние виджеты в очередь отрисовки
		/// </summary>
		public virtual void AddToRenderChain(RenderChain chain)
		{
			for (Node node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
				node.AddToRenderChain(chain);
			}
		}

		protected internal virtual void OnTrigger(string property)
		{
			if (property != "Trigger") {
				return;
			}
			if (String.IsNullOrEmpty(Trigger)) {
				AnimationTime = 0;
				IsRunning = true;
			} else {
				TryRunAnimation(Trigger);
			}
		}

		/// <summary>
		/// Добавляет указанный объект в контейнер этого объекта. Добавляемый объект будет на самом верхнем слое.
		/// Добавляемый объект не должен находится в чьем-либо контейнере
		/// (проверяйте свойство Parent == null и используйте метод Unlink, чтобы удалить его из контейнера)
		/// </summary>
		/// <exception cref="Lime.Exception"/>
		public void AddNode(Node node)
		{
			Nodes.Add(node);
		}

		/// <summary>
		/// Добавляет этот объект в контейнер указанного объекта на самый верхний слой
		/// Объект не должен находится в чьем-либо контейнере
		/// (проверяйте свойство Parent == null и используйте метод Unlink, чтобы удалить этот объект из контейнера)
		/// </summary>
		/// <exception cref="Lime.Exception"/>
		public void AddToNode(Node node)
		{
			node.Nodes.Add(this);
		}

		/// <summary>
		/// Добавляет указанный объект в контейнер этого объекта. Добавляемый объект будет на самом нижнем слое.
		/// Добавляемый объект не должен находится в чьем-либо контейнере
		/// (проверяйте свойство Parent == null и используйте метод Unlink, чтобы удалить его из контейнера)
		/// </summary>
		/// <exception cref="Lime.Exception"/>
		public void PushNode(Node node)
		{
			Nodes.Push(node);
		}

		/// <summary>
		/// Добавляет этот объект в контейнер указанного объекта на самый нижний слой
		/// Объект не должен находится в чьем-либо контейнере
		/// (проверяйте свойство Parent == null и используйте метод Unlink, чтобы удалить этот объект из контейнера)
		/// </summary>
		/// <exception cref="Lime.Exception"/>
		public void PushToNode(Node node)
		{
			node.Nodes.Push(this);
		}

		/// <summary>
		/// Ищет дочерний объект (рекурсивно). Если такого объекта нет, генерирует исключение
		/// </summary>
		/// <typeparam name="T">Тип искомого объекта</typeparam>
		/// <param name="id">Id искомого объекта</param>
		/// <exception cref="Lime.Exception"/>
		public T Find<T>(string id) where T : Node
		{
			T result = TryFindNode(id) as T;
			if (result == null) {
				throw new Lime.Exception("'{0}' of {1} not found for '{2}'", id, typeof(T).Name, ToString());
			}
			return result;
		}

		/// <summary>
		/// Ищет дочерний объект (рекурсивно). Если такого объекта нет, генерирует исключение
		/// </summary>
		/// <typeparam name="T">Тип искомого объекта</typeparam>
		/// <exception cref="Lime.Exception"/>
		public T Find<T>(string format, params object[] args) where T : Node
		{
			return Find<T>(string.Format(format, args));
		}

		/// <summary>
		/// Ищет дочерний объект (рекурсивно). Если такого объекта нет, возвращает false
		/// </summary>
		/// <typeparam name="T">Тип искомого объекта</typeparam>
		/// <param name="id">Id искомого объекта</param>
		/// <param name="node">Переменная, в которую будет записан результат</param>
		public bool TryFind<T>(string id, out T node) where T : Node
		{
			node = TryFindNode(id) as T;
			return node != null;
		}

		/// <summary>
		/// Ищет дочерний объект (рекурсивно). Если такого объекта нет, возвращает null
		/// </summary>
		/// <typeparam name="T">Тип искомого объекта</typeparam>
		/// <param name="id">Id искомого объекта</param>
		public T TryFind<T>(string id) where T : Node
		{
			return TryFindNode(id) as T;
		}

		/// <summary>
		/// Ищет дочерний объект (рекурсивно). Если такого объекта нет, возвращает null
		/// </summary>
		/// <typeparam name="T">Тип искомого объекта</typeparam>
		public T TryFind<T>(string format, params object[] args) where T : Node
		{
			return TryFind<T>(string.Format(format, args));
		}

		/// <summary>
		/// Ищет дочерний объект (рекурсивно). Если такого объекта нет, генерирует исключение
		/// </summary>
		/// <param name="path">Путь к виджету или Id, (пример пути: Root/Human/Head/Eye, где Eye - Id искомого виджета)</param>
		/// <exception cref="Lime.Exception"/>
		public Node FindNode(string path)
		{
			var node = TryFindNode(path);
			if (node == null) {
				throw new Lime.Exception("'{0}' not found for '{1}'", path, ToString());
			}
			return node;
		}

		/// <summary>
		/// Ищет дочерний объект (рекурсивно). Если такого объекта нет, возвращает null
		/// </summary>
		/// <param name="path">Путь к виджету или Id, (пример пути: Root/Human/Head/Eye, где Eye - Id искомого виджета)</param>
		public Node TryFindNode(string path)
		{
			if (path.IndexOf('/') >= 0) {
				return TryFindNodeByPath(path);
			} else {
				return TryFindNodeById(path);
			}
		}

		/// <summary>
		/// Ищет дочерний объект по его пути. Если такого объекта нет, возвращает null
		/// </summary>
		/// <param name="path">Путь к виджету, (пример пути: Root/Human/Head/Eye, где Eye - Id искомого виджета)</param>
		private Node TryFindNodeByPath(string path)
		{
			Node child = this;
			foreach (string id in path.Split('/')) {
				child = child.TryFindNodeById(id);
				if (child == null)
					break;
			}
			return child;
		}

		/// <summary>
		/// Перечисляет все объекты, входящие в контейнер этого и объекта и дочерних объектов
		/// </summary>
		public IEnumerable<Node> Descendants
		{
			get
			{
				for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
					yield return node;
					foreach (var i in node.Descendants) {
						yield return i;
					}
				}
			}
		}

		/// <summary>
		/// Этот метод масштабирует позицию и размер данного нода и всех его потомков.
		/// Метод полезен для адаптирования сцены под экраны с нестандартным DPI.
		/// Если позиция или размер виджета анинимированы, то ключи анимации также подвергаются соответствующему
		/// преобразованию.
		/// Для текстовых виджетов, размер шрифта масштабируется.
		/// </summary>
		/// <param name="roundCoordinates">Если true, то после масштабирования виджета, его размер округляется, а сам виджет сдвигается таким образом,
		/// чтобы его левый верхний угол (точка (0,0) в локальных координатах виджета) перешел в целочисленную позицию.</param>
		public virtual void StaticScale(float ratio, bool roundCoordinates)
		{
			foreach (var node in Nodes) {
				node.StaticScale(ratio, roundCoordinates);
			}
		}

		[ThreadStatic]
		private static Queue<Node> nodeSearchQueue;

		private Node TryFindNodeById(string id)
		{
			if (nodeSearchQueue == null) {
				nodeSearchQueue = new Queue<Node>();
			}
			var queue = nodeSearchQueue;
			queue.Enqueue(this);
			while (queue.Count > 0) {
				var parent = queue.Dequeue();
				var child = parent.Nodes.FirstOrNull();
				for (; child != null; child = child.NextSibling) {
					if (child.Id == id) {
						queue.Clear();
						return child;
					}
					queue.Enqueue(child);
				}
			}
			return null;
		}

		/// <summary>
		/// Продвигает анимацию на указанное время
		/// </summary>
		/// <param name="delta">Время в секундах</param>
		public void AdvanceAnimation(float delta)
		{
			for (var i = 0; i < Animations.Count; i++) {
				var a = Animations[i];
				if (a.IsRunning) {
					a.Advance(this, delta);
				}
			}
		}

		/// <summary>
		/// Загружает шрифты, текстуры и аниматоры для себя и всех дочерних объектов
		/// </summary>
		public void PreloadAssets()
		{
			foreach (var prop in GetType().GetProperties()) {
				if (prop.PropertyType == typeof(ITexture)) {
					PreloadTexture(prop);
				} else if (prop.PropertyType == typeof(SerializableFont)) {
					PreloadFont(prop);
				}
			}
			foreach (var animator in Animators) {
				PreloadAnimatedTextures(animator);
			}
			foreach (var node in Nodes) {
				node.PreloadAssets();
			}
		}

		private static void PreloadAnimatedTextures(IAnimator animator)
		{
			var textureAnimator = animator as Animator<ITexture>;
			if (textureAnimator != null) {
				foreach (var key in textureAnimator.ReadonlyKeys) {
					key.Value.GetHandle();
				}
			}
		}

		private void PreloadFont(PropertyInfo prop)
		{
			var getter = prop.GetGetMethod();
			var font = getter.Invoke(this, new object[]{}) as SerializableFont;
			if (font != null) {
				foreach (var texture in font.Instance.Textures) {
					texture.GetHandle();
				}
			}
		}

		private void PreloadTexture(PropertyInfo prop)
		{
			var getter = prop.GetGetMethod();
			var texture = getter.Invoke(this, new object[]{}) as ITexture;
			if (texture != null) {
				texture.GetHandle();
			}
		}

		protected void LoadContent()
		{
			if (!string.IsNullOrEmpty(ContentsPath)) {
				LoadContentHelper();
			} else {
				for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
					node.LoadContent();
				}
			}
		}

		private void LoadContentHelper()
		{
			Nodes.Clear();
			Markers.Clear();
			var contentsPath = ResolveScenePath(ContentsPath);
			if (contentsPath == null) {
				return;
			}
			var content = new Frame(ContentsPath);
			if (content.AsWidget != null && AsWidget != null) {
				content.Update(0);
				content.AsWidget.Size = AsWidget.Size;
				content.Update(0);
			}
			Markers.AddRange(content.Markers);
			var nodes = content.Nodes.ToList();
			content.Nodes.Clear();
			Nodes.AddRange(nodes);
		}

		private static readonly string[] sceneExtensions = new[] { ".scene", ".model" };

		internal static string ResolveScenePath(string path)
		{
			var candidates = sceneExtensions.Select(ext => Path.ChangeExtension(path, ext)).Where(AssetsBundle.Instance.FileExists);
			if (candidates.Count() > 1) {
				throw new Lime.Exception("Ambiguity between: {0}", string.Join("; ", candidates));
			}
			return candidates.FirstOrDefault();
		}
		#endregion
	}
}