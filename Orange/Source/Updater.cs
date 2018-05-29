using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Lime;
using Octokit;
using Task = System.Threading.Tasks.Task;

namespace Orange
{
	public static class Updater
	{
		private static GitHubClient client = new GitHubClient(new ProductHeaderValue("mrojkov-citrus-auto-updater"));
		private static bool firstUpdate = true;

		public static async Task CheckForUpdates()
		{
			var task = Task.Run(async () => {
				for (;;)
				{
					if (!firstUpdate) {
						await Task.Delay(TimeSpan.FromMinutes(5.0));
					}
					firstUpdate = false;
					var citrusVersion = CitrusVersion.Load();
					if (!citrusVersion.IsStandalone) {
						continue;
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
					var exePath = Path.GetDirectoryName(Uri.UnescapeDataString((new Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath));
#if MAC
					exePath = Path.Combine(exePath, "..", "..", "..");
#endif // MAC
					var updatingFlagPath = Path.Combine(exePath, "UPDATING");
					if (File.Exists(updatingFlagPath)) {
						continue;
					}
					File.Create(updatingFlagPath).Dispose();
					try {
						Console.WriteLine($"oh wow, you had a {tagName} version and new {latest.TagName} version is available! Downloading update!");
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
								var tempPath = Path.Combine(exePath, "previous-release");
								if (Directory.Exists(tempPath)) {
									Directory.Delete(tempPath, true);
								}
								Directory.CreateDirectory(tempPath);
								foreach (var fi in new FileEnumerator(exePath).Enumerate()) {
									if (fi.Path == "UPDATING") {
										continue;
									}
									var dstPath = Path.Combine(tempPath, fi.Path);
									Directory.CreateDirectory(Path.GetDirectoryName(dstPath));
									File.Move(Path.Combine(exePath, fi.Path), dstPath);
								}
								zipArchive.ExtractToDirectory(exePath);
							}
						}
#if MAC
						var process = new System.Diagnostics.Process {
							StartInfo = new System.Diagnostics.ProcessStartInfo {
								FileName = "tar",
								WorkingDirectory = exePath,
								Arguments = "-xvf bundle.tar"
							}
						};
						process.Start();
						process.WaitForExit();
#endif // MAC
						Console.WriteLine("Update finished! Please restart");
					} finally {
						File.Delete(updatingFlagPath);
					}
				}
			});
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
