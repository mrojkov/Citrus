#if OPENGL
#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace Lime
{
	public class VertexBuffer : Buffer
	{
		public VertexBuffer(bool dynamic)
			: base(BufferTarget.ArrayBuffer, dynamic)
		{
		}
	}
}

#endif