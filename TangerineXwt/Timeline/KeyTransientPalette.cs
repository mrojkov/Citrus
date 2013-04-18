using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xwt.Drawing;

namespace Tangerine.Timeline
{
	public static class KeyTransientPalette
	{
		static Color[] colors = new Color[] {
			Color.FromName("#606060"),
			Color.FromName("#FF3030"),
			Color.FromName("#FF8020"),
			Color.FromName("#FFFF40"),
			Color.FromName("#00FF00"),
			Color.FromName("#00FFFF"),
			Color.FromName("#FF40FF"),
			Color.FromName("#8080FF"),
			Color.FromName("#4040FF"),
			Color.FromName("#2020FF"),
			Color.FromName("#808080"),
		};

		public static Color GetColor(int index)
		{
			return colors[index % colors.Length];
		}
	}
}
