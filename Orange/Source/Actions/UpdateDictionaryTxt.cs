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
			//if (The.UI.AskConfirmation("Are you sure you want to update the dictionary?"))
			bool extractTextWithoutBrackets;
			if (The.UI.AskChoice("Extract text without square brackets from scenes?\n\n(Answer 'No' for World Saga)", out extractTextWithoutBrackets)) {
				DictionaryExtractor extractor = new DictionaryExtractor();
				extractor.ExtractDictionary(extractTextWithoutBrackets);
			}
		}
	}
}
