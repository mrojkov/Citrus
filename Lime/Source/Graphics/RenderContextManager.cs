using Lime.Graphics.Platform;

namespace Lime
{
	internal static class RenderContextManager
	{
		public static IPlatformRenderContext CurrentContext { get; private set; }

		public static void MakeCurrent(IPlatformRenderContext ctx)
		{
			CurrentContext = ctx;
		}
	}
}
