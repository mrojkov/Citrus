using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	static partial class Actions
	{
		[MenuItem("Update Localization Dictionary")]
		public static void UpdateLocalizationDictionary()
		{
			if (!The.UI.AskConfirmation("Are you sure you want to update the dictionary?")) {
				return;
			}
			DictionaryExtractor extractor = new DictionaryExtractor();
			extractor.ExtractDictionary();
		}
	}
}
