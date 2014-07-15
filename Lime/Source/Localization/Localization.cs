
namespace Lime
{
	public static class Localization
	{
		public static LocalizationDictionary Dictionary = new LocalizationDictionary();
		
		public static string GetCurrentLanguage()
		{
			return System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
		}

		public static string GetString(string format, params object[] args)
		{
			string s = GetString(format);
			return string.Format(s, args);
		}

		public static string GetString(string key)
		{
			if (string.IsNullOrEmpty(key)) {
				return key;
			}
			if (key.Length >= 2 && key[0] == '[' && key[1] == ']') {
				key = key.Substring(2);
			}
			string text;
			return Dictionary.TryGetText(key, out text) ? text : key;
		}
	}
}