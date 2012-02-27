using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;

namespace Orange
{
	public static class TextureConverterUtils
	{
		struct RGBA
		{
			public byte R, G, B, A;
		}

		public static void ReduceColorsToRGBA4444WithFloydSteinbergDithering(Gdk.Pixbuf pixbuf)
		{
			int width = pixbuf.Width;
			int height = pixbuf.Height;
			int stride = pixbuf.Rowstride;
			if ((stride & 0x3) != 0) {
				throw new Lime.Exception("Invalid pixbuf format");
			}
			unsafe {
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

		private static RGBA AddQuantError(RGBA pixel, int koeff, RGBA error)
		{
			pixel.A = (byte)Math.Min((int)pixel.A + (error.A * koeff >> 4), 255);
			pixel.R = (byte)Math.Min((int)pixel.R + (error.R * koeff >> 4), 255);
			pixel.G = (byte)Math.Min((int)pixel.G + (error.G * koeff >> 4), 255);
			pixel.B = (byte)Math.Min((int)pixel.B + (error.B * koeff >> 4), 255);
			return pixel;
		}
	
		public static void SwapChannels(Gdk.Pixbuf pixbuf)
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
		
		
		public static void PremultiplyAlpha(Gdk.Pixbuf pixbuf, bool swapChannels)
		{
			if ((pixbuf.Rowstride & 0x3) != 0) {
				throw new Lime.Exception("Invalid pixbuf format");
			}
			unsafe {
				RGBA* pixels = (RGBA*)pixbuf.Pixels;
				int width = pixbuf.Width;
				for (int i = 0; i < pixbuf.Height; i++) {
					for (int j = 0; j < width; j++) {
						RGBA c = *pixels;
						if (c.A == 0) {
							c.R = c.G = c.B = 0;
						} else if (c.A < 255) {
							c.R = (byte)(c.R * c.A / 255);
							c.G = (byte)(c.G * c.A / 255);
							c.B = (byte)(c.B * c.A / 255);
						}
						if (swapChannels) {
							RGBA c2;
							c2.A = c.A;
							c2.R = c.B;
							c2.G = c.G;
							c2.B = c.R;
							*pixels = c2;
						} else {
							*pixels = c;
						}
						pixels++;
					}
					pixels += pixbuf.Rowstride / 4 - width;
				}
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
