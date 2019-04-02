using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lime
{
	public static class Pluralizer
	{
		public static int GetPlural(object obj)
		{
			if (obj is int i) {
				return GetPlural(Math.Abs(i));
			} else if (obj is float f) {
				return GetPlural(Math.Abs((int)f));
			}
			return 0;
		}

		public static int GetPlural(int n)
		{
			switch (Application.CurrentLanguage) {
				case "EN":
				case "BR":
				case "DE":
				case "FR":
				case "ES":
				case "IT":
				case "NL":
					return n > 1 ? 1 : 0;
				case "RU":
					if (n % 10 == 1 && n % 100 != 11) {
						return 0;
					} else if (n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20)) {
						return 1;
					} else {
						return 2;
					}
				case "JP":
				case "CN":
				case "TW":
					return 0;
				default:
					return 0;
			}
		}

		private static string GetPluralStringFromTemplate(string s, object[] args)
		{
			int argIndex = 0;
			string casesString = s;
			var casesStart = s.IndexOf(':');
			if (casesStart != -1 && casesStart < s.Length - 1) {
				if (!int.TryParse(s.Substring(0, casesStart), out argIndex)) {
					return s;
				}
				casesString = s.Substring(casesStart + 1);
			}
			var plural = 0;
			if (argIndex < args.Length) {
				plural = GetPlural(args[argIndex]);
			}

			var cases = casesString.Split('|');

			return cases[plural < cases.Length ? plural : 0];
		}

		public static string Pluralize(string s, params object[] args)
		{
			var result = s;
			var startIndex = result.IndexOf('[');
			while (startIndex != -1) {
				var endIndex = result.IndexOf(']', startIndex);
				if (endIndex == -1) {
					break;
				}
				var pluralTemplate = result.Substring(startIndex + 1, endIndex - startIndex - 1);
				result = result.Replace($"[{pluralTemplate}]", GetPluralStringFromTemplate(pluralTemplate, args));
				startIndex = result.IndexOf('[');
			}
			return result;
		}
	}
}
