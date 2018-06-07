using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lime
{
	public class UnpackedAssetBundle : AssetBundle
	{
		public readonly string BaseDirectory;

		public UnpackedAssetBundle(string baseDirectory)
		{
			BaseDirectory = baseDirectory;
		}

		public override Stream OpenFile(string path)
		{
			return new FileStream(Path.Combine(BaseDirectory, path), FileMode.Open, FileAccess.Read);
		}

		public override DateTime GetFileLastWriteTime(string path)
		{
			return File.GetLastWriteTime(Path.Combine(BaseDirectory, path));
		}

		public override int GetFileSize(string path)
		{
			return (int)(new FileInfo(path).Length);
		}

		public override byte[] GetCookingRulesSHA1(string path)
		{
			throw new NotImplementedException();
		}

		public override void DeleteFile(string path)
		{
			File.Delete(Path.Combine(BaseDirectory, path));
		}

		public override bool FileExists(string path)
		{
			return File.Exists(Path.Combine(BaseDirectory, path));
		}

		public override void ImportFile(string path, Stream stream, int reserve, string sourceExtension, AssetAttributes attributes, byte[] cookingRulesSHA1)
		{
			stream.Seek(0, SeekOrigin.Begin);
			var bytes = new byte[stream.Length];
			stream.Read(bytes, 0, bytes.Length);
			var dir = Path.Combine(BaseDirectory, Path.GetDirectoryName(path));
			Directory.CreateDirectory(dir);
			File.WriteAllBytes(Path.Combine(BaseDirectory, path), bytes);
		}

		public override IEnumerable<string> EnumerateFiles(string path = null)
		{
			var baseDirectory = BaseDirectory;
			if (path != null) {
				baseDirectory = Path.Combine(baseDirectory, path);
			}
			baseDirectory += '/';
			var baseUri = new Uri(baseDirectory);
			foreach (var i in Directory.EnumerateFiles(baseDirectory, "*.*", SearchOption.AllDirectories)) {
				var relativePath = baseUri.MakeRelativeUri(new Uri(i)).ToString();
				relativePath = Uri.UnescapeDataString(relativePath);
				yield return relativePath;
			}
		}
	}
}
