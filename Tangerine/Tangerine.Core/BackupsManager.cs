using System;
using System.Collections.Generic;
using System.IO;
using Lime;
using Exception = System.Exception;

namespace Tangerine.Core
{
	public class BackupsManager
	{
		public class AutosaveBackupProcessor : ITaskProvider
		{
			private readonly BackupsManager backupsManager;

			public AutosaveBackupProcessor(BackupsManager backupsManager)
			{
				this.backupsManager = backupsManager;
			}

			public IEnumerator<object> Task()
			{
				while (true) {
					yield return PeriodOfConservation;
					foreach (var document in Project.Current.Documents) {
						if (document.IsModified) {
							backupsManager.Savebackup(document);
						}
					}
				}
			}
		}

		public class SaveBackupProcessor : ITaskProvider
		{
			private readonly BackupsManager backupsManager;

			private Document lastKnown;

			public SaveBackupProcessor(BackupsManager backupsManager)
			{
				this.backupsManager = backupsManager;
			}
			public IEnumerator<object> Task()
			{
				while (true) {
					if (lastKnown != Document.Current) {
						if (lastKnown != null) {
							lastKnown.Saving -= SaveBackup;
						}

						lastKnown = Document.Current;
						lastKnown.Saving += SaveBackup;
					}

					yield return null;
				}
			}

			private void SaveBackup(Document document)
			{
				backupsManager.Savebackup(document);
			}
		}

		private static readonly int[] intervalsOfPreservation = {
				10, 10, 10, 10, 10, 10, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60,
				60, 60, 60, 1440, 1440, 1440, 1440, 1440, 1440, 1440, 43800, 43800
		};

		private class Backup : IComparable<Backup>
		{
			public int CompareTo(Backup other)
			{
				return string.Compare(other.Path, Path, StringComparison.Ordinal);
			}

			public DateTime DateTime;
			public string Path;
		}

		private static int PeriodOfConservation = 60;

		private readonly AutosaveBackupProcessor autosaveBackup;
		private readonly SaveBackupProcessor saveBackup;

		public BackupsManager()
		{
			autosaveBackup = new AutosaveBackupProcessor(this);
			saveBackup = new SaveBackupProcessor(this);
		}

		private string GetTemporalPath(string path)
		{
			return Path.Combine(Lime.Environment.GetDataDirectory("Tangerine"), Path.GetFileName(path));
		}

		private void FillHistory(string path, out List<Backup> history)
		{
			history = new List<Backup>();
			var files = Directory.GetFiles(path, "*.tan", SearchOption.TopDirectoryOnly);
			foreach (string file in files) {
				string name = Path.GetFileNameWithoutExtension(file);
				try {
					var dataTime = DateTime.ParseExact(name, "yyyy-MM-dd-HH-mm-ss",
							System.Globalization.CultureInfo.InvariantCulture);
					history.Add(new Backup {DateTime = dataTime, Path = file});
				} catch (Exception e) {
					Console.WriteLine("Error file name '{0}':\n{1}", file, e);
				}
			}

		}

		private void RemoveBackups(List<Backup> history)
		{
			history.Sort();
			int interval = 0;
			int indexHistory = 0;
			int historyCount = history.Count;
			bool isFirstInterval = true;
			double dateNow = new TimeSpan(DateTime.Now.Ticks).TotalMinutes;
			foreach (int intervalOfPreservation in intervalsOfPreservation) {
				bool isFirstElement = true;
				interval += intervalOfPreservation;
				while (indexHistory < historyCount) {
					double historyTime = new TimeSpan(history[indexHistory].DateTime.Ticks).TotalMinutes;
					if (dateNow - historyTime <  interval ) {
						if (!isFirstInterval && !isFirstElement) {
							File.Delete(history[indexHistory].Path);
						}
						indexHistory++;
						isFirstElement = false;
					} else {
						isFirstInterval = false;
						break;
					}
				}

				if (indexHistory >= historyCount - 1) {
					break;
				}
			}
		}

		private void CreateDirectory(string path)
		{
			if (Directory.Exists(path)) {
				Console.WriteLine("That path exists already.");
				return;
			}

			// Try to create the directory.
			Directory.CreateDirectory(path);
		}

		private void Savebackup(Document document)
		{
			string path = GetTemporalPath(document.Path);
			CreateDirectory(path);
			try {
				document.SaveTo(Path.Combine(path, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")), FileAttributes.Hidden);
			} catch (Exception e) {
				Console.WriteLine("Error on autosave document '{0}':\n{1}", document.Path, e);
			}
			FillHistory(path, out var history);
			RemoveBackups(history);
		}

		public void Activate(TaskList tasks)
		{
			tasks.Add(autosaveBackup);
			tasks.Add(saveBackup);
		}
	}
}
