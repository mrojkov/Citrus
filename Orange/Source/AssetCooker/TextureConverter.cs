using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Lime;

namespace Orange
{
	public static class TextureConverter
	{
		public static void RunEtcTool(Bitmap bitmap, AssetBundle bundle, string path, AssetAttributes attributes, bool mipMaps, bool highQualityCompression, byte[] CookingRulesSHA1)
		{
			var stream = new MemoryStream();
			bitmap.SaveTo(stream);
			stream.WriteByte(mipMaps ? (byte)1 : (byte)0);
			stream.WriteByte(highQualityCompression ? (byte)1 : (byte)0);
			if (CookingRulesSHA1 != null) {
				stream.Write(CookingRulesSHA1, 0, CookingRulesSHA1.Length);
			}
			stream.Position = 0;
			var hashValueString = BitConverter.ToString(SHA256.Create().ComputeHash(stream));
			var cacheDirectory = Path.Combine(The.Workspace.ProjectCacheDirectory, "etcCache",
				hashValueString[0].ToString(), hashValueString[1].ToString(), "");
			var cachePath = Path.Combine(cacheDirectory, hashValueString.Substring(2));
			if (File.Exists(cachePath)) {
				bundle.ImportFile(cachePath, path, 0, "", attributes, CookingRulesSHA1);
				return;
			}

			var hasAlpha = bitmap.HasAlpha;
			var bledBitmap = hasAlpha ? TextureConverterUtils.BleedAlpha(bitmap) : null;
			var ktxPath = Toolbox.GetTempFilePathWithExtension(".ktx");
			var pngPath = Path.ChangeExtension(ktxPath, ".png");
			try {
				(bledBitmap ?? bitmap).SaveTo(pngPath);
				var etcTool = GetToolPath("EtcTool");
				var args =
					$"{pngPath} -format " + (hasAlpha ? "RGBA8" : "RGB8") +
					" -jobs 8 " +
					" -effort " + (highQualityCompression ? "60" : "40") +
					(mipMaps ? " -mipmaps 4" : "") +
					$" -output {ktxPath}";
				if (Process.Start(etcTool, args) != 0) {
					throw new Lime.Exception($"ETCTool error\nCommand line: {etcTool} {args}\"");
				}
				bundle.ImportFile(ktxPath, path, 0, "", attributes, CookingRulesSHA1);
				Directory.CreateDirectory(cacheDirectory);
				File.Copy(ktxPath, cachePath);
			} finally {
				bledBitmap?.Dispose();
				DeletePossibleLockedFile(pngPath);
				DeletePossibleLockedFile(ktxPath);
			}
		}

		public static void RunPVRTexTool(Bitmap bitmap, AssetBundle bundle, string path, AssetAttributes attributes, bool mipMaps, bool highQualityCompression, PVRFormat pvrFormat, byte[] CookingRulesSHA1)
		{
			int width = bitmap.Width;
			int height = bitmap.Height;
			bool hasAlpha = bitmap.HasAlpha;

			int potWidth = TextureConverterUtils.GetNearestPowerOf2(width, 8, 2048);
			int potHeight = TextureConverterUtils.GetNearestPowerOf2(height, 8, 2048);
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
				case PVRFormat.ETC2:
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
			var pvrPath = Toolbox.GetTempFilePathWithExtension(".pvr");
			var tgaPath = Path.ChangeExtension(pvrPath, ".tga");
			try {
				if (hasAlpha) {
					bitmap = TextureConverterUtils.BleedAlpha(bitmap);
				}
				TextureConverterUtils.SaveToTGA(bitmap, tgaPath, swapRedAndBlue: true);
				if (mipMaps) {
					args.Append(" -m");
				}
				args.AppendFormat(" -i \"{0}\" -o \"{1}\" -r {2},{3} -shh", tgaPath, pvrPath, width, height);
#if MAC
				var pvrTexTool = GetToolPath("PVRTexTool");
#else
				var pvrTexTool = GetToolPath("PVRTexToolCli");
#endif
				if (Process.Start(pvrTexTool, args.ToString()) != 0) {
					throw new Lime.Exception($"PVRTextTool error\nCommand line: {pvrTexTool} {args}\"");
				}
				bundle.ImportFile(pvrPath, path, 0, "", attributes, CookingRulesSHA1);
			} finally {
				DeletePossibleLockedFile(tgaPath);
				DeletePossibleLockedFile(pvrPath);
			}
		}

		public static void RunNVCompress(Bitmap bitmap, AssetBundle bundle, string path, AssetAttributes attributes, DDSFormat format, bool mipMaps, byte[] CookingRulesSHA1)
		{
			bool compressed = format == DDSFormat.DXTi;
			Bitmap bledBitmap = null;
			if (bitmap.HasAlpha) {
				bledBitmap = TextureConverterUtils.BleedAlpha(bitmap);
			}
			var ddsPath = Toolbox.GetTempFilePathWithExtension(".dds");
			var tgaPath = Path.ChangeExtension(ddsPath, ".tga");
			try {
				TextureConverterUtils.SaveToTGA(bledBitmap ?? bitmap, tgaPath, swapRedAndBlue: compressed);
				if (bledBitmap != null && bledBitmap != bitmap) {
					bledBitmap.Dispose();
				}
				RunNVCompressHelper(tgaPath, ddsPath, bitmap.HasAlpha, compressed, mipMaps);
				bundle.ImportFile(ddsPath, path, 0, "", attributes, CookingRulesSHA1);
			} finally {
				DeletePossibleLockedFile(ddsPath);
				DeletePossibleLockedFile(tgaPath);
			}
		}

		private static void RunNVCompressHelper(
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
			var nvcompress = GetToolPath("nvcompress");
			srcPath = Path.Combine(Directory.GetCurrentDirectory(), srcPath);
			dstPath = Path.Combine(Directory.GetCurrentDirectory(), dstPath);
			string args = string.Format("{0} {1} \"{2}\" \"{3}\"", mipsFlag, compressionMethod, srcPath, dstPath);
			if (Process.Start(nvcompress, args, Process.Options.RedirectErrors) != 0) {
				throw new Lime.Exception($"NVCompress error\nCommand line: {nvcompress} {args}\"");
			}
		}

		// Invokes PngOptimizerCL on png with option not to optimize idat chunk.
		// It removes all unwanted chunks like itxt and text.
		public static void OptimizePNG(string dstPath)
		{
			var pngOptimizerPath = GetToolPath("PngOptimizerCL");
			dstPath = MakeAbsolutePath(dstPath);
			var args = $"--KeepPixels \"{dstPath}\"";
			if (Process.Start(pngOptimizerPath, args, Process.Options.RedirectErrors) != 0) {
				throw new Lime.Exception($"Error converting '{dstPath}'\nCommand line: {pngOptimizerPath} {args}");
			}
		}

		private static string MakeAbsolutePath(string path)
		{
			if (!Path.IsPathRooted(path)) {
				path = Path.Combine(Directory.GetCurrentDirectory(), path);
			}
			return path;
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

		private static string GetToolPath(string tool)
		{
			var appDirectory = Toolbox.GetApplicationDirectory();
#if MAC
			var toolChainDirectory = Path.Combine(appDirectory, "Toolchain.Mac");
			if (!Directory.Exists(toolChainDirectory)) {
				toolChainDirectory = appDirectory;
			}
#else
			var toolChainDirectory = Path.Combine(appDirectory, "Toolchain.Win");
#endif
			var toolPath = Path.Combine(toolChainDirectory, tool);
#if MAC
			Process.Start("chmod", $"+x {toolPath}");
#endif
			return toolPath;
		}
	}
}
