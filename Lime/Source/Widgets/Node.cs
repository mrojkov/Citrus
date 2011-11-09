using System.Runtime.Serialization;
using Lime;
using System.Collections.Generic;
using System.IO;
using System;
using ProtoBuf;

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
	public class Node
	{
		public static int UpdatedNodes;
		
        [ProtoMember(1)]
		public string Id { get; set; }

        [ProtoMember(2)]
		public string ContentsPath { get; set; }

		[Trigger]
		public string Trigger { get; set; }

		public Node Parent;
		public Widget Widget;

		[ProtoMember(5)]
		public readonly AnimatorCollection Animators = new AnimatorCollection ();

		[ProtoMember(6)]
		public readonly NodeCollection Nodes = new NodeCollection ();

		[ProtoMember(7)]
		public readonly MarkerCollection Markers = new MarkerCollection ();

		[ProtoMember(9)]
		public bool Playing { get; set; }

		private int animationTime;
		[ProtoMember(10)]
		public int AnimationTime {
			get { return animationTime; }
			set {
				animationTime = value;
				int count = Nodes.Count;
				for (int i = 0; i < count; i++) {
					Nodes [i].Animators.Apply (animationTime);
				}
			}
		}

		public void AdvanceAnimation (int delta)
		{
			int count = Markers.Count;
			for (int i = 0; i < count; i++) {
				var marker = Markers [i];
				int markerTime = Animator.FramesToMsecs (marker.Frame);
				if (animationTime <= markerTime && markerTime < animationTime + delta) {
					if (marker.Action == MarkerAction.Jump) {
						var gotoMarker = Markers.Get (marker.JumpTo);
						if (gotoMarker != null) {
							int gotoTime = Animator.FramesToMsecs (gotoMarker.Frame);
							animationTime = gotoTime + (animationTime + delta - markerTime);
						}
					} else if (marker.Action == MarkerAction.Stop) {
						delta = markerTime - animationTime;
						Playing = false;
					} else if (marker.Action == MarkerAction.Destroy) {
						Playing = false;
						Destroy ();
					}
					break;
				}
			}
			count = Nodes.Count;
			for (int i = 0; i < count; i++) {
				var animators = Nodes [i].Animators;
				animators.Apply (animationTime + delta);
				animators.InvokeTriggers (animationTime, animationTime + delta);
			}
			animationTime += delta;
		}

		private static List<Node> collectedNodes = new List<Node> ();

		public Node ()
		{
			Animators.Owner = this;
			Nodes.Owner = this;
		}
		
		static HashSet<string> processingFiles = new HashSet<string> ();

		public static Node CreateFromBundle (string path)
		{
			return CreateFromBundleHelper (path, false);
		}

		public static Node CreateFromBundleCached (string path)
		{
			return CreateFromBundleHelper (path, true);
		}

		static Node CreateFromBundleHelper (string path, bool cached)
		{
			if (processingFiles.Contains (path))
				throw new Lime.Exception ("Cyclic dependency of scenes has detected: {0}", path);
			Node node;
			processingFiles.Add (path);
			try {
				if (cached) {
					node = Serialization.ReadObjectCached<Node> (path);
				} else {
					node = Serialization.ReadObject<Node> (path);
				}
				node.LoadContents ();
			} finally {
				processingFiles.Remove (path);
			}
			return node;
		}

		public Node GetRoot ()
		{
			Node node = this;
			while (node.Parent != null)
				node = node.Parent;
			return node;
		}

		public void PlayAnimation (string markerId)
		{
			Marker marker = Markers.Get (markerId);
			if (marker == null) {
				// Console.WriteLine ("WARNING: Attempt to play animation. Unknown marker '{0}' in node '{1}'", markerId, Id);
				return;
			}
			AnimationFrame = marker.Frame;
			Playing = true;
		}

		public int AnimationFrame {
			get { return Animator.MsecsToFrames (AnimationTime); }
			set { AnimationTime = Animator.FramesToMsecs (value); }
		}
		
		static public void CleanupDeadNodes ()
		{
			foreach (Node node in collectedNodes) {
				if (node.Parent != null)
					node.Parent.Nodes.Remove (node);
			}
			collectedNodes.Clear ();
		}
		
		public Node DeepClone ()
		{
			return Serialization.DeepClone<Node> (this);
		}

		public override string ToString ()
		{
			return string.Format ("{0}, {1}", GetType ().Name, GetDescription ());
		}

		string GetDescription ()
		{
			string r = string.IsNullOrEmpty (Id) ? String.Format ("[{0}]", GetType().Name): Id;
			if (Parent != null) {
				r = Parent.GetDescription () + "/" + r;
			}
			return r;
		}
		
		public void Destroy ()
		{
			collectedNodes.Add (this);
		}
		
		// Delta must be in [0..1000 / WidgetUtils.FramesPerSecond - 1] range
		public virtual void Update (int delta)
		{
			UpdatedNodes++;
			if (Playing) {
				AdvanceAnimation (delta);
			}
			for (int i = Nodes.Count - 1; i >= 0; i--)
				Nodes [i].Update (delta);
		}

		public void SafeUpdate (int delta)
		{
			if (delta < 0)
				throw new Lime.Exception ("Update interval can not be negative");
			const int step = 1000 / Animator.FramesPerSecond - 1;
			while (delta > step) {
				Update (step);
				delta -= step;
			}
			Update (delta);
		}

		public virtual bool WorldShown { get { return true; } }

		public virtual void Render ()
		{
			for (int i = Nodes.Count - 1; i >= 0; i--) {
				var node = Nodes [i];
				if (node.WorldShown)
					node.Render ();
			}
		}

		public void LoadContents ()
		{
			if (!string.IsNullOrEmpty (ContentsPath))
				LoadContentsHelper ();
			else {
				foreach (Node node in Nodes)
					node.LoadContents ();
			}
		}

		void LoadContentsHelper ()
		{
			Nodes.Clear ();
			Markers.Clear ();
			if (File.Exists (ContentsPath)) {
				Node contents = Node.CreateFromBundle (ContentsPath);
				if (contents.Widget != null && Widget != null) {
					contents.Update (0);
					contents.Widget.Size = Widget.Size;
					contents.Update (0);
				}
				foreach (Marker marker in contents.Markers)
					Markers.Add (marker);
				foreach (Node node in contents.Nodes)
					Nodes.Add (node);
			}
		}

		protected internal virtual void OnTrigger (string property)
		{
			if (property == "Trigger") {
				if (String.IsNullOrEmpty (Trigger)) {
					AnimationTime = 0;
					Playing = true;
				} else {
					PlayAnimation (Trigger);
				}
			}
		}

		public T Find<T> (string id) where T : Node
		{
			T result = Find (id) as T;
			if (result == null)
				throw new Lime.Exception (
					String.Format ("Node '{0}' (of type: {1}) not found in '{2}' (type: {3})", 
					id, typeof(T), Id, GetType ()));
			return result;
		}

		public Node Find (string id)
		{
			if (id.Contains ("/")) {
				Node child = this;
				string[] names = id.Split ('/');
				foreach (string name in names) {
					child = child.Find (name);
					if (child == null)
						break;
				}
				return child;
			} else
				return FindHelper (id);
		}

		Node FindHelper (string id)
		{
			Queue<Node> queue = new Queue<Node> ();
			queue.Enqueue (this);
			while (queue.Count > 0) {
				Node node = queue.Dequeue ();
				foreach (Node child in node.Nodes) {
					if (child.Id == id)
						return child;
				}
				foreach (Node child in node.Nodes)
					queue.Enqueue (child);
			}
			return null;
		}
	}
}
