using System;
using System.IO;
using System.Collections.Generic;
using Lime;
using Lemon;

namespace Orange
{
	public class AssetsImporter
	{
		public delegate bool Converter (string path, Stream output);
		
		private static void SyncUpdated (string mask, string convertedFileExtension, int reservePercentage, Converter converter)
		{
			AssetsBundle bundle = AssetsBundle.Instance;
			List<string> files = Helpers.GetAllFiles (".", mask);
			foreach (string path in files) {
				string path1 = Helpers.RemovePathPrefix (path);
				string path2 = path1;
				if (convertedFileExtension != "") {
					path2 = Path.ChangeExtension (path2, convertedFileExtension);
				}
				bool exists = bundle.FileExists (path2);
				if (!exists || File.GetLastWriteTime (path) > bundle.GetFileModificationTime (path2)) {
					if (converter != null) {
						using (MemoryStream stream = new MemoryStream ()) {
							try {
								if (converter (path1, stream)) {
									stream.Seek (0, SeekOrigin.Begin);
									int reserve = (int)stream.Length * reservePercentage / 100;
									Console.WriteLine ((exists ? "* " : "+ ") + path2);
									bundle.ImportFile (path2, stream, reserve);
								}
							} catch (System.Exception) {
								Console.WriteLine ("An exception has caught while processing '{0}'", path1);
								throw;
							}
						}
					}
					else {
 						Console.WriteLine ((exists ? "* " : "+ ") + path1);
						using (Stream stream = File.Open (path, FileMode.Open, FileAccess.Read)) {
							int reserve = (int)stream.Length * reservePercentage / 100;
							bundle.ImportFile (path2, stream, reserve);
						}
					}
				}
			}
		}
		
		private static void SyncDeleted (Dictionary <string, string> extensionsMap)
		{
			AssetsBundle bundle = AssetsBundle.Instance;
			var assets = new List<string> ();
			foreach (string path in bundle.EnumerateFiles ()) {
				assets.Add (path);
			}
			foreach (string path in assets) {
				string path1 = Path.Combine (".", path);
				string ext;
				if (extensionsMap.TryGetValue (Path.GetExtension (path1), out ext)) {
					path1 = Path.ChangeExtension (path1, ext);
				}
				if (!File.Exists (path1)) {
					Console.WriteLine ("- " + path);
					bundle.RemoveFile (path);
				}
			}	
		}
	
		public static void ProcessCurrentDirectory (bool rebuild)
		{
			string bundlePath = Path.Combine ("..", "Assets.dat");
			if (rebuild) {
				File.Delete (bundlePath);
			}
			AssetsBundle.Instance.Open (bundlePath, true);
			try {
				SyncUpdated ("*.png", ".raw", 0, (path, output) => {
						string texturePath = Path.Combine (".", Path.ChangeExtension (path, ".texture"));
						if (File.Exists (texturePath)) {
							// No need to import this image since it is a part of texture atlas.
							// Delete texture from bundle if any exists.
							return false;
						}
						TextureCompressor.CompressTexture (path, output);
						return true;
					}
				);
				SyncUpdated ("*.xml", "", 10, null);
				SyncUpdated ("*.texture", "", 10, (path, output) => {
						string imgPath = Path.ChangeExtension (path, ".raw");
						// No need to import this image since it is a part of texture atlas.
						// Delete texture from bundle if any exists.
						if (AssetsBundle.Instance.FileExists (imgPath)) {
							Console.WriteLine ("- " + imgPath);
							AssetsBundle.Instance.RemoveFile (imgPath);
						}
						var s = new FileStream (path, FileMode.Open);
						s.CopyTo (output);	
						return true;
					}
				);
				SyncUpdated ("*.fnt", ".fnt", 10, (path, output) => {
	                    var importer = new FontImporter (path);
				        var font = importer.ParseFont ();
						Serialization.WriteObject<Font> (path, output, font);
						return true;
					}
				);
				SyncUpdated ("*.scene", ".scene", 10, (path, output) => {
	                    var importer = new SceneImporter (path);
				        var scene = importer.ParseNode ();
						var t = scene.GetType ();
						Serialization.WriteObject<Node> (path, output, scene);
						return true;
					}
				);
				var map = new Dictionary <string, string> ();
				map [".raw"] = ".png";
				SyncDeleted (map);
			} finally {
				Console.WriteLine ("Clean bundle up");
				AssetsBundle.Instance.Close ();
			}
		}
		
		public static void ProcessDirectory (string directory, bool rebuild)
		{
			string currentDirectory = System.IO.Directory.GetCurrentDirectory ();
			try {
				System.IO.Directory.SetCurrentDirectory (directory);
				ProcessCurrentDirectory (rebuild);
			} finally {
				System.IO.Directory.SetCurrentDirectory (currentDirectory);
			}
		}
	}
}

