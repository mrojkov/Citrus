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

		private Dictionary<(EasingFunction, EasingType, int), Texture2D> textures = new Dictionary<(EasingFunction, EasingType, int), Texture2D>();

		public ITexture Get(EasingFunction function, EasingType type, int softness)
		{
			Texture2D texture;
			if (textures.TryGetValue((function, type, softness), out texture)) {
				return texture;
			}
			texture = new Texture2D();
			var pixels = Render(Width, Height, function, type, softness);
			texture.LoadImage(pixels, Width, Height);
			textures.Add((function, type, softness), texture);
			return texture;
		}

		private static Color4[] Render(int width, int height, EasingFunction function, EasingType type, int softness)
		{
			var image = new float[width * height];
			for (float t = 0; t <= 1; t += 0.001f) {
				var v = Easing.Interpolate(t, function, type);
				if (softness != 0) {
					var k = softness * 0.01f;
					v = (1 - k) * v + k * t;
				}
				var x = (t * 0.75f + 0.125f) * width;
				var y = ((1 - v) * 0.5f + 0.25f) * height;
				DrawPoint(x, y, image, width);
			}
			var pixels = new Color4[width * height];
			for (int i = 0; i < pixels.Length; i++) {
				pixels[i] = Color4.Lerp(image[i], ColorTheme.Current.Basic.WhiteBackground, ColorTheme.Current.Basic.BlackText);
			}
			return pixels;
		}

		private static void DrawPoint(float x, float y, float[] image, int width)
		{
			var px = (int)(x + 0.5f);
			var py = (int)(y + 0.5f);
			var dx = x - px;
			var dy = y - py;
			var t = Mathf.Sqrt(dx * dx + dy * dy);
			var i = px + py * width;
			image[i] = Mathf.Max(image[i], 1 - t);
		}
	}
}
