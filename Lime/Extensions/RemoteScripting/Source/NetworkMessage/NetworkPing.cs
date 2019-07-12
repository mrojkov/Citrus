using System.Text;

namespace RemoteScripting
{
	public class NetworkPing : NetworkMessage
	{
		private readonly byte[] emptyByteArray = new byte[0];

		public override NetworkMessageType MessageType => NetworkMessageType.Ping;

		public override byte[] Serialize() => emptyByteArray;
	}
}
