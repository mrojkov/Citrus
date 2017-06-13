using System.IO;
using System.Linq;
using Lime;

namespace Tangerine.UI.FilesystemView
{
	public class Preview
	{
		public Widget RootWidget;

		public Preview()
		{
			RootWidget = new Widget {
				Layout = new VBoxLayout()
			};
		}

		public void Invalidate(Selection selection)
		{
			RootWidget.Nodes.Clear();
			if (selection.Empty) {
				return;
			}
			var filename = selection.First();
			if (Directory.Exists(filename)) {
				return;
			}
			if (filename.EndsWith(".scene") || filename.EndsWith(".tan")) {
				const string SceneThumbnailSeparator = "{8069CDD4-F02F-4981-A3CB-A0BAD4018D00}";
				var allText = File.ReadAllText(filename);
				var index = allText.IndexOf(SceneThumbnailSeparator);
				// Trim zeroes from the end of file since they tend to appear there for unknown reason
				if (index > 0) {
					var endOfBase64Index = allText.Length - 1;
					while (allText[endOfBase64Index] == 0) {
						endOfBase64Index--;
					}
					int startOfBase64Index = index + SceneThumbnailSeparator.Length;
					var previewText = allText.Substring(startOfBase64Index, endOfBase64Index - startOfBase64Index + 1);
					var previewBytes = System.Convert.FromBase64String(previewText);
					var texture = new Texture2D();
					texture.LoadImage(previewBytes);
					var img = new Image(texture);
					img.Texture.MinFilter = img.Texture.MagFilter = TextureFilter.Nearest;
					RootWidget.AddNode(img);
				}
			} else {
				var texture = new Texture2D();
				try {
					texture.LoadImage(filename);
				} catch {
					var bytes = File.ReadAllBytes(filename);
					var len = bytes.Length;
					if (len == 0) {
						return;
					}
					int trueLen = len + 3 - len % 3;
					int side = Mathf.Sqrt(trueLen / 3).Truncate();
					Color4[] pixels = new Color4[side * side];
					for (int i = 0; i < side * side; i++) {
						pixels[i].R = i * 3 + 0 >= len ? (byte)0 : bytes[i * 3 + 0];
						pixels[i].G = i * 3 + 1 >= len ? (byte)0 : bytes[i * 3 + 1];
						pixels[i].B = i * 3 + 2 >= len ? (byte)0 : bytes[i * 3 + 2];
						pixels[i].A = 255;
					}
					texture.LoadImage(pixels, side, side);
				}
				var img = new Image(texture);
				img.Texture.MinFilter = img.Texture.MagFilter = TextureFilter.Nearest;
				RootWidget.AddNode(img);
			}
		}
	}
}