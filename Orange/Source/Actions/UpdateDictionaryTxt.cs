using System.ComponentModel.Composition;

namespace Orange
{
	static partial class Actions
	{
		[Export(nameof(OrangePlugin.MenuItemsWithErrorDetails))]
		[ExportMetadata("Label", "Update Localization Dictionary")]
		public static string UpdateLocalizationDictionary()
		{
			if (!The.UI.AskConfirmation("Are you sure you want to update the dictionary?")) {
				return "Action canceled";
			}
			DictionaryExtractor extractor = new DictionaryExtractor();
			extractor.ExtractDictionary();
			return null;
		}
	}
}
