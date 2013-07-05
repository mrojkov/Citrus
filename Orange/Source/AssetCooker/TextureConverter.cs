using System;
using System.IO;
using System.Net;
using Lime;
using System.Runtime.InteropServices;

namespace Orange
{
	public static class TextureConverter
	{
		private static void ToPVRTexture(string srcPath, string dstPath, PVRFormat pvrFormat, bool mipMaps)
		{
			int width, height;
			bool hasAlpha;
			if (!TextureConverterUtils.GetPngFileInfo(srcPath, out width, out height, out hasAlpha)) {
				throw new Lime.Exception("Wrong png file: " + srcPath);
			}
			int potWidth = TextureConverterUtils.GetNearestPowerOf2(width, 8, 1024);
			int potHeight = TextureConverterUtils.GetNearestPowerOf2(height, 8, 1024);
			
			int maxDimension = Math.Max(potWidth, potHeight);
			int pvrtc4DataLength = maxDimension * maxDimension / 2;
			int rgba16DataLength = potWidth * potHeight * 2;
			
			var pixbuf = new Gdk.Pixbuf(srcPath);
			string formatArguments = "";
			switch (pvrFormat)
			{
			case PVRFormat.PVRTC4:
				formatArguments = "-f PVRTC4";
				potWidth = potHeight = maxDimension;
				break;
			case PVRFormat.RGB565:
				if (hasAlpha) {
					Console.WriteLine("WARNING: texture has alpha channel. Used 'OGL4444' format instead of 'OGL565'.");
					formatArguments = "-f OGL4444 -nt -yflip0";
					TextureConverterUtils.ReduceColorsToRGBA4444WithFloydSteinbergDithering(pixbuf);
				} else {
					formatArguments = "-f OGL565 -nt -yflip0";
				}
				break;
			case PVRFormat.RGBA4:
				formatArguments = "-f OGL4444 -nt -yflip0";
				TextureConverterUtils.ReduceColorsToRGBA4444WithFloydSteinbergDithering(pixbuf);
				break;
			case PVRFormat.ARGB8:
				formatArguments = "-f OGL8888 -nt -yflip0";
				break;
			}
			string tga = Path.ChangeExtension(srcPath, ".tga");
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
		
		private static void ToDDSTexture(string srcPath, string dstPath, DDSFormat format, bool mipMaps)
		{
			int width, height;
			bool hasAlpha;
			if (!TextureConverterUtils.GetPngFileInfo(srcPath, out width, out height, out hasAlpha)) {
				throw new Lime.Exception("Wrong png file: " + srcPath);
			}
			bool compressed = format == DDSFormat.DXTi;
			if (hasAlpha) {
				var pixbuf = new Gdk.Pixbuf(srcPath);
				TextureConverterUtils.PremultiplyAlpha(pixbuf, true);
				string tga = Path.ChangeExtension(srcPath, ".tga");
				try {
					TextureConverterUtils.SaveToTGA(pixbuf, tga);
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
			string nvcompress = Path.Combine(Toolbox.GetApplicationDirectory(), "Toolchain.Win", "nvcompress.exe");
			srcPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), srcPath);
			dstPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), dstPath);
			string args = String.Format("-silent -fast {0} {1} \"{2}\" \"{3}\"", mipsFlag, compressionFlag, srcPath, dstPath);
			int exitCode = Toolbox.StartProcess(nvcompress, args, Toolbox.StartProcessOptions.RedirectErrors);
			if (exitCode != 0) {
				throw new Lime.Exception("Failed to convert '{0}' to DDS format(error code: {1})", srcPath, exitCode);
			}
#else
			string nvcompress = Path.Combine(Helpers.GetApplicationDirectory(), "Toolchain.Mac", "nvcompress");
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
		
		public static void Convert(string srcPath, string dstPath, CookingRules cookingRules, TargetPlatform platform)
		{
			if (platform == TargetPlatform.Unity) {
				throw new Lime.Exception("No need to convert textures for Unity platform!");
			}
			if (Path.GetExtension(dstPath) == ".pvr") {
				ToPVRTexture(srcPath, dstPath, cookingRules.PVRFormat, cookingRules.MipMaps);
			}
			else if (Path.GetExtension(dstPath) == ".dds") {
				ToDDSTexture(srcPath, dstPath, cookingRules.DDSFormat, cookingRules.MipMaps);
			}
			else {
				throw new Lime.Exception("Unknown texture format for: {0}", dstPath);
			}
		}
	}
}
