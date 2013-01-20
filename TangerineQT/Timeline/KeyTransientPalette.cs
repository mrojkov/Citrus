using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine.Timeline
{
	public static class KeyTransientPalette
	{
		static QColor[] colors = new QColor[] {
			new QColor(0x606060),
			new QColor(0xFF3030),
			new QColor(0xFF8020),
			new QColor(0xFFFF40),
			new QColor(0x00FF00),
			new QColor(0x00FFFF),
			new QColor(0xFF40FF),
			new QColor(0x8080FF),
			new QColor(0x4040FF),
			new QColor(0x2020FF),
			new QColor(0x808080),
		};

		public static QColor GetColor(int index)
		{
			return colors[index % colors.Length];
		}
	}
}
