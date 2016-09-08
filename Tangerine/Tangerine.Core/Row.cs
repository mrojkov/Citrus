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
		public Uid Uid { get; private set; }
		public int Index { get; set; }

		public Row(Uid uid)
		{
			Uid = uid;
		}
	}
}