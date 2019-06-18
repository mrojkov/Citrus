using System.Net;
using System.Net.Sockets;

namespace RemoteScripting
{
	public class HostClient : NetworkMember
	{
		public IPAddress RemoteIPAddress => ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address;

		public HostClient(TcpClient tcpClient)
		{
			TcpClient = tcpClient;
		}
	}
}
