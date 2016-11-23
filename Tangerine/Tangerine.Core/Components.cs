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

	public class CommonNodeRow
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

		public CommonNodeRow(Node node)
		{
			Node = node;
		}
	}

	public sealed class NodeRow : CommonNodeRow, IComponent
	{
		public NodeRow(Node node) : base(node) { }
	}

	public sealed class FolderRow : CommonNodeRow, IComponent	
	{
		public FolderEnd FolderEnd { get; set; }
		public FolderBegin FolderBegin { get; private set; }

		public FolderRow(FolderBegin folderBegin) : base(folderBegin)
		{
			FolderBegin = folderBegin;
		}

		public IEnumerable<Node> GetNodes(bool nestedOnly = false)
		{
			var n = Node.Parent.Nodes;
			int a = n.IndexOf(FolderBegin);
			int b = n.IndexOf(FolderEnd);
			if (a < 0 || b < 0) {
				throw new InvalidOperationException();
			}
			var t = nestedOnly ? 1 : 0;
			for (int i = a + t; i <= b - t; i++) {
				yield return n[i];
			}
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
