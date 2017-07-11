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
		public Selection Clone()
		{
			var r = new Selection();
			r.selection = new HashSet<string>(this);
			return r;
		}
		public event Action Changed;
		private HashSet<string> selection = new HashSet<string>();
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
			foreach (var i in Directory.EnumerateDirectories(CurrentPath).OrderBy(f => f)) {
				yield return i;
			}
			foreach (var i in Directory.EnumerateFiles(CurrentPath).OrderBy(f => f)) {
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