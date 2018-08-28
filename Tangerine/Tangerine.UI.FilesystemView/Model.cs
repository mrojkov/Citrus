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

		public override bool Equals(object obj)
		{
			var selection = obj as Selection;
			return selection != null &&
				   EqualityComparer<HashSet<string>>.Default.Equals(this.selection, selection.selection);
		}

		public override int GetHashCode() => 965136989 + EqualityComparer<HashSet<string>>.Default.GetHashCode(this.selection);

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

		enum ItemType
		{
			File,
			Dir
		}

		private string currentPath;
		public string CurrentPath
		{
			get { return currentPath; }
			set {
				try {
					Directory.EnumerateFileSystemEntries(value).ToList();
					currentPath = value;
				}
				catch (Exception e) {
					new AlertDialog(e.Message).Show();
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

		public IEnumerable<string> EnumerateItems(SortType sortType, OrderType orderType)
		{
			foreach (var i in SortItems(Directory.EnumerateDirectories(CurrentPath), sortType, orderType, ItemType.Dir)) {
				if ((File.GetAttributes(i) & FileAttributes.Hidden) != 0) {
					continue;
				}
				yield return i;
			}
			foreach (var i in SortItems(Directory.EnumerateFiles(CurrentPath), sortType, orderType, ItemType.File)) {
				if ((File.GetAttributes(i) & FileAttributes.Hidden) != 0) {
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
						if (itemType == ItemType.Dir)
							return items.OrderBy( f => f.Length);
						else
							return items.OrderBy(f => new FileInfo(f).Length);
					} else {
						if (itemType == ItemType.Dir)
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
