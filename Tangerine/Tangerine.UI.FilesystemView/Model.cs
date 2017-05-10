using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tangerine.Core;

namespace Tangerine.UI.FilesystemView
{
	public class Selection : IEnumerable<string>
	{
		public event Action Changed;
		private readonly HashSet<string> selection = new HashSet<string>();
		public bool Empty => selection.Count == 0;
		public void Select(string path)
		{
			if (!selection.Contains(path)) {
				selection.Add(path);
				Changed?.Invoke();
			}
		}
		public void Deselect(string path)
		{
			if (selection.Contains(path)) {
				selection.Remove(path);
				Changed?.Invoke();
			}
		}
		public void Clear()
		{
			if (selection.Count != 0) {
				selection.Clear();
				Changed?.Invoke();
			}
		}
		public bool Contains(string path)
		{
			return selection.Contains(path);
		}

		public IEnumerator<string> GetEnumerator()
		{
			return selection.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public class Model
	{
		private string currentPath = Project.Current?.AssetsDirectory ?? Directory.GetCurrentDirectory();
		public string CurrentPath
		{
			get { return currentPath; }
			set
			{
				try {
					Directory.EnumerateFileSystemEntries(value).ToList();
					currentPath = value;
				} catch {
					(new AlertDialog("Invalid Directory")).Show();
				}
			}
		}

		public void GoUp()
		{
			var p = Directory.GetParent(CurrentPath);
			if (p == null) {
				return;
			}
			CurrentPath = p.FullName;
		}

		public IEnumerable<string> EnumerateItems()
		{
			return Directory.EnumerateFileSystemEntries(CurrentPath);
		}

		public void GoTo(string path)
		{
			FileAttributes attr = File.GetAttributes(path);
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
				CurrentPath = path;
			}
		}
	}
}