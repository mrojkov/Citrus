using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine
{
	public class AlignPreferences
	{

		public AlignObject AlignObject { get; set; } = AlignObject.Selection;
		public int Spacing { get; set; } = 5;

		public static AlignPreferences Instance = new AlignPreferences();

	}
}
