using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public static class StringExtensions
	{
		public static string Format(this string format, params object[] args)
		{
			return string.Format(format, args);
		}

		public static string Localize(this string text)
		{
			return Localization.GetString(text);
		}

		public static string Localize(this string format, params object[] args)
		{
			return Localization.GetString(format, args);
		}
	}
}
