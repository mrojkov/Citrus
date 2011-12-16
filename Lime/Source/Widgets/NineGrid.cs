using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class NineGrid : Widget
	{
		[ProtoMember (1)]
		public SerializableTexture Texture { get; set; }

		[ProtoMember (2)]
		public float LeftOffset { get; set; }
		
		[ProtoMember (3)]
		public float RightOffset { get; set; }
		
		[ProtoMember (4)]
		public float TopOffset { get; set; }
		
		[ProtoMember (5)]
		public float BottomOffset { get; set; }
	}
}
