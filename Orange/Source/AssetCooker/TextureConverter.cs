using System;
using System.IO;
using System.Linq;
using System.Text;
using Lime;

namespace Orange
{
	public static class TextureConverter
	{
		public static void ToPVR(Bitmap bitmap, string dstPath, bool mipMaps, bool highQualityCompression, PVRFormat pvrFormat)
		{
			int width = bitmap.Width;
			int height = bitmap.Height;
			bool hasAlpha = bitmap.HasAlpha;

			int potWidth = TextureConverterUtils.GetNearestPowerOf2(width, 8, 1024);
			int potHeight = TextureConverterUtils.GetNearestPowerOf2(height, 8, 1024);
			var args = new StringBuilder();
			switch (pvrFormat) {
				case PVRFormat.PVRTC4:
					if (!hasAlpha) {
						args.Append(" -f PVRTC1_2");
					} else {
						args.Append(" -f PVRTC1_4");
					}
					width = height = Math.Max(potWidth, potHeight);
					break;
				case PVRFormat.PVRTC4_Forced:
					args.Append(" -f PVRTC1_4");
					width = height = Math.Max(potWidth, potHeight);
					break;
				case PVRFormat.PVRTC2:
					args.Append(" -f PVRTC1_2");
					width = height = Math.Max(potWidth, potHeight);
					break;
				case PVRFormat.ETC1:
					args.Append(" -f ETC1 -q etcfast");
					break;
				case PVRFormat.RGB565:
					if (hasAlpha) {
						Console.WriteLine("WARNING: texture has alpha channel. " +
							"Used 'RGBA4444' format instead of 'RGB565'.");
						args.Append(" -f r4g4b4a4 -dither");
					} else {
						args.Append(" -f r5g6b5");
					}
					break;
				case PVRFormat.RGBA4:
					args.Append(" -f r4g4b4a4 -dither");
					break;
				case PVRFormat.ARGB8:
					args.Append(" -f r8g8b8a8");
					break;
			}
			if (highQualityCompression && (new [] { PVRFormat.PVRTC2, PVRFormat.PVRTC4, PVRFormat.PVRTC4_Forced }.Contains (pvrFormat))) {
				args.Append(" -q pvrtcbest");
			}
			string tga = Path.ChangeExtension(dstPath, ".tga");
			try {
				if (hasAlpha) {
					args.Append(" -l"); // Enable alpha bleed
				}
				TextureConverterUtils.SaveToTGA(bitmap, tga, swapRedAndBlue: true);
				if (mipMaps) {
					args.Append(" -m");
				}
				args.AppendFormat(" -i \"{0}\" -o \"{1}\" -r {2},{3} -shh", tga, dstPath, width, height);
#if MAC
				var pvrTexTool = Path.Combine(Toolbox.GetApplicationDirectory(), "Toolchain.Mac", "PVRTexTool");
				Mono.Unix.Native.Syscall.chmod(
					pvrTexTool, Mono.Unix.Native.FilePermissions.S_IXOTH | Mono.Unix.Native.FilePermissions.S_IXUSR);
#else
				var pvrTexTool = Path.Combine(Toolbox.GetApplicationDirectory(), "Toolchain.Win", "PVRTexToolCli");
#endif
				int result = Process.Start(pvrTexTool, args.ToString());
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
				}
				catch (System.Exception) {
					System.Threading.Thread.Sleep(100);
					continue;
				}
				break;
			}
		}

		public static void ToDDS(Bitmap bitmap, string dstPath, DDSFormat format, bool mipMaps)
		{
			bool compressed = format == DDSFormat.DXTi;
			Bitmap bledBitmap = null;
			if (bitmap.HasAlpha) {
				bledBitmap = TextureConverterUtils.BleedAlpha(bitmap);
			}
			var tga = Path.ChangeExtension(dstPath, ".tga");
			TextureConverterUtils.SaveToTGA(bledBitmap ?? bitmap, tga, swapRedAndBlue: compressed);
			if (bledBitmap != null && bledBitmap != bitmap) {
				bledBitmap.Dispose();
			}
			ToDDSTextureHelper(tga, dstPath, bitmap.HasAlpha, compressed, mipMaps);

			// Do not delete the bitmap if an exception has occurred
			File.Delete(tga);
		}

		private static void ToDDSTextureHelper(
			string srcPath, string dstPath, bool hasAlpha, bool compressed, bool mipMaps)
		{
			string mipsFlag = mipMaps ? string.Empty : "-nomips";
			string compressionMethod;
			if (compressed) {
				compressionMethod = hasAlpha ? "-bc3" : "-bc1";
			} else {
#if WIN
				compressionMethod = "-rgb";
#else
				compressionMethod = "-rgb -rgbfmt bgra8";
#endif
			}
#if WIN
			string nvcompress = Path.Combine(Toolbox.GetApplicationDirectory(), "Toolchain.Win", "nvcompress.exe");
#else
			string nvcompress = Path.Combine(Toolbox.GetApplicationDirectory(), "Toolchain.Mac", "nvcompress");
			Mono.Unix.Native.Syscall.chmod(
				nvcompress, Mono.Unix.Native.FilePermissions.S_IXOTH | Mono.Unix.Native.FilePermissions.S_IXUSR);
#endif
			srcPath = Path.Combine(Directory.GetCurrentDirectory(), srcPath);
			dstPath = Path.Combine(Directory.GetCurrentDirectory(), dstPath);
			string args = string.Format("{0} {1} \"{2}\" \"{3}\"", mipsFlag, compressionMethod, srcPath, dstPath);
			int result = Process.Start(nvcompress, args, Process.Options.RedirectErrors);
			if (result != 0) {
				throw new Lime.Exception("Failed to convert '{0}' to DDS format(error code: {1})", srcPath, result);
			}
		}

		public static void ToJPG(Bitmap bitmap, string dstPath)
		{
			Bitmap bledBitmap = null;
			if (bitmap.HasAlpha) {
				bledBitmap = TextureConverterUtils.BleedAlpha(bitmap);
			}
			using (var stream = File.OpenWrite(dstPath)) {
				(bledBitmap ?? bitmap).SaveTo(stream, CompressionFormat.Jpeg);
			}
			if (bledBitmap != null && bledBitmap != bitmap) {
				bledBitmap.Dispose();
			}
		}

		public static void ToPNG(Bitmap bitmap, string dstPath)
		{
			using (var stream = File.OpenWrite(dstPath)) {
				bitmap.SaveTo(stream, CompressionFormat.Png);
			}
			var pngcrushTool = Path.Combine(Toolbox.GetApplicationDirectory(), "Toolchain.Win", "pngcrush_1_7_83_w32");
			// -ow - overwrite
			// -s - silent
			// -c 0 - change color type to greyscale
			dstPath = MakeAbsolutePath(dstPath);
			var args = string.Format("-ow -s -c 0 \"{0}\"", dstPath);
			int result = Process.Start(pngcrushTool, args, Process.Options.RedirectErrors);
			if (result != 0) {
				throw new Lime.Exception(
					"Error converting '{0}'\nCommand line: {1}", dstPath, pngcrushTool + ' ' + args);
			}
		}

		private static string MakeAbsolutePath(string path)
		{
			if (!Path.IsPathRooted(path)) {
				path = Path.Combine(Directory.GetCurrentDirectory(), path);
			}
			return path;
		}
	}
}
