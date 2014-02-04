using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;
using ProtoBuf;
using System.ComponentModel;
using System.Diagnostics;

namespace Lime
{
	public class NodeTreeStatistics
	{
		public int TotalNodes;
		public int VisibileNodes;
		public int OldNodes;
		public DateTime OldNodeBarrier = DateTime.Now.AddMinutes(-5);
	}

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
		public UpdateHandler Updating;
		public UpdateHandler Updated;

		public event Action AnimationStopped;
	
		private static readonly List<Node> nodesToUnlink = new List<Node>();

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

		public Node Parent;
		public Widget AsWidget { get; internal set; }

		internal Node NextToRender;
		public int Layer { get; set; }

		[ProtoMember(5)]
		public AnimatorCollection Animators;

		[ProtoMember(6)]
		public NodeCollection Nodes;

		[ProtoMember(7)]
		public MarkerCollection Markers;

		[ProtoMember(9)]
		public bool IsRunning { get; set; }
		public bool IsStopped { get { return !IsRunning; } set { IsRunning = !value; } }

		[ProtoMember(10)]
		public int AnimationTime {
			get { return animationTime; }
			set { animationTime = value; ApplyAnimators(false); }
		}

		public DateTime CreationTime = DateTime.Now; 

		[ProtoMember(11)]
		public string Tag { get; set; }

		[ProtoMember(12)]
		public bool PropagateAnimation { get; set; }

		public string CurrentAnimation { get; private set; }

		public float AnimationSpeed { get; set; }

		#endregion
		#region Methods

		public Node()
		{
			AnimationSpeed = 1;
			Animators = new AnimatorCollection(this);
			Markers = new MarkerCollection();
			Nodes = new NodeCollection(this);
		}

		public Node GetRoot()
		{
			Node node = this;
			while (node.Parent != null) {
				node = node.Parent;
			}
			return node;
		}

		public bool HasChild(Node node)
		{
			return node.ChildOf(this);
		}

		public bool HasChild(string path)
		{
			return this.TryFindNode(path) != null;
		}

		public bool ChildOf(Node node)
		{
			for (Node n = Parent; n != null; n = n.Parent) {
				if (n == node)
					return true;
			}
			return false;
		}

		public IEnumerable<T> GetNodesOfType<T>() where T : Node
		{
			foreach (var node in Nodes) {
				if (node is T) {
					yield return node as T;
				}
			}
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
			get { return Animator.MsecsToFrames(AnimationTime); }
			set { AnimationTime = Animator.FramesToMsecs(value); }
		}

		static public void UnlinkScheduledNodes()
		{
			foreach (Node node in nodesToUnlink) {
				if (node.Parent != null) {
					node.Parent.Nodes.Remove(node);
				}
			}
			nodesToUnlink.Clear();
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
			clone.CreationTime = DateTime.Now;
			clone.Parent = null;
			clone.AsWidget = clone as Widget;
			clone.Animators = AnimatorCollection.SharedClone(clone, Animators);
			clone.Nodes = NodeCollection.DeepCloneFast(clone, Nodes);
			return clone;
		}

		/// <summary>
		/// Use for fast deep cloning of immutable objects. This function based on MemberwiseClone().
		/// Animators keys, Markers and SkinningWeights are shared between clone and original.
		/// </summary>
		public T DeepCloneFast<T>() where T : Node
		{
			return DeepCloneFast() as T;
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}", GetType().Name, GetHierarchyPath());
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

		public void Unlink()
		{
			if (Parent != null) {
				Parent.Nodes.Remove(this);
				Parent = null;
			}
		}
		
		public void UnlinkAfterUpdate()
		{
			nodesToUnlink.Add(this);
		}

		public virtual void Update(int delta)
		{
			if (IsRunning) {
				AdvanceAnimation(delta);
			}
			if (!Nodes.Empty) {
				UpdateChildren(delta);
			}
		}

		public void CollectStatistics(NodeTreeStatistics stat)
		{
			stat.TotalNodes++;
			if (CreationTime < stat.OldNodeBarrier) {
				stat.OldNodes++;
			}
			if (AsWidget != null && AsWidget.GloballyVisible) {
				stat.VisibileNodes++;
			}
			foreach (var child in Nodes.AsArray) {
				child.CollectStatistics(stat);
			}
		}

		protected void UpdateChildren(int delta)
		{
			foreach (Node node in Nodes.AsArray) {
				if (node.AnimationSpeed != 1) {
					int delta2 = (int)(node.AnimationSpeed * delta + 0.5f);
					node.FullUpdate(delta2);
				} else {
					node.FullUpdate(delta);
				}
			}
		}

		public void FullUpdate(int delta)
		{
			if (Updating != null) {
				Updating(delta * 0.001f);
			}
			Update(delta);
			if (Updated != null) {
				Updated(delta * 0.001f);
			}
		}

		internal virtual void OnParentChanged() {}

		public virtual void Render() {}

		public virtual void AddToRenderChain(RenderChain chain)
		{
			foreach (Node node in Nodes.AsArray) {
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
			if (result == null)
				throw new Lime.Exception("'{0}' of {1} not found for '{2}'", id, typeof(T).Name, ToString());
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
				Node child = this;
				string[] ids = path.Split('/');
				foreach (string id in ids) {
					child = child.TryFindNode(id);
					if (child == null)
						break;
				}
				return child;
			} else
				return TryFindNodeHelper(path);
		}

		private Node TryFindNodeHelper(string id)
		{
			var queue = new Queue<Node>();
			queue.Enqueue(this);
			while (queue.Count > 0) {
				Node node = queue.Dequeue();
				foreach (Node child in node.Nodes.AsArray) {
					if (child.Id == id)
						return child;
				}
				foreach (Node child in node.Nodes.AsArray)
					queue.Enqueue(child);
			}
			return null;
		}

		public void AdvanceAnimation(int delta)
		{
			const int maxTimeDelta = 1000 / Animator.FramesPerSecond - 1;
			while (delta > maxTimeDelta) {
				AdvanceAnimationShort(maxTimeDelta);
				delta -= maxTimeDelta;
			}
			AdvanceAnimationShort(delta);
		}

		private void AdvanceAnimationShort(int delta)
		{
			if (IsRunning) {
				int prevFrame = Animator.MsecsToFrames(animationTime - 1);
				int currFrame = Animator.MsecsToFrames(animationTime + delta - 1);
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
						animationTime += Animator.FramesToMsecs(hopFrames);
						prevFrame += hopFrames;
						currFrame += hopFrames;
					}
					break;
				case MarkerAction.Stop:
					animationTime = Animator.FramesToMsecs(marker.Frame);
					prevFrame = currFrame - 1;
					IsRunning = false;
					break;
				case MarkerAction.Destroy:
					animationTime = Animator.FramesToMsecs(marker.Frame);
					prevFrame = currFrame - 1;
					IsRunning = false;
					UnlinkAfterUpdate();
					break;
			}
		}

		private void ApplyAnimators(bool invokeTriggers)
		{
			foreach (Node node in Nodes.AsArray) {
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

		public void Preload()
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
			foreach (var node in Nodes.AsArray) {
				node.Preload();
			}
		}

		private static void PreloadAnimatedTextures(Animator animator)
		{
			var textureAnimator = animator as GenericAnimator<ITexture>;
			if (textureAnimator != null) {
				foreach (var texture in textureAnimator.values) {
					texture.GetHandle();
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
				foreach (var node in Nodes.AsArray) {
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
			foreach (Marker marker in content.Markers) {
				Markers.Add(marker);
			}
			foreach (Node node in content.Nodes.AsArray) {
				node.Unlink();
				Nodes.Add(node);
			}
		}


		#endregion
	}
}