using System;
using System.Collections.Generic;
using System.IO;
using Lime;

namespace Orange
{
	public static class AtlasGenerator
	{
		private static bool TryUpdate (string imagePath, ref TextureParams texture)
		{
			var image = new Gdk.Pixbuf (imagePath);
			if (image.Width != texture.AtlasRect.Width || image.Height != texture.AtlasRect.Height) {
				// Since image size had changed, we need full atlas rebuild.
				return false;
			}
			var atlasPath = Path.ChangeExtension (texture.AtlasTexture, ".png");
			Console.WriteLine ("{0} -> {1}", Helpers.RemovePathPrefix (imagePath), Path.GetFileName (atlasPath));
			var atlas = new Gdk.Pixbuf (atlasPath);
			image.CopyArea (0, 0, image.Width, image.Height, atlas, texture.AtlasRect.A.X, texture.AtlasRect.A.Y);
			atlas.Save (atlasPath, "png");
			return true;
		}
		
		private static void CleanupAtlases (string atlasesDirectory)
		{
			// Remove all atlases
			List<string> atlases = Helpers.GetAllFiles (atlasesDirectory, "*.*");
			foreach (string atlas in atlases) {
				File.Delete (atlas);
			}		
			string pngsDirectory = System.IO.Path.GetDirectoryName (atlasesDirectory);
			List<string> textures = Helpers.GetAllFiles (pngsDirectory, "*.texture");
			foreach (string texture in textures) {
				File.Delete (texture);
			}	
		}
		
		class AtlasItem
		{
			public string Path;
			public Gdk.Pixbuf Pixbuf;
			public IntRectangle AtlasRect;
			public bool Allocated;
		}
		
		private static void BuildAtlases (string atlasesDirectory, int maxAtlasSize)
		{
			string pngsDirectory = System.IO.Path.GetDirectoryName (atlasesDirectory);
			List<string> pngs = Helpers.GetAllFiles (pngsDirectory, "*.png");
			var items = new List<AtlasItem> ();
			foreach (string png in pngs) {
				var pixbuf = new Gdk.Pixbuf (png);
				// Ensure that no image exceede maxAtlasSize limit
				if (pixbuf.Width > maxAtlasSize || pixbuf.Height > maxAtlasSize) {
					int w = Math.Min (pixbuf.Width, maxAtlasSize);
					int h = Math.Min (pixbuf.Height, maxAtlasSize);
					pixbuf = pixbuf.ScaleSimple (w, h, Gdk.InterpType.Bilinear);
					Console.WriteLine (
						String.Format ("WARNING: {0} downscaled to {1}x{2}", png, w, h));
				}
				var item = new AtlasItem {Path = png, Pixbuf = pixbuf};
				items.Add (item);
			}
			// Sort images in descendend size order
			items.Sort ((x, y) => {
				int a = Math.Max (x.Pixbuf.Width, x.Pixbuf.Height);
				int b = Math.Max (y.Pixbuf.Width, y.Pixbuf.Height);
				return b - a;
			});	
			
			for (int atlasId = 1; items.Count > 0; atlasId++) {
				for (int i = 64; i <= maxAtlasSize; i *= 2) {
					foreach (AtlasItem item in items) {
						item.Allocated = false;
					}
					// Take in account 1 pixel border from each side.
					var a = new RectAllocator (new Size (i + 2, i + 2));
					bool allAllocated = true;
					foreach (AtlasItem item in items) {
						if (a.Allocate (new Size (item.Pixbuf.Width + 2, item.Pixbuf.Height + 2), out item.AtlasRect)) {
							item.Allocated = true;
						} else {
							allAllocated = false;
						} 
					}
					if (i == maxAtlasSize || allAllocated) {
						string atlasPath = Path.Combine (atlasesDirectory, atlasId.ToString ("00") + ".png");
						var atlas = new Gdk.Pixbuf (Gdk.Colorspace.Rgb, true, 8, i, i);  
						atlas.Fill (0);
						foreach (AtlasItem item in items) {
							if (item.Allocated) {
								var p = item.Pixbuf;
								p.CopyArea (0, 0, p.Width, p.Height, atlas, item.AtlasRect.A.X, item.AtlasRect.A.Y);														
								var texture = new TextureParams ();
								texture.AtlasRect = item.AtlasRect;
								texture.AtlasRect.B -= new IntVector2 (2, 2);
								texture.AtlasTexture = Path.ChangeExtension (atlasPath.Remove (0, 2), null); // './'
								TextureParams.WriteToFile (texture, Path.ChangeExtension (item.Path, ".texture"));
								Console.WriteLine ("{0} -> {1}", Helpers.RemovePathPrefix (item.Path), Path.GetFileName (atlasPath));
							}
						}
						atlas.Save (atlasPath, "png");
						items.RemoveAll (x => x.Allocated);
						// Console.WriteLine ("Texture atlas '{0}' saved", atlasPath);
						break;
					}
				}
			}
		}
		
		public static void Process (string atlasesDirectory)
		{
			string pngsDirectory = System.IO.Path.GetDirectoryName (atlasesDirectory);
			List<string> pngs = Helpers.GetAllFiles (pngsDirectory, "*.png");
			bool needRebuild = false;
			foreach (string png in pngs) {
				// Ignore content of "Atlases" directory.
				if (Path.GetFileName (Path.GetDirectoryName (png)) == "Atlases") {
					continue;
				}
				// If ".texture" file is missing, we should rebuild atlases
				string texturePath = Path.ChangeExtension (png, ".texture");
				if (!File.Exists (texturePath)) {
					needRebuild = true;
					break;
				}
				var texture = Lime.TextureParams.ReadFromFile (texturePath);
				// if atlas directory differs from directory stored in texture file, that means.
				// atlas had been moved or renamed. We should rebuild it.
				string atlasPath = Path.Combine (".", texture.AtlasTexture);
				if (Path.GetDirectoryName (atlasPath) != atlasesDirectory) {
					needRebuild = true;
					break;
				}
				// if atlas doesn't exist, we have to rebuild all atlases.
				if (!File.Exists (Path.ChangeExtension (atlasPath, ".png"))) {
					needRebuild = true;
					break;
				}
				// If png had been changed after atlas generation, try to update atlas, or otherwise rebuild all atlases.
				if (File.GetLastWriteTime (png) > File.GetLastWriteTime (texturePath))
				{
					if (TryUpdate (png, ref texture)) {
						TextureParams.WriteToFile (texture, texturePath);
					} else {
						needRebuild = true;
						break;
					}
				}
			}
			if (needRebuild) {
				CleanupAtlases (atlasesDirectory);
				BuildAtlases (atlasesDirectory, 1024);
			}
		}
	}
}

