using System.IO;
using Lime;

namespace Calamansi
{
	public class CalamansiFont : Font
	{
		private readonly CalamansiConfig config;

		public CalamansiFont() { }

		public CalamansiFont(CalamansiConfig config, string assetDirectory)
		{
			this.config = config;
			CharCollection = new CalamansiFontCharCollection(config, assetDirectory);
			if (config.SDF) {
				foreach (var texture in Textures) {
					SDFConverter.ConverToSDF(texture.GetPixels(), texture.ImageSize.Width, texture.ImageSize.Height, config.Padding / 2);
				}
			}
		}

		public void SaveAsTft(string path, string assetDirectory)
		{
			var basePath = Path.ChangeExtension(path, null);
			for (int i = 0; i < CharCollection.Textures.Count; i++) {
				var texture = CharCollection.Textures[i];
				var pixels = texture.GetPixels();
				var w = texture.ImageSize.Width;
				var h = texture.ImageSize.Height;
				if (config.SDF) {
					var sourceBitmap = new Bitmap(pixels, w, h);
					var bitmap = sourceBitmap.Rescale((int)(w * config.SDFScale), (int)(h * config.SDFScale));
					sourceBitmap.Dispose();
					bitmap.SaveTo(AssetPath.Combine(assetDirectory, basePath + (i > 0 ? $"{i:00}.png" : ".png")));
					bitmap.Dispose();

				} else {
					using (var bm = new Bitmap(pixels, w, h)) {
						bm.SaveTo(basePath + (i > 0 ? $"{i:00}.png" : ".png"));
					}
				}
				CharCollection.Textures[i] = new SerializableTexture(basePath + (i > 0 ? $"{i:00}" : ""));
			}
			Serialization.WriteObjectToFile(Path.ChangeExtension(path, "tft"), this, Serialization.Format.JSON);
		}
	}
}
