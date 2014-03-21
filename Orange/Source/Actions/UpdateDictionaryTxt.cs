using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	static partial class Actions
	{
		[MenuItem("Update Dictionary.txt")]
		public static void UpdateDictionaryTxt()
		{
			if (!The.UI.AskConfirmation("Are you sure you want to update the dictionary?")) {
				return;
			}
			DictionaryExtractor extractor = new DictionaryExtractor();
			extractor.ExtractDictionary();
		}
	}
}
