using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tangerine
{
	public static class TimelineRowListExtension
	{
		public static bool AreEqual(this List<TimelineRow> rows1, List<TimelineRow> rows2)
		{
			if (rows1.Count != rows2.Count) {
				return false;
			}
			for (int i = 0; i < rows2.Count; i++) {
				if (!rows1[i].Equals(rows2[i])) {
					return false;
				}
			}
			return true;
		}
	}
}
