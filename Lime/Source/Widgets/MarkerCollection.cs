using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class MarkerCollection : ICollection<Marker>
	{
		static List<Marker> emptyList = new List<Marker>();
		List<Marker> markers = emptyList;

		public Marker[] AsArray()
		{
			return markers.ToArray();
		}

		internal static MarkerCollection DeepClone(MarkerCollection source)
		{
			var result = new MarkerCollection();
			foreach (var marker in source.markers) {
				var clone = marker.Clone();
				result.Add(clone);
			}
			return result;
		}

		public Marker this[int index] { 
			get { return markers[index]; }
		}
		
		void ICollection<Marker>.CopyTo(Marker[] a, int index)
		{
			markers.CopyTo(a, index);
		}

		public int Count { get { return markers.Count; } }

		IEnumerator<Marker> IEnumerable<Marker>.GetEnumerator()
		{
			return markers.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return markers.GetEnumerator();
		}

		public Marker TryFind(string id)
		{
			int count = markers.Count;
			for (int i = 0; i < count; i++) {
				var marker = markers[i];
				if (marker.Id == id) {
					return marker;
				}
			}
			return null;
		}

		public Marker Find(string id)
		{
			var marker = TryFind(id);
			if (marker == null) {
				throw new Lime.Exception("Unknown marker '{0}'", id);
			}
			return marker;
		}

		public Marker GetByFrame(int frame)
		{
			int count = markers.Count;
			for (int i = 0; i < count; i++) {
				var marker = markers[i];
				if (marker.Frame == frame) {
					return marker;
				}
			}
			return null;
		}

		public void Add(Marker marker)
		{
			if (markers == emptyList) {
				markers = new List<Marker>();
			}
			markers.Add(marker);
		}

		public void Clear()
		{
			markers = emptyList;
		}
		
		public bool Contains(Marker item)
		{
			return markers.Contains(item);
		}
		
		public bool Remove(Marker item)
		{
			bool result = markers.Remove(item);
			if (markers.Count == 0) {
				markers = emptyList;
			}
			return result;
		}
		
		bool ICollection<Marker>.IsReadOnly { 
			get { return false; }
		}
	}
}
