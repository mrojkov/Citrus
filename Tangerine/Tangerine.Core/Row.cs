using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lime;
using Tangerine.Core;

namespace Tangerine.Core
{
	public class Row : Entity
	{
		public int Index { get; set; }
		public Row Parent { get; set; }
		public bool Selected => SelectedAtUpdate > 0;
		public long SelectedAtUpdate { get; set; }

		public int CalcIndentation()
		{
			int i = 0;
			for (var r = Parent; r != null; r = r.Parent) {
				i++;
			}
			return i;
		}
	}
}