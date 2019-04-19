using System.IO;

namespace Tangerine.Core
{
	public class ProjectLocalization
	{
		public readonly string Code;
		public readonly string DictionaryPath;

		public ProjectLocalization(string code, string dictionaryPath)
		{
			Code = code;
			DictionaryPath = dictionaryPath;
		}

		public void Apply()
		{
			Lime.Localization.Dictionary.Clear();
			try {
				using (var stream = new FileStream(DictionaryPath, FileMode.Open)) {
					Lime.Localization.Dictionary.ReadFromStream(stream);
				}
				System.Console.WriteLine($"Localization was successfully loaded from \"{DictionaryPath}\"");
			} catch (System.Exception exception) {
				System.Console.WriteLine($"Can not read localization from \"{DictionaryPath}\": {exception.Message}");
			}
		}
	}
}
