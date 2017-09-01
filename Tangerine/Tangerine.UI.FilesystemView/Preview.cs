using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;

namespace Tangerine.UI.FilesystemView
{
	public class Preview
	{
		public ThemedScrollView RootWidget;
		private Selection savedSelection = new Selection();
		// TODO: Clear Cache on fs navigation
		private Dictionary<string, ITexture> textureCache = new Dictionary<string, ITexture>();

		public void ClearTextureCache()
		{
			textureCache.Clear();
		}

		public Preview()
		{
			RootWidget = new ThemedScrollView();
			RootWidget.Content.Layout = new FlowLayout();
		}

		public void Invalidate(Selection selection)
		{
			if (RootWidget.Parent == null) {
				return;
			}
			if (savedSelection == selection) {
				return;
			}
			savedSelection = selection.Clone();
			RootWidget.Content.Nodes.Clear();
			if (selection.Empty) {
				return;
			}
			RootWidget.ScrollPosition = 0;
			List<Tuple<string, Image>> previews = new List<Tuple<string, Image>>();
			foreach (var filename in selection) {
				var pv = GeneratePreview(filename);
				if (pv != null) {
					previews.Add(new Tuple<string, Image>(filename, pv));
				}
			}
			previews.Sort((a, b) => {
				var szA = a.Item2.Texture.SurfaceSize;
				var szB = b.Item2.Texture.SurfaceSize;
				return Comparer<float>.Default.Compare((float)szB.Width / szB.Height, (float)szA.Width / szA.Height);
			});
			foreach (var t in previews) {
				RootWidget.Content.AddNode(new Widget {
					Layout = new VBoxLayout(),
					Nodes = {
						t.Item2,
						new ThemedSimpleText {
							OverflowMode = TextOverflowMode.Ellipsis,
							Text = Path.GetFileName(t.Item1)
						}
					}
				});
			}
		}

		private Image GeneratePreview(string filename)
		{
			ITexture texture = null;
			if (textureCache.ContainsKey(filename)) {
				texture = textureCache[filename];
			} else {
				texture = GetTexture(filename);
			}
			if (texture == null) {
				return null;
			}
			textureCache[filename] = texture;
			var img = new Image(texture);
			img.Texture.TextureParams = new TextureParams {
				MinMagFilter = TextureFilter.Nearest
			};
			img.MinMaxSize = img.Size = (Vector2)img.Texture.SurfaceSize;
			return img;
		}

		private static ITexture GetTexture(string filename)
		{
			if (AssetBundle.Instance == null) {
				return null;
			}
			if (Directory.Exists(filename)) {
				return null;
			}
			var extension = Path.GetExtension(filename).ToLower();
			if (extension == ".scene" || extension == ".tan") {
				const string SceneThumbnailSeparator = "{8069CDD4-F02F-4981-A3CB-A0BAD4018D00}";
				var allText = File.ReadAllText(filename);
				var index = allText.IndexOf(SceneThumbnailSeparator);
				// Trim zeroes from the end of file since they tend to appear there for unknown reason
				if (index <= 0) {
					return null;
				}
				var endOfBase64Index = allText.Length - 1;
				while (allText[endOfBase64Index] == 0 || allText[endOfBase64Index] == '\n' || allText[endOfBase64Index] == '\r') {
					endOfBase64Index--;
				}
				int startOfBase64Index = index + SceneThumbnailSeparator.Length;
				var previewText = allText.Substring(startOfBase64Index, endOfBase64Index - startOfBase64Index + 1);
				var previewBytes = System.Convert.FromBase64String(previewText);
				var texture = new Texture2D();
				texture.LoadImage(previewBytes);
				return texture;
			} else {
				var fi = new FileInfo(filename);
				if (fi.Length > 1024 * 1024 * 10) {
					return null;
				}
				if (extension == ".png" || extension == ".jpg") {
					try {
						var texture = new Texture2D();
						texture.LoadImage(filename);
						return texture;
					} catch {
						return LoadFileAsRawBitmap(filename);
					}
				} else {
					return LoadFileAsRawBitmap(filename);
				}
			}
		}

		private static ITexture LoadFileAsRawBitmap(string filename)
		{
			var bytes = File.ReadAllBytes(filename);
			var len = bytes.Length;
			if (len == 0) {
				return null;
			}
			var texture = new Texture2D();
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
			return texture;
		}
	}
}