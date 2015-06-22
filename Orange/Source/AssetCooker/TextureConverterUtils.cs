using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using Gdk;

namespace Orange
{
	public static class TextureConverterUtils
	{
		struct RGBA
		{
			public byte R, G, B, A;
		}

		struct RGB
		{
			public byte R, G, B;
		}

		/// <summary>
		/// Fills RGB channels with alpha values.
		/// </summary>
		public static void ConvertBitmapToAlphaMask(Gdk.Pixbuf pixbuf)
		{
			int stride = pixbuf.Rowstride;
			if ((stride & 0x3) != 0 || !pixbuf.HasAlpha) {
				throw new Lime.Exception("Invalid pixbuf format");
			}
			unsafe {
				int width = pixbuf.Width;
				int height = pixbuf.Height;
				RGBA* pixels = (RGBA*)pixbuf.Pixels;
				for (int i = 0; i < height; i++) {
					for (int j = 0; j < width; j++) {
						RGBA p = *pixels;
						p.R = p.G = p.B = p.A;
						*pixels++ = p;
					}
					pixels += stride / 4 - width;
				}
			}
		}

		/// <summary>
		/// Fills RGB channels with Alpha value, Alpha remains untouched
		/// </summary>
		public static void ConvertBitmapToGrayscaleAlphaMask(Gdk.Pixbuf pixbuf)
		{
			int stride = pixbuf.Rowstride;
			if ((stride & 0x3) != 0 || !pixbuf.HasAlpha) {
				throw new Lime.Exception("Invalid pixbuf format");
			}
			unsafe {
				int width = pixbuf.Width;
				int height = pixbuf.Height;
				RGBA* pixels = (RGBA*)pixbuf.Pixels;
				for (int i = 0; i < height; i++) {
					for (int j = 0; j < width; j++) {
						RGBA p = *pixels;
						p.R = p.G = p.B = p.A;
						*pixels++ = p;
					}
					pixels += stride / 4 - width;
				}
			}
		}

		public static bool IsWhiteImageWithAlpha(Gdk.Pixbuf pixbuf)
		{
			if (pixbuf.NChannels != 4 || !pixbuf.HasAlpha) {
				return false;
			}
			if ((pixbuf.Rowstride & 0x3) != 0 || pixbuf.BitsPerSample != 8) {
				throw new Lime.Exception("Invalid pixbuf format");
			}
			unsafe {
				RGBA* pixels = (RGBA*)pixbuf.Pixels;
				int width = pixbuf.Width;
				for (int i = 0; i < pixbuf.Height; i++) {
					for (int j = 0; j < width; j++) {
						RGBA c = *pixels;
						if (c.A > 0) {
							if (c.R != 255 || c.G != 255 || c.B != 255) {
								return false;
							}
						}
						pixels++;
					}
					pixels += pixbuf.Rowstride / 4 - width;
				}
			}
			return true;
		}

		public static void ReduceTo4BitsPerChannelWithFloydSteinbergDithering(Gdk.Pixbuf pixbuf)
		{
			if (pixbuf.HasAlpha) {
				ReduceRGBATo4BitsPerChannelWithFloydSteinbergDithering(pixbuf);
			} else {
				ReduceRGBTo4BitsPerChannelWithFloydSteinbergDithering(pixbuf);
			}
		}

		private static void ReduceRGBATo4BitsPerChannelWithFloydSteinbergDithering(Gdk.Pixbuf pixbuf)
		{
			int stride = pixbuf.Rowstride;
			if ((stride & 0x3) != 0) {
				throw new Lime.Exception("Invalid pixbuf format");
			}
			unsafe {
				int	width = pixbuf.Width;
				int height = pixbuf.Height;
				RGBA* pixels = (RGBA*)pixbuf.Pixels;
				for (int i = 0; i < height; i++) {
					for (int j = 0; j < width; j++) {
						RGBA quantError = *pixels;
						quantError.A &= 0x0F; 
						quantError.R &= 0x0F; 
						quantError.G &= 0x0F;
						quantError.B &= 0x0F;
						RGBA pixel = *pixels;
						pixel.A &= 0xF0; 
						pixel.R &= 0xF0; 
						pixel.G &= 0xF0;
						pixel.B &= 0xF0;
						*pixels = pixel;
						if (j < width - 1) {
							RGBA* p = pixels + 1; // pixel[x + 1][y]
							*p = AddQuantError(*p, 7, quantError); 
						}
						if (j > 0 && i < height - 1) {
							RGBA* p = pixels - 1 + stride / 4; // pixel[x - 1][y + 1]
							*p = AddQuantError(*p, 3, quantError); 
						}
						if (i < height - 1) {
							RGBA* p = pixels + stride / 4; // pixel[x][y + 1]
							*p = AddQuantError(*p, 5, quantError);
						}
						if (j < width - 1 && i < height - 1) {
							RGBA* p = pixels + 1 + stride / 4; // pixel[x + 1][y + 1]
							*p = AddQuantError(*p, 1, quantError);
						}
						pixels++;
					}
					pixels += stride / 4 - width;
				}
			}
		}

		private static void ReduceRGBTo4BitsPerChannelWithFloydSteinbergDithering(Pixbuf pixbuf)
		{
			int stride = pixbuf.Rowstride;
			unsafe {
				int width = pixbuf.Width;
				int height = pixbuf.Height;
				RGB* pixels = (RGB*)pixbuf.Pixels;
				for (int i = 0; i < height; i++) {
					for (int j = 0; j < width; j++) {
						RGB quantError = *pixels;
						quantError.R &= 0x0F;
						quantError.G &= 0x0F;
						quantError.B &= 0x0F;
						RGB pixel = *pixels;
						pixel.R &= 0xF0;
						pixel.G &= 0xF0;
						pixel.B &= 0xF0;
						*pixels = pixel;
						if (j < width - 1) {
							RGB* p = pixels + 1; // pixel[x + 1][y]
							*p = AddQuantError(*p, 7, quantError);
						}
						if (j > 0 && i < height - 1) {
							RGB* p = pixels - 1 + stride / 4; // pixel[x - 1][y + 1]
							*p = AddQuantError(*p, 3, quantError);
						}
						if (i < height - 1) {
							RGB* p = pixels + stride / 4; // pixel[x][y + 1]
							*p = AddQuantError(*p, 5, quantError);
						}
						if (j < width - 1 && i < height - 1) {
							RGB* p = pixels + 1 + stride / 4; // pixel[x + 1][y + 1]
							*p = AddQuantError(*p, 1, quantError);
						}
						pixels++;
					}
					pixels += stride / 4 - width;
				}
			}
		}

		private static RGBA AddQuantError(RGBA pixel, int koeff, RGBA error)
		{
			pixel.A = (byte)Math.Min((int)pixel.A + (error.A * koeff >> 4), 255);
			pixel.R = (byte)Math.Min((int)pixel.R + (error.R * koeff >> 4), 255);
			pixel.G = (byte)Math.Min((int)pixel.G + (error.G * koeff >> 4), 255);
			pixel.B = (byte)Math.Min((int)pixel.B + (error.B * koeff >> 4), 255);
			return pixel;
		}

		private static RGB AddQuantError(RGB pixel, int koeff, RGB error)
		{
			pixel.R = (byte)Math.Min((int)pixel.R + (error.R * koeff >> 4), 255);
			pixel.G = (byte)Math.Min((int)pixel.G + (error.G * koeff >> 4), 255);
			pixel.B = (byte)Math.Min((int)pixel.B + (error.B * koeff >> 4), 255);
			return pixel;
		}

		public static void SwapRGBChannels(Gdk.Pixbuf pixbuf)
		{
			if (pixbuf.HasAlpha) {
				unsafe {
					for (int i = 0; i < pixbuf.Height; i++) {
						RGBA* pixels = (RGBA*)((byte*)pixbuf.Pixels + pixbuf.Rowstride * i);
						int width = pixbuf.Width;
						for (int j = 0; j < width; j++) {
							RGBA c = *pixels;
							byte r = c.R;
							byte b = c.B;
							c.R = b;
							c.B = r;
							*pixels++ = c;
						}
					}
				}
			} else {
				unsafe {
					for (int i = 0; i < pixbuf.Height; i++) {
						byte* src = (byte*)pixbuf.Pixels + pixbuf.Rowstride * i;
						byte* dst = src;
						int width = pixbuf.Width;
						for (int j = 0; j < width; j++) {
							byte r = *src++;
							byte g = *src++;
							byte b = *src++;
							*dst++ = b;
							*dst++ = g;
							*dst++ = r;
						}
					}
				}
			}
		}
		public static unsafe void BleedAlpha(Gdk.Pixbuf pixbuf)
		{
			if (!pixbuf.HasAlpha) {
				return;
			}
			var pixels = (byte*)pixbuf.Pixels;
			if (pixbuf.Rowstride != pixbuf.Width * 4) {
				throw new InvalidOperationException();
			}
			BleedAlpha(pixels, pixbuf.Width, pixbuf.Height);
		}

		unsafe static void BleedAlpha(byte* image, int width, int height)
		{
			int N = width * height;
			var processed = new bool[N];
			var pending = new List<int>(N);
			var pendingNext = new List<int>(N);
			var hOffsets = new int[8] { -1, 0, 1, -1, 1, -1, 0, 1 };
			var vOffsets = new int[8] { -1, -1, -1, 0, 0, 1, 1, 1 };
			for (int i = 0, j = 3; i < N; i++, j += 4) {
				// If the pixel alpha != 0
				if (image[j] != 0) {
					processed[i] = true;
				} else {
					int x = i % width;
					int y = i / width;
					// Iterate across 8 adjacent pixels
					for (int k = 0; k < 8; k++) {
						int s = hOffsets[k];
						int t = vOffsets[k];
						if (x + s >= 0 && x + s < width && y + t >= 0 && y + t < height) {
							var index = j + 4 * (s + t * width);
							if (image[index] != 0) {
								pending.Add(i);
								break;
							}
						}
					}
				}
			}
			while (pending.Count > 0) {
				pendingNext.Clear();
				for (int p = 0; p < pending.Count; p++) {
					var j = pending[p];
					if (processed[j])
						continue;
					processed[j] = true;
					var i = pending[p] * 4;
					int x = j % width;
					int y = j / width;
					int r = 0;
					int g = 0;
					int b = 0;
					int count = 0;
					for (int k = 0; k < 8; k++) {
						int s = hOffsets[k];
						int t = vOffsets[k];
						if (x + s >= 0 && x + s < width && y + t >= 0 && y + t < height) {
							t *= width;
							if (processed[j + s + t]) {
								var index = i + 4 * (s + t);
								r += image[index + 0];
								g += image[index + 1];
								b += image[index + 2];
								count++;
							} else {
								pendingNext.Add(j + s + t);
							}
						}
					}
					if (count == 0) {
						throw new InvalidOperationException();
					}
					image[i + 0] = (byte)(r / count);
					image[i + 1] = (byte)(g / count);
					image[i + 2] = (byte)(b / count);
				}
				Lime.Toolbox.Swap(ref pending, ref pendingNext);
			}
		}
		
		public static void SaveToTGA(Gdk.Pixbuf pixbuf, string path)
		{
			using (Stream stream = new FileStream(path, FileMode.Create)) {
				using (BinaryWriter o = new BinaryWriter(stream)) {
					o.Write((byte)0); // size of ID field that follows 18 byte header(0 usually)
					o.Write((byte)0); // type of colour map 0 = none, 1 = has palette
					o.Write((byte)2); // type of image 0 = none, 1 = indexed, 2 = rgb, 3 = grey, +8 = rle packed
					o.Write((short)0); // first colour map entry in palette
					o.Write((short)0); // number of colours in palette
					o.Write((byte)0); // number of bits per palette entry 15,16,24,32
					o.Write((short)0); // image x origin
					o.Write((short)0); // image y origin
					o.Write((short)pixbuf.Width); // image width in pixels
					o.Write((short)pixbuf.Height); // image height in pixels
					o.Write((byte)(pixbuf.HasAlpha ? 32 : 24)); // image bits per pixel 8,16,24,32
					o.Write((byte)0); // descriptor
					int bpp = pixbuf.HasAlpha ? 4 : 3;
					byte[] buffer = new byte[pixbuf.Width * bpp];
					for (int i = pixbuf.Height - 1; i >= 0; i--) {
						Marshal.Copy(pixbuf.Pixels + pixbuf.Rowstride * i, buffer, 0, pixbuf.Width * bpp);
						o.Write(buffer, 0, buffer.Length);
					}
				}
			}
		}

		public static bool GetPngFileInfo(string path, out int width, out int height, out bool hasAlpha)
		{
			width = height = 0;
			hasAlpha = false;
			using(var stream = new FileStream(path, FileMode.Open)) {
				using(var reader = new BinaryReader(stream)) {
					byte[] sign = reader.ReadBytes(8); // PNG signature
					if (sign[1] != 'P' || sign[2] != 'N' || sign[3] != 'G')
						return false;
					reader.ReadBytes(4);
					reader.ReadBytes(4); // 'IHDR'
					width = IPAddress.NetworkToHostOrder(reader.ReadInt32());
					height = IPAddress.NetworkToHostOrder(reader.ReadInt32());
					reader.ReadByte(); // color depth
					int colorType = reader.ReadByte();
					hasAlpha = (colorType == 4) ||(colorType == 6);
				}
			}
			return true;
		}

		public static int GetNearestPowerOf2(int x, int min, int max)
		{
			int y = GetNearestPowerOf2Helper(x);
			x = (y - x < x - y / 2) ? y : y / 2;
			x = Math.Max(Math.Min(max, x), min);
			return x;
		}

		static int GetNearestPowerOf2Helper(int value)
		{
			if (!IsPowerOf2(value)) {
				int i = 1;
				while (i < value)
					i *= 2;
				return i;
			}
			return value;
		}

		public static bool IsPowerOf2(int value)
		{
			return value == 1 || (value & (value - 1)) == 0;
		}
	}
}
