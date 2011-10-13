using System;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public struct TextureParams
	{
		[ProtoMember(1)]
		public string AtlasTexture;
		
		[ProtoMember(2)]
		public IntRectangle AtlasRect;
		
		public static TextureParams ReadFromBundle (string path) {
			return Serialization.ReadObjectFromBundle<TextureParams> (path);
		}

		public static TextureParams ReadFromFile (string path) {
			return Serialization.ReadObjectFromFile<TextureParams> (path);
		}
		
		public static void WriteToFile (TextureParams texParams, string path) {
			Serialization.WriteObjectToFile<TextureParams> (path, texParams);
		}	
	}
}

