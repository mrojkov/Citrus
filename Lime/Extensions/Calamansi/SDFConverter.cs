using Lime;

namespace Calamansi
{
	class SDFConverter
	{
		private class Pixel
		{
			public float Dx;
			public float Dy;
			public float Distance;

			public static Pixel Empty => new Pixel {
				Dx = 9999,
				Dy = 9999,
				Distance = 9999 * 9999
			};

			public static Pixel Inside => new Pixel {
				Dx = 0,
				Dy = 0,
				Distance = 0
			};
		}

		private class Grid
		{
			public int Width;
			public int Height;
			private readonly Pixel[] pixels;

			public Grid(int w, int h)
			{
				Width = w;
				Height = h;
				pixels = new Pixel[w * h];
			}

			public Pixel this[int x, int y]
			{
				get => (x >= 0 && y >= 0 && x < Width && y < Height) ? pixels[y * Width + x] : Pixel.Empty;
				set {
					if (x >= 0 && y >= 0 && x < Width && y < Height) {
						pixels[y * Width + x] = value;
					}
				}
			}
		}

		public static void ConverToSDF(Color4[] pixels, int width, int height, float distanceFieldScale)
		{
			var w = width;
			var h = height;
			Grid[] grids = new[] {
					new Grid(w, h),
					new Grid(w, h)
				};
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					if (pixels[x + y * width].A > 128) {
						grids[0][x, y] = Pixel.Empty;
						grids[1][x, y] = Pixel.Inside;
					} else {
						grids[0][x, y] = Pixel.Inside;
						grids[1][x, y] = Pixel.Empty;
					}
				}
			}

			var task1 = System.Threading.Tasks.Task.Run(() => GenerateSDF(grids[0]));
			var task2 = System.Threading.Tasks.Task.Run(() => GenerateSDF(grids[1]));

			System.Threading.Tasks.Task.WaitAll(task1, task2);

			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					var dist1 = Mathf.Sqrt(grids[0][x, y].Distance + 1);
					var dist2 = Mathf.Sqrt(grids[1][x, y].Distance + 1);
					grids[0][x, y].Distance = dist1 - dist2;
				}
			}

			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					var dist = grids[0][x, y].Distance;
					// Clamp and scale
					byte c = (byte)Mathf.Clamp((int)(dist * 64 / distanceFieldScale) + 128, 0, 255);
					Color4 color = new Color4(c, c, c, 255);

					pixels[x + y * width] = color;
				}
			}
		}

		private static void Compare(Grid g, Pixel p, int x, int y, int offsetx, int offsety)
		{
			float add;
			var other = g[x + offsetx, y + offsety];
			if (offsety == 0) {
				add = 2 * other.Dx + 1;
			} else if (offsetx == 0) {
				add = 2 * other.Dy + 1;
			} else {
				add = 2 * (other.Dy + other.Dx + 1);
			}

			if (other.Distance + add < p.Distance) {
				p.Distance = other.Distance + add;
				p.Dx = other.Dx + (offsetx != 0 ? 1 : 0);
				p.Dy = other.Dy + (offsety != 0 ? 1 : 0);
			}
		}

		private static void GenerateSDF(Grid g)
		{
			for (int y = 0; y < g.Height; y++) {
				for (int x = 0; x <= g.Width; x++) {
					var p = g[x, y];
					Compare(g, p, x, y, 0, -1);
					Compare(g, p, x, y, 0, -1);
				}
				for (int x = 1; x <= g.Width; x++) {
					var p = g[x, y];
					Compare(g, p, x, y, -1, 0);
					Compare(g, p, x, y, -1, -1);
				}
				for (int x = g.Width; x >= 0; x--) {
					var p = g[x, y];
					Compare(g, p, x, y, 1, 0);
					Compare(g, p, x, y, 1, -1);
					x--;
					p = g[x, y];
					Compare(g, p, x, y, 1, 0);
					Compare(g, p, x, y, 1, -1);
				}
			}

			for (int y = g.Height - 1; y >= 0; y--) {
				for (int x = 0; x <= g.Width; x++) {
					var p = g[x, y];
					Compare(g, p, x, y, 0, 1);
				}
				for (int x = 0; x <= g.Width; x++) {
					var p = g[x, y];
					Compare(g, p, x, y, -1, 0);
					Compare(g, p, x, y, -1, 1);
				}
				for (int x = g.Width - 1; x >= 0; x--) {
					var p = g[x, y];
					Compare(g, p, x, y, 1, 0);
					Compare(g, p, x, y, 1, 1);
				}
			}
		}

		private Vector2 GetGradient(Grid g, int x, int y, int radius = 1)
		{
			var result = Vector2.Zero;
			var d = g[x, y].Distance;
			for (int dx = -radius; dx <= radius; dx++) {
				for (int dy = -radius; dy <= radius; dy++) {
					if (
						x + dx < 0 ||
						y + dy < 0 ||
						x + dx >= g.Width ||
						y + dy >= g.Height ||
						(x == 0 && y == 0)
					) {
						continue;
					}
					var dist = g[x + dx, y + dy].Distance - d;
					result += new Vector2(dx == 0 ? 0 : dist / dx, dy == 0 ? 0 : dist / dy);
				}
			}

			return result.Normalized;
		}

	}
}
