using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Tangerine.Core
{
	public interface IReadOnlyVersionedCollection<T> : IReadOnlyCollection<T>
	{
		int Version { get; }
	}

	public class VersionedCollection<T> : Collection<T>, IReadOnlyVersionedCollection<T>
	{
		public int Version { get; private set; }

		protected override void ClearItems()
		{
			base.ClearItems();
			Version++;
		}

		protected override void InsertItem(int index, T item)
		{
			base.InsertItem(index, item);
			Version++;
		}

		protected override void RemoveItem(int index)
		{
			base.RemoveItem(index);
			Version++;
		}

		protected override void SetItem(int index, T item)
		{
			base.SetItem(index, item);
			Version++;
		}

		public void AddRange(IEnumerable<T> values)
		{
			foreach (var v in values) {
				Add(v);
			}
		}
	}
}