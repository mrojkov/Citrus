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
		RemoteFileRequest,
		RemoteFile,
	}

	public abstract class NetworkMessage
	{
		private const short MagicConst = 5667;
		private const ushort ProtocolVersion = 1;

		private const int MagicConstBufferLength = 2;
		private const int ProtocolVersionBufferLength = 2;
		private const int MessageTypeBufferLength = 2;
		private const int BodyLengthBufferLength = 4;
		private const int HeaderLength = MagicConstBufferLength + ProtocolVersionBufferLength + MessageTypeBufferLength + BodyLengthBufferLength;

		private static readonly byte[] magicConstBuffer = BitConverter.GetBytes(MagicConst);
		private static readonly byte[] protocolVersionBuffer = BitConverter.GetBytes(ProtocolVersion);
		private static readonly byte[] emptyBuffer = new byte[0];
		private static readonly int messageTypesCount = Enum.GetNames(typeof(NetworkMessageType)).Length;

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
				case NetworkMessageType.RemoteFileRequest:
					return new NetworkRemoteFileRequest(body);
				case NetworkMessageType.RemoteFile:
					return new NetworkRemoteFile(body);
				default:
					throw new NotSupportedException();
			}
		}

		public static async Task<NetworkMessage> ReadMessageFromStreamAsync(NetworkStream stream)
		{
			var headerBuffer = await ReadFromStreamAsync(stream, HeaderLength).ConfigureAwait(continueOnCapturedContext: false);

			var cursor = 0;
			var magicConst = BitConverter.ToInt16(headerBuffer, cursor);
			cursor += sizeof(short);
			if (magicConst != MagicConst) {
				throw new NetworkException("Broken header. Unknown protocol!");
			}

			var protocolVersion = BitConverter.ToUInt16(headerBuffer, cursor);
			cursor += sizeof(ushort);
			if (protocolVersion != ProtocolVersion) {
				throw new NetworkException("Broken header. Unsupported protocol version!");
			}

			var length = BitConverter.ToInt32(headerBuffer, cursor);
			cursor += sizeof(int);
			if (length < 0) {
				throw new NetworkException("Broken header. Body length is out of range!");
			}

			var messageTypeAsShort = BitConverter.ToInt16(headerBuffer, cursor);
			cursor += sizeof(short);
			if (messageTypeAsShort < 0 || messageTypeAsShort >= messageTypesCount) {
				throw new NetworkException("Broken header. Unknown message type!");
			}
			var messageType = (NetworkMessageType)messageTypeAsShort;

			var messageBuffer =
				length > 0 ?
				await ReadFromStreamAsync(stream, length).ConfigureAwait(continueOnCapturedContext: false) :
				emptyBuffer;
			return Create(messageType, messageBuffer);
		}

		public static async Task WriteMessageToStreamAsync(NetworkStream stream, NetworkMessage message)
		{
			var bodyBuffer = message.Serialize();
			var lengthBuffer = BitConverter.GetBytes(bodyBuffer.Length);
			var messageTypeBuffer = BitConverter.GetBytes((short)message.MessageType);

			await stream.WriteAsync(magicConstBuffer, 0, MagicConstBufferLength).ConfigureAwait(continueOnCapturedContext: false);
			await stream.WriteAsync(protocolVersionBuffer, 0, ProtocolVersionBufferLength).ConfigureAwait(continueOnCapturedContext: false);
			await stream.WriteAsync(lengthBuffer, 0, lengthBuffer.Length).ConfigureAwait(continueOnCapturedContext: false);
			await stream.WriteAsync(messageTypeBuffer, 0, messageTypeBuffer.Length).ConfigureAwait(continueOnCapturedContext: false);
			await stream.WriteAsync(bodyBuffer, 0, bodyBuffer.Length).ConfigureAwait(continueOnCapturedContext: false);
		}

		private static async Task<byte[]> ReadFromStreamAsync(NetworkStream stream, int nbytes)
		{
			var buf = new byte[nbytes];
			var readpos = 0;
			while (readpos < nbytes) {
				readpos += await stream.ReadAsync(buf, readpos, nbytes - readpos).ConfigureAwait(continueOnCapturedContext: false);
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
