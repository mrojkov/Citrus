using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public static class Toolbox
	{
		public static QImage LoadIcon(string iconName)
		{
			return new QImage(string.Format("Icons/{0}.png", iconName));
		}
	}
}
