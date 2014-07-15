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
			if (!The.UI.AskConfirmation("Are you sure you want to update the dictionary?")) {
				return;
			}
			var extractor = new DictionaryOldFormatExtractor();
			extractor.ExtractDictionary();
		}
	}
}
