#if OPENGL
#if iOS || ANDROID || WIN
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace Lime
{
	public class IndexBuffer : Buffer
	{
		public IndexBuffer(bool dynamic)
			: base(BufferTarget.ElementArrayBuffer, dynamic)
		{
		}
	}
}

#endif