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
		public NodeVisibility Visibility { get { return Node.EditorState().Visibility; } set { Node.EditorState().Visibility = value; } }
		public bool Locked { get { return Node.EditorState().Locked; } set { Node.EditorState().Locked = value; } }
		public bool Expanded { get { return Node.EditorState().Expanded; } set { Node.EditorState().Expanded = value; } }

		public NodeRow(Node node)
		{
			Node = node;
		}
	}	
}