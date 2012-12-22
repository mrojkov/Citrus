using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qyoto;

namespace Tangerine
{
	/// <summary>
	/// Этот класс необходим чтобы ускорить отрисовку текста и избавится от утечек памяти в Qyoto
	/// </summary>
	public class CachedTextPainter
	{
		Dictionary<string, QStaticText> cache = new Dictionary<string, QStaticText>();

		public QStaticText this[string text]
		{
			get
			{
				QStaticText result;
				if (!cache.TryGetValue(text, out result)) {
					result = new QStaticText(text);
					cache[text] = result;
				}
				return result;
			}
		}

		public void Draw(QPainter ptr, int left, int top, string text)
		{
			ptr.DrawStaticText(left, top, this[text]);
		}
	}
}
