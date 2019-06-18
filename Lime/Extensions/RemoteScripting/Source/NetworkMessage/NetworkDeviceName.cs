using System.Text;

namespace RemoteScripting
{
	public class NetworkDeviceName : NetworkMessage
	{
		public readonly string Name;

		public override NetworkMessageType MessageType => NetworkMessageType.DeviceName;

		public NetworkDeviceName(string name)
		{
			Name = name;
		}

		public NetworkDeviceName(byte[] body)
		{
			Name = Encoding.UTF8.GetString(body);
		}

		public override byte[] Serialize() => Encoding.UTF8.GetBytes(Name);
	}
}
