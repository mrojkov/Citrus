#if WIN
namespace Lime.Oculus
{
	public class OvrExcepton : Lime.Exception
	{
		public OvrExcepton(string message = null) : base(message)
		{
		}
	}
}
#endif
