using System;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public class MarkerList : IList<Marker>
	{
		private readonly List<Marker> markers;
		private readonly Animation owner;

		public int Count => markers.Count;
		public bool IsReadOnly => false;

		public MarkerList(Animation owner)
		{
			this.owner = owner;
			markers = new List<Marker>();
		}

		public MarkerList(Animation owner, int capacity)
		{
			this.owner = owner;
			markers = new List<Marker>(capacity);
		}

		public void Clear() => markers.Clear();
		public bool Contains(Marker item) => markers.Contains(item);
		public void CopyTo(Marker[] array, int arrayIndex) => markers.CopyTo(array, arrayIndex);
		public List<Marker>.Enumerator GetEnumerator() => markers.GetEnumerator();

		IEnumerator<Marker> IEnumerable<Marker>.GetEnumerator() => markers.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => markers.GetEnumerator();

		public int IndexOf(Marker item) => markers.IndexOf(item);

		public void Insert(int index, Marker item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(Marker item)
		{
			int index = GetIndexByFrame(item.Frame);
			if (index < 0) {
				return false;
			}
			markers.RemoveAt(index);
			return true;
		}

		public void RemoveAt(int index) => markers.RemoveAt(index);

		public Marker this[int index]
		{
			get { return markers[index]; }
			set { throw new NotSupportedException(); }
		}
		public Marker this[string id] => Find(id);

		internal static MarkerList DeepClone(MarkerList source, Animation owner)
		{
			var result = new MarkerList(owner, source.Count);
			foreach (var marker in source) {
				result.Add(marker.Clone());
			}
			return result;
		}

		public Marker TryFind(string id)
		{
			foreach (var marker in this) {
				if (marker.Id == id) {
					return marker;
				}
			}
			return null;
		}

		public bool TryFind(string id, out Marker marker)
		{
			marker = TryFind(id);
			return marker != null;
		}

		public Marker Find(string id)
		{
			var marker = TryFind(id);
			if (marker == null) {
				throw new ArgumentException($"Unknown marker '{id}'");
			}
			return marker;
		}

		public Marker GetByFrame(int frame)
		{
			int index = GetIndexByFrame(frame);
			return index >= 0 ? markers[index] : null;
		}

		private int GetIndexByFrame(int frame)
		{
			int l = 0;
			int r = markers.Count - 1;
			while (l <= r) {
				int m = (l + r) / 2;
				if (markers[m].Frame < frame) {
					l = m + 1;
				} else if (markers[m].Frame > frame) {
					r = m - 1;
				} else {
					return m;
				}
			}
			return -1;
		}

		public int FindIndex(Predicate<Marker> match)
		{
			return markers.FindIndex(match);
		}

		public bool Exists(Predicate<Marker> match)
		{
			return markers.Exists(match);
		}

		public void Add(Marker marker)
		{
			markers.Add(marker);
			owner.NextMarker = null;
		}

		public void AddOrdered(Marker marker)
		{
			if (Count == 0 || marker.Frame > this[Count - 1].Frame) {
				markers.Add(marker);
			} else {
				int i = 0;
				while (markers[i].Frame < marker.Frame) {
					i++;
				}
				if (markers[i].Frame == marker.Frame) {
					markers[i] = marker;
				} else {
					markers.Insert(i, marker);
				}
			}
			owner.NextMarker = null;
		}
	}
}
