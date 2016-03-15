using System;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class ModelMaterial
	{
		private ITexture diffuseTexture;
		private ITexture specularTexture;
		private ITexture heightTexture;
		private ITexture opacityTexture;
		private ModelMaterialCap caps;

		[ProtoMember(1)]
		public ITexture DiffuseTexture
		{
			get { return diffuseTexture; }
			set
			{
				diffuseTexture = value;
				SetCapState(ModelMaterialCap.DiffuseTexture, diffuseTexture != null);
			}
		}

		[ProtoMember(2)]
		public ITexture SpecularTexture
		{
			get { return specularTexture; }
			set
			{
				specularTexture = value;
				SetCapState(ModelMaterialCap.SpecularTexture, specularTexture != null);
			}
		}

		[ProtoMember(3)]
		public ITexture HeightTexture
		{
			get { return heightTexture; }
			set
			{
				heightTexture = value;
				SetCapState(ModelMaterialCap.HeightTexture, heightTexture != null);
			}
		}

		[ProtoMember(4)]
		public ITexture OpacityTexture
		{
			get { return opacityTexture; }
			set
			{
				opacityTexture = value;
				SetCapState(ModelMaterialCap.OpacityTexture, opacityTexture != null);
			}
		}

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
			Opacity = 1f;
		}

		private void SetCapState(ModelMaterialCap mask, bool enabled)
		{
			if (enabled) {
				caps |= mask;
			} else {
				caps &= ~mask;
			}
		}

		internal void Apply(ref ModelMaterialExternals externals)
		{
			var technique = ModelMaterialTechnique.Get(caps | externals.Caps);
			technique.Apply(this, ref externals);
		}
	}

	internal struct ModelMaterialExternals
	{
		public Matrix44 WorldViewProj;
		public Matrix44[] Bones;
		public int BoneCount;
		public ModelMaterialCap Caps;
	}

	[Flags]
	internal enum ModelMaterialCap
	{
		None = 0,

		[AtomicFlag]
		DiffuseTexture = 1 << 0,

		[AtomicFlag]
		SpecularTexture = 1 << 1,

		[AtomicFlag]
		HeightTexture = 1 << 2,

		[AtomicFlag]
		OpacityTexture = 1 << 3,

		[AtomicFlag]
		Skin = 1 << 4
	}
}
