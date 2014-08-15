using System;
#if iOS
using MonoTouch.Foundation;
#endif

namespace Lime
{
	public static class Localization
	{
		private static bool useNumericKeys;
		/// <summary>
		/// Use this flag only for compatibility reasons
		/// </summary>
		[Obsolete]
		public static bool UseNumericKeys { get { return useNumericKeys; } set { useNumericKeys = value; } }

		public static LocalizationDictionary Dictionary = new LocalizationDictionary();

		/// <summary>
		/// This will return a two letter code for the currently selected language.
		/// "en" for English, "es" for Spanish, "de" for German, etc.
		/// For more examples, please see this Wikipedia entry (in particular, the 639-1 column):
		/// http://en.wikipedia.org/wiki/List_of_ISO_639-1_codes
		/// </summary>
		public static string GetCurrentLanguage()
		{
#if iOS
			return NSLocale.PreferredLanguages[0];
#else
			return System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
#endif
		}

		public static string GetString(string format, params object[] args)
		{
			string s = GetString(format);
			for (int i = 0; i < args.Length; i++) {
				if (args[i] is string) {
					args[i] = GetString((string)args[i]);
				}
			}
			return string.Format(s, args);
		}

		public static string GetString(string key)
		{
			if (string.IsNullOrEmpty(key)) {
				return key;
			}
			if (useNumericKeys) {
				return GetStringForNumericKey(key);
			} else {
				return GetStringHelper(key);
			}
		}

		private static string GetStringForNumericKey(string taggedString)
		{
			if (taggedString[0] == '[') {
				int closeBrackedPos = 0;
				for (int i = 1; i < taggedString.Length; i++) {
					if (taggedString[i] == ']') {
						closeBrackedPos = i;
						break;
					}
					if (!char.IsDigit(taggedString, i)) {
						break;
					}
				}
				if (closeBrackedPos >= 1) {
					string text;
					if (closeBrackedPos > 1) {
						var key = taggedString.Substring(1, closeBrackedPos - 1);
						if (Dictionary.TryGetText(key, out text)) {
							return text;
						}
					}
					// key/value pair not defined or key is empty ("[]" case).
					text = taggedString.Substring(closeBrackedPos + 1);
					return text;
				}
			}
			return taggedString;
		}

		private static string GetStringHelper(string key)
		{
			if (key.Length >= 2 && key[0] == '[' && key[1] == ']') {
				key = key.Substring(2);
			}
			string text;
			return Dictionary.TryGetText(key, out text) ? text : key;
		}
	}
}