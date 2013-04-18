using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.Timeline
{
	public class Toolbar
	{
		public CustomCanvas Canvas { get; private set; }
		Document doc { get { return The.Document; } }

		public Toolbar()
		{
			Canvas = new CustomCanvas();
			Canvas.MinHeight = doc.RowHeight;
		}
	}
}
