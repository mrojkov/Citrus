using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Lime;
using FluentFTP;

namespace Orange
{
	public class Cache
	{
		private static Cache instance;
		public static Cache Instance
		{
			get {
				if (instance == null) {
					instance = new Cache();
				}
				return instance;
			}
		}

		private const string textureCacheDirectory = "TextureCache";
		private FtpClient ftpClient;
		private string hostAddress = "192.168.0.209";
		private string hostName = "ftp";

		private string lastHandledHashString;

		private Cache()
		{
			ftpClient = new FtpClient(hostAddress);
			ftpClient.Credentials = new System.Net.NetworkCredential(hostName, System.Environment.UserName);
			FtpTrace.EnableTracing = false;
		}

		public void ConnectToRemote()
		{
			if (!ftpClient.IsConnected) {
				ftpClient.Connect();
			}

			if (ftpClient.IsConnected) {
				Console.WriteLine($"FTP: {hostAddress}: Connection established");
			}
			else {
				Console.WriteLine($"FTP: {hostAddress}: Connection failed");
			}
		}

		public void DisconnectFromRemote()
		{
			if (ftpClient.IsConnected) {
				ftpClient.Disconnect();
			}
			Console.WriteLine("FTP: Disconnected");
		}

		public string GetEtcTexture(Bitmap bitmap, bool mipMaps, bool highQualityCompression, byte[] CookingRulesSHA1)
		{
			var stream = new MemoryStream();
			bitmap.SaveTo(stream);
			var extensionBytes = Encoding.UTF8.GetBytes(".etc");
			stream.Write(extensionBytes, 0, extensionBytes.Length);
			stream.WriteByte(mipMaps ? (byte)1 : (byte)0);
			stream.WriteByte(highQualityCompression ? (byte)1 : (byte)0);
			if (CookingRulesSHA1 != null) {
				stream.Write(CookingRulesSHA1, 0, CookingRulesSHA1.Length);
			}
			return GetCachedFile(GetHashString(stream));
		}

		public string GetPvrTexture(Bitmap bitmap, bool mipMaps, bool highQualityCompression, PVRFormat pvrFormat, byte[] CookingRulesSHA1)
		{
			var stream = new MemoryStream();
			bitmap.SaveTo(stream);
			var extensionBytes = Encoding.UTF8.GetBytes(".pvr");
			stream.Write(extensionBytes, 0, extensionBytes.Length);
			stream.WriteByte(mipMaps ? (byte)1 : (byte)0);
			stream.WriteByte(highQualityCompression ? (byte)1 : (byte)0);
			stream.Write(BitConverter.GetBytes((int)pvrFormat), 0, BitConverter.GetBytes((int)pvrFormat).Length);
			if (CookingRulesSHA1 != null) {
				stream.Write(CookingRulesSHA1, 0, CookingRulesSHA1.Length);
			}
			return GetCachedFile(GetHashString(stream));
		}

		public string GetDdsTexture(Bitmap bitmap, bool mipMaps, DDSFormat ddsFormat, byte[] CookingRulesSHA1)
		{
			var stream = new MemoryStream();
			bitmap.SaveTo(stream);
			var extensionBytes = Encoding.UTF8.GetBytes(".dds");
			stream.Write(extensionBytes, 0, extensionBytes.Length);
			stream.WriteByte(mipMaps ? (byte)1 : (byte)0);
			stream.Write(BitConverter.GetBytes((int)ddsFormat), 0, BitConverter.GetBytes((int)ddsFormat).Length);
			if (CookingRulesSHA1 != null) {
				stream.Write(CookingRulesSHA1, 0, CookingRulesSHA1.Length);
			}
			return GetCachedFile(GetHashString(stream));
		}

		/// <summary>
		/// Won't work if GetCachedFile was not used just before
		/// </summary>
		/// <seealso cref="GetCachedFile"/>
		public void Save(string srcPath)
		{
			var localPath = GetLocalPath(lastHandledHashString);
			Directory.CreateDirectory(Path.GetDirectoryName(localPath));
			File.Copy(srcPath, localPath);
			UploadFromLocal(lastHandledHashString);
		}

		private string GetCachedFile(string hashString)
		{
			lastHandledHashString = hashString;
			if (ExistsLocal(hashString, true) || ExistsRemote(hashString, true)) {
				return GetLocalPath(hashString);
			}
			return null;
		}

		private string GetHashString(Stream stream)
		{
			stream.Position = 0;
			return BitConverter.ToString(SHA256.Create().ComputeHash(stream)).Replace("-", string.Empty).ToLower();
		}

		private string GetLocalPath(string hashString)
		{
			return Path.Combine(The.Workspace.ProjectCacheDirectory, textureCacheDirectory,
				hashString.Substring(0, 2), hashString.Substring(2, 2), hashString);
		}

		private string GetRemotePath(string hashString)
		{
			return Path.Combine(textureCacheDirectory, hashString.Substring(0, 2), hashString.Substring(2, 2), hashString);
		}

		/// <param name="updateRemote">If 'true': checks whether file exists at remote server and uploads it if not</param>
		private bool ExistsLocal(string hashString, bool updateRemote)
		{
			if (File.Exists(GetLocalPath(hashString))) {
				if (updateRemote && !ExistsRemote(hashString, false)) {
					UploadFromLocal(hashString);
				}
				return true;
			}
			return false;
		}

		/// <param name="updateLocal">If 'true': checks whether file exists at remote server and uploads it if not</param>
		private bool ExistsRemote(string hashString, bool updateLocal)
		{
			if (ftpClient.IsConnected) {
				if (ftpClient.FileExists(GetRemotePath(hashString))) {
					if (updateLocal && !ExistsLocal(hashString, false)) {
						return DownloadFromRemote(hashString);
					}
					return true;
				}
				return false;
			}
			return false;
		}

		private bool UploadFromLocal(string hashString)
		{
			if (ftpClient.IsConnected) {
				bool successful = ftpClient.UploadFile(GetLocalPath(hashString), GetRemotePath(hashString), FtpExists.Overwrite, true);
				if (!successful) {
					ftpClient.Disconnect();
					Console.WriteLine("FTP: Failed to upload cache. Connection cut off");
					return false;
				}
#if DEBUG
				System.Diagnostics.Debug.WriteLine($"[Debug] FTP: {hashString} uploaded");
#endif
			}
			return true;
		}

		private bool DownloadFromRemote(string hashString)
		{
			if (ftpClient.IsConnected) {
				bool successful = ftpClient.DownloadFile(GetLocalPath(hashString), GetRemotePath(hashString));
				if (!successful) {
					ftpClient.Disconnect();
					Console.WriteLine("FTP: Failed to download cache. Connection cut off");
					return false;
				}
#if DEBUG
				System.Diagnostics.Debug.WriteLine($"[Debug] FTP: {hashString} uploaded");
#endif
			}
			return true;
		}
	}
}
