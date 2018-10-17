using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tangerine.Core;

namespace Tangerine.UI.FilesystemView
{
#pragma warning disable CS0660, CS0661
	public class FilesystemSelection : IEnumerable<string>, IReadOnlyVersionedCollection<string>
	{
		public int Count => selection.Count;
		public int Version { get; private set; }
		public bool Empty => selection.Count == 0;
		private HashSet<string> selection = new HashSet<string>();

		public void Select(string path)
		{
			if (!selection.Contains(path)) {
				selection.Add(path);
				Version++;
			}
		}

		public void SelectRange(IEnumerable<string> source)
		{
			bool changed = false;
			foreach (var path in source) {
				if (!selection.Contains(path)) {
					changed = true;
					selection.Add(path);
				}
			}
			if (changed) {
				Version++;
			}
		}

		public void Deselect(string path)
		{
			if (selection.Contains(path)) {
				selection.Remove(path);
				Version++;
			}
		}

		public void Clear()
		{
			if (selection.Count != 0) {
				selection.Clear();
				Version++;
			}
		}

		public bool Contains(string path) => selection.Contains(path);
		public FilesystemSelection Clone() => new FilesystemSelection { selection = new HashSet<string>(this) };

		public IEnumerator<string> GetEnumerator() => selection.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public static bool operator !=(FilesystemSelection lhs, FilesystemSelection rhs) => !(lhs == rhs);
		public static bool operator ==(FilesystemSelection lhs, FilesystemSelection rhs)
		{
			if (lhs is null && rhs is null) {
				return true;
			}
			if (lhs is null || rhs is null) {
				return false;
			}
			return lhs.SequenceEqual(rhs);
		}
	}
#pragma warning restore CS0660, CS0661

	public class FilesystemModel
	{
		enum ItemType
		{
			File,
			Directory,
		}

		private string currentPath;
		public string CurrentPath
		{
			get => currentPath;
			set {
				try {
					Directory.EnumerateFileSystemEntries(value).ToList();
					currentPath = value;
				} catch (Exception e) {
					new AlertDialog(e.Message).Show();
				}
			}
		}

		public FilesystemModel(string path)
		{
			currentPath = path;
			if (!Directory.Exists(path)) {
				currentPath = Project.Current.AssetsDirectory;
				if (string.IsNullOrEmpty(currentPath)) {
					currentPath = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());
				}
			}
		}

		public void GoUp() => CurrentPath = GetFirstExistentParentPath(Path.GetDirectoryName(CurrentPath));

		private string GetFirstExistentParentPath(string path)
		{
			while (!Directory.Exists(path)) {
				path = Path.GetDirectoryName(path);
			}
			return path;
		}

		public IEnumerable<string> EnumerateItems(SortType sortType, OrderType orderType)
		{
			CurrentPath = GetFirstExistentParentPath(CurrentPath);
			foreach (var i in SortItems(Directory.EnumerateDirectories(CurrentPath), sortType, orderType, ItemType.Directory)) {
				if ((!File.Exists(i) && !Directory.Exists(i)) || (File.GetAttributes(i) & FileAttributes.Hidden) != 0) {
					continue;
				}
				yield return i;
			}
			foreach (var i in SortItems(Directory.EnumerateFiles(CurrentPath), sortType, orderType, ItemType.File)) {
				if ((!File.Exists(i) && !Directory.Exists(i)) || (File.GetAttributes(i) & FileAttributes.Hidden) != 0) {
					continue;
				}
				yield return i;
			}
		}

		private IEnumerable<string> SortItems(IEnumerable<string> items, SortType sortType, OrderType orderType, ItemType itemType)
		{
			switch (sortType) {
				case SortType.Name:
					if (orderType == OrderType.Ascending) {
						return items.OrderBy(f => f);
					} else {
						return items.OrderByDescending(f => f);
					}
				case SortType.Date:
					if (orderType == OrderType.Ascending) {
						return items.OrderBy(f => new FileInfo(f).LastWriteTime);
					} else {
						return items.OrderByDescending(f => new FileInfo(f).LastWriteTime);
					}
				case SortType.Extension:
					if (orderType == OrderType.Ascending) {
						return items.OrderBy(f => new FileInfo(f).Extension);
					} else {
						return items.OrderByDescending(f => new FileInfo(f).Extension);
					}
				case SortType.Size:
					if (orderType == OrderType.Ascending) {
						if (itemType == ItemType.Directory)
							return items.OrderBy( f => f.Length);
						else
							return items.OrderBy(f => new FileInfo(f).Length);
					} else {
						if (itemType == ItemType.Directory)
							return items.OrderByDescending(f => f.Length);
						else
							return items.OrderByDescending(f => new FileInfo(f).Length);
					}
				default:
					throw new ArgumentException();
			}
		}

		public void GoTo(string path)
		{
			var attr = File.GetAttributes(path);
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
				CurrentPath = path;
			} else {
				throw new Lime.Exception("Can only navigate to directories");
			}
		}
	}
}
