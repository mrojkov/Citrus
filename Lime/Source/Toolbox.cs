using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO;

namespace Lime
{
	public static class Toolbox
	{
		public static bool IsNullOrWhiteSpace(this string value)
		{
			return string.IsNullOrWhiteSpace(value);
		}

		public static void Swap<T>(ref T lhs, ref T rhs)
		{
			T temp;
			temp = lhs;
			lhs = rhs;
			rhs = temp;
		}

		public static void Swap<T>(IList<T> list, int lhsIndex, int rhsIndex)
		{
			var temp = list[lhsIndex];
			list[lhsIndex] = list[rhsIndex];
			list[rhsIndex] = temp;
		}

		public static int ComputeHash(byte[] data, int length)
		{
			unchecked {
				const int p = 16777619;
				int hash = (int)2166136261;
				for (int i = 0; i < length; i++) {
					hash = (hash ^ data[i]) * p;
				}
				hash += hash << 13;
				hash ^= hash >> 7;
				hash += hash << 3;
				hash ^= hash >> 17;
				hash += hash << 5;
				return hash;
			}
		}

		public static List<T> Clone<T>(List<T> list)
		{
			var clone = new List<T>();
			for (int i = 0; i < list.Count; i++) {
				clone.Add(list[i]);
			}
			return clone;
		}

		// In asian languages some characters are not allowed at the start or the end of the line.
		const string NotAllowedAtTheStart =
			"!%),.:;>?]}¢¨°·ˇˉ―‖’”„‟†‡›℃∶、。〃〆〈《「『〕〗〞︵︹︽︿﹃﹘﹚﹜！＂％＇），．：；？］｀｜｝～" +
			"ヽヾーァィゥェォッャュョヮヵヶぁぃぅぇぉっゃゅょゎゕゖㇰㇱㇲㇳㇴㇵㇶㇷㇸㇹㇺㇻㇼㇽㇾㇿ々〻";
		const string NotAllowedAtTheEnd =
			"$(*,£¥·‘“〈《「『【〔〖〝﹗﹙﹛＄（．［｛￡￥([｛〔〘｟«";

		internal static void AdjustLineBreakPosition(string text, ref int position)
		{
			if (position > 1 && NotAllowedAtTheStart.IndexOf(text[position]) >= 0) {
				position -= 1;
			} else if (position > 2 && NotAllowedAtTheEnd.IndexOf(text[position - 1]) >= 0) {
				position -= 2;
			}
		}

		internal static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key)
		{
			return GetValueOrDefault(d, key, default(TValue));
		}

		internal static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key, TValue defaultValue)
		{
			TValue value;
			return d.TryGetValue(key, out value) ? value : defaultValue;
		}

		internal static int SizeOf<T>() => SizeOfCache<T>.Value;

		private static class SizeOfCache<T>
		{
			public static readonly int Value = Marshal.SizeOf(typeof(T));
		}
	}

	public class ArrayEqualityComparer<T> : IEqualityComparer<T[]>
	{
		public static readonly ArrayEqualityComparer<T> Default = new ArrayEqualityComparer<T>(EqualityComparer<T>.Default);

		private IEqualityComparer<T> elementComparer;

		public ArrayEqualityComparer(IEqualityComparer<T> elementComparer)
		{
			this.elementComparer = elementComparer;
		}

		public bool Equals(T[] x, T[] y)
		{
			if (x == y) {
				return true;
			}
			if (x == null || y == null || x.Length != y.Length) {
				return false;
			}
			for (var i = 0; i < x.Length; i++) {
				if (!elementComparer.Equals(x[i], y[i])) {
					return false;
				}
			}
			return true;
		}

		public int GetHashCode(T[] x)
		{
			unchecked {
				var hash = x.Length;
				for (var i = 0; i < x.Length; i++) {
					hash = (hash * 397) ^ elementComparer.GetHashCode(x[i]);
				}
				return hash;
			}
		}
	}
}
