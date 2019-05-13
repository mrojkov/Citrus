using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using FluentFTP;
using Lime;
using Debug = System.Diagnostics.Debug;
using Environment = System.Environment;
using Exception = Lime.Exception;

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
		private string localPath = Path.Combine(".orange", "Cache");
		public AssetCacheMode Mode { get; private set; }

		private bool RemoteEnabled => Mode == AssetCacheMode.Both || Mode == AssetCacheMode.Remote;
		private bool LocalEnabled => Mode == AssetCacheMode.Both || Mode == AssetCacheMode.Local;

		private AssetCache() { }

		private void GetSettings()
		{
			dynamic data = null;
			try {
				data = The.Workspace.ProjectJson.AsDynamic.AssetCache;
			} catch { }
			if (data == null) {
				Mode = AssetCacheMode.None;
				Console.WriteLine($"[Cache] Warning: 'AssetCache' field not found in {The.Workspace.ProjectFile}. Caching disabled");
				return;
			}
			string temp = (string) data.Enabled;
			switch (temp) {
				case "None":
					Mode = AssetCacheMode.None;
					Console.WriteLine($"[Cache] Caching disabled via {The.Workspace.ProjectFile}");
					return;
				case "Local":
					Mode = AssetCacheMode.Local;
					break;
				case "Remote":
					Mode = AssetCacheMode.Remote;
					break;
				case "Both":
					Mode = AssetCacheMode.Both;
					break;
				case null:
					Mode = AssetCacheMode.None;
					Console.WriteLine($"[Cache] Error: 'Enabled' field not found in AssetCache settings in {The.Workspace.ProjectFile}. Caching disabled");
					return;
				default:
					Mode = AssetCacheMode.None;
					throw new ArgumentException($"[Cache] Error: '{temp}' is not a valid state for 'Enabled' field in AssetCache in {The.Workspace.ProjectFile}. Caching disabled");
			}
			if (Mode == AssetCacheMode.Local) {
				Console.WriteLine("[Cache] Using LOCAL cache");
				return;
			}
			serverAddress = (string)data.ServerAddress;
			if (serverAddress == null) {
				if (Mode == AssetCacheMode.Both) {
					Mode = AssetCacheMode.Local;
					Console.WriteLine($"[Cache] Warning: 'ServerAddress' field not found in AssetCache settings in {The.Workspace.ProjectFile}. LOCAL cache will be used");
					return;
				}
				Mode = AssetCacheMode.None;
				Console.WriteLine($"[Cache] Error: 'ServerAddress' field not found in AssetCache settings in {The.Workspace.ProjectFile}. Caching disabled");
				return;
			}

			serverUsername = (string)data.ServerUsername;
			if (serverUsername == null) {
				if (Mode == AssetCacheMode.Both) {
					Mode = AssetCacheMode.Local;
					Console.WriteLine($"[Cache] Error: 'ServerUsername' field not found in AssetCache settings in {The.Workspace.ProjectFile}. LOCAL cache will be used");
					return;
				}
				Mode = AssetCacheMode.None;
				Console.WriteLine($"[Cache] Error: 'ServerUsername' field not found in AssetCache settings in {The.Workspace.ProjectFile}. Caching disabled");
				return;
			}

			serverPath = (string)data.ServerPath;
			if (serverPath == null) {
				if (Mode == AssetCacheMode.Both) {
					Mode = AssetCacheMode.Local;
					Console.WriteLine($"[Cache] Error: 'ServerPath' field not found in AssetCache settings in {The.Workspace.ProjectFile}. LOCAL cache will be used");
					return;
				}
				Mode = AssetCacheMode.None;
				Console.WriteLine($"[Cache] Error: 'ServerPath' field not found in AssetCache settings in {The.Workspace.ProjectFile}. Caching disabled");
				return;
			}

			if (Mode == AssetCacheMode.Remote) {
				Console.WriteLine("[Cache] Using REMOTE cache");
			}

			if (Mode == AssetCacheMode.Both) {
				Console.WriteLine("[Cache] Using LOCAL and REMOTE cache");
			}
		}

		public void Initialize()
		{
			GetSettings();

			if (RemoteEnabled) {
				ftpClient = new FtpClient(serverAddress) {
					Credentials = new NetworkCredential(serverUsername, Environment.UserName)
				};
				FtpTrace.EnableTracing = false;
				ftpClient.SocketPollInterval = 3000;
				ftpClient.ConnectTimeout = 3000;
				ftpClient.ReadTimeout = 3000;
			}

			if (!RemoteEnabled) {
				return;
			}

			if (!ftpClient.IsConnected) {
				try {
					ftpClient.Connect();
				}
				catch (System.Exception e) {
					Console.WriteLine(e.Message);
					string ending;
					if (Mode == AssetCacheMode.Both) {
						Mode = AssetCacheMode.Local;
						ending = "LOCAL cache will be used";
					} else {
						Mode = AssetCacheMode.None;
						ending = "Caching disabled";
					}
					Console.WriteLine($"[Cache] Error: Can't connect to {serverUsername}@{serverAddress}. {ending}");
				}
			}

			if (ftpClient.IsConnected) {
				Console.WriteLine($"[Cache] {serverUsername}@{serverAddress}: Connection established");
			}
		}

		public void DisconnectFromRemote()
		{
			if (!RemoteEnabled) {
				return;
			}
			if (ftpClient.IsConnected) {
				ftpClient.Disconnect();
			}
			if (Mode == AssetCacheMode.Both) {
				Mode = AssetCacheMode.Local;
			} else {
				Mode = AssetCacheMode.None;
			}
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
			if (RemoteEnabled) {
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
			if (RemoteEnabled && ftpClient.IsConnected) {
				try {
					return ftpClient.FileExists(GetRemotePath(hashString));
				}
				catch(System.Exception e) {
					Console.WriteLine(e.Message);
					string ending;
					if (Mode == AssetCacheMode.Both) {
						Mode = AssetCacheMode.Local;
						ending = "LOCAL cache will be used";
					} else {
						Mode = AssetCacheMode.None;
						ending = "Caching disabled";
					}
					Console.WriteLine($"[Cache] Error: Can't check existance of file {hashString} at {serverUsername}@{serverAddress}. {ending}");
					return false;
				}
			}
			return false;
		}

		private bool UploadFromLocal(string hashString)
		{
			if (Mode == AssetCacheMode.Both && ftpClient.IsConnected) {
				try {
					if (ExistsRemote(hashString)) {
						return true;
					}
					bool successful = ftpClient.UploadFile(GetLocalPath(hashString), GetRemotePath(hashString),
						FtpExists.Overwrite, true);
					if (!successful) {
						ftpClient.Disconnect();
						string ending;
						if (Mode == AssetCacheMode.Both) {
							Mode = AssetCacheMode.Local;
							ending = "LOCAL cache will be used";
						} else {
							Mode = AssetCacheMode.None;
							ending = "Caching disabled";
						}
						Console.WriteLine($"[Cache] Failed to upload {hashString} to {serverUsername}@{serverAddress}. Disconnected. {ending}");
						return false;
					}
				}
				catch (System.Exception e) {
					Console.WriteLine(e.Message);
					string ending;
					if (Mode == AssetCacheMode.Both) {
						Mode = AssetCacheMode.Local;
						ending = "LOCAL cache will be used";
					} else {
						Mode = AssetCacheMode.None;
						ending = "Caching disabled";
					}
					Console.WriteLine($"[Cache] Error: Failed to upload {hashString} to {serverUsername}@{serverAddress}. {ending}");
					return false;
				}
#if DEBUG
				Debug.WriteLine($"[Debug][Cache] Uploaded {hashString}");
#endif
				return true;
			}
			return false;
		}

		private bool DownloadFromRemote(string hashString)
		{
			if (RemoteEnabled && ftpClient.IsConnected) {
				try {
					if (ExistsLocal(hashString)) {
						return true;
					}
					bool successful = ftpClient.DownloadFile(GetLocalPath(hashString), GetRemotePath(hashString));
					if (!successful) {
						ftpClient.Disconnect();
						string ending;
						if (Mode == AssetCacheMode.Both) {
							Mode = AssetCacheMode.Local;
							ending = "LOCAL cache will be used";
						} else {
							Mode = AssetCacheMode.None;
							ending = "Caching disabled";
						}
						Console.WriteLine(
							$"[Cache] Failed to download {hashString} from {serverUsername}@{serverAddress}. Disconnected. {ending}");
						return false;
					}
				}
				catch (System.Exception e) {
					Console.WriteLine(e.Message);
					string ending;
					if (Mode == AssetCacheMode.Both) {
						Mode = AssetCacheMode.Local;
						ending = "LOCAL cache will be used";
					} else {
						Mode = AssetCacheMode.None;
						ending = "Caching disabled";
					}
					Console.WriteLine($"[Cache] Error: Failed to download {hashString} from {serverUsername}@{serverAddress}. {ending}");
					return false;
				}

#if DEBUG
				Debug.WriteLine($"[Debug][Cache] Downloaded {hashString}");
#endif
				return true;
			}
			return false;
		}
	}

	public enum AssetCacheMode
	{
		None,
		Local,
		Remote,
		Both
	}
}
