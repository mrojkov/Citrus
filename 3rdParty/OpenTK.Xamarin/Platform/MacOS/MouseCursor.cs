using System;
using System.Runtime.InteropServices;

namespace OpenTK
{
	public sealed class MouseCursor
	{
		/// Stores a window icon. A window icon is defined as a 2-dimensional buffer of RGBA values.
		byte[] data;
		int width;
		int height;

		internal byte[] Data { get { return data; } }

		internal int Width { get { return width; } }

		internal int Height { get { return height; } }

		static readonly MouseCursor defaultCursor = new MouseCursor();
		static readonly MouseCursor emptyCursor = new MouseCursor(0, 0, 16, 16, new byte[16 * 16 * 4]);

		int x;
		int y;

		internal int X { get { return x; } }

		internal int Y { get { return y; } }

		MouseCursor()
		{
		}

		internal MouseCursor(int width, int height)
		{
			if (width < 0 || width > 256 || height < 0 || height > 256) {
				throw new ArgumentOutOfRangeException();
			}

			this.width = width;
			this.height = height;
		}

		internal MouseCursor(int width, int height, byte[] data)
			: this(width, height)
		{
			if (data == null) {
				throw new ArgumentNullException();
			}
			if (data.Length < Width * Height * 4) {
				throw new ArgumentOutOfRangeException();
			}

			this.data = data;
		}


		internal MouseCursor(int width, int height, IntPtr data)
			: this(width, height)
		{
			if (data == IntPtr.Zero) {
				throw new ArgumentNullException();
			}

			this.data = new byte[width * height * 4];
			Marshal.Copy(data, this.data, 0, this.data.Length);
		}

		/// Initializes a new instance from a contiguous array of BGRA pixels. Each pixel is composed of 4 bytes, representing B, G, R and A values, respectively.
		/// For correct antialiasing of translucent cursors, the B, G and R components should be premultiplied with the A component:
		/// 
		/// B = (byte)((B * A) / 255)
		/// G = (byte)((G * A) / 255)
		/// R = (byte)((R * A) / 255)
		/// 
		public MouseCursor(int hotx, int hoty, int width, int height, byte[] data)
			: this(width, height, data)
		{
			if (hotx < 0 || hotx >= Width || hoty < 0 || hoty >= Height) {
				throw new ArgumentOutOfRangeException();
			}

			x = hotx;
			y = hoty;
		}

		/// Initializes a new instance from a contiguous array of BGRA pixels. Each pixel is composed of 4 bytes, representing B, G, R and A values, respectively.
		/// For correct antialiasing of translucent cursors, the B, G and R components should be premultiplied with the A component:
		/// 
		/// B = (byte)((B * A) / 255)
		/// G = (byte)((G * A) / 255)
		/// R = (byte)((R * A) / 255)
		/// 
		public MouseCursor(int hotx, int hoty, int width, int height, IntPtr data)
			: this(width, height, data)
		{
			if (hotx < 0 || hotx >= Width || hoty < 0 || hoty >= Height) {
				throw new ArgumentOutOfRangeException();
			}

			x = hotx;
			y = hoty;
		}

		public static MouseCursor Default { get { return defaultCursor; } }

		public static MouseCursor Empty { get { return emptyCursor; } }
	}
}

