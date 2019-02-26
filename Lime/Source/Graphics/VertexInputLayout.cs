using System;
using System.Collections.Generic;
using Lime.Graphics.Platform;

namespace Lime
{
	public class VertexInputLayout : IDisposable
	{
		private static Dictionary<long, VertexInputLayout> layoutCache = new Dictionary<long, VertexInputLayout>();

		private IPlatformVertexInputLayout platformLayout;
		private VertexInputLayoutBinding[] bindings;
		private VertexInputLayoutAttribute[] attributes;

		private VertexInputLayout(
			VertexInputLayoutBinding[] bindings, int bindingCount,
			VertexInputLayoutAttribute[] attributes, int attributeCount)
		{
			this.bindings = new VertexInputLayoutBinding[bindingCount];
			this.attributes = new VertexInputLayoutAttribute[attributeCount];
			Array.Copy(bindings, this.bindings, bindingCount);
			Array.Copy(attributes, this.attributes, attributeCount);
		}

		~VertexInputLayout()
		{
			DisposeInternal();
		}

		public void Dispose()
		{
			DisposeInternal();
			GC.SuppressFinalize(this);
		}

		private void DisposeInternal()
		{
			if (platformLayout != null) {
				var platformLayoutCopy = platformLayout;
				Window.Current.InvokeOnRendering(() => {
					platformLayoutCopy.Dispose();
				});
				platformLayout = null;
			}
		}

		internal IPlatformVertexInputLayout GetPlatformLayout()
		{
			if (platformLayout == null) {
				platformLayout = PlatformRenderer.Context.CreateVertexInputLayout(bindings, attributes);
			}
			return platformLayout;
		}

		private static VertexInputLayoutBinding[] sortedBindings;
		private static VertexInputLayoutAttribute[] sortedAttributes;

		private static readonly BindingSortComparer bindingSortComparer = new BindingSortComparer();
		private static readonly AttributeSortComparer attributeSortComparer = new AttributeSortComparer();

		public static VertexInputLayout New(VertexInputLayoutBinding[] bindings, VertexInputLayoutAttribute[] attributes)
		{
			lock (layoutCache) {
				if (sortedBindings == null || sortedBindings.Length < bindings.Length) {
					sortedBindings = new VertexInputLayoutBinding[bindings.Length];
				}
				if (sortedAttributes == null || sortedAttributes.Length < attributes.Length) {
					sortedAttributes = new VertexInputLayoutAttribute[attributes.Length];
				}
				Array.Copy(bindings, sortedBindings, bindings.Length);
				Array.Copy(attributes, sortedAttributes, attributes.Length);
				Array.Sort(sortedBindings, 0, bindings.Length, bindingSortComparer);
				Array.Sort(sortedAttributes, 0, attributes.Length, attributeSortComparer);
				var hash = ComputeHash(sortedBindings, bindings.Length, sortedAttributes, attributes.Length);
				if (!layoutCache.TryGetValue(hash, out var layout)) {
					layout = new VertexInputLayout(sortedBindings, bindings.Length, sortedAttributes, attributes.Length);
					layoutCache.Add(hash, layout);
				}
				return layout;
			}
		}

		private static long ComputeHash(
			VertexInputLayoutBinding[] bindings, int bindingCount,
			VertexInputLayoutAttribute[] attributes, int attributeCount)
		{
			var hasher = new Hasher();
			hasher.Write(bindings, 0, bindingCount);
			hasher.Write(attributes, 0, attributeCount);
			return hasher.End();
		}

		private class BindingSortComparer : IComparer<VertexInputLayoutBinding>
		{
			public int Compare(VertexInputLayoutBinding x, VertexInputLayoutBinding y)
			{
				return x.Slot - y.Slot;
			}
		}

		private class AttributeSortComparer : IComparer<VertexInputLayoutAttribute>
		{
			public int Compare(VertexInputLayoutAttribute x, VertexInputLayoutAttribute y)
			{
				return x.Location - y.Location;
			}
		}
	}

	public struct VertexInputLayoutBinding
	{
		public int Slot;
		public int Stride;
	}

	public struct VertexInputLayoutAttribute
	{
		public int Slot;
		public int Location;
		public int Offset;
		public Format Format;
	}
}
