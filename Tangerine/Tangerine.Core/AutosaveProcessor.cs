using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;

namespace Tangerine.Core
{
	public class AutosaveProcessor : ITaskProvider
	{
		public static readonly string Prefix = "temp_";

		private readonly Func<int> delayGetter;

		public static string GetTemporaryFilePath(string path)
		{
			return Path.Combine(Lime.Environment.GetDataDirectory("Tangerine"), Prefix + Path.GetFileName(path));
		}

		public AutosaveProcessor(Func<int> delayGetter)
		{
			this.delayGetter = delayGetter;
		}

		public IEnumerator<object> Task()
		{
			while (true) {
				yield return delayGetter();
				foreach (var document in Project.Current.Documents) {
					if (document.Loaded && document.IsModified) {
						var filePath = $"{GetTemporaryFilePath(document.Path)}.{document.GetFileExtension()}";
						try {
							document.ExportToFile(filePath, document.Path, FileAttributes.Hidden);
						} catch (Exception e) {
							Console.WriteLine("Error on autosave document '{0}':\n{1}", document.Path, e);
						}
					}
				}
			}
		}
	}
}
