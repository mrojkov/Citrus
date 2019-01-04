using System;
using System.Collections.Generic;

namespace Lime
{
	public class ShaderParams
	{
		private static Dictionary<ShaderParamSortingKeyInfo, int> orderKeyLookup = new Dictionary<ShaderParamSortingKeyInfo, int>();

		internal int Count => Items.Count;
		internal List<int> SortingKeys = new List<int>();
		internal List<int> ItemIndices = new List<int>();
		internal List<ShaderParam> Items = new List<ShaderParam>();

		internal static int GetSortingKey(string name, Type type)
		{
			lock (orderKeyLookup) {
				var orderKeyInfo = new ShaderParamSortingKeyInfo { Name = name, Type = type };
				int orderKey;
				if (orderKeyLookup.TryGetValue(orderKeyInfo, out orderKey)) {
					return orderKey;
				}
				orderKey = orderKeyLookup.Count;
				orderKeyLookup.Add(orderKeyInfo, orderKey);
				return orderKey;
			}
		}

		public ShaderParamKey<T> GetParamKey<T>(string name)
		{
			var sortingKey = GetSortingKey(name, typeof(T));
			var index = FindParameterIndex(sortingKey);
			if (index < 0) {
				index = ~index;
				SortingKeys.Insert(index, sortingKey);
				ItemIndices.Insert(index, Items.Count);
				Items.Add(null);
			}
			return new ShaderParamKey<T> { Name = name, Index = ItemIndices[index] };
		}

		public void Set<T>(ShaderParamKey<T> key, T value)
		{
			var p = GetParameter(key, 1);
			p.Data[0] = value;
			p.Count = 1;
			unchecked {
				p.Version++;
			}
		}

		public void Set<T>(ShaderParamKey<T> key, T[] value, int count)
		{
			var p = GetParameter(key, count);
			Array.Copy(value, p.Data, count);
			p.Count = count;
			unchecked {
				p.Version++;
			}
		}

		private ShaderParam<T> GetParameter<T>(ShaderParamKey<T> key, int capacity)
		{
			var p = Items[key.Index] as ShaderParam<T>;
			if (p == null || p.Data.Length < capacity) {
				p = new ShaderParam<T>(capacity) { Name = key.Name };
				Items[key.Index] = p;
			}
			return p;
		}

		private int FindParameterIndex(int sortingKey)
		{
			var min = 0;
			var max = SortingKeys.Count - 1;
			while (min <= max) {
				var mid = (min + max) >> 1;
				var midOrderKey = SortingKeys[mid];
				if (midOrderKey < sortingKey) {
					min = mid + 1;
				} else if (midOrderKey > sortingKey) {
					max = mid - 1;
				} else {
					return mid;
				}
			}
			return ~min;
		}
	}

	public struct ShaderParamKey<T>
	{
		internal int Index;
		internal string Name;
	}

	internal class ShaderParam
	{
		public string Name;
		public int Version;
	}

	internal class ShaderParam<T> : ShaderParam
	{
		public T[] Data;
		public int Count;

		public ShaderParam(int capacity)
		{
			Data = new T[capacity];
		}
	}

	internal struct ShaderParamSortingKeyInfo : IEquatable<ShaderParamSortingKeyInfo>
	{
		public string Name;
		public Type Type;

		public bool Equals(ShaderParamSortingKeyInfo other)
		{
			return Name == other.Name && Type == other.Type;
		}

		public override int GetHashCode()
		{
			unchecked {
				var hash = Name.GetHashCode();
				hash = (hash * 397) ^ Type.GetHashCode();
				return hash;
			}
		}
	}
}
