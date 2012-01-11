using System;
using System.IO;
using System.Net;
using Lime;
using System.Runtime.InteropServices;

namespace Orange
{
	public static class TextureConverter
	{
		struct RGBA
		{
			public byte R, G, B, A;
		}
		
		private static void PremultiplyAlpha(Gdk.Pixbuf pixbuf, bool swapChannels)
		{
			unsafe {
				RGBA* pixels = (RGBA*)pixbuf.Pixels;
				for (int i = 0; i < pixbuf.Height; i++) {
					int h = pixbuf.Width;
					for (int j = 0; j < h; j++) {
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
					pixels += pixbuf.Rowstride - h * 4;
				}
			}
		}
		
		private static void SaveToTGA(Gdk.Pixbuf pixbuf, string path)
		{
			using(Stream stream = new FileStream(path, FileMode.Create)) {
				using(BinaryWriter o = new BinaryWriter(stream)) {
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
					o.Write((byte)32); // image bits per pixel 8,16,24,32
					o.Write((byte)0); // descriptor
					byte[] buffer = new byte[pixbuf.Width * 4];
					for (int i = pixbuf.Height - 1; i >= 0; i--) {
						Marshal.Copy(pixbuf.Pixels + pixbuf.Rowstride * i, buffer, 0, pixbuf.Width * 4);
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
		
		private static int GetNearestPowerOf2(int x, int min, int max)
		{
			int y = Utils.NearestPowerOf2(x);
			x = (y - x < x - y / 2) ? y : y / 2;
			x = Math.Max(Math.Min(max, x), min);
			return x;
		
		}
		
		private static void ToPVRTexture(string srcPath, string dstPath, bool compressed, bool mipMaps)
		{
			int width, height;
			bool hasAlpha;
			if (!GetPngFileInfo(srcPath, out width, out height, out hasAlpha)) {
				throw new Lime.Exception("Wrong png file: " + srcPath);
			}
			int potWidth = GetNearestPowerOf2(width, 8, 1024);
			int potHeight = GetNearestPowerOf2(height, 8, 1024);
			
			// HACK, just for testing purposes
			//if (potWidth > 8 && potHeight > 8) {
			//	potWidth /= 2;
			//	potHeight /= 2;
			//}
			
			int maxDimension = Math.Max(potWidth, potHeight);
			int pvrtc4DataLength = maxDimension * maxDimension / 2;
			int rgba16DataLength = potWidth * potHeight * 2;
			
			string formatFlag;
			if (rgba16DataLength < pvrtc4DataLength) {
				if (hasAlpha)
					formatFlag = "-f 4444 -nt";
				else
					formatFlag = "-f 565 -nt";
			} else {
				formatFlag = "-f PVRTC4";
				potWidth = potHeight = maxDimension;
			}
			string mipsFlag = mipMaps ? "-m" : "";
			string pvrTexTool = Path.Combine(Helpers.GetApplicationDirectory(), "Toolchain.Mac", "PVRTexTool");
			Mono.Unix.Native.Syscall.chmod(pvrTexTool, Mono.Unix.Native.FilePermissions.S_IXOTH | Mono.Unix.Native.FilePermissions.S_IXUSR);
			string args = String.Format("{0} -i '{1}' -o '{2}' {3} -pvrtcfast -premultalpha -silent -x {4} -y {5}", 
				formatFlag,	srcPath, dstPath, mipsFlag, potWidth, potHeight);
			var p = System.Diagnostics.Process.Start(pvrTexTool, args);
			p.WaitForExit();
			if (p.ExitCode != 0) {
				throw new Lime.Exception("Failed to convert '{0}' to PVR format(error code: {1})", srcPath, p.ExitCode);
			}
		}
		
		private static void ToDDSTexture(string srcPath, string dstPath, bool compressed, bool mipMaps)
		{
			int width, height;
			bool hasAlpha;
			GetPngFileInfo(srcPath, out width, out height, out hasAlpha);
			if (!GetPngFileInfo(srcPath, out width, out height, out hasAlpha)) {
				throw new Lime.Exception("Wrong png file: " + srcPath);
			}
			if (hasAlpha) {
				var pixbuf = new Gdk.Pixbuf(srcPath);
				PremultiplyAlpha(pixbuf, true);
				string tga = Path.ChangeExtension(srcPath, ".tga");
				try {
					SaveToTGA(pixbuf, tga);
					ToDDSTextureHelper(tga, dstPath, true, compressed, mipMaps);
				} finally {
					File.Delete(tga);
				}
			} else {
				ToDDSTextureHelper(srcPath, dstPath, false, compressed, mipMaps);
			}
		}
		
		private static void ToDDSTextureHelper(string srcPath, string dstPath, bool hasAlpha, bool compressed, bool mipMaps)
		{
			string mipsFlag = mipMaps ? "" : "-nomips";
			string compressionFlag = compressed ? (hasAlpha ? "-bc3" : "-bc1") : "-rgb";
#if WIN
			string nvcompress = Path.Combine(Helpers.GetApplicationDirectory(), "Toolchain.Win", "nvcompress.exe");
			string args = String.Format("-silent -fast {0} {1} \"{2}\" \"{3}\"", mipsFlag, compressionFlag, srcPath, dstPath);
			var psi = new System.Diagnostics.ProcessStartInfo(nvcompress, args);
			psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			var p = System.Diagnostics.Process.Start(psi);
#else
			string nvcompress = Path.Combine(Helpers.GetApplicationDirectory(), "Toolchain.Mac", "nvcompress");
			Mono.Unix.Native.Syscall.chmod(nvcompress, Mono.Unix.Native.FilePermissions.S_IXOTH | Mono.Unix.Native.FilePermissions.S_IXUSR);
			string args = String.Format("-silent -fast {0} {1} '{2}' '{3}'", mipsFlag, compressionFlag, srcPath, dstPath);
			var psi = new System.Diagnostics.ProcessStartInfo(nvcompress, args);
			var p = System.Diagnostics.Process.Start(psi);
#endif		
			while (!p.HasExited) {
				p.WaitForExit();
			}
			if (p.ExitCode != 0) {
				throw new Lime.Exception("Failed to convert '{0}' to DDS format(error code: {1})", srcPath, p.ExitCode);
			}
		}
		
		public static void Convert(string srcPath, string dstPath, bool compressed, bool mipMaps, TargetPlatform platform)
		{
			if (Path.GetExtension(dstPath) == ".pvr") {
				ToPVRTexture(srcPath, dstPath, compressed, mipMaps);
			}
			else if (Path.GetExtension(dstPath) == ".dds") {
				ToDDSTexture(srcPath, dstPath, compressed, mipMaps);
			}
			else {
				throw new Lime.Exception("Unknown texture format for: {0}", dstPath);
			}
		}
	}
}
