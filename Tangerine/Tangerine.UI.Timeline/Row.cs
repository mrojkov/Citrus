using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class Row : Entity
	{
		public Uid Uid { get; private set; }
		public int Index { get; set; }

		public float Top => Components.Get<Components.IGridWidget>().Widget.Y;
		public float Bottom => Top + Height;
		public float Height => Components.Get<Components.IGridWidget>().Widget.Height;

		public Row(Uid uid)
		{
			Uid = uid;
		}
	}
}