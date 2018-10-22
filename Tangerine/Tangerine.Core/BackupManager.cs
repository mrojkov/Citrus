using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;
using Exception = System.Exception;

namespace Tangerine.Core
{
	public class BackupManager
	{
		private class AutosaveBackupProcessor : ITaskProvider
		{
			private readonly BackupManager backupManager;

			public AutosaveBackupProcessor(BackupManager backupManager)
			{
				this.backupManager = backupManager;
			}

			public IEnumerator<object> Task()
			{
				while (true) {
					yield return periodOfConservation;
					backupManager.SaveModifiedDocuments();
				}
			}
		}

		public class Backup : IComparable<Backup>
		{
			public Backup(DateTime dateTime, string path, bool isActual)
			{
				DateTime = dateTime;
				Path = path;
				IsActual = isActual;
			}

			public int CompareTo(Backup other)
			{
				return DateTime.Compare(other.DateTime, DateTime);
			}

			public DateTime DateTime;
			public string Path;
			public bool IsActual;
		}

		private static readonly int[] intervalsOfPreservation = {
			10, 10, 10, 10, 10, 10, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60,
			60,	60, 60, 60, 1440, 1440, 1440, 1440, 1440, 1440, 1440, 43800, 43800
		};

		private static int periodOfConservation = 60;

		private static BackupManager instance;
		public static BackupManager Instance => instance ?? (instance = new BackupManager());

		private static Document lastKnownDocument;

		private readonly AutosaveBackupProcessor autosaveBackup;

		private string projectName;

		enum Mode
		{
			Normal,
			SaveOriginal,
			Scan
		}

		public event Action BackupSaved;

		private Mode mode;
		private bool activated;

		private BackupManager()
		{
			autosaveBackup = new AutosaveBackupProcessor(this);
			mode = Mode.Normal;
		}

		private string GetTemporaryPath(string path)
		{
			return Path.Combine(Lime.Environment.GetDataDirectory("Tangerine"), "Backups", projectName, "Data", path);
		}

		public List<Backup> GetHistory(Document document)
		{
			if (Document.Current != null) {

				var path = GetTemporaryPath(document.Path);
				var history = GetHistory(path);
				if (mode != Mode.Normal) {
					if (history != null) {
						history.Last().IsActual = true;
					}
				}

				return history;
			}

			return null;
		}

		private List<Backup> GetHistory(string path)
		{
			if (!Directory.Exists(path)) {
				return null;
			}

			var history = new List<Backup>();
			var files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);

			foreach (var file in files) {
				var name = Path.GetFileNameWithoutExtension(file);
				try {
					var dataTime = DateTime.ParseExact(name, "yyyy-MM-dd-HH-mm-ss",
						System.Globalization.CultureInfo.InvariantCulture);
					history.Add(new Backup(dataTime, file, false));
				} catch (Exception e) {
					Console.WriteLine($"Invalid file name '{file}':\n{e}");
				}
			}

			return history;
		}

		private void RemoveBackups(List<Backup> history)
		{
			history.Sort();
			int intervalIndex = 0;
			double interval = intervalsOfPreservation[intervalIndex];
			bool isFirstInterval = true;
			var dateNow = DateTime.Now;

			for (int i = 0; i < history.Count && intervalIndex < intervalsOfPreservation.Length; i++) {
				if ((dateNow - history[i].DateTime).TotalMinutes < interval) {
					if (!isFirstInterval) {
						if (i + 1 < history.Count && (dateNow - history[i + 1].DateTime).TotalMinutes < interval) {
							File.Delete(history[i].Path);
						}
					}
				} else {
					if (intervalIndex < intervalsOfPreservation.Length - 1) {
						intervalIndex++;
					}
					interval += intervalsOfPreservation[intervalIndex];
					isFirstInterval = false;
					i--;
				}
			}
		}

		private void SaveBackup(Document document)
		{
			var path = GetTemporaryPath(document.Path);
			Directory.CreateDirectory(path);
			try {
				document.SaveTo(Path.Combine(path, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")));
			} catch (Exception e) {
				Console.WriteLine($"Error on autosave document '{document.Path}':\n{e}");
			}

			var history = GetHistory(path);
			if (history != null) {
				if (!(document == Document.Current && mode == Mode.SaveOriginal)) {
					RemoveBackups(history);
				}
			}

			if (lastKnownDocument == document && mode == Mode.Scan) {
				mode = Mode.Normal;
			}

			BackupSaved?.Invoke();
		}

		public void Activate(TaskList tasks)
		{
			if (activated == false) {
				activated = true;
				tasks.Add(autosaveBackup);
				Project.Opening += OnProjectOpening;
				Project.DocumentSaving += OnDocumentSaving;
			}
		}

		public void SelectBackup(Backup backup)
		{
			switch (mode) {
				case Mode.Normal: {
					mode = Mode.SaveOriginal;
					Document.Current.Save();
					RestoreBackup(backup);
					mode = Mode.Scan;
				}
				break;
				case Mode.Scan: {
					RestoreBackup(backup);
				}
				break;
			}
		}

		private void RestoreBackup(Backup backup)
		{
			var localPath = Document.Current.Path;
			string systemPath;
			Project.Current.GetFullPath(Document.Current.Path, out systemPath);
			Project.Current.CloseDocument(Document.Current);
			File.Delete(systemPath);
			File.Copy(backup.Path, systemPath, true);
			lastKnownDocument = Project.Current.OpenDocument(localPath);
		}

		private void SaveModifiedDocuments()
		{
			foreach (var document in Project.Current.Documents) {
				if (document.IsModified) {
					SaveBackup(document);
				}
			}
		}

		private void OnProjectOpening(string path)
		{
			projectName = Path.GetFileNameWithoutExtension(path);
		}

		private void OnDocumentSaving(Document document)
		{
			if (lastKnownDocument != null && lastKnownDocument != Document.Current) {
				mode = Mode.Normal;
			}
			lastKnownDocument = Document.Current;
			SaveBackup(document);
		}
	}
}
