using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	static partial class Actions
	{
		[MenuItem("Update Localization Dictionary (old format)")]
		public static void UpdateLocalizationDictionaryOldFormat()
		{
			var extractor = new DictionaryOldFormatExtractor();
			extractor.ExtractDictionary();
		}
	}
}
