using System.Runtime.Serialization;
using Lime;
using System.Collections.Generic;
using System.IO;
using System;
using ProtoBuf;

namespace Lemon
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
		
		#region Properties
        [ProtoMember(1)]
		public string Id { get; set; }

        [ProtoMember(2)]
		public string ContentsPath { get; set; }
		
        [ProtoMember(3)]
		public string Trigger { 
			get { return trigger; } 
			set { 
				trigger = value;
				if (Animator.DidKeyFrameLeap) {
					if (string.IsNullOrEmpty (value)) {
						CurrentTime = 0;
						Playing = true;
					} else					
						PlayAnimation (value);
				}
			} 
		}

		private string trigger;

		public virtual Node Parent { get { return parent; } set { parent = value; } }
		private Node parent;

		public Widget Widget;

		[ProtoMember(5)]
		public readonly AnimatorCollection Animators = new AnimatorCollection ();

		[ProtoMember(6)]
		public readonly NodeCollection Nodes = new NodeCollection ();

		[ProtoMember(7)]
		public readonly MarkerCollection Markers = new MarkerCollection ();

		[ProtoMember(9)]
		public bool Playing {
			get { return playing; }
			set {
				playing = value;
				if (playing)
					Update (0);
			}
		}

		[ProtoMember(10)]
		public int CurrentTime {
			get { return currentTime; }
			set {
				currentTime = value;
				int count = Nodes.Count;
				for (int i = 0; i < count; i++) {
					Nodes [i].Animators.Apply (currentTime);
				}
			}
		}
		private int currentTime;
		#endregion
		
		#region Methods
		private static List<Node> killList = new List<Node> ();

		public Node ()
		{
			Animators.Owner = this;
			Nodes.Owner = this;
		}
		
		static HashSet<string> processingFiles = new HashSet<string> ();

		public static Node CreateFromBundle (string path)
		{
			if (processingFiles.Contains (path))
				throw new Lime.Exception ("Cyclic dependency of scenes has detected: {0}", path);
			Node node;
			processingFiles.Add (path);
			try {
				node = Serialization.ReadObjectFromBundle<Node> (path);
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

		private bool playing = false;

		public void PlayAnimation (string markerId)
		{
			Marker marker = Markers.Find (markerId);
			if (marker == null) {
				// System.Diagnostics.Debug.WriteLine ("WARNING: Attempt to play animation. Unknown marker '{0}' in node '{1}'", markerId, Id);
				return;
			}
			CurrentFrame = marker.Frame;
			Playing = true;
		}

		public int CurrentFrame {
			get { return WidgetUtils.MillisecondsToFrame (CurrentTime); }
			set { CurrentTime = WidgetUtils.FrameToMilliseconds (value); }
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
		
		public void Kill ()
		{
			killList.Add (this);
		}
		
		private void ProcessMarker ()
		{
			Marker marker = Markers.FindByFrame (CurrentFrame);
			if (marker != null) {
				switch (marker.Action) {
				case MarkerAction.Jump:
					{
						var gotoMarker = Markers.Find (marker.JumpTo);
						if (gotoMarker != null)
							CurrentTime = WidgetUtils.FrameToMilliseconds (gotoMarker.Frame);
						break;
					}
				case MarkerAction.Stop:
					{
						CurrentTime = WidgetUtils.RoundMillisecondsToFrame (CurrentTime);
						Playing = false;
						break;
					}
				case MarkerAction.Destroy:
					{
						Playing = false;
						Kill ();
						break;
					}
				}
			}
		}

		protected void AnimateChildren (int delta)
		{
			int frameBefore = CurrentFrame;
			CurrentTime = CurrentTime + delta;
			if (frameBefore != CurrentFrame && Markers.Count > 0) {
				ProcessMarker ();
			}
		}

		// Delta must be in [0..1000 / WidgetUtils.FramesPerSecond - 1] range
		public virtual void Update (int delta)
		{
			UpdatedNodes++;
			if (Playing)
				AnimateChildren (delta);
			for (int i = Nodes.Count - 1; i >= 0; i--)
				Nodes [i].Update (delta);
		}

		public void SafeUpdate (int delta)
		{
			if (delta < 0)
				throw new Lime.Exception ("Update interval can not be negative");		
			const int step = 1000 / WidgetUtils.FramesPerSecond - 1;
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
		#endregion
	}
}
