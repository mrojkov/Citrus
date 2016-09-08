using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lime;
using Tangerine.Core;

namespace Tangerine.Core.Components
{
	public class CurveRow : IComponent
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
}