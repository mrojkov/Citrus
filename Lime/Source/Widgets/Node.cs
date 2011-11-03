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

		public string Description { get { return GetDescription (); } }

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
						Suicide ();
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

		private static List<Node> killList = new List<Node> ();

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
			foreach (Node node in killList) {
				if (node.Parent != null)
					node.Parent.Nodes.Remove (node);
			}
			killList.Clear ();
		}
		
		public Node DeepClone ()
		{
			return Serialization.DeepClone<Node> (this);
		}

		public string GetDescription ()
		{
			string r = string.IsNullOrEmpty (Id) ? String.Format ("[{0}]", GetType().Name): Id;
			if (Parent != null) {
				r = Parent.GetDescription () + "/" + r;
			}
			return r;
		}
		
		public void Suicide ()
		{
			killList.Add (this);
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

		protected internal virtual bool IsTriggerableProperty (string property)
		{
			return property == "Trigger";
		}
	}
}
