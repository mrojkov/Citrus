using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RemoteScripting
{
	public enum NetworkMessageType
	{
		Ping,
		DeviceName,
		Text,
		RemoteProcedureCall,
	}

	public abstract class NetworkMessage
	{
		private const int BodyLengthBufferLength = 4;
		private const int MessageTypeBufferLength = 2;
		private const int HeaderLength = BodyLengthBufferLength + MessageTypeBufferLength;

		public abstract NetworkMessageType MessageType { get; }
		public abstract byte[] Serialize();

		public static NetworkMessage Create(NetworkMessageType type, byte[] body)
		{
			switch (type) {
				case NetworkMessageType.Ping:
					return new NetworkPing();
				case NetworkMessageType.DeviceName:
					return new NetworkDeviceName(body);
				case NetworkMessageType.Text:
					return new NetworkText(body);
				case NetworkMessageType.RemoteProcedureCall:
					return new NetworkRemoteProcedureCall(body);
				default:
					throw new NotSupportedException();
			}
		}

		public static async Task<NetworkMessage> ReadMessageFromStreamAsync(NetworkStream stream)
		{
			var headerBuffer = await ReadFromStreamAsync(stream, HeaderLength);
			var length = BitConverter.ToInt32(headerBuffer, 0);
			var messageType = (NetworkMessageType)BitConverter.ToInt16(headerBuffer, BodyLengthBufferLength);
			var messageBuffer =
				length > 0 ?
				await ReadFromStreamAsync(stream, length) :
				new byte[0];

			return Create(messageType, messageBuffer);
		}

		public static async Task WriteMessageToStreamAsync(NetworkStream stream, NetworkMessage message)
		{
			var bodyBuffer = message.Serialize();
			var lengthBuffer = BitConverter.GetBytes(bodyBuffer.Length);
			var messageTypeBuffer = BitConverter.GetBytes((short)message.MessageType);

			await stream.WriteAsync(lengthBuffer, 0, lengthBuffer.Length);
			await stream.WriteAsync(messageTypeBuffer, 0, messageTypeBuffer.Length);
			await stream.WriteAsync(bodyBuffer, 0, bodyBuffer.Length);
		}

		private static async Task<byte[]> ReadFromStreamAsync(NetworkStream stream, int nbytes)
		{
			var buf = new byte[nbytes];
			var readpos = 0;
			while (readpos < nbytes) {
				readpos += await stream.ReadAsync(buf, readpos, nbytes - readpos);
			}
			return buf;
		}

		protected static byte[] Zip(string str)
		{
			var bytes = Encoding.UTF8.GetBytes(str);
			using (var msi = new MemoryStream(bytes)) {
				using (var mso = new MemoryStream()) {
					using (var gs = new GZipStream(mso, CompressionLevel.Fastest)) {
						msi.CopyTo(gs);
					}
					return mso.ToArray();
				}
			}
		}

		protected static string Unzip(byte[] bytes)
		{
			using (var msi = new MemoryStream(bytes)) {
				using (var mso = new MemoryStream()) {
					using (var gs = new GZipStream(msi, CompressionMode.Decompress)) {
						gs.CopyTo(mso);
					}
					return Encoding.UTF8.GetString(mso.ToArray());
				}
			}
		}
	}
}
