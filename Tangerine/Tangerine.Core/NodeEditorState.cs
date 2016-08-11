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
		Hide = 1,
		Show = 2,
	}

	public class NodeEditorState
	{
		readonly Node node;
		public Uid Uid { get; private set; } = Uid.Generate();
		public NodeVisibility Visibility
		{
			get
			{
				if (node.TangerineFlags[0]) {
					return NodeVisibility.Show;
				} else if (node.TangerineFlags[1]) {
					return NodeVisibility.Hide;
				} else {
					return NodeVisibility.Default;
				}
			}
			set
			{
				node.TangerineFlags[0] = value == NodeVisibility.Show;
				node.TangerineFlags[1] = value == NodeVisibility.Hide;
			}
		}
		public bool Locked { get { return node.TangerineFlags[2]; } set { node.TangerineFlags[2] = value; } }
		public bool Expanded { get { return node.TangerineFlags[3]; } set { node.TangerineFlags[3] = value; } }

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
	}
}