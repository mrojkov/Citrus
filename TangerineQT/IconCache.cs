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
				string location = GetEntryAssemblyDirectory();
				var path = string.Format("{0}/Icons/{1}.png", location, iconPath);
				icon = new QIcon(path);
				icons[iconPath] = icon;
			}
			return icon;
		}

		private static string GetEntryAssemblyDirectory()
		{
			var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
			string location = System.IO.Path.GetDirectoryName(entryAssembly.Location);
			return location;
		}
	}
}
