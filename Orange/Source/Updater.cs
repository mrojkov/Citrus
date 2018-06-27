using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using Lime;
using Octokit;
using FileMode = System.IO.FileMode;
using Task = System.Threading.Tasks.Task;

namespace Orange
{
	public static class Updater
	{
		private static GitHubClient client = new GitHubClient(new ProductHeaderValue("mrojkov-citrus-auto-updater"));
		private static bool firstUpdate = true;
		private const string lockFileName = "update_lock";
		private static string citrusDirectory = Toolbox.CalcCitrusDirectory();
		private static string lockPath = Path.Combine(citrusDirectory, lockFileName);
		private static string citrusListPath = Path.Combine(citrusDirectory, "citrus_list");
		private static string newReleasePath = Path.Combine(citrusDirectory, newReleaseDirectoryName);
		private const string newReleaseDirectoryName = "new_release";
		private static string previousReleasePath = Path.Combine(citrusDirectory, "previous_release");

		private static bool TryOpenFile(string filename, out FileStream stream)
		{
			stream = null;

			try {
				var file = new System.IO.FileInfo(filename);
				stream = file.Open(System.IO.FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
			} catch (IOException) {
				return false;
			}
			return true;
		}

		public static async Task CheckForUpdates()
		{
			bool locked = false;
			bool becameEmpty;
			RefreshCitrusPidList(WhenRefreshPidList.OnStartup, out becameEmpty);
			Lime.Application.Exiting += () => {
				while (locked) {
					// in case we're shuting down while update is downloading
					System.Threading.Thread.Sleep(200);
				}
				RefreshCitrusPidList(WhenRefreshPidList.OnShutdown, out becameEmpty);
				if (becameEmpty) {
					ApplyUpdateIfPresent();
					File.Delete(citrusListPath);
				}
				return true;
			};
			var task = Task.Run(async () => {
				for (;;)
				{
					if (!firstUpdate) {
						await Task.Delay(TimeSpan.FromMinutes(5.0));
					}
					firstUpdate = false;
					if (IsUpdateLocked()) {
						continue;
					}
					LockUpdate();
					locked = true;
					try {
						var citrusVersion = CitrusVersion.Load();
						if (!citrusVersion.IsStandalone) {
							continue;
						}
						string newReleaseTagName = null;
						if (Directory.Exists(newReleasePath)) {
							using (var s = File.OpenRead(Path.Combine(newReleasePath, CitrusVersion.Filename))) {
								var newReleaseCitrusVersion = CitrusVersion.Load(s);
								newReleaseTagName = $"gh_{newReleaseCitrusVersion.Version}_{newReleaseCitrusVersion.BuildNumber}";
							}
						}
						var releases = await client.Repository.Release.GetAll("mrojkov", "Citrus");
						if (releases.Count == 0) {
							Console.WriteLine("Self Updater Error: zero releases available");
							continue;
						}
						var latest = releases[0];
						var tagName = $"gh_{citrusVersion.Version}_{citrusVersion.BuildNumber}";
						if (tagName == latest.TagName) {
							continue;
						}
						if (newReleaseTagName != null && newReleaseTagName == latest.TagName) {
							// this release is already downloaded
							continue;
						}
						var citrusDirectory = Toolbox.CalcCitrusDirectory();
						Console.WriteLine($"Current version is {tagName}. New {latest.TagName} version is available. Downloading update.");
						// TODO: select corresponding asset for OS
						var platformString =
#if WIN
						"win";
#elif MAC
						"mac";
#endif // WIN
						string platformAssetUrl = null;
						foreach (var asset in latest.Assets) {
							if (asset.Name.StartsWith($"citrus_{platformString}", StringComparison.OrdinalIgnoreCase)) {
								platformAssetUrl = asset.Url;
							}
						}
						if (platformAssetUrl == null) {
							Console.WriteLine($"Update error: can't find release asset corresponding to platform {platformString}");
							continue;
						}
						var response = await client.Connection.Get<object>(new Uri(platformAssetUrl), new Dictionary<string, string>(), "application/octet-stream");
						var zipFileBytes = response.Body as byte[];
						using (var compressedFileStream = new MemoryStream()) {
							compressedFileStream.Write(zipFileBytes, 0, zipFileBytes.Length);
							using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Read, false)) {
								if (Directory.Exists(newReleasePath)) {
									Directory.Delete(newReleasePath, true);
								}
								Directory.CreateDirectory(newReleasePath);
								zipArchive.ExtractToDirectory(newReleasePath);
							}
						}
#if MAC
						var process = new System.Diagnostics.Process {
							StartInfo = new System.Diagnostics.ProcessStartInfo {
								FileName = "tar",
								WorkingDirectory = newReleasePath,
								Arguments = "-xvf bundle.tar"
							}
						};
						process.Start();
						process.WaitForExit();
#endif // MAC
						Console.WriteLine("Update finished! Please restart");
					} finally {
						locked = false;
						File.Delete(lockPath);
					}
				}
			});
		}

		private enum WhenRefreshPidList
		{
			OnStartup,
			OnShutdown,
		}

		private static void RefreshCitrusPidList(WhenRefreshPidList when, out bool becameEmpty)
		{
			FileStream stream;
			while (!TryOpenFile(citrusListPath, out stream)) {
				System.Threading.Thread.Sleep(5);

			}
			using (stream) {
				byte[] buffer = new byte[stream.Length];
				stream.Read(buffer, 0, (int)stream.Length);
				var text = Encoding.UTF8.GetString(buffer);
				var lines = text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
				stream.Seek(0, SeekOrigin.Begin);
				stream.SetLength(0);
				List<int> activePids = new List<int>();
				var ownPid = System.Diagnostics.Process.GetCurrentProcess().Id;
				foreach (var l in lines) {
					bool exists = false;
					int pid = 0;
					try {
						pid = int.Parse(l);
						var process = System.Diagnostics.Process.GetProcessById(pid);
						if (process != null && !process.HasExited) {
							exists = true;
						}
					} catch {
					}
					if (exists && (when != WhenRefreshPidList.OnShutdown || pid != ownPid)) {
						activePids.Add(pid);
					}
				}
				if (when == WhenRefreshPidList.OnStartup) {
					activePids.Add(ownPid);
				}
				foreach (var pid in activePids) {
					buffer = Encoding.UTF8.GetBytes(pid.ToString());
					stream.Write(buffer, 0, buffer.Length);
					stream.WriteByte(13);
					stream.WriteByte(10);
				}
				stream.Flush(true);
				becameEmpty = activePids.Count == 0;
			}
		}

		private static void ApplyUpdateIfPresent()
		{
			if (!Directory.Exists(newReleasePath)) {
				return;
			}
			Directory.CreateDirectory(previousReleasePath);
			foreach (var fi in new FileEnumerator(citrusDirectory).Enumerate()) {
				if (fi.Path == lockFileName) {
					continue;
				}
				if (fi.Path.StartsWith(newReleaseDirectoryName, StringComparison.OrdinalIgnoreCase)) {
					continue;
				}
				var dstPath = Path.Combine(previousReleasePath, fi.Path);
				Directory.CreateDirectory(Path.GetDirectoryName(dstPath));
				File.Move(Path.Combine(citrusDirectory, fi.Path), dstPath);
			}
			foreach (var fi in new FileEnumerator(newReleasePath).Enumerate()) {
				var srcPath = Path.Combine(newReleasePath, fi.Path);
				var dstPath = Path.Combine(citrusDirectory, fi.Path);
				Directory.CreateDirectory(Path.GetDirectoryName(dstPath));
				File.Move(srcPath, dstPath);
			}
			Directory.Delete(newReleasePath, true);
		}

		private static bool IsUpdateLocked()
		{
			if (File.Exists(lockPath)) {
				try {
					int lockingPid = int.Parse(File.ReadAllText(lockPath));
					var lockingProcess = System.Diagnostics.Process.GetProcessById(lockingPid);
					if (lockingProcess != null && !lockingProcess.HasExited) {
						return true;
					}
				} catch {
					return false;
				}
			}
			return false;
		}

		private static void LockUpdate()
		{
			int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
			File.WriteAllText(lockPath, pid.ToString());
		}

		public static async void ShowUpdaterWindow()
		{
			var window = new Window(new WindowOptions {
				Style = WindowStyle.Dialog,
				Visible = false,
				FixedSize = false,
			});
			ThemedScrollView scrollView;
			var windowWidget = new ThemedInvalidableWindowWidget(window) {
				Id = "MainWindow",
				Layout = new HBoxLayout {
					Spacing = 6
				},
				Padding = new Thickness(6),
				Size = window.ClientSize,
				Nodes = {
					new Widget {
						Layout = new VBoxLayout {
							Spacing = 6,
						},
						Nodes = {
							(scrollView = new ThemedScrollView() {
							}),
							new ThemedFrame {
								Padding = new Thickness(10),
								Layout = new HBoxLayout(),
								LayoutCell = new LayoutCell {
									StretchY = 0,
								},
								Nodes = {
									new ThemedButton("&Close"),
									new Widget { LayoutCell = new LayoutCell { StretchX = float.MaxValue } }
								}
							}
						}
					}
				}
			};
			scrollView.Behaviour.Content.Padding = new Thickness(4);
			scrollView.Behaviour.Content.Layout = new VBoxLayout();
#pragma warning disable 4014
			client.Repository.Release.GetAll("mrojkov", "Citrus").ContinueWith((releases) => {
#pragma warning restore 4014
				foreach (var release in releases.Result) {
					scrollView.Content.AddNode(new Widget {
						Layout = new HBoxLayout(),
						Nodes = {
							new ThemedSimpleText(release.TagName),
							new ThemedSimpleText(release.Name),
							new ThemedSimpleText(release.Body),
						},
					});
				}
			});
			window.ShowModal();
		}
	}
}
