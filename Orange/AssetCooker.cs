using System;
using System.IO;
using System.Collections.Generic;

namespace Orange
{
	public class AssetCooker
	{
		private delegate bool Converter (string srcPath, string dstPath);
	
		private Lime.AssetsBundle AssetsBundle = Lime.AssetsBundle.Instance;
		private string assetsDirectory;
		private TargetPlatform platform;
		private Dictionary<string, CookingRules> cookingRulesMap;
		
		string GetOriginalAssetExtension (string path)
		{
			switch (Path.GetExtension (path)) {
			case ".dds":
			case ".raw":
			case ".pvr":
			case ".atlasPart":
				return ".png";
			default:
				return Path.GetExtension (path);
			}
		}
		
		string BundlePathToNative (string path)
		{
			return path.Replace ('/', Path.DirectorySeparatorChar);			
		}
		
		string GetPlatformTextureExtension ()
		{
			if (platform == TargetPlatform.iOS)
				return ".pvr";
			else
				return ".dds";
		}
		
		public AssetCooker (string assetsDirectory, TargetPlatform platform)
		{
			this.platform = platform;
			this.assetsDirectory = assetsDirectory;
		}
		
		public void Cook (bool rebuild)
		{
			cookingRulesMap = CookingRulesBuilder.Build (assetsDirectory);
			string bundlePath = Path.ChangeExtension (assetsDirectory, Helpers.GetTargetPlatformString (platform));
			if (rebuild && File.Exists (bundlePath)) {
				File.Delete (bundlePath);
			}
			AssetsBundle.Open (bundlePath, true);
			try {
				using (new DirectoryChanger (assetsDirectory)) {
					Console.WriteLine ("Synchronizing {0}", bundlePath);
					SyncAtlases ();
					SyncDeleted ();	
					SyncUpdated ("*.png", GetPlatformTextureExtension (), (srcPath, dstPath) => {
						CookingRules rules = cookingRulesMap [Path.ChangeExtension (dstPath, ".png")];
						if (rules.TextureAtlas != null) {
							// No need to cache this texture since it is a part of texture atlas.
							return false;
						}
						Helpers.CreateDirectoryRecursive (Path.GetDirectoryName (dstPath));
						string tmpFile = Path.ChangeExtension (srcPath, GetPlatformTextureExtension ());
						TextureConverter.Convert (srcPath, tmpFile, rules.Compression, rules.MipMaps, platform);
						AssetsBundle.ImportFile (tmpFile, dstPath, 0);
						File.Delete (tmpFile);
						return true;
					});
					SyncUpdated ("*.xml", ".xml", null);
					SyncUpdated ("*.fnt", ".fnt", (srcPath, dstPath) => {
						string fontPngFile = Path.ChangeExtension (srcPath, ".png");
						Lime.Size size;
						bool hasAlpha;
						if (!TextureConverter.GetPngFileInfo (fontPngFile, out size.Width, out size.Height, out hasAlpha)) {
							throw new Lime.Exception ("Font doesn't have an appropriate png texture file");
						}
						var importer = new FontImporter (srcPath);
						var font = importer.ParseFont (size);
						font.Texture = new Lime.PersistentTexture (Path.ChangeExtension (dstPath, null));
						Helpers.CreateDirectoryRecursive (Path.GetDirectoryName (dstPath));
						Lime.Serialization.WriteObjectToBundle<Lime.Font> (AssetsBundle, dstPath, font);
						return true;
					});
					SyncUpdated ("*.scene", ".scene", (srcPath, dstPath) => {
						var importer = new SceneImporter (srcPath);
						var node = importer.ParseNode ();
						Helpers.CreateDirectoryRecursive (Path.GetDirectoryName (dstPath));
						Lime.Serialization.WriteObjectToBundle<Lemon.Node> (AssetsBundle, dstPath, node);
						return true;
					});
				}
			} finally {
				AssetsBundle.Close ();
			}
		}
	
		void SyncDeleted ()
		{
			var assetsFiles = new HashSet<string> ();
			using (new DirectoryChanger (assetsDirectory)) {
				foreach (string path in Helpers.GetAllFiles (".", "*.*", true)) {
					assetsFiles.Add (path);
				}
			}
			foreach (string path in AssetsBundle.EnumerateFiles ()) {
				// Ignoring texture atlases
				if (path.StartsWith ("Atlases")) {
					continue;
				}
				// Ignore atlas parts
				if (Path.GetExtension (path) == ".atlasPart") {
					continue;
				}
				string assetPath = Path.ChangeExtension (path, GetOriginalAssetExtension (path));
				if (!assetsFiles.Contains (BundlePathToNative (assetPath))) {
					Console.WriteLine ("- " + path);
					AssetsBundle.DeleteFile (path);
				}
			}
		}
	
		void SyncUpdated (string mask, string newFileExtension, Converter converter)
		{
			var files = Helpers.GetAllFiles (".", mask, true);
			foreach (string srcPath in files) {
				string dstPath = Path.ChangeExtension (srcPath, newFileExtension);
				bool cached = AssetsBundle.FileExists (dstPath);
				if (!cached || File.GetLastWriteTime (srcPath) > AssetsBundle.GetFileLastWriteTime (dstPath)) {
					if (converter != null) {
						try {
							if (converter (srcPath, dstPath)) {
								Console.WriteLine ((cached ? "* " : "+ ") + dstPath);
							}
						} catch (System.Exception) {
							Console.WriteLine ("An exception was caught while processing '{0}'", srcPath);
							throw;
						}
					}
					else {
 						Console.WriteLine ((cached ? "* " : "+ ") + dstPath);
						using (Stream stream = new FileStream (srcPath, FileMode.Open)) {
							Helpers.CreateDirectoryRecursive (Path.GetDirectoryName (dstPath));
							AssetsBundle.ImportFile (dstPath, stream, 0);
						}
					}
				}
			}
		}	
	
		class AtlasItem
		{
			public string Path;
			public Gdk.Pixbuf Pixbuf;
			public Lime.IntRectangle AtlasRect;
			public bool Allocated;
			public bool MipMapped;
			public bool Compressed;
		}
		
		string GetAtlasPath (string atlasChain, int index)
		{
			return Path.Combine ("Atlases", atlasChain + "." + index.ToString ("00") + GetPlatformTextureExtension ());
		}
		
		void BuildAtlasChain (string atlasChain)		
		{
			for (int i = 0; i < 100; i++) {
				string atlasPath = GetAtlasPath (atlasChain, i);
				if (AssetsBundle.FileExists (atlasPath)) {
					Console.WriteLine ("- " + atlasPath);
					AssetsBundle.DeleteFile (atlasPath);
				} else {
					break;
				}
			}
			int maxAtlasSize = (platform == TargetPlatform.Desktop) ? 2048 : 1024;
			var items = new List<AtlasItem> ();
			foreach (var p in cookingRulesMap) {
				if (p.Value.TextureAtlas == atlasChain && Path.GetExtension (p.Key) == ".png") {
					var srcTexturePath = Path.Combine (assetsDirectory, p.Key);
					var pixbuf = new Gdk.Pixbuf (srcTexturePath);
					// Ensure that no image exceede maxAtlasSize limit
					if (pixbuf.Width > maxAtlasSize || pixbuf.Height > maxAtlasSize) {
						int w = Math.Min (pixbuf.Width, maxAtlasSize);
						int h = Math.Min (pixbuf.Height, maxAtlasSize);
						pixbuf = pixbuf.ScaleSimple (w, h, Gdk.InterpType.Bilinear);
						Console.WriteLine (
							String.Format ("WARNING: {0} downscaled to {1}x{2}", srcTexturePath, w, h));
					}					
					var item = new AtlasItem {Path = Path.ChangeExtension (p.Key, ".atlasPart"), 
						Pixbuf = pixbuf, MipMapped = p.Value.MipMaps,
						Compressed = p.Value.Compression};
					items.Add (item);
				}
			}
			// Sort images in descendend size order
			items.Sort ((x, y) => {
				int a = Math.Max (x.Pixbuf.Width, x.Pixbuf.Height);
				int b = Math.Max (y.Pixbuf.Width, y.Pixbuf.Height);
				return b - a;
			});	
			for (int atlasId = 0; items.Count > 0; atlasId++) {
				for (int i = 64; i <= maxAtlasSize; i *= 2) {
					foreach (AtlasItem item in items) {
						item.Allocated = false;
					}
					// Take in account 1 pixel border for each side.
					var a = new RectAllocator (new Lime.Size (i + 2, i + 2));
					bool allAllocated = true;
					foreach (AtlasItem item in items) {
						var size = new Lime.Size (item.Pixbuf.Width + 2, item.Pixbuf.Height + 2);
						if (a.Allocate (size, out item.AtlasRect)) {
							item.Allocated = true;
						} else {
							allAllocated = false;
						}
					}
					if (i != maxAtlasSize && !allAllocated) {
						continue;
					}
					if (atlasId > 99) {
						throw new Lime.Exception ("Too many textures in the atlas chain {0}", atlasChain);
					}
					string atlasPath = GetAtlasPath (atlasChain, atlasId);
					var atlas = new Gdk.Pixbuf (Gdk.Colorspace.Rgb, true, 8, i, i);  
					atlas.Fill (0);
					bool compressed = false;
					bool mipMapped = false;
					foreach (AtlasItem item in items) {		
						if (!item.Allocated) {
							continue;
						}
						compressed |= item.Compressed;
						mipMapped |= item.MipMapped;
						var p = item.Pixbuf;
						p.CopyArea (0, 0, p.Width, p.Height, atlas, item.AtlasRect.A.X, item.AtlasRect.A.Y);														
						var atlasPart = new Lime.TextureAtlasPart ();
						atlasPart.AtlasRect = item.AtlasRect;
						atlasPart.AtlasRect.B -= new Lime.IntVector2 (2, 2);
						atlasPart.AtlasTexture = Path.ChangeExtension (atlasPath, null);
						Helpers.CreateDirectoryRecursive (Path.GetDirectoryName (item.Path));
						
						//Console.WriteLine ("+ " + item.Path);
						Lime.Serialization.WriteObjectToBundle<Lime.TextureAtlasPart> (AssetsBundle,
							item.Path, atlasPart);						
						
						// Delete non-atlased texture since now its useless
						var texturePath = Path.ChangeExtension (item.Path, GetPlatformTextureExtension ());
						if (AssetsBundle.FileExists (texturePath)) {
							//Console.WriteLine ("- " + texturePath);
							AssetsBundle.DeleteFile (texturePath);
						}
					}
					Console.WriteLine ("+ " + atlasPath);
					string inFile = "$TMP$.png";
					string outFile = Path.ChangeExtension (inFile, GetPlatformTextureExtension ());
					atlas.Save (inFile, "png");
					TextureConverter.Convert (inFile, outFile, compressed, mipMapped, platform);
					AssetsBundle.ImportFile (outFile, atlasPath, 0);
					File.Delete (inFile);
					File.Delete (outFile);
					items.RemoveAll (x => x.Allocated);							
					break;
				}
			}
		}
		
		void SyncAtlases ()
		{
			var atlasChainsToRebuild = new HashSet<string> ();
			// Figure out atlas chains to rebuld
			foreach (string atlasPartPath in AssetsBundle.EnumerateFiles ()) {
				if (Path.GetExtension (atlasPartPath) != ".atlasPart")
					continue;
				// If atlas part has been outdated we should rebuild full atlas chain
				string srcTexturePath = Path.Combine (assetsDirectory,
					Path.ChangeExtension (atlasPartPath, GetOriginalAssetExtension (atlasPartPath)));
				if (!File.Exists (srcTexturePath) || AssetsBundle.GetFileLastWriteTime (atlasPartPath) < File.GetLastWriteTime (srcTexturePath)) {
					var part = Lime.TextureAtlasPart.ReadFromBundle (atlasPartPath);
					string atlasChain = Path.GetFileNameWithoutExtension (part.AtlasTexture);
					atlasChainsToRebuild.Add (atlasChain);
					if (!File.Exists (srcTexturePath)) {
						Console.WriteLine ("- " + atlasPartPath);
						AssetsBundle.DeleteFile (atlasPartPath);
					} else {
						srcTexturePath = Path.ChangeExtension (BundlePathToNative (atlasPartPath), ".png");
						CookingRules rules = cookingRulesMap [srcTexturePath];
						if (rules.TextureAtlas != null) {
							atlasChainsToRebuild.Add (rules.TextureAtlas);
						} else {
							Console.WriteLine ("- " + atlasPartPath);
							AssetsBundle.DeleteFile (atlasPartPath);
						}
					}
				}
			}
			// Find which new textures must be added to the atlas chain
			foreach (var p in cookingRulesMap) {
				string atlasPartPath = Path.ChangeExtension (p.Key, ".atlasPart");
				bool atlasNeedRebuld = p.Value.TextureAtlas != null && 
					Path.GetExtension (p.Key) == ".png" && !AssetsBundle.FileExists (atlasPartPath);
				if (atlasNeedRebuld) {
					atlasChainsToRebuild.Add (p.Value.TextureAtlas);
				}
			}
			foreach (string atlasChain in atlasChainsToRebuild) {
				BuildAtlasChain (atlasChain);
			}
		}
	}
}

