using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.Core.Components
{
	public sealed class CurveRow : IComponent
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

	public class NodeRow : IComponent
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
			get { return Node.EditorState().Expanded; }
			set { Node.EditorState().Expanded = value; }
		}

		public NodeRow(Node node)
		{
			Node = node;
		}
	}

	public sealed class FolderRow : IComponent
	{
		public Folder Folder { get; private set; }

		public FolderRow(Folder folder)
		{
			Folder = folder;
		}
	}

	public sealed class PropertyRow : IComponent
	{
		public Node Node { get; private set; }
		public IAnimator Animator { get; private set; }

		public PropertyRow(Node node, IAnimator animator)
		{
			Node = node;
			Animator = animator;
		}
	}
}
