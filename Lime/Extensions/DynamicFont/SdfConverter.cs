using Lime;

namespace Lime
{
	internal class SdfConverter
	{
		private struct Pixel
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
			public readonly int Width;
			public readonly int Height;
			private readonly Pixel[] pixels;

			public Grid(int w, int h)
			{
				Width = w;
				Height = h;
				pixels = new Pixel[w * h];
			}

			public Pixel this[int x, int y]
			{
				get => x >= 0 && y >= 0 && x < Width && y < Height ? pixels[y * Width + x] : Pixel.Empty;
				set
				{
					if (x >= 0 && y >= 0 && x < Width && y < Height) {
						pixels[y * Width + x] = value;
					}
				}
			}
		}

		public static void ConvertToSdf(Color4[] pixels, int width, int height, float distanceFieldScale)
		{
			var w = width;
			var h = height;
			Grid[] grids = {
					new Grid(w, h),
					new Grid(w, h),
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
			var task1 = System.Threading.Tasks.Task.Run(() => GenerateSdf(grids[0]));
			var task2 = System.Threading.Tasks.Task.Run(() => GenerateSdf(grids[1]));
			System.Threading.Tasks.Task.WaitAll(task1, task2);
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					var dist1 = Mathf.Sqrt(grids[0][x, y].Distance + 1);
					var dist2 = Mathf.Sqrt(grids[1][x, y].Distance + 1);
					var p = grids[0][x, y];
					p.Distance = dist1 - dist2;
					grids[0][x, y] = p;
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

		private static void Compare(Grid g, ref Pixel p, int x, int y, int offsetX, int offsetY)
		{
			float add;
			var other = g[x + offsetX, y + offsetY];
			if (offsetY == 0) {
				add = 2 * other.Dx + 1;
			} else if (offsetX == 0) {
				add = 2 * other.Dy + 1;
			} else {
				add = 2 * (other.Dy + other.Dx + 1);
			}
			if (other.Distance + add < p.Distance) {
				p.Distance = other.Distance + add;
				p.Dx = other.Dx + (offsetX != 0 ? 1 : 0);
				p.Dy = other.Dy + (offsetY != 0 ? 1 : 0);
			}
		}

		private static void GenerateSdf(Grid g)
		{
			for (int y = 0; y < g.Height; y++) {
				for (int x = 0; x <= g.Width; x++) {
					var p = g[x, y];
					Compare(g, ref p, x, y, 0, -1);
					Compare(g, ref p, x, y, 0, -1);
					g[x, y] = p;
				}
				for (int x = 1; x <= g.Width; x++) {
					var p = g[x, y];
					Compare(g, ref p, x, y, -1, 0);
					Compare(g, ref p, x, y, -1, -1);
					g[x, y] = p;
				}
				for (int x = g.Width; x >= 0; x--) {
					var p = g[x, y];
					Compare(g, ref p, x, y, 1, 0);
					Compare(g, ref p, x, y, 1, -1);
					g[x, y] = p;
					x--;
					p = g[x, y];
					Compare(g, ref p, x, y, 1, 0);
					Compare(g, ref p, x, y, 1, -1);
					g[x, y] = p;
				}
			}
			for (int y = g.Height - 1; y >= 0; y--) {
				for (int x = 0; x <= g.Width; x++) {
					var p = g[x, y];
					Compare(g, ref p, x, y, 0, 1);
					g[x, y] = p;
				}
				for (int x = 0; x <= g.Width; x++) {
					var p = g[x, y];
					Compare(g, ref p, x, y, -1, 0);
					Compare(g, ref p, x, y, -1, 1);
					g[x, y] = p;
				}
				for (int x = g.Width - 1; x >= 0; x--) {
					var p = g[x, y];
					Compare(g, ref p, x, y, 1, 0);
					Compare(g, ref p, x, y, 1, 1);
					g[x, y] = p;
				}
			}
		}
	}
}
