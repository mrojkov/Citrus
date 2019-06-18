using System.Text;

namespace RemoteScripting
{
	public class NetworkText : NetworkMessage
	{
		public readonly string Text;

		public override NetworkMessageType MessageType => NetworkMessageType.Text;

		public NetworkText(string text)
		{
			Text = text;
		}

		public NetworkText(byte[] body)
		{
			Text = Encoding.UTF8.GetString(body);
		}

		public override byte[] Serialize() => Encoding.UTF8.GetBytes(Text);
	}
}
