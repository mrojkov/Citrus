using System;
using System.Collections.Generic;

namespace Lime
{
	public class VertexInputLayout
	{
		private static Dictionary<VertexInputElement[], VertexInputLayout> layoutCache =
			new Dictionary<VertexInputElement[], VertexInputLayout>(
				ArrayEqualityComparer<VertexInputElement>.Default
			);

		internal VertexInputElement[] Elements;
		internal int AttribMask;

		private VertexInputLayout()
		{ }

		public static VertexInputLayout New(VertexInputElement[] elements)
		{
			VertexInputLayout layout;
			if (layoutCache.TryGetValue(elements, out layout)) {
				return layout;
			}
			var sortedElements = (VertexInputElement[])elements.Clone();
			Array.Sort(sortedElements, CompareVertexInputElement);
			if (layoutCache.TryGetValue(sortedElements, out layout)) {
				return layout;
			}
			var attributeMask = 0;
			foreach (var e in sortedElements) {
				attributeMask |= 1 << e.Attribute;
			}
			layout = new VertexInputLayout {
				Elements = sortedElements,
				AttribMask = attributeMask
			};
			layoutCache.Add(sortedElements, layout);
			return layout;
		}

		private static int CompareVertexInputElement(VertexInputElement x, VertexInputElement y)
		{
			if (x.Slot < y.Slot) {
				return -1;
			}
			if (x.Slot > y.Slot) {
				return 1;
			}
			if (x.Attribute < y.Attribute) {
				return -1;
			}
			if (x.Attribute > y.Attribute) {
				return 1;
			}
			if (x.Offset < y.Offset) {
				return -1;
			}
			if (x.Offset > y.Offset) {
				return 1;
			}
			if (x.Format < y.Format) {
				return -1;
			}
			if (x.Format > y.Format) {
				return 1;
			}
			return 0;
		}
	}

	public struct VertexInputElement : IEquatable<VertexInputElement>
	{
		public int Slot;
		public int Attribute;
		public int Offset;
		public int Stride;
		public VertexInputElementFormat Format;

		public override bool Equals(object other)
		{
			return other is VertexInputElement && Equals((VertexInputElement)other);
		}

		public bool Equals(VertexInputElement other)
		{
			return Slot == other.Slot &&
				Attribute == other.Attribute &&
				Offset == other.Offset &&
				Stride == other.Stride &&
				Format == other.Format;
		}

		public override int GetHashCode()
		{
			unchecked {
				var hash = Slot;
				hash = (hash * 397) ^ Attribute;
				hash = (hash * 397) ^ Offset;
				hash = (hash * 397) ^ Stride;
				hash = (hash * 397) ^ Format.GetHashCode();
				return hash;
			}
		}
	}

	public enum VertexInputElementFormat
	{
		Byte1,
		Byte1Norm,
		Byte2,
		Byte2Norm,
		Byte4,
		Byte4Norm,

		Short1,
		Short1Norm,
		Short2,
		Short2Norm,
		Short4,
		Short4Norm,

		UByte1,
		UByte1Norm,
		UByte2,
		UByte2Norm,
		UByte4,
		UByte4Norm,

		UShort1,
		UShort1Norm,
		UShort2,
		UShort2Norm,
		UShort4,
		UShort4Norm,

		Float1,
		Float2,
		Float3,
		Float4
	}
}