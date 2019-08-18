using Yuzu;

namespace RemoteScripting
{
	public class RemoteFileRequest
	{
		[YuzuMember]
		public string Path;
	}

	public class NetworkRemoteFileRequest : NetworkYuzuObject<RemoteFileRequest>
	{
		public override NetworkMessageType MessageType => NetworkMessageType.RemoteFileRequest;

		public NetworkRemoteFileRequest(RemoteFileRequest instance) : base(instance) { }
		public NetworkRemoteFileRequest(byte[] body) : base(body) { }
	}
}
