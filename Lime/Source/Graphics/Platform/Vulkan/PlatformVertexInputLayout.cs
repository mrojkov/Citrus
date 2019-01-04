namespace Lime.Graphics.Platform.Vulkan
{
	internal class PlatformVertexInputLayout : IPlatformVertexInputLayout
	{
		private static long referenceHashCounter = 0;

		internal readonly long ReferenceHash = System.Threading.Interlocked.Increment(ref referenceHashCounter);
		internal readonly VertexInputLayoutBinding[] Bindings;
		internal readonly VertexInputLayoutAttribute[] Attributes;

		public PlatformVertexInputLayout(
			PlatformRenderContext context, VertexInputLayoutBinding[] bindings, VertexInputLayoutAttribute[] attributes)
		{
			Bindings = (VertexInputLayoutBinding[])bindings.Clone();
			Attributes = (VertexInputLayoutAttribute[])attributes.Clone();
		}

		public void Dispose() { }
	}
}
