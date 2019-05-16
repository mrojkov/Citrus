using System;
using System.IO;
using System.Net;
using FluentFTP;

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

		private FtpClient ftpClient;
		private string serverAddress;
		private string serverUsername;
		private string serverPath;
		private string tempFilePath;
		public AssetCacheMode Mode { get; private set; }

		private bool IsLocalEnabled => (Mode & AssetCacheMode.Local) != 0;
		private bool IsRemoteEnabled => (Mode & AssetCacheMode.Remote) != 0;

		private AssetCache() { }

		private void GetSettings()
		{
			dynamic data = The.Workspace.ProjectJson.AsDynamic.AssetCache;
			if (data == null) {
				Mode = AssetCacheMode.None;
				Console.WriteLine($"[Cache] Warning: 'AssetCache' field not found in {The.Workspace.ProjectFile}. Cache disabled");
				return;
			}
			Mode = The.Workspace.AssetCacheMode;
			if (Mode == AssetCacheMode.None) {
				Console.WriteLine("[Cache] Cache disabled via WorkspaceConfig");
				return;
			}
			tempFilePath = Path.Combine(The.Workspace.AssetCacheLocalPath, "cache.tmp");
			if (Mode == AssetCacheMode.Local) {
				Console.WriteLine("[Cache] Using LOCAL cache");
				return;
			}
			serverAddress = (string)data.ServerAddress;
			if (serverAddress == null) {
				HandleSetupFailure($"'ServerAddress' field not found in AssetCache settings in {The.Workspace.ProjectFile}");
				return;
			}
			serverUsername = (string)data.ServerUsername;
			if (serverUsername == null) {
				HandleSetupFailure($"'ServerUsername' field not found in AssetCache settings in {The.Workspace.ProjectFile}");
				return;
			}
			serverPath = (string)data.ServerPath;
			if (serverPath == null) {
				HandleSetupFailure($"'ServerPath' field not found in AssetCache settings in {The.Workspace.ProjectFile}");
				return;
			}
			if (Mode == AssetCacheMode.Remote) {
				Console.WriteLine("[Cache] Using REMOTE cache");
			}
			if (Mode == (AssetCacheMode.Local | AssetCacheMode.Remote)) {
				Console.WriteLine("[Cache] Using LOCAL and REMOTE cache");
			}
		}

		public void Initialize()
		{
			GetSettings();

			if (IsRemoteEnabled) {
				ftpClient = new FtpClient(serverAddress) {
					Credentials = new NetworkCredential(serverUsername, Environment.UserName)
				};
				FtpTrace.EnableTracing = false;
				ftpClient.SocketPollInterval = 3000;
				ftpClient.ConnectTimeout = 3000;
				ftpClient.ReadTimeout = 3000;
			}

			if (!IsRemoteEnabled) {
				return;
			}

			if (!ftpClient.IsConnected) {
				try {
					ftpClient.Connect();
				}
				catch (System.Exception e) {
					Console.WriteLine(e.Message);
					HandleSetupFailure("Can't connect");
				}
			}

			if (ftpClient.IsConnected) {
				Console.WriteLine($"[Cache] {serverUsername}@{serverAddress}: Connection established");
			}
		}

		public void DisconnectFromRemote()
		{
			if (!IsRemoteEnabled) {
				return;
			}
			if (ftpClient.IsConnected) {
				ftpClient.Disconnect();
			}
			Mode = IsLocalEnabled ? AssetCacheMode.Local : AssetCacheMode.None;
			Console.WriteLine($"[Cache] Disconnected from {serverUsername}@{serverAddress}");
		}

		public void Save(string srcPath, string hashString)
		{
			if (Mode == AssetCacheMode.None) {
				return;
			}
			// Maybe we should not save files locally when local cache is disabled
			var path = GetLocalPath(hashString);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.Copy(srcPath, path);
			if (IsRemoteEnabled) {
				UploadFromLocal(hashString);
			}
		}

		/// <summary>
		/// Uploads file to remote server if it doesn't exists there
		/// </summary>
		public string Load(string hashString)
		{
			if (ExistsLocal(hashString)) {
				UploadFromLocal(hashString);
				return GetLocalPath(hashString);
			}
			if (ExistsRemote(hashString)) {
				if (!DownloadFromRemote(hashString)) {
					return null;
				}
				return GetLocalPath(hashString);
			}
			return null;
		}

		private string GetLocalPath(string hashString)
		{
			return Path.Combine(The.Workspace.ProjectDirectory, The.Workspace.AssetCacheLocalPath,
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
			if (IsRemoteEnabled && ftpClient.IsConnected) {
				try {
					return ftpClient.FileExists(GetRemotePath(hashString));
				}
				catch(System.Exception e) {
					Console.WriteLine(e.Message);
					HandleRemoteCacheFailure(hashString, "Can't check existance of file");
					return false;
				}
			}
			return false;
		}

		private bool UploadFromLocal(string hashString)
		{
			if (Mode == (AssetCacheMode.Local | AssetCacheMode.Remote) && ftpClient.IsConnected) {
				try {
					if (ExistsRemote(hashString)) {
						return true;
					}
					bool successful = ftpClient.UploadFile(GetLocalPath(hashString), GetRemotePath(hashString),
						FtpExists.Overwrite, true);
					if (!successful) {
						ftpClient.Disconnect();
						HandleRemoteCacheFailure(hashString, "Upload failed");
						return false;
					}
				}
				catch (System.Exception e) {
					Console.WriteLine(e.Message);
					HandleRemoteCacheFailure(hashString, "Upload failed");
					return false;
				}
#if DEBUG
				System.Diagnostics.Debug.WriteLine($"[Debug][Cache] Uploaded {hashString}");
#endif
				return true;
			}
			return false;
		}

		private bool DownloadFromRemote(string hashString)
		{
			if (IsRemoteEnabled && ftpClient.IsConnected) {
				try {
					if (ExistsLocal(hashString)) {
						return true;
					}
					bool successful = ftpClient.DownloadFile(tempFilePath, GetRemotePath(hashString));
					if (!successful) {
						ftpClient.Disconnect();
						HandleRemoteCacheFailure(hashString, "Download failed");
						if (File.Exists(tempFilePath)) {
							File.Delete(tempFilePath);
						}
						return false;
					}
					var fullLocalPath = GetLocalPath(hashString);
					Directory.CreateDirectory(Path.GetDirectoryName(fullLocalPath));
					File.Copy(tempFilePath, fullLocalPath, true);
					File.Delete(tempFilePath);
				}
				catch (System.Exception e) {
					Console.WriteLine(e.Message);
					HandleRemoteCacheFailure(hashString, "Download failed");
					if (File.Exists(tempFilePath)) {
							File.Delete(tempFilePath);
					}
					return false;
				}

#if DEBUG
				System.Diagnostics.Debug.WriteLine($"[Debug][Cache] Downloaded {hashString}");
#endif
				return true;
			}
			return false;
		}

		private void HandleSetupFailure(string errorMessage)
		{
			string ending;
			if (IsLocalEnabled) {
				Mode = AssetCacheMode.Local;
				ending = "Switched to LOCAL cache";
			} else {
				Mode = AssetCacheMode.None;
				ending = "Cache disabled";
			}
			Console.WriteLine($"[Cache] ERROR: {errorMessage}. {ending}");
		}

		private void HandleRemoteCacheFailure(string hashString, string errorMessage)
		{
			string ending;
			if (IsLocalEnabled) {
				Mode = AssetCacheMode.Local;
				ending = "Switched to LOCAL cache";
			} else {
				Mode = AssetCacheMode.None;
				ending = "Cache disabled";
			}
			Console.WriteLine($"[Cache] ERROR {serverUsername}@{serverAddress}: {errorMessage} ({hashString}). {ending}");
		}
	}

	[Flags]
	public enum AssetCacheMode
	{
		None = 0,
		Local = 1,
		Remote = 2
	}
}
