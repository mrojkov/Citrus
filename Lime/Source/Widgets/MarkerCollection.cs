using System;
using System.Collections.Generic;

namespace Lime
{
	public class MarkerCollection : List<Marker>
	{
		private readonly Animation owner;

		public MarkerCollection(Animation owner)
		{
			this.owner = owner;
		}

		public MarkerCollection(Animation owner, int capacity) : base(capacity)
		{
			this.owner = owner;
		}

		public Marker this[string id]
		{
			get { return Find(id); }
		}

		internal static MarkerCollection DeepClone(MarkerCollection source, Animation owner)
		{
			var result = new MarkerCollection(owner, source.Count);
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
			if (Count != 0 && marker.Frame <= this[Count - 1].Frame) {
				throw new InvalidOperationException();
			}
			base.Add(marker);
			owner.NextMarkerOrTriggerTime = null;
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
			owner.NextMarkerOrTriggerTime = null;
		}
	}
}