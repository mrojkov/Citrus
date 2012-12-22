using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	public static class KeyTransientPalette
	{
		static QColor[] colors = new QColor[] {
			new QColor(255, 0, 0),
			new QColor(192, 64, 0),
			new QColor(128, 128, 0),
			new QColor(64, 192, 0),
			new QColor(0, 255, 0),
			new QColor(0, 192, 64),
			new QColor(0, 128, 128),
			new QColor(0, 64, 192),
			new QColor(0, 0, 255),
		};

		public static QColor GetColor(int index)
		{
			return colors[index % colors.Length];
		}
	}
}
