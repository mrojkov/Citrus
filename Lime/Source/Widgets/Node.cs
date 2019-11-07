using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Yuzu;

namespace Lime
{
	public class CyclicDependencyException : Exception
	{
		public CyclicDependencyException(string message = null) : base(message) { }
		public CyclicDependencyException(string format, params object[] args) : base(string.Format(format, args)) { }
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
	public sealed class YuzuSpecializeWithAttribute : Attribute
	{
		public Type Type;
		public YuzuSpecializeWithAttribute(Type type)
		{
			Type = type;
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
	public sealed class YuzuDontGenerateDeserializerAttribute : Attribute
	{ }

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
		SceneNode = 2048,
		SerializableMask = Hidden | Locked | Shown | ColorBit1 | ColorBit2 | ColorBit3,
	}

	public delegate void UpdateHandler(float delta);

	/// <summary>
	/// Scene tree element.
	/// </summary>
	[YuzuDontGenerateDeserializer]
	[DebuggerTypeProxy(typeof(NodeDebugView))]
	public abstract class Node : IDisposable, IAnimationHost, IFolderItem, IFolderContext, IRenderChainBuilder, IAnimable, ICloneable
	{
		[Flags]
		protected internal enum DirtyFlags
		{
			None = 0,
			Visible = 1 << 0,
			Color = 1 << 1,
			Shader = 1 << 2,
			Blending = 1 << 3,
			LayoutManager = 1 << 4,
			LocalTransform = 1 << 5,
			GlobalTransform = 1 << 6,
			GlobalTransformInverse = 1 << 7,
			ParentBoundingRect = 1 << 8,
			Enabled = 1 << 9,
			EffectiveAnimationSpeed = 1 << 10,
			Frozen = 1 << 11,
			All = ~None
		}

		/// <summary>
		/// Gets a manager of this node.
		/// </summary>
		public NodeManager Manager { get; internal set; }

		/// <summary>
		/// Is invoked after default animation has been stopped (e.g. hit "Stop" marker).
		/// Note: DefaultAnimation.Stopped will be set to null after invocation or after RunAnimation call.
		/// The correct usage is to call RunAnimation first and then apply any required AnimationStopped handlers.
		/// </summary>
		public event Action AnimationStopped
		{
			add => DefaultAnimation.Stopped += value;
			remove => DefaultAnimation.Stopped -= value;
		}

		private string id;

		/// <summary>
		/// A node identifier. May be non-unique.
		/// </summary>
		[YuzuMember]
		[TangerineDefaultCharset]
		[TangerineStaticProperty]
		public string Id
		{
			get => id;
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
		[TangerineDefaultCharset]
		[TangerineStaticProperty]
#if TANGERINE
		[TangerineIgnoreIf(nameof(ShouldInspectContentsPath))]
		[TangerineOnPropertySet(nameof(OnContentsPathChange))]
#endif // TANGERINE
		public string ContentsPath
		{
			get => InternalPersistence.Current?.ShrinkPath(contentsPath) ?? contentsPath;
			set => contentsPath = InternalPersistence.Current?.ExpandPath(value) ?? value;
		}
#if TANGERINE

		/// <summary>
		/// Occurs when ContentsPath property has been changed in Tangerine inspector.
		/// </summary>
		protected void OnContentsPathChange()
		{
			if (string.IsNullOrEmpty(ContentsPath)) {
				Nodes.Clear();
				Components.Clear();
			}
			LoadExternalScenes();
		}
#endif


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

		IAnimable IAnimable.Owner { get => null; set => throw new NotSupportedException(); }

		private Node parent;

		/// <summary>
		/// Gets the parent node.
		/// </summary>
		public Node Parent
		{
			get => parent;
			internal set
			{
				if (parent != value) {
					for (var n = Parent; n != null; n = n.Parent) {
						n.DescendantAnimatorsVersion++;
					}
					var oldParent = parent;
					parent = value;
					PropagateDirtyFlags();
					for (var n = Parent; n != null; n = n.Parent) {
						n.DescendantAnimatorsVersion++;
					}
					OnParentChanged(oldParent);
				}
			}
		}

		private TangerineFlags tangerineFlags;

		/// <summary>
		/// Flags that are used by Tangerine.
		/// </summary>
		[TangerineIgnore]
		[YuzuMember]
		public TangerineFlags TangerineFlags
		{
			get => tangerineFlags;
			set
			{
				if (tangerineFlags != value) {
					tangerineFlags = value;
					PropagateDirtyFlags(DirtyFlags.Visible | DirtyFlags.Frozen);
					Manager?.FilterNode(this);
				}
			}
		}

		/// <summary>
		/// Retrieves the value of the specified Tangerine flag.
		/// </summary>
		/// <param name="flag"></param>
		/// <returns></returns>
		public bool GetTangerineFlag(TangerineFlags flag)
		{
			return (tangerineFlags & flag) != 0;
		}

		/// <summary>
		/// Sets the value for the specified Tangerine flag.
		/// </summary>
		/// <param name="flag">A Tangerine flag</param>
		/// <param name="value">A value to set.</param>
		public void SetTangerineFlag(TangerineFlags flag, bool value)
		{
			if (value) {
				TangerineFlags |= flag;
			} else {
				TangerineFlags &= ~flag;
			}
		}

		/// <summary>
		/// Occurs when Parent property changes.
		/// </summary>
		/// <param name="oldParent"></param>
		protected virtual void OnParentChanged(Node oldParent) { }

		/// <summary>
		/// Cached result of cast to widget.
		/// </summary>
		public Widget AsWidget { get; internal set; }

		/// <summary>
		/// Cached result of cast to Node3D.
		/// </summary>
		public Node3D AsNode3D { get; internal set; }

		/// <summary>
		/// The presenter used for rendering and hit-testing the node.
		/// TODO: Add Add/RemovePresenter methods. Remove CompoundPresenter property and Presenter setter.
		/// </summary>
		public IPresenter Presenter { get; set; }

		/// <summary>
		/// Makes the presenter compound and returns it.
		/// </summary>
		public CompoundPresenter CompoundPresenter =>
			(Presenter as CompoundPresenter) ?? (CompoundPresenter)(Presenter = new CompoundPresenter(Presenter));

		/// <summary>
		/// The presenter used for rendering the node after rendering its children.
		/// </summary>
		public IPresenter PostPresenter { get; set; }

		/// <summary>
		/// Makes the post presenter compound and returns it.
		/// </summary>
		public CompoundPresenter CompoundPostPresenter =>
			(PostPresenter as CompoundPresenter) ?? (CompoundPresenter)(PostPresenter = new CompoundPresenter(PostPresenter));

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
		/// Occurs at node first update.
		/// </summary>
		public event Action<Node> Awoke
		{
			add => Components.GetOrAdd<AwakeBehavior>().Action += value;
			remove => Components.GetOrAdd<AwakeBehavior>().Action -= value;
		}

		/// <summary>
		/// Called before Update.
		/// </summary>
		public UpdateHandler Updating
		{
			get => Components.GetOrAdd<UpdateBehavior>().Updating;
			set => Components.GetOrAdd<UpdateBehavior>().Updating = value;
		}

		/// <summary>
		/// Called after Update.
		/// </summary>
		public UpdateHandler Updated
		{
			get => Components.GetOrAdd<UpdatedBehavior>().Updated;
			set => Components.GetOrAdd<UpdatedBehavior>().Updated = value;
		}

		/// <summary>
		/// Tasks that are called before Update.
		/// </summary>
		public TaskList Tasks => Components.GetOrAdd<TasksBehavior>().Tasks;

		/// <summary>
		/// Tasks that are called after Update.
		/// </summary>
		public TaskList LateTasks => Components.GetOrAdd<LateTasksBehavior>().Tasks;

		private float animationSpeed;

		/// <summary>
		/// Animation speed multiplier.
		/// </summary>
		public float AnimationSpeed
		{
			get => animationSpeed;
			set
			{
				if (animationSpeed != value) {
					animationSpeed = value;
					PropagateDirtyFlags(DirtyFlags.EffectiveAnimationSpeed);
				}
			}
		}

		private float effectiveAnimationSpeed;

		/// <summary>
		/// Gets absolute animation speed that's product of self animation speed multiplied by parent absolute animation speed.
		/// EffectiveAnimationSpeed = AnimationSpeed * Parent.EffectiveAnimationSpeed.
		/// </summary>
		public float EffectiveAnimationSpeed
		{
			get {
				if (CleanDirtyFlags(DirtyFlags.EffectiveAnimationSpeed)) {
					RecalcEffectiveAnimationSpeed();
				}
				return effectiveAnimationSpeed;
			}
		}

		private void RecalcEffectiveAnimationSpeed()
		{
			effectiveAnimationSpeed = AnimationSpeed;
			if (Parent != null) {
				effectiveAnimationSpeed *= Parent.EffectiveAnimationSpeed;
			}
		}

		/// <summary>
		/// Gets the render chain builder.
		/// </summary>
		public IRenderChainBuilder RenderChainBuilder { get; set; }

		/// <summary>
		/// Gets or sets Z-order (the rendering order).
		/// 0 - inherit Z-order from the parent node. 1 - the lowest, (RenderChain.LayerCount - 1) - the topmost.
		/// </summary>
		public int Layer { get; set; }

		/// <summary>
		/// Collections of Components.
		/// If node component class don't need to be serialized mark it with <see cref="NodeComponentDontSerializeAttribute">.
		/// </summary>
		[YuzuMember]
		public NodeComponentCollection Components { get; private set; }

		Component IAnimationHost.GetComponent(Type type) => Components.Get(type);

		/// <summary>
		/// Collections of Animators.
		/// </summary>
		[YuzuMember]
		public AnimatorCollection Animators { get; private set; }

		/// <summary>
		/// Child nodes.
		/// For enumerating all descendants use Descendants.
		/// </summary>
		[TangerineIgnore]
		[YuzuMember]
		[YuzuSerializeIf(nameof(ShouldSerializeNodes))]
		public NodeList Nodes { get; private set; }

#if !TANGERINE
		/// <summary>
		/// Markers of default animation.
		/// </summary>
		[Obsolete("Use DefaultAnimation.Markers")]
		public MarkerList Markers => DefaultAnimation.Markers;

		/// <summary>
		/// Returns true if this node is running animation.
		/// </summary>
		[Obsolete("Use DefaultAnimation.IsRunning")]
		public bool IsRunning
		{
			get => DefaultAnimation.IsRunning;
			set => DefaultAnimation.IsRunning = value;
		}

		/// <summary>
		/// Returns true if this node isn't running animation.
		/// </summary>
		[Obsolete("Use !DefaultAnimation.IsRunning")]
		public bool IsStopped {
			get => !IsRunning;
			set => IsRunning = !value;
		}

		/// <summary>
		/// Gets or sets time of current frame of default animation (in milliseconds).
		/// </summary>
		[Obsolete("Use DefaultAnimation.Time")]
		public double AnimationTime
		{
			get => DefaultAnimation.Time;
			set => DefaultAnimation.Time = value;
		}

		/// <summary>
		/// Get or sets the current animation frame.
		/// </summary>
		[Obsolete("Use AnimationUtils.SecondsToFrames(DefaultAnimation.Time)")]
		public int AnimationFrame
		{
			get => AnimationUtils.SecondsToFrames(DefaultAnimation.Time);
			set => DefaultAnimation.Time = AnimationUtils.FramesToSeconds(value);
		}
#endif

		/// <summary>
		/// Returns the first animation in the animation collection
		/// or creates an animation if the collection is empty.
		/// </summary>
		public Animation DefaultAnimation => Components.GetOrAdd<AnimationComponent>().DefaultAnimation;

		/// <summary>
		/// Custom data. Can be set via Tangerine (this way it will contain path to external scene).
		/// Can't use obsolete attribute because of Yuzu Generated Binary Deserializers. But its obsolete, dont use it.
		/// Use Node Components instead.
		/// </summary>
		[TangerineIgnore]
		[YuzuMember]
		public string Tag
		{
			get => tag;
			set
			{
				if (tag == value) {
					return;
				}
				tag = value;
			}
		}
		private string tag;

		/// <summary>
		/// Gets the animations.
		/// </summary>
		[YuzuMember]
		[YuzuSerializeIf(nameof(NeedSerializeAnimations))]
		[TangerineIgnore]
		public AnimationCollection Animations => Components.GetOrAdd<AnimationComponent>().Animations;

		internal int DescendantAnimatorsVersion { get; private set; }

		void IAnimationHost.OnAnimatorCollectionChanged()
		{
			for (var n = Parent; n != null; n = n.Parent) {
				n.DescendantAnimatorsVersion++;
			}
		}

#if TANGERINE
		protected bool ShouldInspectContentsPath() => !Parent?.GetTangerineFlag(TangerineFlags.SceneNode) ?? true;

		protected bool ShouldInspectPosition() => !Parent?.GetTangerineFlag(TangerineFlags.SceneNode) ?? true;
#endif // TANGERINE

		public bool NeedSerializeAnimations()
		{
			var c = Components.Get<AnimationComponent>();
			return c != null && (c.Animations.Count > 1 || (c.Animations.Count == 1 && (!c.Animations[0].IsLegacy || c.Animations[0].Markers.Count > 0)));
		}

		/// <summary>
		/// Gets the folder descriptors.
		/// </summary>
		[TangerineIgnore]
		[YuzuMember]
		public List<Folder.Descriptor> Folders { get; set; }

		/// <summary>
		/// Name of last started marker of default animation. Is set to null by default.
		/// </summary>
		public string CurrentAnimation
		{
			get => DefaultAnimation.RunningMarkerId;
			set => DefaultAnimation.RunningMarkerId = value;
		}

		/// <summary>
		/// Custom data. Can only be set programmatically.
		/// </summary>
		public object UserData { get; set; }

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		internal protected DirtyFlags DirtyMask = DirtyFlags.All;

		/// <summary>
		/// TODO: Add summary
		/// </summary>
		protected bool IsDirty(DirtyFlags mask) { return (DirtyMask & mask) != 0; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected bool CleanDirtyFlags(DirtyFlags mask)
		{
			if ((DirtyMask & mask) != 0) {
				DirtyMask &= ~mask;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets or sets the value that's indicating if the node responds the mouse/touch events.
		/// </summary>
		public bool HitTestTarget;

#if TANGERINE
		public static bool TangerineFastForwardInProgress = false;
#endif

		public static int CreatedCount = 0;
		public static int FinalizedCount = 0;

		/// <summary>
		/// Initializes a new instance of node.
		/// </summary>
		protected Node()
		{
			AnimationSpeed = 1;
			Components = new NodeComponentCollection(this);
			Animators = new AnimatorCollection(this);
			Nodes = new NodeList(this);
			Presenter = DefaultPresenter.Instance;
			RenderChainBuilder = this;
			++CreatedCount;
		}

		private GestureList gestures;

		/// <summary>
		/// Gets the gesture recognizers.
		/// </summary>
		public GestureList Gestures => gestures ?? (gestures = new GestureList(this));

		/// <summary>
		/// Checks if the node has at least one gesture recognizer.
		/// </summary>
		/// <returns>True is the node has at least one gesture recognizer, otherwise false.</returns>
		public bool HasGestures() => gestures != null && gestures.Count > 0;

		public virtual bool IsNotDecorated() => true;

		public bool ShouldSerializeNodes()
		{
			return IsNotDecorated() && Nodes.Count > 0;
		}

		/// <summary>
		/// Disposes the node and all of it components, animations, tasks, etc...
		/// </summary>
		public virtual void Dispose()
		{
			foreach (var component in Components) {
				component.Dispose();
			}
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
			PropagateDirtyFlagsHelper(mask);
		}

		private void PropagateDirtyFlagsHelper(DirtyFlags mask)
		{
			DirtyMask |= mask;
			for (var n = FirstChild; n != null; n = n.NextSibling) {
				if ((n.DirtyMask & mask) != mask) {
					n.PropagateDirtyFlagsHelper(mask);
				}
			}
		}

#if LIME_COUNT_NODES
		~Node() {
			++FinalizedCount;
		}
#endif

		/// <summary>
		/// Gets the root node.
		/// </summary>
		/// <returns>The root node.</returns>
		public Node GetRoot()
		{
			Node node = this;
			while (node.Parent != null) {
				node = node.Parent;
			}
			return node;
		}

		/// <summary>
		/// Checks if this node is a descendant of the specified node.
		/// </summary>
		/// <param name="node">A parent node.</param>
		/// <returns>True if this node is a descendant of the specified node, otherwise false.</returns>
		public bool DescendantOf(Node node)
		{
			for (var n = Parent; n != null; n = n.Parent) {
				if (n == node)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Checks if this node is the specified node or a descendant of.
		/// </summary>
		/// <param name="node">A parent node.</param>
		/// <returns>if this node is the specified node or a descendant of, otherwise false.</returns>
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
		/// Returns this (to use with yield return)
		/// </summary>
		public Node RunAnimation(string markerId, string animationId = null)
		{
			Animations.Run(animationId, markerId);
			return this;
		}

		[YuzuBeforeSerialization]
		public void OnBeforeSerialization()
		{
			foreach (var c in Components) {
				c.OnBeforeNodeSerialization();
			}
		}

		[YuzuAfterSerialization]
		public void OnAfterSerialization()
		{
			foreach (var c in Components) {
				c.OnAfterNodeSerialization();
			}
		}

		[YuzuAfterDeserialization]
		public virtual void OnAfterDeserialization() { }

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
				r += string.IsNullOrEmpty(p.Id) ? p.GetType().Name : $"'{p.Id}'";
				if (!string.IsNullOrEmpty(p.Tag)) {
					r += $" ({p.Tag})";
				}
				var bundlePath = Components.Get<Node.AssetBundlePathComponent>();
				if (bundlePath != null) {
					r += $" ({bundlePath})";
				}
			}
			return r;
		}

		/// <summary>
		/// Removes this node from children of the parent node.
		/// </summary>
		public void Unlink()
		{
			Parent?.Nodes.Remove(this);
		}

		/// <summary>
		/// Removes this node from children of the parent node and disposes it.
		/// </summary>
		public void UnlinkAndDispose()
		{
			Unlink();
			Dispose();
		}

		/// <summary>
		/// TODO: Add summary.
		/// </summary>
		/// <returns></returns>
		protected internal virtual RenderObject GetRenderObject() => null;

		/// <summary>
		/// Decides what descendant nodes should be added to render chain and under which conditions.
		/// This includes children Nodes as well as this Node itself. i.e. if you want Render()
		/// of this node to be called you should invoke AddSelfToRenderChain in AddToRenderChain override.
		/// </summary>
		public abstract void AddToRenderChain(RenderChain chain);

		/// <summary>
		/// Add this node and it children to the render chain.
		/// </summary>
		/// <param name="chain">A render chain into which should be added.</param>
		/// <param name="layer">A node layer.</param>
		public void AddSelfAndChildrenToRenderChain(RenderChain chain, int layer)
		{
			if (layer == 0) {
				if (PostPresenter != null) {
					chain.Add(this, PostPresenter);
				}
				for (var node = FirstChild; node != null; node = node.NextSibling) {
					node.RenderChainBuilder?.AddToRenderChain(chain);
				}
				if (Presenter != null) {
					chain.Add(this, Presenter);
				}
			} else {
				var savedLayer = chain.CurrentLayer;
				chain.CurrentLayer = layer;
				AddSelfAndChildrenToRenderChain(chain, 0);
				chain.CurrentLayer = savedLayer;
			}
		}

		/// <summary>
		/// Add this node to the render chain.
		/// </summary>
		/// <param name="chain">A render chain into which should be added.</param>
		/// <param name="layer">A node layer.</param>
		protected void AddSelfToRenderChain(RenderChain chain, int layer)
		{
			if (layer == 0) {
				if (PostPresenter != null) {
					chain.Add(this, PostPresenter);
				}
				if (Presenter != null) {
					chain.Add(this, Presenter);
				}
			} else {
				var savedLayer = chain.CurrentLayer;
				chain.CurrentLayer = layer;
				AddSelfToRenderChain(chain, 0);
				chain.CurrentLayer = savedLayer;
			}
		}

		/// <summary>
		/// Invokes when the trigger property has been changed by animation.
		/// </summary>
		/// <param name="property">A property name.</param>
		/// <param name="value">A new property value.</param>
		/// <param name="animationTimeCorrection">An animation time correction.</param>
		public virtual void OnTrigger(string property, object value, double animationTimeCorrection = 0)
		{
			foreach (var c in Components) {
				if (c is NodeBehavior b) {
					b.OnTrigger(property, value, animationTimeCorrection);
				}
			}
			if (property != "Trigger") {
				return;
			}
			if (string.IsNullOrEmpty(value as string)) {
				DefaultAnimation.Time = animationTimeCorrection;
				DefaultAnimation.IsRunning = true;
			} else {
				TriggerMultipleAnimations((string)value, animationTimeCorrection);
			}
		}

		/// <summary>
		/// TODO: Add summary.
		/// </summary>
		/// <param name="trigger"></param>
		/// <param name="animationTimeCorrection"></param>
		protected void TriggerMultipleAnimations(string trigger, double animationTimeCorrection = 0)
		{
			if (trigger.IndexOf(',') >= 0) {
				foreach (var s in trigger.Split(',')) {
					TriggerAnimation(s.Trim(), animationTimeCorrection);
				}
			} else {
				TriggerAnimation(trigger, animationTimeCorrection);
			}
		}

		private void TriggerAnimation(string markerWithOptionalAnimationId, double animationTimeCorrection = 0)
		{
			if (markerWithOptionalAnimationId.IndexOf('@') >= 0) {
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
		/// <param name="node">A child node.</param>
		public void AddNode(Node node)
		{
			Nodes.Add(node);
		}

		/// <summary>
		/// Adds this node to the lowermost layer of provided node (i.e. it will be covered
		/// by other nodes). This node shouldn't belong to any node (check for Parent == null
		/// and use Unlink if needed).
		/// </summary>
		/// <param name="node">A parent node.</param>
		public void AddToNode(Node node)
		{
			node.Nodes.Add(this);
		}

		/// <summary>
		/// Adds provided node to the topmost layer of this node (i.e. it will cover
		/// other nodes). Provided node shouldn't belong to any node (check for Parent == null
		/// and use Unlink if needed).
		/// </summary>
		/// <param name="node">A child node.</param>
		public void PushNode(Node node)
		{
			Nodes.Push(node);
		}

		/// <summary>
		/// Adds this node to the topmost layer of provided node (i.e. it will cover
		/// other nodes). This node shouldn't belong to any node (check for Parent == null
		/// and use Unlink if needed).
		/// </summary>
		/// <param name="node">A parent node.</param>
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
			if (!(TryFindNode(path) is T result)) {
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
			var start = 0;

			while (start <= path.Length) {
				var end = path.IndexOf('/', start);
				end = end >= 0 ? end : path.Length;
				child = child.TryFindNodeById(path, start, end - start);
				if (child == null) {
					break;
				}
				start = end + 1;
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
			return TryFindNodeById(id, 0, id.Length);
		}

		private Node TryFindNodeById(string path, int start, int length)
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
					var isEqual =
						child.Id != null &&
						child.Id.Length == length &&
						string.CompareOrdinal(child.Id, 0, path, start, length) == 0;
					if (isEqual) {
						queue.Clear();
						return child;
					}
					queue.Enqueue(child);
				}
			}
			return null;
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
			if (animator is Animator<ITexture> textureAnimator) {
				foreach (var key in textureAnimator.ReadonlyKeys) {
					key.Value.GetPlatformTexture();
				}
			}
		}

		private void PreloadTexture(PropertyInfo prop)
		{
			var getter = prop.GetGetMethod();
			var texture = getter.Invoke(this, new object[]{}) as ITexture;
			texture?.GetPlatformTexture();
		}

		public void RemoveAnimatorsForExternalAnimations()
		{
			foreach (var animation in Animations) {
				if (!string.IsNullOrEmpty(animation.ContentsPath)) {
					var animators = new List<IAnimator>();
					animation.FindAnimators(animators);
					foreach (var animator in animators) {
						animator.Owner.Animators.Remove(animator);
					}
				}
			}
		}

		private static readonly ThreadLocal<HashSet<string>> scenesBeingLoaded = new ThreadLocal<HashSet<string>>(() => new HashSet<string>());

		public delegate bool SceneLoadingDelegate(string path, ref Node instance, bool external, bool ignoreExternals);
		public delegate void SceneLoadedDelegate(string path, Node instance, bool external);
		public static ThreadLocal<SceneLoadingDelegate> SceneLoading;
		public static ThreadLocal<SceneLoadedDelegate> SceneLoaded;

		/// <summary>
		/// For each root node of deserialized node hierarchy this component will be added and set to path in asset bundle.
		/// </summary>
		[NodeComponentDontSerialize]
		public class AssetBundlePathComponent : NodeComponent
		{
			[YuzuMember]
			public string Path;
			public AssetBundlePathComponent()
			{

			}
			public AssetBundlePathComponent(string path)
			{
				this.Path = path;
			}
			public override string ToString()
			{
				return Path;
			}
		}

		object ICloneable.Clone()
		{
			var clone = Lime.InternalPersistence.Instance.Clone(this);
			clone.NotifyOnBuilt();
			return clone;
		}

		public static T CreateFromAssetBundle<T>(string path, T instance = null, InternalPersistence persistence = null, bool ignoreExternals = false) where T : Node
		{
			persistence = persistence ?? InternalPersistence.Instance;
			return (T) CreateFromAssetBundleHelper(path, instance, persistence, external: false, ignoreExternals: ignoreExternals);
		}

		public static Node CreateFromAssetBundle(string path, Node instance = null, InternalPersistence persistence = null, bool ignoreExternals = false)
		{
			persistence = persistence ?? InternalPersistence.Instance;
			return CreateFromAssetBundleHelper(path, instance, persistence, external: false, ignoreExternals: ignoreExternals);
		}

		public static Node CreateFromStream(string path, Node instance = null, InternalPersistence persistence = null, Stream stream = null, bool ignoreExternals = false)
		{
			persistence = persistence ?? InternalPersistence.Instance;
			return CreateFromAssetBundleHelper(path, instance, persistence, stream, external: false, ignoreExternals);
		}

		private static Node CreateFromAssetBundleHelper(string path, Node instance = null, InternalPersistence persistence = null, Stream stream = null, bool external = false, bool ignoreExternals = false)
		{
			if (SceneLoading?.Value?.Invoke(path, ref instance, external, ignoreExternals) ?? false) {
				if (!external) {
					instance.NotifyOnBuilt();
				}
				SceneLoaded?.Value?.Invoke(path, instance, external);
				return instance;
			}
			var fullPath = stream != null && sceneExtensions.Any(e => path.EndsWith(e, StringComparison.OrdinalIgnoreCase)) ? path : ResolveScenePath(path);
			if (fullPath == null) {
				throw new FileNotFoundException($"None of {string.Join(", ", sceneExtensions.Select(e => $"{path}{e}"))} exists");
			}
			if (scenesBeingLoaded.Value.Contains(fullPath)) {
				throw new CyclicDependencyException($"Cyclic scenes dependency was detected: {fullPath}");
			}
			scenesBeingLoaded.Value.Add(fullPath);
			try {
				if (stream != null) {
					instance = persistence.ReadObject<Node>(fullPath, stream, instance);
				} else {
					using (stream = AssetBundle.Current.OpenFileLocalized(fullPath)) {
						instance = persistence.ReadObject<Node>(fullPath, stream, instance);
					}
				}
				instance.Components.Add(new AssetBundlePathComponent(fullPath));
				if (!ignoreExternals) {
					instance.LoadExternalScenes(persistence, !external);
				}
			} finally {
				scenesBeingLoaded.Value.Remove(fullPath);
			}
			SceneLoaded?.Value?.Invoke(path, instance, external);
			return instance;
		}

		public void NotifyOnBuilt()
		{
			foreach (var n in Nodes) {
				n.NotifyOnBuilt();
			}
			NotifyOnBuiltSuperficial();
		}

		public void NotifyOnBuiltSuperficial()
		{
			foreach (var c in Components) {
				c.OnBuilt();
			}
			OnBuilt();
		}

		protected virtual void OnBuilt()
		{
		}

		public virtual void LoadExternalScenes(InternalPersistence persistence = null, bool isExternalRoot = true)
		{
			persistence = persistence ?? InternalPersistence.Instance;
			if (string.IsNullOrEmpty(ContentsPath)) {
				foreach (var child in Nodes) {
					child.LoadExternalScenes(persistence, false);
				}
			} else if (ResolveScenePath(ContentsPath) != null) {
				var content = CreateFromAssetBundleHelper(ContentsPath, null, persistence, external: true);
				ReplaceContent(content);
			}

			if (isExternalRoot) {
				NotifyOnBuilt();
			}
		}

		public void ReplaceContent(Node content)
		{
			var nodeType = GetType();
			var contentType = content.GetType();
			if (content is IPropertyLocker propertyLocker) {
				var properties = nodeType.GetProperties(
					BindingFlags.Instance |
					BindingFlags.Public
				);

				foreach (var property in properties) {
					if (!propertyLocker.IsPropertyLocked(property.Name, true)) {
						if (property.Name == nameof(Widget.Size)) {
							property.SetValue(content, property.GetValue(this));
						}
						continue;
					}

					property.SetValue(this, property.GetValue(content));
				}
			} else {
				if ((content is Widget widget) && (this is Widget)) {
					widget.Size = ((Widget)this).Size;
				}
			}

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
			if (nodeType != contentType && !contentType.IsSubclassOf(nodeType) && !nodeType.IsSubclassOf(contentType)) {
				// Handle legacy case: Replace Button content by external Frame
				if (nodeType == typeof(Button) && contentType == typeof(Frame)) {
					Components.Remove(typeof(AssetBundlePathComponent));
					var assetBundlePathComponent = content.Components.Get<AssetBundlePathComponent>();
					if (assetBundlePathComponent != null) {
						Components.Add(Cloner.Clone(assetBundlePathComponent));
					}
				} else {
					throw new Exception($"Can not replace {nodeType.FullName} content with {contentType.FullName}");
				}
			} else {
				foreach (var c in Components.Where(i => NodeComponent.IsSerializable(i.GetType())).ToList()) {
					Components.Remove(c);
				}
				foreach (var c in content.Components.Where(i => NodeComponent.IsSerializable(i.GetType()))) {
					Components.Add(Cloner.Clone(c));
				}
			}
			Components.Remove<AnimationComponent>();
			var animationComponent = content.Components.Get<AnimationComponent>();
			if (animationComponent != null) {
				var newAnimationComponent = new AnimationComponent();
				foreach (var a in animationComponent.Animations) {
					newAnimationComponent.Animations.Add(Cloner.Clone(a));
				}
				Components.Add(newAnimationComponent);
			}
		}

		private static readonly string[] sceneExtensions = { ".t3d", ".tan" };

		/// <summary>
		/// Returns path to scene if it exists in bundle. Returns null otherwise.
		/// Throws exception if there is more than one scene file with such path.
		/// </summary>
		public static string ResolveScenePath(string path)
		{
			var candidates = sceneExtensions.Select(ext => path + $"{ext}").Where(AssetBundle.Current.FileExists);
			if (candidates.Count() > 1) {
				throw new Exception("Ambiguity between: {0}", string.Join("; ", candidates.ToArray()));
			}
			return candidates.FirstOrDefault();
		}

		public bool IsMouseOver()
		{
#if WIN || MAC
			if (Application.WindowUnderMouse != Window.Current) {
				return false;
			}
#endif
			var context = WidgetContext.Current;
			if (context.NodeUnderMouse != this) {
				return false;
			}
			return context.NodeCapturedByMouse == null || context.NodeCapturedByMouse == this;
		}

		public bool IsMouseOverThisOrDescendant()
		{
#if WIN || MAC
			if (Application.WindowUnderMouse != Window.Current) {
				return false;
			}
#endif
			var context = WidgetContext.Current;
			var nodeUnderMouse = context.NodeUnderMouse;
			if (nodeUnderMouse == null || !nodeUnderMouse.SameOrDescendantOf(this)) {
				return false;
			}
			return context.NodeCapturedByMouse?.SameOrDescendantOf(this) ?? true;
		}

		protected internal virtual bool PartialHitTest(ref HitTestArgs args)
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

		public virtual void UpdateBoundingRect()
		{
			for (var n = FirstChild; n != null; n = n.NextSibling) {
				n.UpdateBoundingRect();
			}
		}

		private bool frozen;

		public bool Frozen
		{
			get => frozen;
			set {
				if (frozen != value) {
					frozen = value;
					PropagateDirtyFlags(DirtyFlags.Frozen);
					Manager?.FilterNode(this);
				}
			}
		}

		protected bool globallyFrozen;

		public bool GloballyFrozen
		{
			get {
				if (CleanDirtyFlags(DirtyFlags.Frozen)) {
					RecalcGloballyFrozen();
				}
				return globallyFrozen;
			}
		}

		protected virtual void RecalcGloballyFrozen()
		{
			globallyFrozen = Frozen;
			if (Parent != null) {
				globallyFrozen |= Parent.GloballyFrozen;
			}
		}
	}

	public interface IUpdatableNode
	{
		void OnUpdate(float delta);
	}

	public static class NodeCompatibilityExtensions
	{
		/// <summary>
		/// Returns a clone of the node hierarchy.
		/// </summary>
		public static T Clone<T>(this Node node) where T : Node
		{
			return (T)Clone(node);
		}

		/// <summary>
		/// Returns a clone of the node hierarchy.
		/// </summary>
		public static Node Clone(this Node node)
		{
			return Cloner.Clone(node);
		}

		public static Action<Node, float> AdvanceAnimationsRecursiveHook = DefaultAdvanceAnimationsRecursive;

		private static void DefaultAdvanceAnimationsRecursive(Node node, float delta)
		{
			var c = node.Components.AnimationComponent;
			if (c != null) {
				foreach (var a in c.Animations) {
					a.Advance(delta);
				}
			}
			foreach (var child in node.Nodes) {
				DefaultAdvanceAnimationsRecursive(child, delta * child.AnimationSpeed);
			}
		}

		public static void AdvanceAnimationsRecursive(this Node node, float delta)
		{
			AdvanceAnimationsRecursiveHook(node, delta);
		}

		private static void RegisterLegacyBehaviors(Node node)
		{
			foreach (var component in node.Components) {
				if (component is NodeBehavior legacyBehavior) {
					legacyBehavior.Register();
				}
			}
		}

		public static void Update(this Node node, float delta)
		{
			var boneBehavior = node.Components.Get<BoneBehavior>();
			if (boneBehavior != null && boneBehavior.Owner.Parent != null) {
				boneBehavior.Register();
			}
			RegisterLegacyBehaviors(node);
			var legacyEarlyBehaviorContainer = node.Components.Get<LegacyEarlyBehaviorContainer>();
			if (legacyEarlyBehaviorContainer != null) {
				legacyEarlyBehaviorContainer.Update(delta);
			}
			var visible = true;
			if (node.AsWidget != null) {
				visible = node.AsWidget.GloballyVisible;
			} else if (node.AsNode3D != null) {
				visible = node.AsNode3D.GloballyVisible;
			}
			if (visible) {
				var animationComponent = node.Components.Get<AnimationComponent>();
				if (animationComponent != null) {
					foreach (var a in animationComponent.Animations) {
						a.Advance(delta);
					}
				}
				for (var child = node.FirstChild; child != null; child = child.NextSibling) {
					Update(child, delta * child.AnimationSpeed);
				}
			}
			RegisterLegacyBehaviors(node);
			var legacyLateBehaviorContainer = node.Components.Get<LegacyLateBehaviorContainer>();
			if (legacyLateBehaviorContainer != null) {
				legacyLateBehaviorContainer.Update(delta);
			}
			var boneArrayUpdaterBehavior = node.Components.Get<BoneArrayUpdaterBehavior>();
			if (boneArrayUpdaterBehavior != null) {
				boneArrayUpdaterBehavior.Update(delta);
			}
			var updatableNodeBehavior = node.Components.Get<UpdatableNodeBehavior>();
			if (updatableNodeBehavior != null) {
				updatableNodeBehavior.CheckOwner();
				updatableNodeBehavior.Update(delta);
			}
		}
	}

	[MutuallyExclusiveDerivedComponents]
	[NodeComponentDontSerialize]
	[UpdateStage(typeof(LateUpdateStage))]
	public class UpdatableNodeBehavior : BehaviorComponent
	{
		private IUpdatableNode node;

		internal void CheckOwner()
		{
			node = (IUpdatableNode)Owner;
		}

		protected internal override void Start()
		{
			CheckOwner();
		}

		protected internal override void Update(float delta)
		{
			node.OnUpdate(delta);
		}
	}

	[NodeComponentDontSerialize]
	public class AnimationComponent : NodeComponent
	{
		internal AnimationProcessor Processor;
		internal int Depth = -1;

		public AnimationCollection Animations { get; private set; }

		public Animation DefaultAnimation
		{
			get {
				foreach (var a in Animations) {
					if (a.IsLegacy) {
						return a;
					}
				}
				var newAnimation = new Animation() { IsLegacy = true };
				Animations.Add(newAnimation);
				return newAnimation;
			}
		}

		internal void OnAnimationRun(Animation animation)
		{
			Processor?.OnAnimationRun(animation);
		}

		internal void OnAnimationStopped(Animation animation)
		{
			Processor?.OnAnimationStopped(animation);
		}

		public AnimationComponent()
		{
			Animations = new AnimationCollection(this);
		}
	}

	[NodeComponentDontSerialize]
	[UpdateStage(typeof(LateUpdateStage))]
	[UpdateBeforeBehavior(typeof(UpdatableNodeBehavior))]
	public class BoneArrayUpdaterBehavior : BehaviorComponent
	{
		private bool attached;
		private List<Bone> bones = new List<Bone>();
		private bool needResort = false;

		public void AddBone(Bone bone)
		{
			bones.Add(bone);
			OnBoneCollectionChanged();
		}

		public void RemoveBone(Bone bone)
		{
			bones.Remove(bone);
			OnBoneCollectionChanged();
		}

		private void OnBoneCollectionChanged()
		{
			needResort = true;
			CheckActivity();
		}

		protected internal override void Start()
		{
			attached = true;
			CheckActivity();
		}

		protected internal override void Stop(Node owner)
		{
			attached = false;
		}

		protected internal override void Update(float delta)
		{
			if (needResort) {
				bones.Sort((x, y) => x.Index.CompareTo(y.Index));
				needResort = false;
			}
			foreach (var b in bones) {
				b.OnUpdate(delta);
			}
		}

		private void CheckActivity()
		{
			if (attached) {
				if (bones.Count > 0) {
					Resume();
				} else {
					Suspend();
				}
			}
		}
	}
}
