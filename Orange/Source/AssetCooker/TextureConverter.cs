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
		public static void Convert(Gdk.Pixbuf pixbuf, string dstPath, CookingRules cookingRules, TargetPlatform platform)
		{
			switch (platform) {
				case TargetPlatform.Unity:
					throw new Lime.Exception("No need to convert textures for Unity platform!");
				case TargetPlatform.Android:
					CookForAndroid(pixbuf, dstPath, cookingRules.PVRFormat, cookingRules.MipMaps);
					break;
				case TargetPlatform.iOS:
					CookForIOS(pixbuf, dstPath, cookingRules.PVRFormat, cookingRules.MipMaps);
					break;
				case TargetPlatform.Desktop:
					CookForDesktop(pixbuf, dstPath, cookingRules.DDSFormat, cookingRules.MipMaps);
					break;
				default:
					throw new ArgumentException();
			}
		}

		private static void CookForIOS(Gdk.Pixbuf pixbuf, string dstPath, PVRFormat pvrFormat, bool mipMaps)
		{
			int width = pixbuf.Width;
			int height = pixbuf.Height;
			bool hasAlpha = pixbuf.HasAlpha;

			int potWidth = TextureConverterUtils.GetNearestPowerOf2(width, 8, 1024);
			int potHeight = TextureConverterUtils.GetNearestPowerOf2(height, 8, 1024);
			
			int maxDimension = Math.Max(potWidth, potHeight);

			string formatArguments = "";
			switch (pvrFormat) {
			case PVRFormat.Compressed:
				formatArguments = "-f PVRTC4";
				potWidth = potHeight = maxDimension;
				break;
			case PVRFormat.RGB565:
				if (hasAlpha) {
					Console.WriteLine("WARNING: texture has alpha channel. Used 'RGBA4444' format instead of 'RGB565'.");
					formatArguments = "-f OGL4444 -nt -yflip0";
					TextureConverterUtils.ReduceTo4BitsPerChannelWithFloydSteinbergDithering(pixbuf);
				} else {
					formatArguments = "-f OGL565 -nt -yflip0";
				}
				break;
			case PVRFormat.RGBA4:
				formatArguments = "-f OGL4444 -nt -yflip0";
				TextureConverterUtils.ReduceTo4BitsPerChannelWithFloydSteinbergDithering(pixbuf);
				break;
			case PVRFormat.ARGB8:
				formatArguments = "-f OGL8888 -nt -yflip0";
				break;
			}
			string tga = Path.ChangeExtension(dstPath, ".tga");
			try {
				TextureConverterUtils.SwapRGBChannels(pixbuf);
				TextureConverterUtils.SaveToTGA(pixbuf, tga);
				string mipsFlag = mipMaps ? "-m" : "";
				string pvrTexTool = Path.Combine(Toolbox.GetApplicationDirectory(), "Toolchain.Mac", "PVRTexTool");
				Mono.Unix.Native.Syscall.chmod(pvrTexTool, Mono.Unix.Native.FilePermissions.S_IXOTH | Mono.Unix.Native.FilePermissions.S_IXUSR);
				string args = String.Format("{0} -i '{1}' -o '{2}' {3} -pvrtcfast -premultalpha -silent -x {4} -y {5}",
					formatArguments, tga, dstPath, mipsFlag, potWidth, potHeight);
				var p = System.Diagnostics.Process.Start(pvrTexTool, args);
				p.WaitForExit();
				if (p.ExitCode != 0) {
					throw new Lime.Exception("Failed to convert '{0}' to PVR format(error code: {1})", tga, p.ExitCode);
				}
			} finally {
				File.Delete(tga);
			}
		}

		private static void CookForAndroid(Gdk.Pixbuf pixbuf, string dstPath, PVRFormat pvrFormat, bool mipMaps)
		{
			string formatArguments;
			switch (pvrFormat) {
			case PVRFormat.Compressed:
				formatArguments = "-f etc1 -q etcfast";
				break;
			case PVRFormat.RGB565:
				if (pixbuf.HasAlpha) {
					Console.WriteLine("WARNING: texture has alpha channel. Used 'RGBA4444' format instead of 'RGB565'.");
					formatArguments = "-f r4g4b4a4";
					TextureConverterUtils.ReduceTo4BitsPerChannelWithFloydSteinbergDithering(pixbuf);
				} else {
					formatArguments = "-f r5g6b5";
				}
				break;
			case PVRFormat.RGBA4:
				formatArguments = "-f r4g4b4a4";
				TextureConverterUtils.ReduceTo4BitsPerChannelWithFloydSteinbergDithering(pixbuf);
				break;
			case PVRFormat.ARGB8:
				formatArguments = "-f r8g8b8a8";
				break;
			default:
				throw new ArgumentException();
			}
			var tga = Path.ChangeExtension(dstPath, ".tga");
			try {
				TextureConverterUtils.SwapRGBChannels(pixbuf);
				TextureConverterUtils.SaveToTGA(pixbuf, tga);
				var pvrTexTool = Path.Combine(Toolbox.GetApplicationDirectory(), "Toolchain.Win", "PVRTexToolCli");
				// -p - premultiply alpha
				// -shh - silent
				tga = MakeAbsolutePath(tga);
				dstPath = MakeAbsolutePath(dstPath);
				var args = String.Format("{0} -i \"{1}\" -o \"{2}\" {3} -p -shh", formatArguments, tga, dstPath, mipMaps ? "-m" : "");
				int result = Process.Start(pvrTexTool, args, Process.Options.RedirectErrors);
				if (result != 0) {
					throw new Lime.Exception("Error converting '{0}'\nCommand line: {1}", tga, pvrTexTool + " " + args);
				}
			} finally {
				try {
					File.Delete(tga);
				} catch { }
			}
		}

		private static string MakeAbsolutePath(string path)
		{
			if (!Path.IsPathRooted(path)) {
				path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), path);
			}
			return path;
		}

		private static void CookForDesktop(Gdk.Pixbuf pixbuf, string dstPath, DDSFormat format, bool mipMaps)
		{
			bool compressed = format == DDSFormat.DXTi;
			if (pixbuf.HasAlpha) {
				TextureConverterUtils.PremultiplyAlpha(pixbuf, swapChannels: compressed);
			} else if (compressed) {
				// DXT1
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
			string args = String.Format("-silent -fast {0} {1} \"{2}\" \"{3}\"", mipsFlag, compressionFlag, srcPath, dstPath);
			int result = Process.Start(nvcompress, args, Process.Options.RedirectErrors);
			if (result != 0) {
				throw new Lime.Exception("Failed to convert '{0}' to DDS format(error code: {1})", srcPath, result);
			}
#else
			string nvcompress = Path.Combine(Toolbox.GetApplicationDirectory(), "Toolchain.Mac", "nvcompress");
			Mono.Unix.Native.Syscall.chmod(nvcompress, Mono.Unix.Native.FilePermissions.S_IXOTH | Mono.Unix.Native.FilePermissions.S_IXUSR);
			string args = String.Format("-silent -fast {0} {1} '{2}' '{3}'", mipsFlag, compressionFlag, srcPath, dstPath);
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
	}
}
