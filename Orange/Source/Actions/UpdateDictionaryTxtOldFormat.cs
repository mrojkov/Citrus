#if ORANGE_GUI
using System.ComponentModel.Composition;

namespace Orange
{
	static partial class Actions
	{
		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Update Localization Dictionary (old format)")]
		public static void UpdateLocalizationDictionaryOldFormat()
		{
			var extractor = new DictionaryOldFormatExtractor();
			extractor.ExtractDictionary();
		}
	}
}
#endif // ORANGE_GUI
