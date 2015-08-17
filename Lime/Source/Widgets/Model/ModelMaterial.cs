using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class ModelMaterial
	{
		[ProtoMember(1)]
		public ITexture DiffuseTexture { get; set; }

		[ProtoMember(2)]
		public ITexture OpacityTexture { get; set; }

		[ProtoMember(3)]
		public ITexture SpecularTexture { get; set; }

		[ProtoMember(4)]
		public ITexture BumpTexture { get; set; }

		[ProtoMember(5)]
		public Color4 DiffuseColor { get; set; }

		[ProtoMember(6)]
		public Color4 EmissiveColor { get; set; }

		[ProtoMember(7)]
		public Color4 SpecularColor { get; set; }

		[ProtoMember(8)]
		public float SpecularPower { get; set; }

		[ProtoMember(9)]
		public float Opacity { get; set; }

		public ModelMaterial()
		{
			DiffuseColor = Color4.White;
			EmissiveColor = Color4.White;
			SpecularColor = Color4.White;

		}
	}
}
