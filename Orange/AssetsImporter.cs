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
		
		private static string GetAssetPath (string path)
		{
			string path1 = path.Remove (0, 2); // './'
			return path1;
		}
						
		private static void SyncUpdated (string mask, string convertedFileExtension, Converter converter)
		{
			AssetsBundle bundle = AssetsBundle.Instance;
			List<string> files = Helpers.GetAllFiles (".", mask);
			foreach (string path in files) {
				string path1 = GetAssetPath (path);
				string path2 = path1;
				if (convertedFileExtension != "") {
					path2 = Path.ChangeExtension (path2, convertedFileExtension);
				}
				bool exists = bundle.FileExists (path2);
				if (!exists || File.GetLastWriteTime (path) > bundle.GetFileModificationTime (path2)) {
					if (converter != null) {
						Console.WriteLine ((exists ? "Updating: " : "Adding: ") + path2);
						using (MemoryStream stream = new MemoryStream ()) {
							if (converter (path1, stream)) {
								stream.Seek (0, SeekOrigin.Begin);
								bundle.ImportFile (path2, stream);								
							}
						}
					}
					else {
 						Console.WriteLine ((exists ? "Updating: " : "Adding: ") + path1);
						bundle.ImportFile (path1);						
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
				string path1 = "./" + path;
				string ext;
				if (extensionsMap.TryGetValue (Path.GetExtension (path1), out ext)) {
					path1 = Path.ChangeExtension (path1, ext);
				}
				if (!File.Exists (path1)) {
					bundle.RemoveFile (path);
					Console.WriteLine ("Deleted: " + path);
				}
			}	
		}
	
		public static void ProcessCurrentDirectory ()
		{
			AssetsBundle.Instance.Open ("../Assets.dat", true);
			SyncUpdated ("*.png", ".raw", (path, output) => {
					if (File.Exists (Path.ChangeExtension (path, ".texture"))) {
						// No need to import this image since it is a part of texture atlas
						return false;	
					}					
					TextureCompressor.CompressTexture (path, output);
					return true;
				}
			);
			SyncUpdated ("*.xml", "", null);
			SyncUpdated ("*.texture", "", null);
			SyncUpdated ("*.fnt", ".fnt", (path, output) => {
                    var importer = new FontImporter (path);
			        var font = importer.ParseFont ();
					Serialization.WriteObject<Font> (path, output, font);
					return true;
				}
			);
			SyncUpdated ("*.scene", ".scene", (path, output) => {
                    var importer = new SceneImporter (path);
			        var scene = importer.ParseNode ();
					Serialization.WriteObject<Node> (path, output, scene);
					return true;
				}
			);
			var map = new Dictionary <string, string> ();
			map [".raw"] = ".png";
			SyncDeleted (map);
			AssetsBundle.Instance.Close ();
		}
		
		public static void ProcessDirectory (string directory)
		{
			string currentDirectory = System.IO.Directory.GetCurrentDirectory ();
			try {
				System.IO.Directory.SetCurrentDirectory (directory);
				ProcessCurrentDirectory ();
			} finally {
				System.IO.Directory.SetCurrentDirectory (currentDirectory);
			}
		}
	}
}

