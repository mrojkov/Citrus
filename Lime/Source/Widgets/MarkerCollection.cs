using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Коллекция маркеров анимации
	/// </summary>
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
		
		/// <summary>
		/// Ищет маркер с указанным Id. Если такого маркера нет, возвращает null
		/// </summary>
		public Marker TryFind(string id)
		{
			foreach (var marker in this) {
				if (marker.Id == id) {
					return marker;
				}
			}
			return null;
		}

		/// <summary>
		/// Ищет маркер с указанным Id. Если такого маркера нет, возвращает false
		/// </summary>
		/// <param name="id">Id маркера</param>
		/// <param name="marker">Переменная, в которую будет записан результат</param>
		public bool TryFind(string id, out Marker marker)
		{
			marker = TryFind(id);
			return marker != null;
		}

		/// <summary>
		/// Ищет маркер с указанным Id. Если такого маркера нет, генерирует исключение
		/// </summary>
		/// <exception cref="Lime.Exception"/>
		public Marker this[string id]
		{
			get { return Find(id); }
		}

		/// <summary>
		/// Ищет маркер с указанным Id. Если такого маркера нет, генерирует исключение
		/// </summary>
		/// <exception cref="Lime.Exception"/>
		public Marker Find(string id)
		{
			var marker = TryFind(id);
			if (marker == null) {
				throw new Lime.Exception("Unknown marker '{0}'", id);
			}	
			return marker;
		}

		/// <summary>
		/// Возвращает маркер, находящийся на указанном кадре. Если маркера нет, возвращает null
		/// </summary>
		public Marker GetByFrame(int frame)
		{
			foreach (var marker in this) {
				if (marker.Frame == frame) {
					return marker;
				}
			}
			return null;
		}

		/// <summary>
		/// Добавляет Stop-маркер (маркер конца анимации)
		/// </summary>
		/// <param name="id">Название маркера</param>
		/// <param name="frame">Номер кадра, на котором будет установлен маркер</param>
		public void AddStopMarker(string id, int frame)
		{
			Add(new Marker() { Id = id, Action = MarkerAction.Stop, Frame = frame });
		}

		/// <summary>
		/// Добавляет Play-маркер (маркер начала анимации)
		/// </summary>
		/// <param name="id">Название маркера</param>
		/// <param name="frame">Номер кадра, на котором будет установлен маркер</param>
		public void AddPlayMarker(string id, int frame)
		{
			Add(new Marker() { Id = id, Action = MarkerAction.Play, Frame = frame });
		}
	}
}
