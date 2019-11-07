using Yuzu;

namespace RemoteScripting
{
	public class RemoteProcedureCall
	{
		[YuzuMember]
		public byte[] AssemblyRawBytes;

		[YuzuMember]
		public byte[] PdbRawBytes;

		[YuzuMember]
		public string ClassName;

		[YuzuMember]
		public string MethodName;
	}

	public class NetworkRemoteProcedureCall : NetworkYuzuObject<RemoteProcedureCall>
	{
		public override NetworkMessageType MessageType => NetworkMessageType.RemoteProcedureCall;

		public NetworkRemoteProcedureCall(RemoteProcedureCall instance) : base(instance) { }
		public NetworkRemoteProcedureCall(byte[] body) : base(body) { }
	}
}
