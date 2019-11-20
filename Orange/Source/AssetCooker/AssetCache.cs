using System;
using System.IO;
using System.Net;
using FluentFTP;

namespace Orange
{
	/// <summary>
	/// Used to handle all operations with cache. Able to store data in local and remote cache and to load data from storage.
	/// This class is not thread safe.
	/// </summary>
	public class AssetCache
	{
		/// <summary>
		/// Downloadable data is stored at this file during loading from remote server.
		/// It prevents data corruption on possible loading interruptions.
		/// </summary>
		public const string TempFileName = "cache.tmp";

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

		/// <summary>
		/// Client used to communicate with remote server.
		/// </summary>
		private FtpClient ftpClient;

		/// <summary>
		/// Ip address of ftp server that stores cache.
		/// </summary>
		private string serverAddress;

		/// <summary>
		/// Username used to login to remote ftp server
		/// </summary>
		private string serverUsername;

		/// <summary>
		/// Path to cache on remote server
		/// </summary>
		private string serverPath;

		/// <summary>
		/// Absolute path to temp file
		/// </summary>
		private string tempFilePath;

		/// <summary>
		/// Asset cache mode used for current operation
		/// </summary>
		public AssetCacheMode Mode { get; private set; }

		private bool IsLocalEnabled => (Mode & AssetCacheMode.Local) != 0;
		private bool IsRemoteEnabled => (Mode & AssetCacheMode.Remote) != 0;

		private AssetCache() { }

		/// <summary>
		/// Load current cache settings using workspace config and .citproj
		/// </summary>
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
			tempFilePath = Path.Combine(The.Workspace.LocalAssetCachePath, TempFileName);
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

		/// <summary>
		/// Load settings, intiate connection to remote server (if needed)
		/// </summary>
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
				} catch (System.Exception e) {
					Console.WriteLine(e.Message);
					HandleSetupFailure("Can't connect");
				}
			}

			if (ftpClient.IsConnected) {
				Console.WriteLine($"[Cache] {serverUsername}@{serverAddress}: Connection established");
			}
		}

		/// <summary>
		/// Drop connection with remote server
		/// </summary>
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

		/// <summary>
		/// Saves file to local cache, load it to remote cache if neccessary
		/// </summary>
		/// <param name="srcPath">Path to file that should be saved</param>
		/// <param name="hashString">SHA256 of file's binary data</param>
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
		/// Tries to load file from cache (local or remote). Return null if cache for this file not exists.
		/// Copies file cache to server if it existed locally but not existed remotely.
		/// </summary>
		/// /// <param name="hashString">SHA256 of file's binary data</param>
		/// <returns>Path to locally cached file or null</returns>
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

		/// <summary>
		/// Returns absolute path to local cache file
		/// </summary>
		/// <param name="hashString">SHA256 of file's binary data</param>
		private string GetLocalPath(string hashString)
		{
			return Path.Combine(The.Workspace.LocalAssetCachePath,
				hashString.Substring(0, 2), hashString.Substring(2, 2), hashString);
		}

		/// <summary>
		/// Returns path to cache file on remote server
		/// </summary>
		/// <param name="hashString">SHA256 of file's binary data</param>
		private string GetRemotePath(string hashString)
		{
			return Path.Combine(serverPath, hashString.Substring(0, 2), hashString.Substring(2, 2), hashString);
		}

		/// <summary>
		/// Checks if cache for provided file exists locally
		/// </summary>
		/// <param name="hashString">SHA256 of file's binary data</param>
		private bool ExistsLocal(string hashString)
		{
			return File.Exists(GetLocalPath(hashString));
		}

		/// <summary>
		/// Checks if cache for provided file exists remotely
		/// </summary>
		/// <param name="hashString">SHA256 of file's binary data</param>
		private bool ExistsRemote(string hashString)
		{
			if (IsRemoteEnabled && ftpClient.IsConnected) {
				try {
					return ftpClient.FileExists(GetRemotePath(hashString));
				} catch(System.Exception e) {
					Console.WriteLine(e.Message);
					HandleRemoteCacheFailure(hashString, "Can't check existance of file");
					return false;
				}
			}
			return false;
		}

		/// <summary>
		/// Uploads local cache file to remote server
		/// </summary>
		/// <param name="hashString">SHA256 of file's binary data</param>
		/// <returns>True if upload succeded, false otherwise</returns>
		public bool UploadFromLocal(string hashString)
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
				} catch (System.Exception e) {
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

		/// <summary>
		/// Downloads cache file from remote server
		/// </summary>
		/// <param name="hashString">SHA256 of file's binary data</param>
		/// <returns>True if download succeded, false otherwise</returns>
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
						return false;
					}
					var localPath = GetLocalPath(hashString);
					Directory.CreateDirectory(Path.GetDirectoryName(localPath));
					File.Move(tempFilePath, localPath);
				} catch (System.Exception e) {
					Console.WriteLine(e.Message);
					HandleRemoteCacheFailure(hashString, "Download failed");
					return false;
				}

#if DEBUG
				System.Diagnostics.Debug.WriteLine($"[Debug][Cache] Downloaded {hashString}");
#endif
				return true;
			}
			return false;
		}

		/// <summary>
		/// Switch cache mode to viable on current conditions and write corresponding message to console.
		/// Used to handle setup errors.
		/// </summary>
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
			Console.WriteLine($"[Cache] WARNING: {errorMessage}. {ending}");
		}

		/// <summary>
		///Switch cache mode to viable on current conditions and write corresponding message to console.
		/// Used to handle errors related to communication with remote serer
		/// </summary>
		/// <param name="hashString">SHA256 of file's binary data</param>
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
			Console.WriteLine($"[Cache] WARNING {serverUsername}@{serverAddress}: {errorMessage} ({hashString}). {ending}");
			if (File.Exists(tempFilePath)) {
				File.Delete(tempFilePath);
			}
		}
	}

	/// <summary>
	/// List of cache mode options
	/// </summary>
	[Flags]
	public enum AssetCacheMode
	{
		None = 0,
		Local = 1,
		Remote = 2
	}
}
