using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Lime;

namespace Orange
{
	public static class LocalCache
	{
		public static readonly string EtcCacheDirectory = "etcCache";
		public static readonly string PvrCacheDirectory = "pvrCache";
		public static readonly string DdsCacheDirectory = "ddsCache";

		private static string cacheDirectory;

		public static string FindEtcTexture(Bitmap bitmap, bool mipMaps, bool highQualityCompression, byte[] CookingRulesSHA1)
		{
			var stream = new MemoryStream();
			bitmap.SaveTo(stream);
			stream.WriteByte(mipMaps ? (byte)1 : (byte)0);
			stream.WriteByte(highQualityCompression ? (byte)1 : (byte)0);
			if (CookingRulesSHA1 != null) {
				stream.Write(CookingRulesSHA1, 0, CookingRulesSHA1.Length);
			}
			return GetCachedFilePath(stream, EtcCacheDirectory);
		}

		public static string FindPvrTexture(Bitmap bitmap, bool mipMaps, bool highQualityCompression, PVRFormat pvrFormat, byte[] CookingRulesSHA1)
		{
			var stream = new MemoryStream();
			bitmap.SaveTo(stream);
			stream.WriteByte(mipMaps ? (byte)1 : (byte)0);
			stream.WriteByte(highQualityCompression ? (byte)1 : (byte)0);
			stream.Write(BitConverter.GetBytes((int)pvrFormat), 0, BitConverter.GetBytes((int)pvrFormat).Length);
			if (CookingRulesSHA1 != null) {
				stream.Write(CookingRulesSHA1, 0, CookingRulesSHA1.Length);
			}
			return GetCachedFilePath(stream, PvrCacheDirectory);
		}

		public static string FindNvTexture(Bitmap bitmap, bool mipMaps, DDSFormat ddsFormat, byte[] CookingRulesSHA1)
		{
			var stream = new MemoryStream();
			bitmap.SaveTo(stream);
			stream.WriteByte(mipMaps ? (byte)1 : (byte)0);
			stream.Write(BitConverter.GetBytes((int)ddsFormat), 0, BitConverter.GetBytes((int)ddsFormat).Length);
			if (CookingRulesSHA1 != null) {
				stream.Write(CookingRulesSHA1, 0, CookingRulesSHA1.Length);
			}
			return GetCachedFilePath(stream, DdsCacheDirectory);
		}

		/// <summary>
		/// Make sure that you tried to find cache before saving or
		/// this won't work because it won't know full path to file
		/// </summary>
		public static void Save(string srcPath, string dstPath)
		{
			Directory.CreateDirectory(cacheDirectory);
			File.Copy(srcPath, dstPath);
		}

		private static string GetCachedFilePath(Stream stream, string directory)
		{
			stream.Position = 0;
			var hashString = BitConverter.ToString(SHA256.Create().ComputeHash(stream));
			cacheDirectory = Path.Combine(The.Workspace.ProjectCacheDirectory, directory,
				hashString[0].ToString(), hashString[1].ToString(), "");
			return Path.Combine(cacheDirectory, hashString.Substring(2));
		}
	}
}
