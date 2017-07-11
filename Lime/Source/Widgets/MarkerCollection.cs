using System;
using System.Collections.Generic;

namespace Lime
{
	public class MarkerCollection : List<Marker>
	{
		public MarkerCollection() { }
		public MarkerCollection(int capacity) : base(capacity) { }

		public Marker this[string id]
		{
			get { return Find(id); }
		}

		internal static MarkerCollection DeepClone(MarkerCollection source)
		{
			var result = new MarkerCollection(source.Count);
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
			foreach (var marker in this) {
				if (marker.Frame == frame) {
					return marker;
				}
			}
			return null;
		}

		public new void Add(Marker marker)
		{
			if (Count == 0 || marker.Frame > this[Count - 1].Frame) {
				base.Add(marker);
			} else {
				throw new InvalidOperationException();
			}
		}

		public void AddOrdered(Marker marker)
		{
			if (Count == 0 || marker.Frame > this[Count - 1].Frame) {
				base.Add(marker);
			} else {
				int i = 0;
				while (this[i].Frame < marker.Frame) {
					i++;
				}
				if (this[i].Frame == marker.Frame) {
					this[i] = marker;
				} else {
					Insert(i, marker);
				}
			}
		}
	}
}