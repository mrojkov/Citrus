using System.Collections.Generic;
using System.IO;
using System;
using ProtoBuf;
using System.ComponentModel;
using System.Diagnostics;

namespace Lime
{
	public delegate void BareEventHandler();
	public delegate void UpdateDelegate(float delta);

	[ProtoContract]
	[ProtoInclude(101, typeof(Widget))]
	[ProtoInclude(102, typeof(ParticleModifier))]
	[ProtoInclude(103, typeof(ImageCombiner))]
	[ProtoInclude(104, typeof(PointObject))]
	[ProtoInclude(105, typeof(Bone))]
	[ProtoInclude(106, typeof(Audio))]
	[ProtoInclude(107, typeof(SplineGear))]
	[ProtoInclude(108, typeof(TextStyle))]
	[DebuggerTypeProxy(typeof(NodeDebugView))]
	public class Node
	{
		public event UpdateDelegate Updating;
		public event UpdateDelegate Updated;
		public event BareEventHandler AnimationStopped;

		public static int UpdatedNodes;
		private static List<Node> nodesToUnlink = new List<Node>();

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
		public Widget Widget { get; internal set; }

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
			while (node.Parent != null)
				node = node.Parent;
			return node;
		}

		public void ClearUpdatingEvent()
		{
			this.Updating = null;
		}

		public void ClearUpdatedEvent()
		{
			this.Updated = null;
		}

		public bool HasChild(Node node)
		{
			return node.ChildOf(this);
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

		public virtual void PreloadTextures()
		{
			foreach (var node in Nodes) {
				node.PreloadTextures();
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
			Node clone = (Node)MemberwiseClone();
			clone.Parent = null;
			clone.Widget = clone as Widget;
			clone.Animators = AnimatorCollection.SharedClone(clone, Animators);
			clone.Nodes = NodeCollection.DeepClone(clone, Nodes);
			return clone;
		}

		/// <summary>
		/// Use for fast deep cloning of unchangeable objects. This function based on MemberwiseClone().
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
			Parent.Nodes.Remove(this);
			Parent = null;
		}
		
		public void UnlinkAfterUpdate()
		{
			nodesToUnlink.Add(this);
		}

		public virtual void Update(int delta)
		{
			UpdatedNodes++;
			if (IsRunning) {
				AdvanceAnimation(delta);
			}
			if (Nodes.Count > 0) {
				UpdateChildren(delta);
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

		public virtual void Render() {}

		public virtual void AddToRenderChain(RenderChain chain)
		{
			foreach (Node node in Nodes.AsArray) {
				node.AddToRenderChain(chain);
			}
		}

		protected internal virtual void OnTrigger(string property)
		{
			if (property == "Trigger") {
				if (String.IsNullOrEmpty(Trigger)) {
					AnimationTime = 0;
					IsRunning = true;
				} else {
					TryRunAnimation(Trigger);
				}
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

		public T TryFind<T>(string id) where T : Node
		{
			return TryFindNode(id) as T;
		}

		public T TryFind<T>(string format, params object[] args) where T : Node
		{
			return TryFind<T>(string.Format(format, args));
		}

		public Node FindNode(string id)
		{
			var node = TryFindNode(id);
			if (node == null) {
				throw new Lime.Exception("'{0}' not found for '{1}'", id, ToString());
			}
			return node;
		}

		public Node TryFindNode(string id)
		{
			if (id.IndexOf('/') >= 0) {
				Node child = this;
				string[] names = id.Split('/');
				foreach (string name in names) {
					child = child.TryFindNode(name);
					if (child == null)
						break;
				}
				return child;
			} else
				return TryFindNodeHelper(id);
		}

		private Node TryFindNodeHelper(string id)
		{
			Queue<Node> queue = new Queue<Node>();
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
			const int MaxTimeDelta = 1000 / Animator.FramesPerSecond - 1;
			while (delta > MaxTimeDelta) {
				AdvanceAnimationShort(MaxTimeDelta);
				delta -= MaxTimeDelta;
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
		#endregion
	}
}