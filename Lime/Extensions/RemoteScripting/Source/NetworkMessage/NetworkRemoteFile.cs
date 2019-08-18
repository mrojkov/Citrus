using Yuzu;

namespace RemoteScripting
{
	public class RemoteFile
	{
		[YuzuMember]
		public string Path;

		[YuzuMember]
		public byte[] Bytes;
	}

	public class NetworkRemoteFile : NetworkYuzuObject<RemoteFile>
	{
		public override NetworkMessageType MessageType => NetworkMessageType.RemoteFile;

		public NetworkRemoteFile(RemoteFile instance) : base(instance) { }
		public NetworkRemoteFile(byte[] body) : base(body) { }
	}
}
