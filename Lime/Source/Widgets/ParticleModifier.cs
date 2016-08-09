using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using Yuzu;

namespace Lime
{
	[ProtoContract]
	public class ParticleModifier : Node
	{
		[ProtoMember(1)]
		[YuzuMember]
		public float Scale { get; set; }

		[ProtoMember(2)]
		[YuzuMember]
		public float AspectRatio { get; set; }

		[ProtoMember(3)]
		[YuzuMember]
		public float Velocity { get; set; }

		[ProtoMember(4)]
		[YuzuMember]
		public float Spin { get; set; }

		[ProtoMember(5)]
		[YuzuMember]
		public float AngularVelocity { get; set; }

		[ProtoMember(6)]
		[YuzuMember]
		public float GravityAmount { get; set; }

		[ProtoMember(7)]
		[YuzuMember]
		public float WindAmount { get; set; }

		[ProtoMember(8)]
		[YuzuMember]
		public float MagnetAmount { get; set; }

		[ProtoMember(9)]
		[YuzuMember]
		public Color4 Color { get; set; }

		[ProtoMember(10)]
		[YuzuMember]
		public int FirstFrame { get; set; }

		[ProtoMember(11)]
		[YuzuMember]
		public int LastFrame { get; set; }

		[ProtoMember(12)]
		[YuzuMember]
		public float AnimationFps { get; set; }

		[ProtoMember(13)]
		[YuzuMember]
		public bool LoopedAnimation { get; set; }

		ITexture texture = new SerializableTexture();
		[ProtoMember(14)]
		[YuzuMember]
		public ITexture Texture { get { return texture; } set { texture = value; textures = null; } }

		public ParticleModifier()
		{
			Scale = 1;
			AspectRatio = 1;
			Velocity = 1;
			Spin = 1;
			AngularVelocity = 1;
			WindAmount = 1;
			GravityAmount = 1;
			MagnetAmount = 1;
			Color = Color4.White;
			FirstFrame = 1;
			LastFrame = 1;
			AnimationFps = 20;
			LoopedAnimation = true;
		}

		bool ChangeTextureFrameIndex(ref string path, int frame)
		{
			if (frame < 0 || frame > 99)
				return false;
			int i = path.Length;
			//for (; i >= 0; i--)
			//	if (path[i] == '.')
			//		break;
			if (i < 2)
				return false;
			if (char.IsDigit(path, i - 1) && char.IsDigit(path, i - 2)) {
				var s = new StringBuilder(path);
				s[i - 1] = (char)(frame % 10 + '0');
				s[i - 2] = (char)(frame / 10 + '0');
				path = s.ToString();
				return true;
			}
			return false;
		}

		List<SerializableTexture> textures;

		internal ITexture GetTexture(int index)
		{
			if (FirstFrame == LastFrame) {
				return texture;
			}
			if (textures == null) {
				textures = new List<SerializableTexture>();
				var path = texture.SerializationPath;
				for (int i = 0; i < 100; i++) {
					if (!ChangeTextureFrameIndex(ref path, i))
						break;
					if (PackedAssetsBundle.Instance.FileExists(path + ".atlasPart") ||
						PackedAssetsBundle.Instance.FileExists(path))
					{
						var t = new SerializableTexture(path);
						textures.Add(t);
					} else if (i > 0)
						break;
				}
			}
			if (textures.Count == 0)
				return texture;
			index = Mathf.Clamp(index, 0, textures.Count - 1);
			return textures[index];
		}
	}
}
