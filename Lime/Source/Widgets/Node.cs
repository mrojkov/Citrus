using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using ProtoBuf;

namespace Lime
{
	public interface IAnimable
	{
		AnimatorCollection Animators { get; }
		void OnTrigger(string property);
	}

	public class TangerineAttribute : Attribute
	{
		public int ColorIndex;

		public TangerineAttribute(int colorIndex)
		{
			ColorIndex = colorIndex;
		}
	}

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
	[ProtoInclude(110, typeof(Node3D))]
	[DebuggerTypeProxy(typeof(NodeDebugView))]
	public class Node : IDisposable, IAnimable
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
		[Tangerine(0)]
		public string Id { get; set; }

		/// <summary>
		/// Denotes the path to the external scene. If this path isn't null during node loading,
		/// the node children are replaced by the external scene nodes.
		/// </summary>
		[ProtoMember(2)]
		[Tangerine(0)]
		public string ContentsPath { get; set; }

		/// <summary>
		/// May contain the marker id of the default animation.  When an animation hits a trigger keyframe,
		/// it automatically runs the child animation from the given marker id.
		/// </summary>
		[Trigger]
		[Tangerine(1)]
		public string Trigger { get; set; }

		private Node parent;
		public Node Parent
		{
			get { return parent; }
			internal set
			{
				if (parent != value) {
					var oldParent = parent;
					parent = value;
					PropagateDirtyFlags();
					OnParentChanged(oldParent);
				}
			}
		}

		public BitSet32 TangerineFlags;

		protected virtual void OnParentChanged(Node oldParent) { }

		public Widget AsWidget { get; internal set; }

		public Node3D AsNode3D { get; internal set; }

		/// <summary>
		/// The presenter used for rendering and hit-testing the node.
		/// </summary>
		public IPresenter Presenter { get; set; }

		public CompoundPresenter CompoundPresenter
		{
			get { return (Presenter as CompoundPresenter) ?? (CompoundPresenter)(Presenter = new CompoundPresenter(Presenter)); }
		}

		/// <summary>
		/// The presenter used for rendering the node after rendering its children.
		/// </summary>
		public IPresenter PostPresenter { get; set; }

		public CompoundPresenter CompoundPostPresenter
		{
			get { return (PostPresenter as CompoundPresenter) ?? (CompoundPresenter)(PostPresenter = new CompoundPresenter(PostPresenter)); }
		}

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
		public AnimatorCollection Animators { get; private set; }

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

		/// <summary>
		/// Returns true if this node is running animation.
		/// </summary>
		public bool IsRunning
		{
			get { return DefaultAnimation.IsRunning; }
			set { DefaultAnimation.IsRunning = value; }
		}

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
		protected DirtyFlags DirtyMask = DirtyFlags.All;

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		protected bool IsDirty(DirtyFlags mask) { return (DirtyMask & mask) != 0; }

		public bool HitTestTarget;

		public static int CreatedCount = 0;
		public static int FinalizedCount = 0;

		protected bool Awoken;

		public Node()
		{
			AnimationSpeed = 1;
			Animators = new AnimatorCollection(this);
			Animations = new AnimationList(this);
			Nodes = new NodeList(this);
			Presenter = DefaultPresenter.Instance;
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
			if (Window.Current != null) {
				Window.Current.Invalidate();
			}
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

		public Node GetRoot()
		{
			Node node = this;
			while (node.Parent != null) {
				node = node.Parent;
			}
			return node;
		}

		/// <summary>
		/// Returns true if this node is a descendant of provided node.
		/// </summary>
		public bool DescendantOf(Node node)
		{
			for (Node n = Parent; n != null; n = n.Parent) {
				if (n == node)
					return true;
			}
			return false;
		}

		public bool DescendantOrThis(Node node)
		{
			return node == this || DescendantOf(node);
		}

		/// <summary>
		/// Runs animation with provided Id and provided marker.
		/// Returns false if sought-for animation or marker doesn't exist.
		/// </summary>
		public bool TryRunAnimation(string markerId, string animationId = null)
		{
			return Animations.TryRun(animationId, markerId);
		}

		/// <summary>
		/// Runs animation with provided Id and provided marker.
		/// Throws an exception if sought-for animation or marker doesn't exist.
		/// </summary>
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
		/// TODO: Translate
		/// Используйте для быстрого клонирования с сохранением иерархии для неанимированных объектов.
		/// Функция основана на MemberwiseClone().
		/// Свойства Animators, Markers, SkinningWeights будут общими у клона и оригинала
		/// </summary>
		public virtual Node Clone()
		{
			var clone = (Node)MemberwiseClone();
			++CreatedCount;
			clone.Parent = null;
			clone.NextSibling = null;
			clone.AsWidget = clone as Widget;
			clone.Animations = Animations.Clone(clone);
			clone.Animators = AnimatorCollection.SharedClone(clone, Animators);
			clone.Nodes = Nodes.Clone(clone);
			clone.Awoken = false;
			if (Presenter != null) {
				clone.Presenter = Presenter.Clone();
			}
			if (PostPresenter != null) {
				clone.PostPresenter = PostPresenter.Clone();
			}
			clone.DirtyMask = DirtyFlags.All;
			return clone;
		}

		/// <summary>
		/// TODO: Translate
		/// Используйте для быстрого клонирования с сохранением иерархии для неанимированных объектов.
		/// Функция основана на MemberwiseClone().
		/// Свойства Animators, Markers, SkinningWeights будут общими у клона и оригинала
		/// </summary>
		public T Clone<T>() where T : Node
		{
			return (T)Clone();
		}

		/// <summary>
		/// Returns the <see cref="string"/> representation of this <see cref="Node"/>.
		/// </summary>
		public override string ToString()
		{
			string r = "";
			for (var p = this; p != null; p = p.Parent) {
				if (p != this) {
					r += " in ";
				}
				r += string.IsNullOrEmpty(p.Id) ? p.GetType().Name : string.Format("'{0}'", p.Id);
				if (!string.IsNullOrEmpty(p.Tag)) {
					r += string.Format(" ({0})", p.Tag);
				}
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
		/// <param name="delta">Time delta since last Update.</param>
		public virtual void Update(float delta)
		{
			delta *= AnimationSpeed;
			if (!Awoken) {
				Awake();
				Awoken = true;
			}
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
		/// Awake is called on the first node update, before changing node state.
		/// </summary>
		protected virtual void Awake() { }

		/// <summary>
		/// Called before updating child nodes.
		/// </summary>
		protected virtual void SelfUpdate(float delta) { }

		/// <summary>
		/// Called after updating child nodes.
		/// </summary>
		protected virtual void SelfLateUpdate(float delta) { }

		public virtual void Render() { }

		/// <summary>
		/// Adds this node and all its descendant nodes to render chain.
		/// </summary>
		public virtual void AddToRenderChain(RenderChain chain)
		{
			AddContentsToRenderChain(chain);
		}

		protected void AddContentsToRenderChain(RenderChain chain)
		{
			var savedLayer = chain.CurrentLayer;
			if (Layer != 0) {
				chain.CurrentLayer = Layer;
			}
			if (PostPresenter != null) {
				chain.Add(this, PostPresenter);
			}
			for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
				node.AddToRenderChain(chain);
			}
			if (Presenter != null) {
				chain.Add(this, Presenter);
			}
			chain.CurrentLayer = savedLayer;
		}

		protected void AddSelfToRenderChain(RenderChain chain)
		{
			var savedLayer = chain.CurrentLayer;
			if (Layer != 0) {
				chain.CurrentLayer = Layer;
			}
			if (PostPresenter != null) {
				chain.Add(this, PostPresenter);
			}
			if (Presenter != null) {
				chain.Add(this, Presenter);
			}
			chain.CurrentLayer = savedLayer;
		}

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		public virtual void OnTrigger(string property)
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
		/// Searches for node with provided path or id in this node's descendants.
		/// Throws an exception if sought-for node doesn't exist.
		/// <para>This function is thread safe.</para>
		/// </summary>
		/// <typeparam name="T">Type of sought-for node.</typeparam>
		/// <param name="path">Id or path of Node. Path can be incomplete
		/// (i.e. for path Root/Human/Head/Eye Human or Head can be ommited).</param>
		public T Find<T>(string path) where T : Node
		{
			T result = TryFindNode(path) as T;
			if (result == null) {
				throw new Lime.Exception("'{0}' of {1} not found for '{2}'", path, typeof(T).Name, ToString());
			}
			return result;
		}

		/// <summary>
		/// Searches for node with provided path or id in this node's descendants.
		/// Returns null if sought-for node doesn't exist.
		/// <para>This function is thread safe.</para>
		/// </summary>
		/// <typeparam name="T">Type of sought-for node.</typeparam>
		/// <param name="format">Id or path of Node. Path can be incomplete
		/// (i.e. for path Root/Human/Head/Eye Human or Head can be ommited).</param>
		public T Find<T>(string format, params object[] args) where T : Node
		{
			return Find<T>(string.Format(format, args));
		}

		/// <summary>
		/// Searches for node with provided path or id in this node's descendants.
		/// Returns null if sought-for node doesn't exist.
		/// <para>This function is thread safe.</para>
		/// </summary>
		/// <typeparam name="T">Type of sought-for node.</typeparam>
		/// <param name="path">Id or path of Node. Path can be incomplete
		/// (i.e. for path Root/Human/Head/Eye Human or Head can be ommited).</param>
		public bool TryFind<T>(string path, out T node) where T : Node
		{
			node = TryFindNode(path) as T;
			return node != null;
		}

		/// <summary>
		/// Searches for node with provided path or id in this node's descendants.
		/// Returns null if sought-for node doesn't exist.
		/// <para>This function is thread safe.</para>
		/// </summary>
		/// <typeparam name="T">Type of sought-for node.</typeparam>
		/// <param name="path">Id or path of Node. Path can be incomplete
		/// (i.e. for path Root/Human/Head/Eye Human or Head can be ommited).</param>
		public T TryFind<T>(string path) where T : Node
		{
			return TryFindNode(path) as T;
		}

		/// <summary>
		/// Searches for node with provided path or id in this node's descendants.
		/// Returns null if sought-for node doesn't exist.
		/// <para>This function is thread safe.</para>
		/// </summary>
		/// <typeparam name="T">Type of sought-for node.</typeparam>
		/// <param name="format">Id or path of Node. Path can be incomplete
		/// (i.e. for path Root/Human/Head/Eye Human or Head can be ommited).</param>
		public T TryFind<T>(string format, params object[] args) where T : Node
		{
			return TryFind<T>(string.Format(format, args));
		}

		/// <summary>
		/// Searches for node with provided path or id in this node's descendants.
		/// Throws an exception if sought-for node doesn't exist.
		/// <para>This function is thread safe.</para>
		/// </summary>
		/// <param name="path">Id or path of Node. Path can be incomplete
		/// (i.e. for path Root/Human/Head/Eye Human or Head can be ommited).</param>
		public Node FindNode(string path)
		{
			var node = TryFindNode(path);
			if (node == null) {
				throw new Lime.Exception("'{0}' not found for '{1}'", path, ToString());
			}
			return node;
		}

		/// <summary>
		/// Searches for node with provided path or id in this node's descendants.
		/// Returns null if sought-for node doesn't exist.
		/// <para>This function is thread safe.</para>
		/// </summary>
		/// <param name="path">Id or path of Node. Path can be incomplete
		/// (i.e. for path Root/Human/Head/Eye Human or Head can be ommited).</param>
		public Node TryFindNode(string path)
		{
			if (path.IndexOf('/') >= 0) {
				return TryFindNodeByPath(path);
			} else {
				return TryFindNodeById(path);
			}
		}

		/// <summary>
		/// Searches for node with provided path in this node's descendants.
		/// Returns null if sought-for node doesn't exist.
		/// <para>This function is thread safe.</para>
		/// </summary>
		/// <param name="path">Path of node. Can be incomplete
		/// (i.e. for path Root/Human/Head/Eye Human or Head can be ommited).</param>
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
		/// Gets a DFS-enumeration of all descendant nodes.
		/// </summary>
		public IEnumerable<Node> Descendants
		{
			get { return new DescendantsEnumerable(this); }
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

		[ThreadStatic]
		private static Queue<Node> nodeSearchQueue;

		/// <summary>
		/// Searches for node with provided id in this node's descendants.
		/// Returns null if sought-for node doesn't exist.
		/// <para>This function is thread safe.</para>
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
		/// Advances all running animations by provided delta.
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
			if (string.IsNullOrEmpty(ContentsPath)) {
				for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
					node.LoadContent();
				}
				return;
			}
			Nodes.Clear();
			Markers.Clear();
			var contentsPath = ResolveScenePath(ContentsPath);
			if (contentsPath == null) {
				return;
			}
			var content = new Frame(ContentsPath);
			if (content.AsWidget != null && AsWidget != null) {
				content.AsWidget.Size = AsWidget.Size;
			}
			Markers.AddRange(content.Markers);
			var nodes = content.Nodes.ToList();
			content.Nodes.Clear();
			Nodes.AddRange(nodes);
		}

		private static readonly string[] sceneExtensions = new[] { ".scene", ".daeModel", ".fbxModel" };

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

		internal protected virtual bool PartialHitTest(ref HitTestArgs args)
		{
			return false;
		}

		private class DescendantsEnumerable : IEnumerable<Node>
		{
			private readonly Node root;

			public DescendantsEnumerable(Node root)
			{
				this.root = root;
			}

			public IEnumerator<Node> GetEnumerator()
			{
				return new Enumerator(root);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			private class Enumerator : IEnumerator<Node>
			{
				private readonly Node root;
				private Node current;

				public Enumerator(Node root)
				{
					this.root = root;
				}

				public Node Current
				{
					get
					{
						if (current == null) {
							throw new InvalidOperationException();
						}
						return current;
					}
				}

				object IEnumerator.Current
				{
					get { return Current; }
				}

				public void Dispose()
				{
					current = null;
				}

				public bool MoveNext()
				{
					if (current == null) {
						current = root;
					}
					var node = current.Nodes.FirstOrNull();
					if (node != null) {
						current = node;
						return true;
					}
					if (current == root) {
						return false;
					}
					while (current.NextSibling == null) {
						current = current.Parent;
						if (current == root) {
							return false;
						}
					}
					current = current.NextSibling;
					return true;
				}

				public void Reset()
				{
					current = null;
				}
			}
		}
	}
}
