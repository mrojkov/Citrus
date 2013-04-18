using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine
{
	public static class IconCache
	{
		static Dictionary<string, Xwt.Drawing.Image> icons = new Dictionary<string, Xwt.Drawing.Image>();

		public static Xwt.Drawing.Image Get(string iconPath)
		{
			Xwt.Drawing.Image icon;
			if (!icons.TryGetValue(iconPath, out icon)) {
				string location = GetEntryAssemblyDirectory();
				var path = string.Format("{0}/Icons/{1}.png", location, iconPath);
				icon = Xwt.Drawing.Image.FromFile(path);
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
