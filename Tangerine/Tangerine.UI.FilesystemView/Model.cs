using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tangerine.Core;

namespace Tangerine.UI.FilesystemView
{
	public class Selection : IEnumerable<string>, IReadOnlyVersionedCollection<string>
	{
		public Selection Clone()
		{
			var r = new Selection();
			r.selection = new HashSet<string>(this);
			return r;
		}
		private HashSet<string> selection = new HashSet<string>();
		public bool Empty => selection.Count == 0;
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

		public static bool operator !=(Selection lhs, Selection rhs)
		{
			return !(lhs == rhs);
		}

		public static bool operator ==(Selection lhs, Selection rhs)
		{
			if (object.ReferenceEquals(lhs, null) && object.ReferenceEquals(rhs, null)) {
				return true;
			}
			if (object.ReferenceEquals(lhs, null) || object.ReferenceEquals(rhs, null)) {
				return false;
			}
			return lhs.SequenceEqual(rhs);
		}

		public int Count => selection.Count;

		public int Version { get; private set; }
	}

	public class Model
	{
		private string currentPath;
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

		public Model(string path)
		{
			currentPath = path;
			if (!Directory.Exists(path)) {
				currentPath = Directory.GetCurrentDirectory();
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
			return EnumerateItems(SortType.Name, OrderType.Ascending);
		}

		public IEnumerable<string> EnumerateItems(SortType sortType, OrderType orderType)
		{
			IEnumerable<string> dirs = Directory.EnumerateDirectories(CurrentPath);
			IEnumerable<string> files = Directory.EnumerateFiles(CurrentPath);
			switch (sortType) {
				case SortType.Name:
					if (orderType == OrderType.Ascending) {
						dirs = dirs.OrderBy(f => f);
						files = files.OrderBy(f => f);
					} else {
						dirs = dirs.OrderByDescending(f => f);
						files = files.OrderByDescending(f => f);
					}
					break;
				case SortType.Date:
					if (orderType == OrderType.Ascending) {
						dirs = dirs.OrderBy(f => new FileInfo(f).LastWriteTime);
						files = files.OrderBy(f => new FileInfo(f).LastWriteTime);
					} else {
						dirs = dirs.OrderByDescending(f => new FileInfo(f).LastWriteTime);
						files = files.OrderByDescending(f => new FileInfo(f).LastWriteTime);
					}
					break;
				case SortType.Extension:
					if (orderType == OrderType.Ascending) {
						dirs = dirs.OrderBy(f => new FileInfo(f).Extension);
						files = files.OrderBy(f => new FileInfo(f).Extension);
					} else {
						dirs = dirs.OrderByDescending(f => new FileInfo(f).Extension);
						files = files.OrderByDescending(f => new FileInfo(f).Extension);
					}
					break;
				case SortType.Size:
					if (orderType == OrderType.Ascending) {
						dirs = dirs.OrderBy(f => f.Length);
						files = files.OrderBy(f => f.Length);
					} else {
						dirs = dirs.OrderByDescending(f => f.Length);
						files = files.OrderByDescending(f => f.Length);
					}
					break;
				default:
					if (orderType == OrderType.Ascending) {
						dirs = dirs.OrderBy(f => f);
						files = files.OrderBy(f => f);
					} else {
						dirs = dirs.OrderByDescending(f => f);
						files = files.OrderByDescending(f => f);
					}
					break;
			}

			foreach (var i in dirs) {
				yield return i;
			}
			foreach (var i in files) {
				yield return i;
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
