using System;
using System.Collections.Generic;
using System.IO;
using Lime;
using Exception = System.Exception;

namespace Tangerine.Core
{
	public class BackupsManager
	{
		private class AutosaveBackupProcessor : ITaskProvider
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
					backupsManager.SaveModifiedDocuments();
				}
			}
		}

		private class SaveBackupProcessor : ITaskProvider
		{
			private readonly BackupsManager backupsManager;

			public SaveBackupProcessor(BackupsManager backupsManager)
			{
				this.backupsManager = backupsManager;
			}

			public IEnumerator<object> Task()
			{
				while (true) {

					backupsManager.ProcessCurrentDocumentChanging();
					yield return null;
				}
			}
		}

		public class Backup : IComparable<Backup>
		{
			public Backup(DateTime dateTime, string path)
			{
				DateTime = dateTime;
				Path = path;
			}

			public int CompareTo(Backup other)
			{
				return DateTime.Compare(other.DateTime, DateTime);
			}

			public DateTime DateTime;
			public string Path;
		}

		private static readonly int[] intervalsOfPreservation = {
			10, 10, 10, 10, 10, 10, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60,
			60,	60, 60, 60, 1440, 1440, 1440, 1440, 1440, 1440, 1440, 43800, 43800
		};

		private static int PeriodOfConservation = 60;

		private static BackupsManager instance;

		public static BackupsManager Instance => instance ?? (instance = new BackupsManager());

		private static Document lastKnownDocument;

		private readonly AutosaveBackupProcessor autosaveBackup;
		private readonly SaveBackupProcessor saveBackup;

		enum Mode
		{
			Normal,
			SaveOriginal,
			Scan
		}

		public Action BackupSaved;

		private Mode mode;
		private bool activated;

		private BackupsManager()
		{
			autosaveBackup = new AutosaveBackupProcessor(this);
			saveBackup = new SaveBackupProcessor(this);
			mode = Mode.Normal;
		}

		private string GetTemporalPath(string path)
		{
			return Path.Combine(Lime.Environment.GetDataDirectory("Tangerine"), Path.GetFileName(path));
		}

		public List<Backup> GetHistory(Document document)
		{
			if (Document.Current != null) {

				string path = GetTemporalPath(document.Path);
				return GetHistory(path);
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

			foreach (string file in files) {
				string name = Path.GetFileNameWithoutExtension(file);
				try {
					var dataTime = DateTime.ParseExact(name, "yyyy-MM-dd-HH-mm-ss",
						System.Globalization.CultureInfo.InvariantCulture);
					history.Add(new Backup(dataTime, file));
				} catch (Exception e) {
					Console.WriteLine("Error file name '{0}':\n{1}", file, e);
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
					intervalIndex++;
					interval += intervalsOfPreservation[intervalIndex];
					isFirstInterval = false;
					i--;
				}
			}
		}

		private void SaveBackup(Document document)
		{
			string path = GetTemporalPath(document.Path);
			Directory.CreateDirectory(path);
			try {
				document.SaveTo(Path.Combine(path, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")));
			} catch (Exception e) {
				Console.WriteLine("Error on autosave document '{0}':\n{1}", document.Path, e);
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
				tasks.Add(saveBackup);
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
			string localPath = Document.Current.Path;
			string systemPath;
			Project.Current.GetSystemPath(Document.Current.Path, out systemPath);
			Project.Current.CloseDocument(Document.Current);
			File.Delete(systemPath);
			File.Copy(backup.Path, systemPath, true);
			if (lastKnownDocument != null) {
				lastKnownDocument.Saving -= SaveBackup;
			}

			lastKnownDocument = Project.Current.OpenDocument(localPath);
			if (lastKnownDocument != null) {
				lastKnownDocument.Saving += SaveBackup;
			}
		}

		private void ProcessCurrentDocumentChanging()
		{
			if (lastKnownDocument != Document.Current) {
				mode = Mode.Normal;
				if (lastKnownDocument != null) {
					lastKnownDocument.Saving -= SaveBackup;
				}

				lastKnownDocument = Document.Current;
				if (lastKnownDocument != null) {
					lastKnownDocument.Saving += SaveBackup;
				}
			}
		}

		private void SaveModifiedDocuments()
		{
			foreach (var document in Project.Current.Documents) {
				if (document.IsModified) {
					SaveBackup(document);
				}
			}
		}
	}
}
