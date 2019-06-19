using System.IO;
using Lime;

namespace RemoteScripting
{
	public abstract class NetworkYuzuObject<T> : NetworkMessage where T : class, new()
	{
		public readonly T Instance;

		protected NetworkYuzuObject(T instance)
		{
			Instance = instance;
		}

		protected NetworkYuzuObject(byte[] body)
		{
			using (var memoryStream = new MemoryStream(body)) {
				Instance = Serialization.ReadObject<T>(string.Empty, memoryStream);
			}
		}

		public override byte[] Serialize()
		{
			using (var memoryStream = new MemoryStream()) {
				Serialization.WriteObject(string.Empty, memoryStream, Instance, Serialization.Format.Binary);
				return memoryStream.ToArray();
			}
		}
	}
}
