using Lime;

namespace RemoteScripting
{
	public class NetworkException : Exception
	{
		public NetworkException(string message) : base(message) { }
	}
}
