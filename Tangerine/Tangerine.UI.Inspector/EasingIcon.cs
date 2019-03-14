using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.Inspector
{
	public class EasingIcons
	{
		const int Width = 22;
		const int Height = 22;

		public static readonly EasingIcons Instance = new EasingIcons();

		private Dictionary<(EasingFunction, EasingType), Texture2D> textures = new Dictionary<(EasingFunction, EasingType), Texture2D>();

		public ITexture Get(EasingFunction function, EasingType type)
		{
			Texture2D texture;
			if (textures.TryGetValue((function, type), out texture)) {
				return texture;
			}
			texture = new Texture2D();
			var pixels = Render(Width, Height, function, type);
			texture.LoadImage(pixels, Width, Height);
			textures.Add((function, type), texture);
			return texture;
		}

		private static Color4[] Render(int width, int height, EasingFunction function, EasingType type)
		{
			var image = new float[width * height];
			float py = 0, px = 0;
			for (float t = 0; t <= 1; t += 0.001f) {
				var v = Easing.Interpolate(t, function, type);
				var x = (t * 0.75f + 0.125f) * width;
				var y = ((1 - v) * 0.5f + 0.25f) * height;
				var ix = (int)x;
				var iy = (int)y;
				if (t > 0 && (y - py).Abs() > (x - px).Abs()) {
					DrawPoint(ix + (x - ix > 0.5f ? 1 : -1), iy, x, y, false, image, width, height);
					DrawPoint(ix, iy, x, y, false, image, width, height);
				} else {
					DrawPoint(ix, iy, x, y, true, image, width, height);
					DrawPoint(ix, iy + (y - iy > 0.5f ? 1 : -1), x, y, true, image, width, height);
				}
				px = x;
				py = y;
			}
			var pixels = new Color4[width * height];
			for (int i = 0; i < pixels.Length; i++) {
				pixels[i] = Color4.Lerp(image[i], Theme.Colors.WhiteBackground, Theme.Colors.BlackText);
			}
			return pixels;
		}

		private static void DrawPoint(int px, int py, float rx, float ry, bool majorXAxis, float[] image, int width, int height)
		{
			if (py >= 0 && py < height) {
				float t = majorXAxis ? (ry - (py + 0.5f)).Abs() : (rx - (px + 0.5f)).Abs();
				var i = px + py * width;
				image[i] = Mathf.Max(image[i], 1 - t);
			}
		}
	}
}
