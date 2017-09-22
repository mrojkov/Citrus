using System.Collections.Generic;
using System.Text;
using Yuzu;

namespace Lime
{
	[AllowedParentTypes(typeof(ParticleEmitter))]
	public class ParticleModifier : Node
	{
		[YuzuMember]
		public Vector2 Size { get; set; }

		[YuzuMember]
		public Vector2 Scale { get; set; }

		[YuzuMember]
		public float Velocity { get; set; }

		[YuzuMember]
		public float Spin { get; set; }

		[YuzuMember]
		public float AngularVelocity { get; set; }

		[YuzuMember]
		public float GravityAmount { get; set; }

		[YuzuMember]
		public float WindAmount { get; set; }

		[YuzuMember]
		public float MagnetAmount { get; set; }

		[YuzuMember]
		public Color4 Color { get; set; }

		[YuzuMember]
		public int FirstFrame { get; set; }

		[YuzuMember]
		public int LastFrame { get; set; }

		[YuzuMember]
		public float AnimationFps { get; set; }

		[YuzuMember]
		public bool LoopedAnimation { get; set; }

		ITexture texture = new SerializableTexture();
		[YuzuMember]
		public ITexture Texture { get { return texture; } set { texture = value; textures = null; } }

		public ParticleModifier()
		{
			RenderChainBuilder = null;
			Size = Widget.DefaultWidgetSize;
			Scale = Vector2.One;
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

		public ITexture GetTexture(int index)
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
					if (AssetBundle.Current.FileExists(path + ".atlasPart") ||
						AssetBundle.Current.FileExists(path + ".png")
					) {
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
