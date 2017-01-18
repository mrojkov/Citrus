using System.ComponentModel.Composition;

namespace Orange
{
	static partial class Actions
	{
		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Update Localization Dictionary")]
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
