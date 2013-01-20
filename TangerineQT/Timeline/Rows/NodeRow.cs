using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Tangerine.Timeline
{
	/// <summary>
	/// Строка представляющая один нод на таймлайне
	/// </summary>
	public class NodeRow : Row
	{
		public Lime.Node Node { get; private set; }

		[ProtoMember(1)]
		public bool Visible;

		[ProtoMember(2)]
		public bool Hidden;

		public NodeRow(Lime.Node node)
		{
			this.Node = node;
		}

		public override int GetLastKeyframeColumn()
		{
			int numCols = 0;
			foreach (var ani in Node.Animators) {
				numCols = Math.Max(numCols, ani.Duration);
			}
			return numCols;
		}

		protected override RowView CreateView()
		{
			return new NodeView(this);
		}
	}
}
