using System;
using System.IO;
using Lime;

namespace Orange
{
	public static class TextureCompressor
	{
		private static void SaveAs16BitImage (string path, Stream output)
		{
			var pixbuf = new Gdk.Pixbuf (path);
			var writer = new BinaryWriter (output);
			writer.Write ('R');
			writer.Write ('A');
			writer.Write ('W');
			writer.Write (' ');
			writer.Write ((Int32)pixbuf.Width);
			writer.Write ((Int32)pixbuf.Height);
			writer.Write ((Int32)pixbuf.Width);  
			writer.Write ((Int32)pixbuf.Height);
			if (pixbuf.HasAlpha) {
				int count = pixbuf.Width * pixbuf.Height;
				writer.Write ((Int32)BitmapFormat.RGBA4);
				writer.Write ((Int32)(count * 2));
				byte [] pixels1 = new byte [count * 2];
				unsafe {
					fixed (byte* dest = pixels1) {						
						short* dest1 = (short*)dest;
						for (int j = 0; j < pixbuf.Height; j++) {
							byte* source = (byte*)pixbuf.Pixels + pixbuf.Rowstride * j;
							for (int i = 0; i < pixbuf.Width; i++) {							
								int R = (*source++ >> 4) << 12;
								int G = (*source++ >> 4) << 8;
								int B = (*source++ >> 4) << 4;
								int A = (*source++ >> 4);
								*dest1++ = (short)(R | G | B | A);
							}
						}
					}
					writer.Write (pixels1, 0, count * 2);
				}
			} else {
				int count = pixbuf.Width * pixbuf.Height;
				writer.Write ((Int32)BitmapFormat.R5G6B5);
				writer.Write ((Int32)(count * 2));
				byte [] pixels1 = new byte [count * 2];
				unsafe {
					fixed (byte* dest = pixels1) {						
						short* dest1 = (short*)dest;
						for (int j = 0; j < pixbuf.Height; j++) {
							byte* source = (byte*)pixbuf.Pixels + pixbuf.Rowstride * j;
							for (int i = 0; i < pixbuf.Width; i++) {
								int R = (*source++ >> 3) << 11;
								int G = (*source++ >> 2) << 5;
								int B = (*source++ >> 3);
								*dest1++ = (short)(R | G | B);
							}
						}
					}
				}
				writer.Write (pixels1, 0, count * 2);
			}
			writer.Flush ();
		}
		
		public static void CompressTexture (string path, Stream output)
		{
			SaveAs16BitImage (path, output);
		}
	}
}

