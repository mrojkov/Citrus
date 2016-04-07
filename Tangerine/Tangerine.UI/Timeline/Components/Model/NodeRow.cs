using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Components
{
	public class NodeRow : IComponent
	{
		public Node Node { get; private set; }
		public bool Visible { get { return Node.EditorState().Visible; } set { Node.EditorState().Visible = value; } }
		public bool Hidden { get { return Node.EditorState().Hidden; } set { Node.EditorState().Hidden = value; } }
		public bool Expanded { get { return Node.EditorState().Expanded; } set { Node.EditorState().Expanded = value; } }

		public NodeRow(Node node)
		{
			Node = node;
		}
	}	
}