using System.Collections.Generic;
using System.IO;
using System;
using ProtoBuf;
using System.ComponentModel;

namespace Lime
{
	[ProtoContract]
	[ProtoInclude(101, typeof(Widget))]
	[ProtoInclude(102, typeof(ParticleModifier))]
	[ProtoInclude(103, typeof(ImageCombiner))]
	[ProtoInclude(104, typeof(PointObject))]
	[ProtoInclude(105, typeof(Bone))]
	[ProtoInclude(106, typeof(Audio))]
	[ProtoInclude(107, typeof(SplineGear))]
	[ProtoInclude(108, typeof(TextStyle))]
	public class Node
	{
		public static int UpdatedNodes;
		private static List<Node> collectedNodes = new List<Node>();
		
		[ProtoMember(1)]
		public string Id { get; set; }

		[ProtoMember(2)]
		public string ContentsPath { get; set; }

		[Trigger]
		public string Trigger { get; set; }

		public Node Parent;
		public Widget Widget;

		public Node NextToRender;
		public int Layer;

		[ProtoMember(5)]
		public AnimatorCollection Animators;

		[ProtoMember(6)]
		public NodeCollection Nodes;

		[ProtoMember(7)]
		public MarkerCollection Markers;

		[ProtoMember(9)]
		public bool Playing;
		public bool Stopped { get { return !Playing; } set { Playing = !value; } }

		private int animationTime;
		[ProtoMember(10)]
		public int AnimationTime {
			get { return animationTime; }
			set {
				animationTime = value;
				foreach (Node node in Nodes.AsArray) {
					node.Animators.Apply(animationTime);
				}
			}
		}

		[ProtoMember(11)]
		public string Tag { get; set; }

		public string CurrentAnimation { get; private set; }

		public void AdvanceAnimation(int delta)
		{
			int prevFrame = Animator.MsecsToFrames(animationTime - 1);
			int currFrame = Animator.MsecsToFrames(animationTime + delta - 1);
			animationTime += delta;
			if (prevFrame != currFrame && Markers.Count > 0) {
				Marker marker = Markers.GetByFrame(currFrame);
				if (marker != null) {
					switch (marker.Action) {
					case MarkerAction.Jump:
						var gotoMarker = Markers.Get(marker.JumpTo);
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
						Playing = false;
						break;
					case MarkerAction.Destroy:
						animationTime = Animator.FramesToMsecs(marker.Frame);
						prevFrame = currFrame - 1;
						Playing = false;
						Detach();
						break;
					}
				}
			}
			foreach (Node node in Nodes.AsArray) {
				var animators = node.Animators;
				animators.Apply(animationTime);
				if (prevFrame != currFrame) {
					animators.InvokeTriggers(currFrame);
				}
			}
		}

		public Node()
		{
			Animators = new AnimatorCollection(this);
			Markers = new MarkerCollection();
			Nodes = new NodeCollection(this);
		}
		
		static HashSet<string> processingFiles = new HashSet<string>();

		public static Node CreateFromBundle(string path)
		{
			if (processingFiles.Contains(path))
				throw new Lime.Exception("Cyclic dependency of scenes has detected: {0}", path);
			Node node;
			processingFiles.Add(path);
			try {
				using (Stream stream = AssetsBundle.Instance.OpenFileLocalized(path)) {
					node = Serialization.ReadObject<Node>(path, stream);
				}
				node.LoadContents();
			} finally {
				processingFiles.Remove(path);
			}
			return node;
		}

		public Node GetRoot()
		{
			Node node = this;
			while (node.Parent != null)
				node = node.Parent;
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

		public bool PlayAnimation(string markerId)
		{
			Marker marker = Markers.Get(markerId);
			if (marker == null) {
				// Console.WriteLine("WARNING: Attempt to play animation. Unknown marker '{0}' in node '{1}'", markerId, Id);
				return false;
			}
			AnimationFrame = marker.Frame;
			CurrentAnimation = markerId;
			Playing = true;
			return true;
		}

		public int AnimationFrame {
			get { return Animator.MsecsToFrames(AnimationTime); }
			set { AnimationTime = Animator.FramesToMsecs(value); }
		}

		static public void CleanupDeadNodes()
		{
			foreach (Node node in collectedNodes) {
				if (node.Parent != null) {
					node.Parent.Nodes.Remove(node);
				}
			}
			collectedNodes.Clear();
		}

		/// <summary>
		/// Slow but safe deep cloning. This function is based on serialization/deserialization.
		/// </summary>
		public Node DeepCloneSafe()
		{
			return Serialization.DeepClone<Node>(this);
		}

		/// <summary>
		/// Use for fast deep cloning of unchangeable objects. This function based of MemberwiseClone().
		/// Animators keys, Markers and SkinningWeights are shared between clone and original.
		/// </summary>
		public virtual Node DeepCloneFast()
		{
			Node clone = (Node)MemberwiseClone();
			clone.Widget = clone as Widget;
			clone.Animators = AnimatorCollection.SharedClone(clone, Animators);
			clone.Nodes = NodeCollection.DeepClone(clone, Nodes);
			return clone;
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}", GetType().Name, GetHierarchyPath());
		}

		string GetHierarchyPath()
		{
			string r = string.IsNullOrEmpty(Id) ? String.Format("[{0}]", GetType().Name): Id;
			if (Parent != null) {
				r = Parent.GetHierarchyPath() + "/" + r;
			}
			return r;
		}
		
		public void Detach()
		{
			collectedNodes.Add(this);
		}

		[Obsolete("Use Detach() instead")]
		public void Destroy()
		{
			Detach();
		}

		// Delta must be in[0..1000 / WidgetUtils.FramesPerSecond - 1] range
		public virtual void Update(int delta)
		{
			UpdatedNodes++;
			if (Playing) {
				AdvanceAnimation(delta);
			}
			foreach (Node node in Nodes.AsArray) {
				node.Update(delta);
			}
		}

		public virtual void LateUpdate(int delta)
		{
			foreach (Node node in Nodes.AsArray) {
				node.LateUpdate(delta);
			}
		}

		protected const int MaxTimeDelta = 1000 / Animator.FramesPerSecond - 1;

		public void SafeUpdate(int delta)
		{
			if (delta < 0)
				throw new Lime.Exception("Update interval can not be negative");
			while (delta > MaxTimeDelta) {
				Update(MaxTimeDelta);
				LateUpdate(MaxTimeDelta);
				delta -= MaxTimeDelta;
			}
			Update(delta);
			LateUpdate(delta);
		}

		public virtual void Render()
		{
		}

		public virtual void AddToRenderChain(RenderChain chain)
		{
			foreach (Node node in Nodes.AsArray) {
				node.AddToRenderChain(chain);
			}
		}

		public void LoadContents()
		{
			if (!string.IsNullOrEmpty(ContentsPath))
				LoadContentsHelper();
			else {
				foreach (Node node in Nodes.AsArray)
					node.LoadContents();
			}
		}

		void LoadContentsHelper()
		{
			Nodes.Clear();
			Markers.Clear();
			if (AssetsBundle.Instance.FileExists(ContentsPath)) {
				Node contents = Node.CreateFromBundle(ContentsPath);
				if (contents.Widget != null && Widget != null) {
					contents.Update(0);
					contents.Widget.Size = Widget.Size;
					contents.Update(0);
				}
				foreach (Marker marker in contents.Markers)
					Markers.Add(marker);
				foreach (Node node in contents.Nodes.AsArray)
					Nodes.Add(node);
			}
		}

		protected internal virtual void OnTrigger(string property)
		{
			if (property == "Trigger") {
				if (String.IsNullOrEmpty(Trigger)) {
					AnimationTime = 0;
					Playing = true;
				} else {
					PlayAnimation(Trigger);
				}
			}
		}

		public T Find<T>(string id, bool throwException = true) where T : Node
		{
			T result = Find(id, throwException) as T;
			if (throwException && result == null)
				throw new Lime.Exception("'{0}' of {1} not found for '{2}'", id, typeof(T).Name, ToString());
			return result;
		}

		public Button FindButton(string id, bool throwException = true)
		{
			return Find<Button>(id, throwException);
		}

		public Widget FindWidget(string id, bool throwException = true)
		{
			return Find<Widget>(id, throwException);
		}

		public Frame FindFrame(string id, bool throwException = true)
		{
			return Find<Frame>(id, throwException);
		}

		public SimpleText FindText(string id, bool throwException = true)
		{
			return Find<SimpleText>(id, throwException);
		}

		public Node Find(string id, bool throwException = true)
		{
			if (id.IndexOf('/') >= 0) {
				Node child = this;
				string[] names = id.Split('/');
				foreach (string name in names) {
					child = child.Find(name, throwException);
					if (child == null)
						break;
				}
				if (throwException && child == null)
					throw new Lime.Exception("'{0}' not found for '{1}'", id, ToString());
				return child;
			} else
				return FindHelper(id, throwException);
		}

		Node FindHelper(string id, bool throwException)
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
			if (throwException)
				throw new Lime.Exception("'{0}' not found for '{1}'", id, ToString());
			return null;
		}
	}
}
	