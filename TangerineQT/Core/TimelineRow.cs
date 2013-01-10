using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Tangerine
{
	/// <summary>
	/// Абстрактная строка на таймлайне
	/// </summary>
	[ProtoContract]
	[ProtoInclude(101, typeof(TimelineNodeRow))]
	public abstract class TimelineRow
	{
		private TimelineRowView view;

		[ProtoMember(1)]
		public string Guid { get; protected set; }

		public int Index { get; set; }

		public abstract int GetLastKeyframeColumn();

		public TimelineRowView View
		{
			get {
				if (view == null) {
					view = CreateView();
				}
				return view;
			}
		}

		protected abstract TimelineRowView CreateView();
	}

	/// <summary>
	/// Строка представляющая один нод на таймлайне
	/// </summary>
	public class TimelineNodeRow : TimelineRow
	{
		public Lime.Node Node { get; private set; }

		[ProtoMember(1)]
		public bool Visible;

		[ProtoMember(2)]
		public bool Hidden;

		public TimelineNodeRow(Lime.Node node)
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

		protected override TimelineRowView CreateView()
		{
			return new TimelineNodeView(this);
		}
	}
}
