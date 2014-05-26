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
	[DebuggerTypeProxy(typeof(NodeDebugView))]
	public class Node
	{
		public event Action AnimationStopped;
	
		private int animationTime;

		#region properties
#if WIN
		public Guid Guid { get; set; }
#endif
	
		[ProtoMember(1)]
		public string Id { get; set; }

		[ProtoMember(2)]
		public string ContentsPath { get; set; }

		[Trigger]
		[TangerineProperty(1)]
		public string Trigger { get; set; }

		public Node Parent { get; internal set; }

		public Widget AsWidget { get; internal set; }

		internal Node NextToRender;
		internal Node NextSibling;

		public int Layer { get; set; }

		[ProtoMember(5)]
		public AnimatorCollection Animators;

		[ProtoMember(6)]
		public NodeList Nodes;

		[ProtoMember(7)]
		public MarkerCollection Markers;

		[ProtoMember(9)]
		public bool IsRunning { get; set; }
		public bool IsStopped { 
			get { return !IsRunning; } 
			set { IsRunning = !value; } 
		}

		[ProtoMember(10)]
		public int AnimationTime {
			get { return animationTime; }
			set { 
				animationTime = value; 
				ApplyAnimators(invokeTriggers: false); 
			}
		}

		[ProtoMember(11)]
		public string Tag { get; set; }

		[ProtoMember(12)]
		public bool PropagateAnimation { get; set; }

		public string CurrentAnimation { get; private set; }

		public float AnimationSpeed;

		public object UserData { get; set; }

		#endregion
		#region Methods

		public Node()
		{
			AnimationSpeed = 1;
			Animators = new AnimatorCollection(this);
			Markers = new MarkerCollection();
			Nodes = new NodeList(this);
		}

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

		public bool TryRunAnimation(string markerId)
		{
			Marker marker = Markers.TryFind(markerId);
			if (marker == null) {
				return false;
			}
			AnimationStopped = null;
			AnimationFrame = marker.Frame;
			CurrentAnimation = markerId;
			IsRunning = true;
			return true;
		}

		public void RunAnimation(string markerId)
		{
			if (!TryRunAnimation(markerId)) {
				throw new Lime.Exception("Unknown animation '{0}' in node '{1}'", markerId, this.ToString());
			}
		}

		public int AnimationFrame {
			get { return AnimationUtils.MsecsToFrames(AnimationTime); }
			set { AnimationTime = AnimationUtils.FramesToMsecs(value); }
		}

		/// <summary>
		/// Slow but safe deep cloning. This function is based on protobuf-net serialization.
		/// </summary>
		public Node DeepCloneSafe()
		{
			return Serialization.DeepClone<Node>(this);
		}

		/// <summary>
		/// Slow but safe deep cloning. This function is based on protobuf-net serialization.
		/// </summary>
		public T DeepCloneSafe<T>() where T : Node
		{
			return DeepCloneSafe() as T;
		}

		/// <summary>
		/// Use for fast deep cloning of unchangeable objects. This function based on MemberwiseClone().
		/// Animators keys, Markers and SkinningWeights are shared between clone and original.
		/// </summary>
		public virtual Node DeepCloneFast()
		{
			var clone = (Node)MemberwiseClone();
			clone.Parent = null;
			clone.NextSibling = null;
			clone.AsWidget = clone as Widget;
			clone.Animators = AnimatorCollection.SharedClone(clone, Animators);
			clone.Markers = MarkerCollection.DeepClone(Markers);
			clone.Nodes = Nodes.DeepCloneFast(clone);
			return clone;
		}

		/// <summary>
		/// Use for fast deep cloning of immutable objects. This function based on MemberwiseClone().
		/// Animators keys, Markers and SkinningWeights are shared between clone and original.
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
		/// Unlinks the node from its parent. 
		/// </summary>
		public void Unlink()
		{
			if (Parent != null) {
				Parent.Nodes.Remove(this);
			}
		}

		public virtual void Update(float delta)
		{
			delta *= AnimationSpeed;
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
		}

		protected virtual void SelfUpdate(float delta) { }

		protected virtual void SelfLateUpdate(float delta) { }

		public virtual void Render() {}

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

		public void AddNode(Node node)
		{
			Nodes.Add(node);
		}

		public void AddToNode(Node node)
		{
			node.Nodes.Add(this);
		}

		public void PushNode(Node node)
		{
			Nodes.Push(node);
		}

		public void PushToNode(Node node)
		{
			node.Nodes.Push(this);
		}

		public T Find<T>(string id) where T : Node
		{
			T result = TryFindNode(id) as T;
			if (result == null) {
				throw new Lime.Exception("'{0}' of {1} not found for '{2}'", id, typeof(T).Name, ToString());
			}
			return result;
		}

		public T Find<T>(string format, params object[] args) where T : Node
		{
			return Find<T>(string.Format(format, args));
		}

		public bool TryFind<T>(string id, out T node) where T : Node
		{
			node = TryFindNode(id) as T;
			return node != null;
		}

		public T TryFind<T>(string id) where T : Node
		{
			return TryFindNode(id) as T;
		}

		public T TryFind<T>(string format, params object[] args) where T : Node
		{
			return TryFind<T>(string.Format(format, args));
		}

		public Node FindNode(string path)
		{
			var node = TryFindNode(path);
			if (node == null) {
				throw new Lime.Exception("'{0}' not found for '{1}'", path, ToString());
			}
			return node;
		}

		public Node TryFindNode(string path)
		{
			if (path.IndexOf('/') >= 0) {
				return TryFindNodeByPath(path);
			} else {
				return TryFindNodeById(path);
			}
		}

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

		protected void LayoutChildren(Vector2 sizeDelta)
		{
			for (var node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
				node.Layout(sizeDelta);
			}
		}

		protected virtual void Layout(Vector2 parentSizeDelta) { }

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

		public void AdvanceAnimation(float delta)
		{
			int deltaMs = (int)(delta * 1000 + 0.5f);
			while (deltaMs > AnimationUtils.MsecsPerFrame) {
				AdvanceAnimationShort(AnimationUtils.MsecsPerFrame);
				deltaMs -= AnimationUtils.MsecsPerFrame;
			}
			AdvanceAnimationShort(deltaMs);
		}

		private void AdvanceAnimationShort(int delta)
		{
			if (IsRunning) {
				int prevFrame = AnimationUtils.MsecsToFrames(animationTime - 1);
				int currFrame = AnimationUtils.MsecsToFrames(animationTime + delta - 1);
				animationTime += delta;
				if (prevFrame != currFrame && Markers.Count > 0) {
					Marker marker = Markers.GetByFrame(currFrame);
					if (marker != null) {
						ProcessMarker(marker, ref prevFrame, ref currFrame);
					}
				}
				bool invokeTriggers = prevFrame != currFrame;
				ApplyAnimators(invokeTriggers);
				if (!IsRunning) {
					OnAnimationStopped();
				}
			}
		}

		protected virtual void OnAnimationStopped()
		{
			if (AnimationStopped != null) {
				AnimationStopped();
			}
		}

		private void ProcessMarker(Marker marker, ref int prevFrame, ref int currFrame)
		{
			switch (marker.Action) {
				case MarkerAction.Jump:
					var gotoMarker = Markers.TryFind(marker.JumpTo);
					if (gotoMarker != null) {
						int hopFrames = gotoMarker.Frame - AnimationFrame;
						animationTime += AnimationUtils.FramesToMsecs(hopFrames);
						prevFrame += hopFrames;
						currFrame += hopFrames;
					}
					break;
				case MarkerAction.Stop:
					animationTime = AnimationUtils.FramesToMsecs(marker.Frame);
					prevFrame = currFrame - 1;
					IsRunning = false;
					break;
				case MarkerAction.Destroy:
					animationTime = AnimationUtils.FramesToMsecs(marker.Frame);
					prevFrame = currFrame - 1;
					IsRunning = false;
					Unlink();
					break;
			}
		}

		private void ApplyAnimators(bool invokeTriggers)
		{
			for (Node node = Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
				var animators = node.Animators;
				animators.Apply(animationTime);
				if (invokeTriggers) {
					animators.InvokeTriggers(AnimationFrame);
				}
				if (PropagateAnimation) {
					node.animationTime = animationTime;
					node.ApplyAnimators(invokeTriggers);
				}
			}
		}

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
			var contentsPath = Path.ChangeExtension(ContentsPath, "scene");
			if (!AssetsBundle.Instance.FileExists(contentsPath)) {
				return;
			}
			var content = new Frame(ContentsPath);
			if (content.AsWidget != null && AsWidget != null) {
				content.Update(0);
				content.AsWidget.Size = AsWidget.Size;
				content.Update(0);
			}
			Markers.AddRange(content.Markers);
			var nodes = content.Nodes.ToArray();
			content.Nodes.Clear();
			Nodes.AddRange(nodes);
		}
		#endregion
	}
}