using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Lime
{
	internal class NodeDebugView
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Node node;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public Node[] Nodes { get { return node.Nodes.ToArray(); } }

		public NodeDebugView(Node node)
		{
			this.node = node;
		}
	}
}
