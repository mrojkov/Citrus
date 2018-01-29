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

		public static string GetTemporalFilePath(string path)
		{
			return Path.Combine(Path.GetDirectoryName(path), Prefix + Path.GetFileName(path));
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
					if (document.IsModified) {
						var path = GetTemporalFilePath(document.Path);
						document.SaveTo(path, System.IO.FileAttributes.Hidden);
					}
				}
			}
		}
	}
}
