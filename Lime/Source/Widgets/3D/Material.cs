using System;
using ProtoBuf;
using Yuzu;

namespace Lime
{
	[ProtoContract]
	public class Material
	{
		private ITexture diffuseTexture;
		private ITexture specularTexture;
		private ITexture heightTexture;
		private ITexture opacityTexture;
		private MaterialCap caps;
		private FogMode fogMode;

		[ProtoMember(1)]
		[YuzuMember]
		public ITexture DiffuseTexture
		{
			get { return diffuseTexture; }
			set
			{
				diffuseTexture = value;
				SetCapState(MaterialCap.DiffuseTexture, diffuseTexture != null);
			}
		}

		[ProtoMember(4)]
		[YuzuMember]
		public ITexture OpacityTexture
		{
			get { return opacityTexture; }
			set
			{
				opacityTexture = value;
				SetCapState(MaterialCap.OpacityTexture, opacityTexture != null);
			}
		}

		[ProtoMember(5)]
		[YuzuMember]
		public Color4 DiffuseColor { get; set; }

		[ProtoMember(6)]
		[YuzuMember]
		public Color4 EmissiveColor { get; set; }

		[ProtoMember(7)]
		[YuzuMember]
		public Color4 SpecularColor { get; set; }

		[ProtoMember(8)]
		[YuzuMember]
		public float SpecularPower { get; set; }

		[ProtoMember(9)]
		[YuzuMember]
		public float Opacity { get; set; }

		[ProtoMember(10)]
		[YuzuMember]
		public string Name { get; set; }

		[ProtoMember(11)]
		[YuzuMember]
		public FogMode FogMode
		{
			get { return fogMode; }
			set
			{
				fogMode = value;
				caps &= ~MaterialCap.Fog;
				if (fogMode == FogMode.Linear) {
					caps |= MaterialCap.FogLinear;
				} else if (fogMode == FogMode.Exp) {
					caps |= MaterialCap.FogExp;
				} else if (fogMode == FogMode.Exp2) {
					caps |= MaterialCap.FogExp2;
				}
			}
		}

		[ProtoMember(12)]
		[YuzuMember]
		public Color4 FogColor;

		[ProtoMember(13)]
		[YuzuMember]
		public float FogStart;

		[ProtoMember(14)]
		[YuzuMember]
		public float FogEnd;

		[ProtoMember(15)]
		[YuzuMember]
		public float FogDensity;

		public Material()
		{
			DiffuseColor = Color4.White;
			EmissiveColor = Color4.White;
			SpecularColor = Color4.White;
			Opacity = 1f;
			FogMode = FogMode.None;
		}

		private void SetCapState(MaterialCap mask, bool enabled)
		{
			if (enabled) {
				caps |= mask;
			} else {
				caps &= ~mask;
			}
		}

		internal void Apply(ref MaterialExternals externals)
		{
			var technique = MaterialTechnique.Get(caps | externals.Caps);
			technique.Apply(this, ref externals);
		}

		public Material Clone()
		{
			return MemberwiseClone() as Material;
		}
	}

	internal struct MaterialExternals
	{
		public Color4 ColorFactor;
		public Matrix44 WorldView;
		public Matrix44 WorldViewProj;
		public Matrix44[] Bones;
		public int BoneCount;
		public MaterialCap Caps;
	}

	[Flags]
	internal enum MaterialCap
	{
		None = 0,

		[AtomicFlag]
		DiffuseTexture = 1 << 0,

		[AtomicFlag]
		OpacityTexture = 1 << 1,

		[AtomicFlag]
		Skin = 1 << 2,

		[AtomicFlag]
		FogLinear = 1 << 3,

		[AtomicFlag]
		FogExp = 1 << 4,

		[AtomicFlag]
		FogExp2 = 1 << 5,

		Fog = FogLinear | FogExp | FogExp2
	}

	public enum FogMode
	{
		None,
		Linear,
		Exp,
		Exp2
	}
}
