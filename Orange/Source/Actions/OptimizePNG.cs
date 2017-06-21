#if WIN
using System;
using System.ComponentModel.Composition;

namespace Orange
{
	static partial class Actions
	{
		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Optimize PNGs")]
		[ExportMetadata("Priority", 51)]
		public static void OptimizePNG()
		{
			// f for format file size
			Func<ulong, string> f = (ulong size) => {
				double dsize = size;
				string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
				int order = 0;
				while (size >= 1024 && order < suffixes.Length - 1) {
					order++;
					size /= 1024;
					dsize /= 1024.0;
				}
				return $"{dsize:0.##} {suffixes[order]}";
			};
			ulong totalLengthAfter = 0;
			ulong totalLengthBefore = 0;
			using (new DirectoryChanger(The.Workspace.AssetsDirectory)) {
				foreach (var fi in The.Workspace.AssetFiles.Enumerate(".png")) {
					ulong lengthBefore = (ulong)(new System.IO.FileInfo(fi.Path)).Length;
					totalLengthBefore += lengthBefore;
					TextureConverter.OptimizePNG(fi.Path);
					ulong lengthAfter = (ulong)(new System.IO.FileInfo(fi.Path)).Length;
					totalLengthAfter += lengthAfter;
					Console.WriteLine($"{fi.Path} : {f(lengthBefore)} => {f(lengthAfter)}, diff: {f(lengthBefore - lengthAfter)}");
				}
			}
			Console.WriteLine($"Totals: {f(totalLengthBefore)} => {f(totalLengthAfter)}, diff: {f(totalLengthBefore - totalLengthAfter)}");
		}
	}
}
#endif // WIN
