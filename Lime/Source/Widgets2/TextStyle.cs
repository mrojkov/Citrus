using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Lime.Widgets2
{
	[ProtoContract]
	public class TextStyle : Node
	{
		public static TextStyle Default = new TextStyle();

		[ProtoContract]
		public enum ImageUsageEnum
		{
			[ProtoEnum]
			Bullet,
			[ProtoEnum]
			Overlay
		}

		[ProtoMember(1)]
		public SerializableTexture ImageTexture { get; set; }
		
		[ProtoMember(2)]
		public Vector2 ImageSize { get; set; }
		
		[ProtoMember(3)]
		public ImageUsageEnum ImageUsage { get; set; }
		
		[ProtoMember(4)]
		public SerializableFont Font { get; set; }
		
		[ProtoMember(5)]
		public float Size { get; set; }
		
		[ProtoMember(6)]
		public float SpaceAfter { get; set; }
		
		[ProtoMember(7)]
		public bool Bold { get; set; }
		
		[ProtoMember(8)]
		public bool CastShadow { get; set; }
		
		[ProtoMember(9)]
		public Vector2 ShadowOffset { get; set; }

		[ProtoMember(10)]
		public Color4 TextColor { get; set; }

		[ProtoMember(11)]
		public Color4 ShadowColor { get; set; }

		public TextStyle()
		{
			Size = 15;
			TextColor = Color4.White;
			ShadowColor = Color4.Black;
			ShadowOffset = Vector2.One;
			Font = new SerializableFont();
			ImageTexture = new SerializableTexture();
		}
	}

}
