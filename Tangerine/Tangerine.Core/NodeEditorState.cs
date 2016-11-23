using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lime;

namespace Tangerine
{
	public enum NodeVisibility
	{
		Default = 0,
		Hidden = 1,
		Shown = 2,
	}

	public class NodeEditorState
	{
		readonly Node node;
		public Uid Uid { get; private set; } = Uid.Generate();
		public string ThumbnailData { get; set; }
		public NodeVisibility Visibility
		{
			get
			{
				if (node.GetTangerineFlag(TangerineFlags.Shown)) {
					return NodeVisibility.Shown;
				} else if (node.GetTangerineFlag(TangerineFlags.Hidden)) {
					return NodeVisibility.Hidden;
				} else {
					return NodeVisibility.Default;
				}
			}
			set
			{
				node.SetTangerineFlag(TangerineFlags.Shown, value == NodeVisibility.Shown);
				node.SetTangerineFlag(TangerineFlags.Hidden, value == NodeVisibility.Hidden);
			}
		}
		public bool Locked { get { return node.GetTangerineFlag(TangerineFlags.Locked); } set { node.SetTangerineFlag(TangerineFlags.Locked, value); } }
		public bool Expanded { get { return node.GetTangerineFlag(TangerineFlags.Expanded); } set { node.SetTangerineFlag(TangerineFlags.Expanded, value); } }

		public NodeEditorState(Node node)
		{
			this.node = node;
		}
	}

	public static class NodeExtensions
	{
		public static NodeEditorState EditorState(this Node node)
		{
			if (node.UserData == null) {
				node.UserData = new NodeEditorState(node);
			}
			return (NodeEditorState)node.UserData;
		}

		public static int CollectionIndex(this Node node)
		{
			return node.Parent.Nodes.IndexOf(node);
		}
	}
}