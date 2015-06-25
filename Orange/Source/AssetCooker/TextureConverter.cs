using System;
using System.IO;
using System.Net;
using Gtk;
using Lime;
using System.Runtime.InteropServices;

namespace Orange
{
	public static class TextureConverter
	{
		public enum PVRCompressionScheme
		{
			PVRTC4,
			ETC1,
		}

		public static void ToPVR(Gdk.Pixbuf pixbuf, string dstPath, bool mipMaps, PVRFormat pvrFormat, PVRCompressionScheme compression)
		{
			int width = pixbuf.Width;
			int height = pixbuf.Height;
			bool hasAlpha = pixbuf.HasAlpha;

			int potWidth = TextureConverterUtils.GetNearestPowerOf2(width, 8, 1024);
			int potHeight = TextureConverterUtils.GetNearestPowerOf2(height, 8, 1024);

			string args = "";
			switch (pvrFormat) {
				case PVRFormat.Compressed:
					if (compression == PVRCompressionScheme.PVRTC4) {
						if (pixbuf.HasAlpha) {
							args += " -f PVRTC1_4";
						} else {
							args += " -f PVRTC1_2";
						}
						width = height = Math.Max(potWidth, potHeight);
					} else {
						args = " -f ETC1 -q etcfast";
					}
					break;
				case PVRFormat.RGB565:
					if (hasAlpha) {
						Console.WriteLine("WARNING: texture has alpha channel. Used 'RGBA4444' format instead of 'RGB565'.");
						args += " -f r4g4b4a4";
						TextureConverterUtils.ReduceTo4BitsPerChannelWithFloydSteinbergDithering(pixbuf);
					} else {
						args += " -f r5g6b5";
					}
					break;
				case PVRFormat.RGBA4:
					args += " -f r4g4b4a4";
					TextureConverterUtils.ReduceTo4BitsPerChannelWithFloydSteinbergDithering(pixbuf);
					break;
				case PVRFormat.ARGB8:
					args += " -f r8g8b8a8";
					break;
			}
			string tga = Path.ChangeExtension(dstPath, ".tga");
			try {
				if (pixbuf.HasAlpha) {
					args += " -l"; // Enable alpha bleed
				}
				TextureConverterUtils.SwapRGBChannels(pixbuf);
				TextureConverterUtils.SaveToTGA(pixbuf, tga);
				if (mipMaps) {
					args += " -m";
				}
				args += String.Format(" -i \"{0}\" -o \"{1}\" -r {2},{3} -shh", tga, dstPath, width, height);
#if MAC
				var pvrTexTool = Path.Combine(Toolbox.GetApplicationDirectory(), "Toolchain.Mac", "PVRTexTool");
				Mono.Unix.Native.Syscall.chmod(pvrTexTool, Mono.Unix.Native.FilePermissions.S_IXOTH | Mono.Unix.Native.FilePermissions.S_IXUSR);
#else
				var pvrTexTool = Path.Combine(Toolbox.GetApplicationDirectory(), "Toolchain.Win", "PVRTexToolCli");
#endif
				int result = Process.Start(pvrTexTool, args);
				if (result != 0) {
					throw new Lime.Exception("Failed to convert '{0}' to PVR format(error code: {1})", tga, result);
				}
			} finally {
				DeletePossibleLockedFile(tga);
			}
		}

		private static void DeletePossibleLockedFile(string file)
		{
			while (true) {
				try {
					File.Delete(file);
				} catch (System.Exception) {
					System.Threading.Thread.Sleep(100);
					continue;
				}
				break;
			}
		}

		public static void ToDDS(Gdk.Pixbuf pixbuf, string dstPath, DDSFormat format, bool mipMaps)
		{
			bool compressed = format == DDSFormat.DXTi;
			if (pixbuf.HasAlpha) {
				TextureConverterUtils.BleedAlpha(pixbuf);
			}
			if (compressed) {
				TextureConverterUtils.SwapRGBChannels(pixbuf);
			}
			var tga = Path.ChangeExtension(dstPath, ".tga");
			try {
				TextureConverterUtils.SaveToTGA(pixbuf, tga);
				ToDDSTextureHelper(tga, dstPath, pixbuf.HasAlpha, compressed, mipMaps);
			} finally {
				File.Delete(tga);
			}
		}
		
		private static void ToDDSTextureHelper(string srcPath, string dstPath, bool hasAlpha, bool compressed, bool mipMaps)
		{
			string mipsFlag = mipMaps ? "" : "-nomips";
			string compressionFlag = compressed ? (hasAlpha ? "-bc3" : "-bc1") : "-rgb";
#if WIN
			string nvcompress = Path.Combine(Toolbox.GetApplicationDirectory(), "Toolchain.Win", "nvcompress.exe");
			srcPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), srcPath);
			dstPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), dstPath);
			string args = String.Format("{0} {1} \"{2}\" \"{3}\"", mipsFlag, compressionFlag, srcPath, dstPath);
			int result = Process.Start(nvcompress, args, Process.Options.RedirectErrors);
			if (result != 0) {
				throw new Lime.Exception("Failed to convert '{0}' to DDS format(error code: {1})", srcPath, result);
			}
#else
			string nvcompress = Path.Combine(Toolbox.GetApplicationDirectory(), "Toolchain.Mac", "nvcompress");
			Mono.Unix.Native.Syscall.chmod(nvcompress, Mono.Unix.Native.FilePermissions.S_IXOTH | Mono.Unix.Native.FilePermissions.S_IXUSR);
			string args = String.Format("{0} {1} '{2}' '{3}'", mipsFlag, compressionFlag, srcPath, dstPath);
			Console.WriteLine(nvcompress);
			var psi = new System.Diagnostics.ProcessStartInfo(nvcompress, args);
			var p = System.Diagnostics.Process.Start(psi);
			while (!p.HasExited) {
				p.WaitForExit();
			}
			if (p.ExitCode != 0) {
				throw new Lime.Exception("Failed to convert '{0}' to DDS format(error code: {1})", srcPath, p.ExitCode);
			}
#endif
		}

		public static void ToJPG(Gdk.Pixbuf pixbuf, string dstPath, bool mipMaps)
		{
			if (pixbuf.HasAlpha) {
				TextureConverterUtils.BleedAlpha(pixbuf);
			}
			pixbuf.Savev(dstPath, "jpeg", new string[] { "quality" }, new string[] { "80" });
		}

		public static void ToGrayscaleAlphaPNG(Gdk.Pixbuf pixbuf, string dstPath, bool mipMaps)
		{
			TextureConverterUtils.ConvertBitmapToGrayscaleAlphaMask(pixbuf);
			pixbuf.Save(dstPath, "png");
			var pngcrushTool = Path.Combine(Toolbox.GetApplicationDirectory(), "Toolchain.Win", "pngcrush_1_7_83_w32");
			// -ow - overwrite
			// -s - silent
			// -c 0 - change color type to greyscale
			dstPath = MakeAbsolutePath(dstPath);
			var args = String.Format("-ow -s -c 0 \"{0}\"", dstPath);
			int result = Process.Start(pngcrushTool, args, Process.Options.RedirectErrors);
			if (result != 0) {
				throw new Lime.Exception("Error converting '{0}'\nCommand line: {1}", dstPath, pngcrushTool + " " + args);
			}
		}

		private static string MakeAbsolutePath(string path)
		{
			if (!Path.IsPathRooted(path)) {
				path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), path);
			}
			return path;
		}
	}
}
