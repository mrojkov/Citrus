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
		// zoom, maxZoom for each element
		private List<Tuple<int, int>> zoom = new List<Tuple<int, int>>();
		private int maxZoom = zoomValues.Length - 1;
		private static float[] zoomValues = { 1/32.0f, 1/24.0f, 1/16.0f, 1/12.0f, 1/8.0f, 1/6.0f, 1/4.0f, 1/3.0f, 1/2.0f, 1/1.5f,
			1.0f, 1.5f, 2.0f, 3.0f, 4.0f, 6.0f, 8.0f, 12.0f, 16.0f, 24.0f, 32.0f };
		// TODO: Clear Cache on fs navigation
		private Dictionary<string, ITexture> textureCache = new Dictionary<string, ITexture>();

		public void ClearTextureCache()
		{
			textureCache.Clear();
		}

		private ITexture PrepareChessTexture(Color4 color1, Color4 color2)
		{
			var chessTexture = new Texture2D();
			chessTexture.LoadImage(new[] { color1, color2, color2, color1 }, 2, 2);
			chessTexture.TextureParams = new TextureParams {
				WrapMode = TextureWrapMode.Repeat,
				MinMagFilter = TextureFilter.Nearest,
			};
			return chessTexture;
		}

		public Preview()
		{
			var t = PrepareChessTexture(ColorTheme.Current.Basic.ZebraColor1.Transparentify(0.5f), ColorTheme.Current.Basic.ZebraColor2);
			const float ChessCellSize = 50;
			//Color4 Color1 = Core.UserPreferences.Instance.Get<UserPreferences>().BackgroundColorA;
			//Color4 Color2 = Core.UserPreferences.Instance.Get<UserPreferences>().BackgroundColorB;
			RootWidget = new ThemedScrollView();
			RootWidget.Content.Layout = new FlowLayout {
				Spacing = 5.0f,
			};
			RootWidget.Content.Padding = new Thickness(5.0f);
			RootWidget.CompoundPresenter.Insert(1, new DelegatePresenter<Widget>((w) => {
				w.PrepareRendererState();
				var ratio = ChessCellSize * 1.0f;
				Renderer.DrawSprite(
					t,
					Color4.White,
					Vector2.Zero,
					w.Size,
					-Vector2.Zero / ratio,
					 (w.Size - Vector2.Zero) / ratio);
			}));
			RootWidget.Updated += (dt) => {
				if (RootWidget.IsMouseOver()) {
					if (!RootWidget.Input.IsKeyPressed(Key.Control)) {
						return;
					}
					int zoomDelta = 0;
					if (RootWidget.Input.WasKeyPressed(Key.MouseWheelUp)) {
						zoomDelta = 1;
					}
					if (RootWidget.Input.WasKeyPressed(Key.MouseWheelDown)) {
						zoomDelta = -1;
					}
					if (zoomDelta != 0) {
						for (int i = 0; i < zoom.Count; i++) {
							var z = zoom[i];
							int newZoom = Mathf.Clamp(z.Item1 + zoomDelta, 0, z.Item2);
							if (newZoom != z.Item1) {
								zoom[i] = new Tuple<int, int>(newZoom, z.Item2);
							}
						}
						ApplyZoom();
					}
				}
			};
		}

		private void CalcZoomAndMaxZoom()
		{
			int identitiyZoom = zoomValues.ToList().IndexOf(1.0f);
			zoom.Clear();
			foreach (var n in RootWidget.Content.Nodes) {
				var w = n.Nodes[0] as Widget;
				var z = zoomValues.Length - 1;
				while (z > 0 && w.MinSize.X * zoomValues[z] > RootWidget.Size.X) {
					z--;
				}
				zoom.Add(new Tuple<int, int>(identitiyZoom > z ? z : identitiyZoom, z));
				if (identitiyZoom > z) {
					w.MinMaxSize = (Vector2)w.Texture.ImageSize * z;
				}
			}
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
					pv.MinMaxSize = (Vector2)pv.Texture.ImageSize;
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
			CalcZoomAndMaxZoom();
		}

		private void ApplyZoom()
		{
			for (int i = 0; i < RootWidget.Content.Nodes.Count; i++) {
				var w = RootWidget.Content.Nodes[i].Nodes[0] as Widget;
				w.MinMaxSize = (Vector2)w.Texture.ImageSize * zoomValues[zoom[i].Item1];
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
			try {
				if (AssetBundle.Current == null) {
					return null;
				}
			} catch (Lime.Exception) {
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
				try {
					if (extension == ".png" || extension == ".jpg") {
						try {
							var texture = new Texture2D();
							using (var stream = new FileStream(filename, FileMode.Open)) {
								texture.LoadImage(stream);
							}
							return texture;
						} catch {
							return LoadFileAsRawBitmap(filename);
						}
					} else {
						return LoadFileAsRawBitmap(filename);
					}
				} catch {
					return new Texture2D();
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