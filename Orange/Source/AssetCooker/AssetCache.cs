using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using FluentFTP;
using Lime;
using Debug = System.Diagnostics.Debug;
using Environment = System.Environment;

namespace Orange
{
	// This class is not thread safe. Use ThreadLocal if you want thread safety.
	public class AssetCache
	{
		private static AssetCache instance;
		public static AssetCache Instance
		{
			get
			{
				if (instance == null) {
					instance = new AssetCache();
				}
				return instance;
			}
		}

		private readonly FtpClient ftpClient;
		private readonly string serverAddress;
		private readonly string serverUsername;
		private readonly string serverPath;
		private readonly string localPath;
		private readonly bool isActive;
		private readonly bool remoteEnabled;

		private string lastHandledHashString;

		private AssetCache()
		{
			var data = The.Workspace.ProjectJson.AsDynamic.AssetCache;
			if (data == null) {
				isActive = false;
				Console.WriteLine("Warning: 'AssetCache' field not presented in .citproj. Caching disabled");
				return;
			}
			serverAddress = (string)data.ServerAddress;
			serverUsername = (string)data.ServerUsername;
			serverPath = (string)data.ServerPath;
			localPath = (string)data.LocalPath;
			remoteEnabled = (bool)data.RemoteEnabled;
			if (!remoteEnabled) {
				Console.WriteLine("Remote cache disabled by .citproj");
			}

			ftpClient = new FtpClient(serverAddress) {
				Credentials = new NetworkCredential(serverUsername, Environment.UserName)
			};
			FtpTrace.EnableTracing = false;
			isActive = true;
		}

		public void ConnectToRemote()
		{
			if (!isActive || !remoteEnabled) {
				return;
			}

			if (!ftpClient.IsConnected) {
				ftpClient.Connect();
			}

			if (ftpClient.IsConnected) {
				Console.WriteLine($"FTP: {serverUsername}@{serverAddress}: Connection established");
			} else {
				Console.WriteLine($"FTP: {serverUsername}@{serverAddress}: Connection failed");
			}
		}

		public void DisconnectFromRemote()
		{
			if (!isActive || !remoteEnabled) {
				return;
			}

			if (ftpClient.IsConnected) {
				ftpClient.Disconnect();
			}
			Console.WriteLine("FTP: Disconnected");
		}

		public string GetTexture(Bitmap bitmap, string extension, string commandLineArgs)
		{
			using (var stream = new MemoryStream()) {
				bitmap.SaveTo(stream);
				var extensionBytes = Encoding.UTF8.GetBytes(extension);
				stream.Write(extensionBytes, 0, extensionBytes.Length);
				var commandLineArgsBytes = Encoding.UTF8.GetBytes(commandLineArgs);
				stream.Write(commandLineArgsBytes, 0, commandLineArgsBytes.Length);
				return GetCachedFile(GetHashString(stream));
			}
		}

		/// <summary>
		/// Won't work if GetCachedFile was not used just before
		/// </summary>
		/// <seealso cref="GetCachedFile"/>
		public void Save(string srcPath)
		{
			if (!isActive) {
				return;
			}
			var path = GetLocalPath(lastHandledHashString);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.Copy(srcPath, path);
			if (remoteEnabled) {
				UploadFromLocal(lastHandledHashString);
			}
		}

		private string GetCachedFile(string hashString)
		{
			lastHandledHashString = hashString;
			if (ExistsLocal(hashString)) {
				UploadFromLocal(hashString);
				return GetLocalPath(hashString);
			}
			if (ExistsRemote(hashString)) {
				DownloadFromRemote(hashString);
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
			return Path.Combine(The.Workspace.ProjectDirectory, localPath,
				hashString.Substring(0, 2), hashString.Substring(2, 2), hashString);
		}

		private string GetRemotePath(string hashString)
		{
			return Path.Combine(serverPath, hashString.Substring(0, 2), hashString.Substring(2, 2), hashString);
		}

		private bool ExistsLocal(string hashString)
		{
			return File.Exists(GetLocalPath(hashString));
		}

		private bool ExistsRemote(string hashString)
		{
			if (isActive && remoteEnabled && ftpClient.IsConnected) {
				return ftpClient.FileExists(GetRemotePath(hashString));
			}
			return false;
		}

		private bool UploadFromLocal(string hashString)
		{
			if (isActive && remoteEnabled && ftpClient.IsConnected) {
				bool successful = ftpClient.UploadFile(GetLocalPath(hashString), GetRemotePath(hashString), FtpExists.Overwrite, true);
				if (!successful) {
					ftpClient.Disconnect();
					Console.WriteLine("FTP: Failed to upload cache. Connection cut off");
					return false;
				}
#if DEBUG
				Debug.WriteLine($"[Debug] FTP: {hashString} uploaded");
#endif
				return true;
			}
			return false;
		}

		private bool DownloadFromRemote(string hashString)
		{
			if (isActive && remoteEnabled && ftpClient.IsConnected) {
				bool successful = ftpClient.DownloadFile(GetLocalPath(hashString), GetRemotePath(hashString));
				if (!successful) {
					ftpClient.Disconnect();
					Console.WriteLine("FTP: Failed to download cache. Connection cut off");
					return false;
				}
#if DEBUG
				Debug.WriteLine($"[Debug] FTP: {hashString} uploaded");
#endif
				return true;
			}
			return false;
		}
	}
}
