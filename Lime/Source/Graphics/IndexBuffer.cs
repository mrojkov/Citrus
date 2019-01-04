using Lime.Graphics.Platform;

namespace Lime
{
	public class IndexBuffer : Buffer
	{
		public IndexBuffer(bool dynamic)
			: base(BufferType.Index, dynamic)
		{
		}
	}
}
