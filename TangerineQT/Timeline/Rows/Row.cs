using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Tangerine.Timeline
{
	/// <summary>
	/// Абстрактная строка на таймлайне
	/// </summary>
	[ProtoContract]
	[ProtoInclude(101, typeof(NodeRow))]
	public abstract class Row
	{
		private RowView view;

		[ProtoMember(1)]
		public string Guid { get; protected set; }

		public int Index { get; set; }

		public abstract int GetLastKeyframeColumn();

		public RowView View
		{
			get {
				if (view == null) {
					view = CreateView();
				}
				return view;
			}
		}

		protected abstract RowView CreateView();
	}
}
