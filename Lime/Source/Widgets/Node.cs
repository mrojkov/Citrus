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
	/// Scene tree element.
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
		[Flags]
		protected internal enum DirtyFlags
		{
			None      = 0,
			Visible   = 1 << 0,
			Color     = 1 << 1,
			Transform = 1 << 2,
			All       = ~None
		}

		/// <summary>
		/// Is invoked after default animation has been stopped (e.g. hit "Stop" marker).
		/// Note: DefaultAnimation.Stopped will be set to null after invocation.
		/// </summary>
		public event Action AnimationStopped
		{
			add { DefaultAnimation.Stopped += value; }
			remove { DefaultAnimation.Stopped -= value; }
		}

#if WIN
		public Guid Guid { get; set; }
#endif

		/// <summary>
		/// Name field in HotStudio.
		/// May be non-unique.
		/// </summary>
		[ProtoMember(1)]
		public string Id { get; set; }

		/// <summary>
		/// Denotes the path to the external scene. If this path isn't null during node loading, 
		/// the node children are replaced by the external scene nodes.
		/// </summary>
		[ProtoMember(2)]
		public string ContentsPath { get; set; }

		/// <summary>
		/// May contain the marker id of the default animation.  When an animation hits a trigger keyframe, 
		/// it automatically runs the child animation from the given marker id.
		/// </summary>
		[Trigger]
		public string Trigger { get; set; }

		private Node parent;
		public Node Parent
		{
			get { return parent; }
			internal set
			{
				if (parent != value) {
					parent = value;
					PropagateDirtyFlags();
				}
			}
		}

		public Widget AsWidget { get; internal set; }

		public ModelNode AsModelNode { get; internal set; }

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		internal Node NextToRender;

		/// <summary>
		/// Shortcut to the next element of nodes of this parent.
		/// </summary>
		public Node NextSibling;

		/// <summary>
		/// TODO: Translate
		/// Позволяет вручную задать Z-order (порядок отрисовки). 0 - по умолчанию, 1 - самый нижний слой, 99 - самый верхний слой
		/// </summary>
		public int Layer { get; set; }

		/// <summary>
		/// TODO: Translate
		/// Аниматоры, изменяющие анимированные параметры объекта в время обновления
		/// </summary>
		[ProtoMember(5)]
		public AnimatorCollection Animators;

		/// <summary>
		/// Child nodes.
		/// For enumerating all descendants use Descendants.
		/// </summary>
		[ProtoMember(6)]
		public NodeList Nodes;

		/// <summary>
		/// Markers of default animation.
		/// </summary>
		public MarkerCollection Markers { get { return DefaultAnimation.Markers; } }

		// SUGGESTION: This property should have private/protected setter.
		/// <summary>
		/// Returns true if this node is running animation.
		/// </summary>
		public bool IsRunning
		{
			get { return DefaultAnimation.IsRunning; }
			set { DefaultAnimation.IsRunning = value; }
		}

		// SUGGESTION: This property should have private/protected setter.
		/// <summary>
		/// Returns true if this node isn't running animation.
		/// </summary>
		public bool IsStopped {
			get { return !IsRunning; }
			set { IsRunning = !value; }
		}

		/// <summary>
		/// Gets or sets time of current frame of default animation (in milliseconds).
		/// </summary>
		public int AnimationTime
		{
			get { return DefaultAnimation.Time; }
			set { DefaultAnimation.Time = value; }
		}

		/// <summary>
		/// Returns first animation in animation list
		/// (or creates an empty animation if list is empty).
		/// </summary>
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
		/// Custom data. Can be set via HotStudio (this way it will contain path to external scene).
		/// </summary>
		[ProtoMember(11)]
		public string Tag { get; set; }

		[ProtoMember(13)]
		public AnimationList Animations;

		// SUGGESTION: This property should have private/protected setter
		/// <summary>
		/// Name of last started marker of default animation. Is set to null by default.
		/// </summary>
		public string CurrentAnimation
		{
			get { return DefaultAnimation.RunningMarkerId; }
			set { DefaultAnimation.RunningMarkerId = value; }
		}

		/// <summary>
		/// Animation speed multiplier.
		/// </summary>
		public float AnimationSpeed { get; set; }

		/// <summary>
		/// Custom data. Can only be set programmatically.
		/// </summary>
		public object UserData { get; set; }

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		protected DirtyFlags DirtyMask;

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		protected bool IsDirty(DirtyFlags mask) { return (DirtyMask & mask) != 0; }

		public bool HitTestTarget;

		public static int CreatedCount = 0;
		public static int FinalizedCount = 0;

		public Node()
		{
			AnimationSpeed = 1;
			Animators = new AnimatorCollection(this);
			Animations = new AnimationList(this);
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

		/// <summary>
		/// Propagates dirty flags to all descendants by provided mask.
		/// </summary>
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

		/// <summary>
		/// TODO: Add summary
		/// </summary>
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

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		protected virtual void RecalcDirtyGlobalsUsingParents() { }

#if LIME_COUNT_NODES
		~Node() {
			++FinalizedCount;
		}
#endif

		// SUGGESTION: Transform to Root property?
		public Node GetRoot()
		{
			Node node = this;
			while (node.Parent != null) {
				node = node.Parent;
			}
			return node;
		}

		// SUGGESTION: rename to DescendantOf?
		/// <summary>
		/// Returns true if this node is a descendant of provided node.
		/// </summary>
		public bool ChildOf(Node node)
		{
			for (Node n = Parent; n != null; n = n.Parent) {
				if (n == node)
					return true;
			}
			return false;
		}

		/// <summary>
		/// TODO: Translate
		/// Запускает анимацию и возвращает true. Если такой анимации нет, возвращает false
		/// </summary>
		/// <param name="markerId">Название маркера анимации</param>
		public bool TryRunAnimation(string markerId, string animationId = null)
		{
			return Animations.TryRun(animationId, markerId);
		}

		/// <summary>
		/// TODO: Translate
		/// Запускает анимацию. Если такой анимации нет, генерирует исключение
		/// </summary>
		/// <param name="markerId">Название маркера анимации</param>
		/// <exception cref="Lime.Exception"/>
		public void RunAnimation(string markerId, string animationId = null)
		{
			Animations.Run(animationId, markerId);
		}

		/// <summary>
		/// TODO: Translate
		/// Текущий кадр анимации
		/// </summary>
		public int AnimationFrame {
			get { return AnimationUtils.MsecsToFrames(AnimationTime); }
			set { AnimationTime = AnimationUtils.FramesToMsecs(value); }
		}

		/// <summary>
		/// Slow, but safe way to clone node. This function is based on ProtoBuf.
		/// </summary>
		public Node DeepCloneSafe()
		{
			return Serialization.DeepClone<Node>(this);
		}

		/// <summary>
		/// Slow, but safe way to clone node. This function is based on ProtoBuf.
		/// </summary>
		public T DeepCloneSafe<T>() where T : Node
		{
			return DeepCloneSafe() as T;
		}

		/// <summary>
		/// TODO: Translate
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
			clone.Animations = Animations.Clone(clone);
			clone.Animators = AnimatorCollection.SharedClone(clone, Animators);
			clone.Nodes = Nodes.DeepCloneFast(clone);
			return clone;
		}

		/// <summary>
		/// TODO: Translate
		/// Используйте для быстрого клонирования с сохранением иерархии для неанимированных объектов.
		/// Функция основана на MemberwiseClone().
		/// Свойства Animators, Markers, SkinningWeights будут общими у клона и оригинала
		/// </summary>
		public T DeepCloneFast<T>() where T : Node
		{
			return (T)DeepCloneFast();
		}

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		public override string ToString()
		{
			return string.Format("{0}, \"{1}\", {2}", GetType().Name, Id ?? "", GetHierarchyPath());
		}

		/// <summary>
		/// TODO: Add summary
		/// </summary>
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
		/// Removes this node from parent node.
		/// </summary>
		public void Unlink()
		{
			if (Parent != null) {
				Parent.Nodes.Remove(this);
			}
		}

		public void UnlinkAndDispose()
		{
			Unlink();
			Dispose();
		}

		/// <summary>
		/// Advances animations of this node and calls Update of all its children.
		/// Usually called once a frame.
		/// </summary>
		/// <param name="delta">Time delta from last Update.</param>
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

		/// <summary>
		/// Called before updating child nodes.
		/// </summary>
		protected virtual void SelfUpdate(float delta) { }

		/// <summary>
		/// Called after updating child nodes.
		/// </summary>
		protected virtual void SelfLateUpdate(float delta) { }

		public virtual void Render() {}

		/// <summary>
		/// Adds this node and all its descendant nodes to render chain.
		/// </summary>
		public virtual void AddToRenderChain(RenderChain chain)
		{
			for (Node node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
				node.AddToRenderChain(chain);
			}
		}

		/// <summary>
		/// TODO: Add summary
		/// </summary>
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
		/// Adds provided node to the lowermost layer of this node (i.e. it will be covered
		/// by other nodes). Provided node shouldn't belong to any node (check for Parent == null
		/// and use Unlink if needed).
		/// </summary>
		public void AddNode(Node node)
		{
			Nodes.Add(node);
		}

		/// <summary>
		/// Adds this node to the lowermost layer of provided node (i.e. it will be covered
		/// by other nodes). This node shouldn't belong to any node (check for Parent == null
		/// and use Unlink if needed).
		/// </summary>
		public void AddToNode(Node node)
		{
			node.Nodes.Add(this);
		}

		/// <summary>
		/// Adds provided node to the topmost layer of this node (i.e. it will cover
		/// other nodes). Provided node shouldn't belong to any node (check for Parent == null
		/// and use Unlink if needed).
		/// </summary>
		public void PushNode(Node node)
		{
			Nodes.Push(node);
		}

		/// <summary>
		/// Adds this node to the topmost layer of provided node (i.e. it will cover
		/// other nodes). This node shouldn't belong to any node (check for Parent == null
		/// and use Unlink if needed).
		/// </summary>
		public void PushToNode(Node node)
		{
			node.Nodes.Push(this);
		}

		/// <summary>
		/// TODO: Translate
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
		/// TODO: Translate
		/// Ищет дочерний объект (рекурсивно). Если такого объекта нет, генерирует исключение
		/// </summary>
		/// <typeparam name="T">Тип искомого объекта</typeparam>
		/// <exception cref="Lime.Exception"/>
		public T Find<T>(string format, params object[] args) where T : Node
		{
			return Find<T>(string.Format(format, args));
		}

		/// <summary>
		/// TODO: Translate
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
		/// TODO: Translate
		/// Ищет дочерний объект (рекурсивно). Если такого объекта нет, возвращает null
		/// </summary>
		/// <typeparam name="T">Тип искомого объекта</typeparam>
		/// <param name="id">Id искомого объекта</param>
		public T TryFind<T>(string id) where T : Node
		{
			return TryFindNode(id) as T;
		}

		/// <summary>
		/// TODO: Translate
		/// Ищет дочерний объект (рекурсивно). Если такого объекта нет, возвращает null
		/// </summary>
		/// <typeparam name="T">Тип искомого объекта</typeparam>
		public T TryFind<T>(string format, params object[] args) where T : Node
		{
			return TryFind<T>(string.Format(format, args));
		}

		/// <summary>
		/// TODO: Translate
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
		/// TODO: Translate
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
		/// TODO: Translate
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
		/// Enumerates all nodes (except this one) of hierarchy inside this node.
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
		/// TODO: Translate
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

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		[ThreadStatic]
		private static Queue<Node> nodeSearchQueue;

		/// <summary>
		/// TODO: Add summary
		/// </summary>
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
		/// Advances animation by provided delta.
		/// </summary>
		/// <param name="delta">Time delta (in seconds).</param>
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
		/// Loads all textures, fonts and animators for this node and for all its descendants.
		/// Forces reloading of textures if they are null.
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

		/// <summary>
		/// TODO: Add summary
		/// </summary>
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

		/// <summary>
		/// TODO: Add summary
		/// </summary>
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

		/// <summary>
		/// Returns path to scene if it exists in bundle. Returns null otherwise.
		/// Throws exception if there is more than one scene file with such path.
		/// </summary>
		internal static string ResolveScenePath(string path)
		{
			var candidates = sceneExtensions.Select(ext => Path.ChangeExtension(path, ext)).Where(AssetsBundle.Instance.FileExists);
			if (candidates.Count() > 1) {
				throw new Lime.Exception("Ambiguity between: {0}", string.Join("; ", candidates.ToArray()));
			}
			return candidates.FirstOrDefault();
		}

		protected internal virtual void PerformHitTest() { }
	}
}
