using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class MarkerCollection : ICollection<Marker>
	{
		private readonly List<Marker> markers = new List<Marker> ();

		public Marker this [int index] { 
			get { return markers [index]; }
		}
		
		void ICollection<Marker>.CopyTo (Marker[] a, int index)
		{
			markers.CopyTo (a, index);
		}

		public int Count { get { return markers.Count; } }

		IEnumerator<Marker> IEnumerable<Marker>.GetEnumerator ()
		{
			return markers.GetEnumerator ();
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return markers.GetEnumerator ();
		}

		public Marker Find (string id)
		{
			int count = markers.Count;
			for (int i = 0; i < count; i++) {
				var marker = markers [i];
				if (marker.Id == id) {
					return marker;
				}
			}
			return null;
		}

		public Marker FindByFrame (int frame)
		{
			int count = markers.Count;
			for (int i = 0; i < count; i++) {
				var marker = markers [i];
				if (marker.Frame == frame) {
					return marker;
				}
			}
			return null;
		}

		public void Add (Marker marker)
		{
			markers.Add (marker);
		}

		public void Clear ()
		{
			markers.Clear ();
		}
		
		public bool Contains (Marker item)
		{
			return markers.Contains (item);
		}
		
		public bool Remove (Marker item)
		{
			return markers.Remove (item);
		}
		
		bool ICollection<Marker>.IsReadOnly { 
			get { return false; }
		}
	}
}
