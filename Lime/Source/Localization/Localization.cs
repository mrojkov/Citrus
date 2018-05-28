using System;
#if iOS
using Foundation;
#endif

namespace Lime
{
	/// <summary>
	/// Класс, предоставляющий функции локализации
	/// </summary>
	public static class Localization
	{
		private static bool useNumericKeys;

		/// <summary>
		/// Устаревший. Использовать только для совместимости со старыми проектами.
		/// Раньше в словаре в качестве ключей использовались числа. Сейчас - вся строка. Этот флаг включает старый режим
		/// </summary>
		public static bool UseNumericKeys { get { return useNumericKeys; } set { useNumericKeys = value; } }

		public static bool DebugKeys { get; set; }
		/// <summary>
		/// Текущий словарь локализации
		/// </summary>
		public static LocalizationDictionary Dictionary = new LocalizationDictionary();

		/// <summary>
		/// Возвращает две буквы для имени текущего языка.
		/// Например "en" для English, "es" для Spanish, "de" для Deutch и т.п.
		/// Для более подробной информации см ссылку (особенно раздел 639-1)
		/// http://en.wikipedia.org/wiki/List_of_ISO_639-1_codes
		/// </summary>
		public static string GetCurrentLanguage()
		{
#if iOS
			string language = NSLocale.PreferredLanguages[0];
			language = language.Substring(0, 2);
				return language;
#else
			return System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
#endif
		}

		/// <summary>
		/// Возвращает локализованную строку из текущего словаря по ее ключу
		/// </summary>
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

		/// <summary>
		/// Возвращает локализованную строку из текущего словаря по ее ключу
		/// </summary>
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
							if (DebugKeys) {
								text = "#[" + key + "]" + text;
							}
							return text;
						}
					}
					// key/value pair not defined or key is empty ("[]" case).
					text = DebugKeys ? "#" + taggedString : taggedString.Substring(closeBrackedPos + 1);
					return text;
				}
			}
			return taggedString;
		}

		private static string GetStringHelper(string key)
		{
			if (key.Length == 0 || key[0] != '[') {
				return key;
			}
			if (key.Length >= 2 && key[0] == '[' && key[1] == ']') {
				key = key.Substring(2);
			}
			string text;
			if (Dictionary.TryGetText(key, out text)) {
				return text;
			}
			// Leave selector in debug build to help translators identify string from the UI.
#if DEBUG
			return key;
#else
			if (key.Length > 0 && key[0] != '[') {
				return key;
			}
			int index = key.IndexOf(']');
			if (index != -1) {
				return key.Substring(+1);
			}
			return key;
#endif
		}
	}
}
