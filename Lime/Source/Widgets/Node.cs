using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Yuzu;

namespace Lime
{
	public interface IAnimable
	{
		AnimatorCollection Animators { get; }
		void OnTrigger(string property, double animationTimeCorrection = 0);
	}

	public sealed class TangerineExportAttribute : Attribute
	{ }

	/// <summary>
	/// Denotes a property which can not be animated within Tangerine.
	/// </summary>
	public sealed class TangerineStaticPropertyAttribute : Attribute
	{ }

	public sealed class TangerineKeyframeColorAttribute : Attribute
	{
		public int ColorIndex;

		public TangerineKeyframeColorAttribute(int colorIndex)
		{
			ColorIndex = colorIndex;
		}
	}

	public sealed class TangerineNodeBuilderAttribute : Attribute
	{
		public string MethodName { get; private set; }

		public TangerineNodeBuilderAttribute(string methodName)
		{
			MethodName = methodName;
		}
	}

	public sealed class AllowedParentTypes : Attribute
	{
		public Type[] Types;

		public AllowedParentTypes(params Type[] types)
		{
			Types = types;
		}
	}

	public sealed class AllowedChildrenTypes : Attribute
	{
		public Type[] Types;

		public AllowedChildrenTypes(params Type[] types)
		{
			Types = types;
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class TangerineIgnoreIfAttribute : Attribute
	{
		public readonly string Method;

		private Func<object, bool> checker;

		public TangerineIgnoreIfAttribute(string method)
		{
			Method = method;
		}

		public bool Check(object obj)
		{
			if (checker == null) {
				var fn = obj.GetType().GetMethod(Method, BindingFlags.NonPublic | BindingFlags.Instance);
				if (fn == null) {
					throw new System.Exception("Couldn't find method " + Method);
				}

				var p = Expression.Parameter(typeof(object));
				var e = Expression.Call(Expression.Convert(p, obj.GetType()), fn);
				checker = Expression.Lambda<Func<object, bool>>(e, p).Compile();
			}

			return checker(obj);
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class TangerineIgnoreAttribute : Attribute
	{ }

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class TangerineInspectAttribute : Attribute
	{ }

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class TangerineGroupAttribute : Attribute
	{
		public readonly string Name;

		public TangerineGroupAttribute(string name)
		{
			Name = name ?? String.Empty;
		}
	}

	[Flags]
	public enum TangerineFlags
	{
		None = 0,
		Hidden = 1,
		Locked = 2,
		Shown = 4,
		PropertiesExpanded = 8,
		ChildrenExpanded = 16,
		HiddenOnExposition = 32,
		IgnoreMarkers = 64,
		DisplayContent = 128,
		ColorBit1 = 256,
		ColorBit2 = 512,
		ColorBit3 = 1024,
	}

	public delegate void UpdateHandler(float delta);

	/// <summary>
	/// Scene tree element.
	/// </summary>
	[DebuggerTypeProxy(typeof(NodeDebugView))]
	public abstract class Node : IDisposable, IAnimable, IFolderItem, IFolderContext
	{
		[Flags]
		protected internal enum DirtyFlags
		{
			None = 0,
			Visible = 1 << 0,
			Color = 1 << 1,
			Shader = 1 << 2,
			Blending = 1 << 3,
			Transform = 1 << 4,
			LayoutManager = 1 << 5,
			All = ~None
		}

		/// <summary>
		/// Is invoked after default animation has been stopped (e.g. hit "Stop" marker).
		/// Note: DefaultAnimation.Stopped will be set to null after invocation or after RunAnimation call.
		/// The correct usage is to call RunAnimation first and then apply any required AnimationStopped handlers.
		/// </summary>
		public event Action AnimationStopped
		{
			add { DefaultAnimation.Stopped += value; }
			remove { DefaultAnimation.Stopped -= value; }
		}

		private string id;

		/// <summary>
		/// Name field in HotStudio.
		/// May be non-unique.
		/// </summary>
		[YuzuMember]
		[TangerineStaticProperty]
		public string Id
		{
			get { return id; }
			set
			{
				if (id != value) {
					id = value;
					InvalidateNodeReferenceCache();
				}
			}
		}

		private string contentsPath;
		/// <summary>
		/// Denotes the path to the external scene. If this path isn't null during node loading,
		/// the node children are replaced by the external scene nodes.
		/// </summary>
		[YuzuMember]
		[TangerineStaticProperty]
		public string ContentsPath
		{
			get { return Serialization.ShrinkPath(contentsPath); }
			set { contentsPath = Serialization.ExpandPath(value); }
		}

		internal static long NodeReferenceCacheValidationCode = 1;

		internal static void InvalidateNodeReferenceCache()
		{
			System.Threading.Interlocked.Increment(ref NodeReferenceCacheValidationCode);
		}

		/// <summary>
		/// May contain the marker id of the default animation.  When an animation hits a trigger keyframe,
		/// it automatically runs the child animation from the given marker id.
		/// </summary>
		[Trigger]
		[TangerineKeyframeColor(1)]
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

		private TangerineFlags tangerineFlags;

		[YuzuMember]
		public TangerineFlags TangerineFlags
		{
			get { return tangerineFlags; }
			set
			{
				if (tangerineFlags != value) {
					tangerineFlags = value;
					PropagateDirtyFlags(DirtyFlags.Visible);
				}
			}
		}

		public bool GetTangerineFlag(TangerineFlags flag)
		{
			return (tangerineFlags & flag) != 0;
		}

		public void SetTangerineFlag(TangerineFlags flag, bool value)
		{
			if (value) {
				TangerineFlags |= flag;
			} else {
				TangerineFlags &= ~flag;
			}
		}

		protected virtual void OnParentChanged(Node oldParent) { }

		public Widget AsWidget { get; internal set; }

		public Node3D AsNode3D { get; internal set; }

		public IRenderChainBuilder RenderChainBuilder { get; set; }

		/// <summary>
		/// The presenter used for rendering and hit-testing the node.
		/// TODO: Add Add/RemovePresenter methods. Remove CompoundPresenter property and Presenter setter.
		/// </summary>
		public IPresenter Presenter { get; set; }

		public CompoundPresenter CompoundPresenter =>
			(Presenter as CompoundPresenter) ?? (CompoundPresenter)(Presenter = new CompoundPresenter(Presenter));

		/// <summary>
		/// The presenter used for rendering the node after rendering its children.
		/// </summary>
		public IPresenter PostPresenter { get; set; }

		public CompoundPresenter CompoundPostPresenter =>
			(PostPresenter as CompoundPresenter) ?? (CompoundPresenter)(PostPresenter = new CompoundPresenter(PostPresenter));

		/// <summary>
		/// Indicates whether the node is awake. Node gets awake on the first update.
		/// </summary>
		public bool IsAwake { get; protected set; }

		internal int RunningAnimationsCount;

		/// <summary>
		/// Gets the cached reference to the first animation in the animation collection.
		/// </summary>
		public Animation FirstAnimation { get; internal set; }

		/// <summary>
		/// Gets the cached reference to the first children node. 
		/// Use it for fast iteration through nodes collection in a performance-critical code.
		/// </summary>
		public Node FirstChild { get; internal set; }

		/// <summary>
		/// Gets the cached reference to the next node in the NodeList. 
		/// Use it for fast iteration through nodes collection in a performance-critical code.
		/// </summary>
		public Node NextSibling { get; internal set; }

		/// <summary>
		/// Animation speed multiplier.
		/// </summary>
		public float AnimationSpeed { get; set; }

		/// <summary>
		/// Gets or sets Z-order (the rendering order).
		/// 0 - inherit Z-order from the parent node. 1 - the lowest, (RenderChain.LayerCount - 1) - the topmost.
		/// </summary>
		public int Layer { get; set; }

		/// <summary>
		/// Collections of Components.
		/// </summary>
		[YuzuMember]
		public NodeComponentCollection Components { get; private set; }

		/// <summary>
		/// Collections of Animators.
		/// </summary>
		[YuzuMember]
		public AnimatorCollection Animators { get; private set; }

		/// <summary>
		/// Child nodes.
		/// For enumerating all descendants use Descendants.
		/// </summary>
		[YuzuMember]
		[YuzuSerializeIf(nameof(IsNotDecorated))]
		public NodeList Nodes { get; private set; }

		/// <summary>
		/// Markers of default animation.
		/// </summary>
		public MarkerList Markers { get { return DefaultAnimation.Markers; } }

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
		public double AnimationTime
		{
			get { return DefaultAnimation.Time; }
			set { DefaultAnimation.Time = value; }
		}

		/// <summary>
		/// Returns the first animation in the animation collection
		/// or creates an animation if the collection is empty.
		/// </summary>
		public Animation DefaultAnimation
		{
			get
			{
				if (FirstAnimation == null) {
					Animations.Add(new Animation());
				}
				return FirstAnimation;
			}
		}

		/// <summary>
		/// Custom data. Can be set via Tangerine (this way it will contain path to external scene).
		/// </summary>
		[YuzuMember]
		public string Tag
		{
			get { return tag; }
			set
			{
				if (tag == value) {
					return;
				}
				tag = value;
				OnTagChanged();
			}
		}
		private string tag;
		protected virtual void OnTagChanged() { }

		[YuzuMember]
		[YuzuSerializeIf(nameof(NeedSerializeAnimations))]
		public AnimationCollection Animations { get; private set; }

		public bool NeedSerializeAnimations() =>
			Animations.Count > 1 || (Animations.Count == 1 && (FirstAnimation.Id != null || FirstAnimation.Markers.Count > 0));

		[YuzuMember]
		public List<Folder.Descriptor> Folders { get; set; }

		/// <summary>
		/// Name of last started marker of default animation. Is set to null by default.
		/// </summary>
		public string CurrentAnimation
		{
			get { return DefaultAnimation.RunningMarkerId; }
			set { DefaultAnimation.RunningMarkerId = value; }
		}

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected bool Undirty(DirtyFlags mask)
		{
			if ((DirtyMask & mask) != 0) {
				DirtyMask &= ~mask;
				return true;
			}
			return false;
		}

		public bool HitTestTarget;

		public static int CreatedCount = 0;
		public static int FinalizedCount = 0;

		public Action<Node> Awoken;

		protected Node()
		{
			AnimationSpeed = 1;
			Components = new NodeComponentCollection(this);
			Animators = new AnimatorCollection(this);
			Animations = new AnimationCollection(this);
			Nodes = new NodeList(this);
			Presenter = DefaultPresenter.Instance;
			RenderChainBuilder = DefaultRenderChainBuilder.Instance;
			++CreatedCount;
		}

		private GestureList gestures;
		public GestureList Gestures => gestures ?? (gestures = new GestureList(this));
		public bool HasGestures() => gestures != null;

		public virtual bool IsNotDecorated() => true;

		public virtual void Dispose()
		{
			for (var n = FirstChild; n != null; n = n.NextSibling) {
				n.Dispose();
			}
			Nodes.Clear();
			Animators.Dispose();
		}

		/// <summary>
		/// Propagates dirty flags to all descendants by provided mask.
		/// </summary>
		protected internal void PropagateDirtyFlags(DirtyFlags mask = DirtyFlags.All)
		{
			if ((DirtyMask & mask) == mask)
				return;
			Window.Current?.Invalidate();
			DirtyMask |= mask;
			for (var n = FirstChild; n != null; n = n.NextSibling) {
				if ((n.DirtyMask & mask) != mask) {
					n.PropagateDirtyFlags(mask);
				}
			}
		}

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
		/// Returns true if this node is a descendant of the provided node.
		/// </summary>
		public bool DescendantOf(Node node)
		{
			for (var n = Parent; n != null; n = n.Parent) {
				if (n == node)
					return true;
			}
			return false;
		}

		public bool SameOrDescendantOf(Node node)
		{
			return node == this || DescendantOf(node);
		}

		/// <summary>
		/// Runs animation with provided Id and provided marker.
		/// Returns false if sought-for animation or marker doesn't exist.
		/// </summary>
		public bool TryRunAnimation(string markerId, string animationId = null, double animationTimeCorrection = 0)
		{
			return Animations.TryRun(animationId, markerId, animationTimeCorrection);
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
		/// Get or sets the current animation frame.
		/// </summary>
		public int AnimationFrame
		{
			get { return AnimationUtils.SecondsToFrames(AnimationTime); }
			set { AnimationTime = AnimationUtils.FramesToSeconds(value); }
		}

		/// <summary>
		/// Returns a clone of the node hierarchy.
		/// </summary>
		public virtual Node Clone()
		{
			var clone = (Node)MemberwiseClone();
			++CreatedCount;
			clone.Parent = null;
			clone.FirstAnimation = null;
			clone.FirstChild = null;
			clone.NextSibling = null;
			clone.gestures = null;
			clone.AsWidget = clone as Widget;
			clone.Animations = Animations.Clone(clone);
			clone.Animators = AnimatorCollection.SharedClone(clone, Animators);
			clone.Nodes = Nodes.Clone(clone);
			clone.Components = Components.Clone(clone);
			clone.IsAwake = false;
			if (RenderChainBuilder != null) {
				clone.RenderChainBuilder = RenderChainBuilder.Clone();
			}
			if (Presenter != null) {
				clone.Presenter = Presenter.Clone();
			}
			if (PostPresenter != null) {
				clone.PostPresenter = PostPresenter.Clone();
			}
			clone.DirtyMask = DirtyFlags.All;
			clone.UserData = null;
			return clone;
		}

		/// <summary>
		/// Returns a clone of the node hierarchy.
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
			Parent?.Nodes.Remove(this);
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
#if PROFILE
			var watch = System.Diagnostics.Stopwatch.StartNew();
#endif
			if (!IsAwake) {
				RaiseAwake();
			}
			if (delta > Application.MaxDelta) {
#if PROFILE
				watch.Stop();
				NodeProfiler.RegisterUpdate(this, watch.ElapsedTicks);
#endif
				SafeUpdate(delta);
			} else {
				AdvanceAnimation(delta);
				SelfUpdate(delta);
#if PROFILE
				watch.Stop();
#endif
				for (var node = FirstChild; node != null;) {
					var next = node.NextSibling;
					node.Update(node.AnimationSpeed * delta);
					node = next;
				}
#if PROFILE
				watch.Start();
#endif
				SelfLateUpdate(delta);

#if PROFILE
				watch.Stop();
				NodeProfiler.RegisterUpdate(this, watch.ElapsedTicks);
#endif
			}
		}

		protected void SafeUpdate(float delta)
		{
			var remainDelta = delta;
			do {
				delta = Mathf.Min(remainDelta, Application.MaxDelta);
				Update(delta);
				remainDelta -= delta;
			} while (remainDelta > 0f);
		}

		protected void RaiseAwake()
		{
			IsAwake = true;
			foreach (var c in Components) {
				c.Awake();
			}
			Awake();
			Awoken?.Invoke(this);
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
		/// Decides what descendant nodes should be added to render chain and under which conditions.
		/// This includes children Nodes as well as this Node itself. i.e. if you want Render()
		/// of this node to be called you should invoke AddSelfToRenderChain in AddToRenderChain override.
		/// </summary>
		internal protected abstract void AddToRenderChain(RenderChain chain);

		protected void AddChildrenToRenderChain(RenderChain chain)
		{
			var savedLayer = chain.CurrentLayer;
			if (Layer != 0) {
				chain.CurrentLayer = Layer;
			}
			if (PostPresenter != null) {
				chain.Add(this, PostPresenter);
			}
			for (var node = FirstChild; node != null; node = node.NextSibling) {
				node.RenderChainBuilder?.AddToRenderChain(node, chain);
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
		public virtual void OnTrigger(string property, double animationTimeCorrection = 0)
		{
			if (property != "Trigger") {
				return;
			}
			if (String.IsNullOrEmpty(Trigger)) {
				AnimationTime = animationTimeCorrection;
				IsRunning = true;
			} else {
				TriggerMultipleAnimations(animationTimeCorrection);
			}
		}

		private void TriggerMultipleAnimations(double animationTimeCorrection = 0)
		{
			if (Trigger.Contains(',')) {
				foreach (var s in Trigger.Split(',')) {
					TriggerAnimation(s.Trim(), animationTimeCorrection);
				}
			} else {
				TriggerAnimation(Trigger, animationTimeCorrection);
			}
		}

		private void TriggerAnimation(string markerWithOptionalAnimationId, double animationTimeCorrection = 0)
		{
			if (markerWithOptionalAnimationId.Contains('@')) {
				var s = markerWithOptionalAnimationId.Split('@');
				if (s.Length == 2) {
					var markerId = s[0];
					var animationId = s[1];
					TryRunAnimation(markerId, animationId, animationTimeCorrection);
				}
			} else {
				TryRunAnimation(markerWithOptionalAnimationId, null, animationTimeCorrection);
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
			if (path == null) {
				return null;
			}
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
		public IEnumerable<Node> Descendants => new DescendantsEnumerable(this);

		/// <summary>
		/// Enumerate all node's ancestors.
		/// </summary>
		public IEnumerable<Node> Ancestors
		{
			get
			{
				for (var p = Parent; p != null; p = p.Parent) {
					yield return p;
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
				var child = parent.FirstChild;
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
			if (RunningAnimationsCount > 0) {
				for (var a = FirstAnimation; a != null; a = a.Next) {
					a.Advance(delta);
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
			texture?.GetHandle();
		}

		[ThreadStatic]
		private static HashSet<string> scenesBeingLoaded;
		[ThreadStatic]
		public static Action<string> SceneLoading;

		public static Node CreateFromAssetBundle(string path, Node instance = null)
		{
			if (scenesBeingLoaded == null) {
				scenesBeingLoaded = new HashSet<string>();
			}
			var fullPath = ResolveScenePath(path);
			if (fullPath == null) {
				throw new Exception($"Scene '{path}' not found in current asset bundle");
			}
			if (scenesBeingLoaded.Contains(fullPath)) {
				throw new Exception($"Cyclic scenes dependency was detected: {fullPath}");
			}
			scenesBeingLoaded.Add(fullPath);
			try {
				using (Stream stream = AssetBundle.Current.OpenFileLocalized(fullPath)) {
					instance = Serialization.ReadObject<Node>(fullPath, stream, instance);
				}
				instance.LoadExternalScenes();
				if (!Application.IsTangerine) {
					instance.Tag = fullPath;
				}
			} finally {
				scenesBeingLoaded.Remove(fullPath);
			}
			if (instance is Model3D) {
				var attachment = new Model3DAttachmentParser().Parse(path);
				attachment?.ApplyOnInstance((Model3D)instance);
			}
			return instance;
		}

		public void LoadExternalScenes()
		{
			if (string.IsNullOrEmpty(ContentsPath)) {
				foreach (var child in Nodes) {
					child.LoadExternalScenes();
				}
			} else if (ResolveScenePath(ContentsPath) != null) {
				SceneLoading?.Invoke(ContentsPath);
				var content = CreateFromAssetBundle(ContentsPath, null);
				ReplaceContent(content);
			}
		}

		public void ReplaceContent(Node content)
		{
			if ((content is Widget) && (this is Widget)) {
				((Widget)content).Size = (this as Widget).Size;
			}
			Animations.Clear();
			var animations = content.Animations.ToList();
			content.Animations.Clear();
			Animations.AddRange(animations);
			if ((content is Viewport3D) && (this is Node3D) && (content.Nodes.Count > 0)) {
				// Handle a special case: the 3d scene is wrapped up with a Viewport3D.
				var node = content.Nodes[0];
				node.Unlink();
				Nodes.Clear();
				Nodes.Add(node);
			} else {
				var nodes = content.Nodes.ToList();
				content.Nodes.Clear();
				Nodes.Clear();
				Nodes.AddRange(nodes);
			}

			RenderChainBuilder = content.RenderChainBuilder?.Clone();
			Presenter = content.Presenter?.Clone();
			PostPresenter = content.PostPresenter?.Clone();
			Components = content.Components.Clone(this);
		}

		private static readonly string[] sceneExtensions = { ".scene", ".model", ".tan" };

		/// <summary>
		/// Returns path to scene if it exists in bundle. Returns null otherwise.
		/// Throws exception if there is more than one scene file with such path.
		/// </summary>
		public static string ResolveScenePath(string path)
		{
			var candidates = sceneExtensions.Select(ext => Path.ChangeExtension(path, ext)).Where(AssetBundle.Current.FileExists);
			if (candidates.Count() > 1) {
				throw new Exception("Ambiguity between: {0}", string.Join("; ", candidates.ToArray()));
			}
			return candidates.FirstOrDefault();
		}

		public bool IsMouseOver()
		{
			var context = WidgetContext.Current;
			if (context.NodeUnderMouse != this) {
				return false;
			}
			return context.NodeCapturedByMouse == null || context.NodeCapturedByMouse == this;
		}

		public bool IsMouseOverThisOrDescendant()
		{
			var context = WidgetContext.Current;
			var nodeUnderMouse = context.NodeUnderMouse;
			if (nodeUnderMouse == null || !nodeUnderMouse.SameOrDescendantOf(this)) {
				return false;
			}
			return context.NodeCapturedByMouse?.SameOrDescendantOf(this) ?? true;
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

				object IEnumerator.Current => Current;

				public void Dispose()
				{
					current = null;
				}

				public bool MoveNext()
				{
					if (current == null) {
						current = root;
					}
					var node = current.FirstChild;
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
