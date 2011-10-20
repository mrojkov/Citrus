using System;
using System.IO;
using System.Net;
using Lime;

namespace Orange
{
	public static class TextureConverter
	{
		public static bool GetPngFileInfo (string path, out int width, out int height, out bool hasAlpha)
		{
			width = height = 0;
			hasAlpha = false;
			using (var stream = new FileStream (path, FileMode.Open)) {
				using (var reader = new BinaryReader (stream)) {
					byte[] sign = reader.ReadBytes (8); // PNG signature
					if (sign [1] != 'P' || sign [2] != 'N' || sign [3] != 'G')
						return false;
					reader.ReadBytes (4);
					reader.ReadBytes (4); // 'IHDR'
					width = IPAddress.NetworkToHostOrder (reader.ReadInt32());
	            	height = IPAddress.NetworkToHostOrder (reader.ReadInt32());
					reader.ReadByte (); // color depth
					int colorType = reader.ReadByte ();
					hasAlpha = (colorType == 4) || (colorType == 6);
				}
			}
			return true;
		}
		
		private static int GetNearestPowerOf2 (int x, int min, int max)
		{
			int y = Utils.NearestPowerOf2 (x);
			x = (y - x < x - y / 2) ? y : y / 2;
			x = Math.Max (Math.Min (max, x), min);
			return x;
		
		}
		
		private static void ToPVRTexture (string srcPath, string dstPath, bool compressed, bool mipMaps)
		{
			int width, height;
			bool hasAlpha;
			if (!GetPngFileInfo (srcPath, out width, out height, out hasAlpha)) {
				throw new Lime.Exception ("Wrong png file: " + srcPath);
			}

			int potWidth = GetNearestPowerOf2 (width, 8, 1024);
			int potHeight = GetNearestPowerOf2 (height, 8, 1024);
			
			int maxDimension = Math.Max (potWidth, potHeight);
			int pvrtc4DataLength = maxDimension * maxDimension / 2;
			int rgba16DataLength = potWidth * potHeight * 2;
			
			string formatFlag;
			if (rgba16DataLength < pvrtc4DataLength) {
				if (hasAlpha)
					formatFlag = "OGL4444";
				else
					formatFlag = "OGL565";
			} else {
				formatFlag = "PVRTC4";
				potWidth = potHeight = maxDimension;
			}
			string mipsFlag = mipMaps ? "-m" : "";
			string pvrTexTool = Path.Combine (Helpers.GetApplicationDirectory (), "PVRTexTool", "PVRTexTool");
			string args = String.Format ("-f {0} -i '{1}' -o '{2}' {3} -pvrtcfast -premultalpha -silent -x {4} -y {5}", 
				formatFlag,	srcPath, dstPath, mipsFlag, potWidth, potHeight);
			var p = System.Diagnostics.Process.Start (pvrTexTool, args);
			p.WaitForExit ();
			if (p.ExitCode != 0) {
				throw new Lime.Exception ("Failed to convert '{0}' to PVR format (error code: {1})", srcPath, p.ExitCode);
			}
		}

		private static void ToDDSTexture (string srcPath, string dstPath, bool compressed, bool mipMaps)
		{
			int width, height;
			bool hasAlpha;
			GetPngFileInfo (srcPath, out width, out height, out hasAlpha);
			if (!GetPngFileInfo (srcPath, out width, out height, out hasAlpha)) {
				throw new Lime.Exception ("Wrong png file: " + srcPath);
			}			
			string mipsFlag = mipMaps ? "" : "-nomips";
			string compressionFlag = compressed ? (hasAlpha ? "-bc3" : "-bc1") : "-rgb";
			string pvrTexTool = Path.Combine (Helpers.GetApplicationDirectory (), "NVCompress", "nvcompress");
			string args = String.Format ("-premula -silent -fast {0} {1} '{2}' '{3}'", mipsFlag, compressionFlag, srcPath, dstPath);
			var p = System.Diagnostics.Process.Start (pvrTexTool, args);
			p.WaitForExit ();
			if (p.ExitCode != 0) {
				throw new Lime.Exception ("Failed to convert '{0}' to DDS format (error code: {1})", srcPath, p.ExitCode);
			}
		}
		
		private static void SaveAs16BitImage (string srcPath, string dstPath)
		{
			var pixbuf = new Gdk.Pixbuf (srcPath);
			using (var stream = new FileStream (dstPath, FileMode.Create)) {
				var writer = new BinaryWriter (stream);
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
		}
		
		public static void Convert (string srcPath, string dstPath, bool compressed, bool mipMaps, TargetPlatform platform)
		{
			if (Path.GetExtension (dstPath) == ".pvr") {
				ToPVRTexture (srcPath, dstPath, compressed, mipMaps);
			}
			else if (Path.GetExtension (dstPath) == ".dds") {
				ToDDSTexture (srcPath, dstPath, compressed, mipMaps);
			}
			else {
				throw new Lime.Exception ("Unknown texture format for: {0}", dstPath);
			}
		}
	}
}
