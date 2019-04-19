using System;
using System.Collections.Generic;
using Lime;

using System.Linq;

namespace Tangerine.Core.Components
{
	public sealed class CurveRow : Component
	{
		public Node Node { get; private set; }
		public IAnimator Animator { get; private set; }
		public CurveEditorState State { get; private set; }

		public CurveRow(Node node, IAnimator animator, CurveEditorState state)
		{
			Node = node;
			Animator = animator;
			State = state;
		}
	}

	public sealed class NodeRow : Component
	{
		public Node Node { get; private set; }

		public NodeVisibility Visibility
		{
			get { return Node.EditorState().Visibility; }
			set { Node.EditorState().Visibility = value; }
		}

		public bool Locked
		{
			get { return Node.EditorState().Locked; }
			set { Node.EditorState().Locked = value; }
		}

		public bool Expanded
		{
			get { return Node.EditorState().PropertiesExpanded; }
			set { Node.EditorState().PropertiesExpanded = value; }
		}

		public NodeRow(Node node)
		{
			Node = node;
		}
	}

	public sealed class FolderRow : Component
	{
		public Folder Folder { get; private set; }

		public FolderRow(Folder folder)
		{
			Folder = folder;
		}
	}

	public sealed class PropertyRow : Component
	{
		public Node Node { get; private set; }
		public IAnimator Animator { get; private set; }

		public PropertyRow(Node node, IAnimator animator)
		{
			Node = node;
			Animator = animator;
		}
	}

	public sealed class BoneRow : Component
	{
		public Bone Bone { get; private set; }

		public bool ChildrenExpanded
		{
			get { return Bone.EditorState().ChildrenExpanded; }
			set { Bone.EditorState().ChildrenExpanded = value; }
		}

		public bool HaveChildren { get; set; }

		public BoneRow(Bone bone)
		{
			Bone = bone;
		}
	}

	public sealed class AnimationTrackRow : Component
	{
		public AnimationTrack Track { get; private set; }

		public AnimationTrackRow(AnimationTrack track)
		{
			Track = track;
		}

		public AnimationTrackVisibility Visibility
		{
			get { return Track.EditorState().Visibility; }
			set { Track.EditorState().Visibility = value; }
		}

		public bool Locked
		{
			get { return Track.EditorState().Locked; }
			set { Track.EditorState().Locked = value; }
		}
	}

	[NodeComponentDontSerialize]
	public class TimelineOffset : NodeComponent
	{
		public Vector2 Offset { get; set; }
	}
}
