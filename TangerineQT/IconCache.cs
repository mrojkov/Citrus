using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public static class IconCache
	{
		static Dictionary<string, QIcon> icons = new Dictionary<string, QIcon>();

		public static QIcon Get(string iconPath)
		{
			QIcon icon;
			if (!icons.TryGetValue(iconPath, out icon)) {
				icon = new QIcon(string.Format("Icons/{0}.png", iconPath));
				icons[iconPath] = icon;
			}
			return icon;
		}
	}
}
