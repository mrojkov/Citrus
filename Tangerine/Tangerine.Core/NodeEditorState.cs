using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lime;

namespace Tangerine
{
	public class NodeEditorState
	{
		readonly Node node;
		public Uid Uid { get; private set; } = Uid.Generate();
		public bool Visible { get { return node.TangerineFlags[0]; } set { node.TangerineFlags[0] = value; } }
		public bool Hidden { get { return node.TangerineFlags[1]; } set { node.TangerineFlags[1] = value; } }
		public bool Expanded { get { return node.TangerineFlags[2]; } set { node.TangerineFlags[2] = value; } }

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