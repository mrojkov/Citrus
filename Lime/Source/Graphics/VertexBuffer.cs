using Lime.Graphics.Platform;

namespace Lime
{
	public class VertexBuffer : Buffer
	{
		public VertexBuffer(bool dynamic)
			: base(BufferType.Vertex, dynamic)
		{
		}
	}
}
