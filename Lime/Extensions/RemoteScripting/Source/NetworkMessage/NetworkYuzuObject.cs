using System.IO;
using Lime;

namespace RemoteScripting
{
	public abstract class NetworkYuzuObject<T> : NetworkMessage where T : class, new()
	{
		public readonly T Data;

		protected NetworkYuzuObject(T data)
		{
			Data = data;
		}

		protected NetworkYuzuObject(byte[] body)
		{
			using (var memoryStream = new MemoryStream(body)) {
				Data = Serialization.ReadObject<T>(string.Empty, memoryStream);
			}
		}

		public override byte[] Serialize()
		{
			using (var memoryStream = new MemoryStream()) {
				Serialization.WriteObject(string.Empty, memoryStream, Data, Serialization.Format.Binary);
				return memoryStream.ToArray();
			}
		}
	}
}
