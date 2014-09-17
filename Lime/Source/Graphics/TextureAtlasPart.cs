using System;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public struct TextureAtlasPart
	{
		[ProtoMember(1)]
		public string AtlasTexture;
		
		[ProtoMember(2)]
		public IntRectangle AtlasRect;

		public static TextureAtlasPart ReadFromBundle(string path) {
			return Serialization.ReadObject<TextureAtlasPart>(path);
		}

		public static TextureAtlasPart ReadFromFile(string path) {
			return Serialization.ReadObjectFromFile<TextureAtlasPart>(path);
		}
		
		public static void WriteToFile(TextureAtlasPart texParams, string path) {
			Serialization.WriteObjectToFile<TextureAtlasPart>(path, texParams);
		}
	}
}
