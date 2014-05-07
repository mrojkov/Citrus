using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class MarkerCollection : List<Marker>
	{
		public MarkerCollection() { }
		public MarkerCollection(int capacity) 
			: base(capacity)
		{ }

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

		public Marker this[string id]
		{
			get { return Find(id); }
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
			foreach (var marker in this) {
				if (marker.Frame == frame) {
					return marker;
				}
			}
			return null;
		}

		public void AddStopMarker(string id, int frame)
		{
			Add(new Marker() { Id = id, Action = MarkerAction.Stop, Frame = frame });
		}

		public void AddPlayMarker(string id, int frame)
		{
			Add(new Marker() { Id = id, Action = MarkerAction.Play, Frame = frame });
		}
	}
}
