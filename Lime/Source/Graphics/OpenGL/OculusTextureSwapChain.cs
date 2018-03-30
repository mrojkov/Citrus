#if WIN
using OculusWrap;
using System;
using System.Linq;
using static OculusWrap.OVRTypes;

namespace Lime.Oculus
{
	public class OculusTextureSwapChain
	{
		private OculusTexture[] textures;
		private OculusTexture curTexture;
		private readonly OvrProvider provider;
		public readonly TextureSwapChain TextureChain;
		public ITexture CurrentTexture => curTexture;
		public readonly IntVector2 Size;

		public void SetAsRenderTarget()
		{
			if (TextureChain.TextureSwapChainPtr != IntPtr.Zero) {
				uint curTexId = 0;
				int curIndex;
				TextureChain.GetCurrentIndex(out curIndex);
				TextureChain.GetBufferGL(curIndex, out curTexId);
				curTexture = textures.FirstOrDefault(t => t.GetHandle() == curTexId);
			}
		}

		public void Commit()
		{
			if (TextureChain != null) {
				var result = TextureChain.Commit();
				provider.WriteErrorDetails(result, "Unable to commit texture chain");
			}
		}

		public OculusTextureSwapChain(OvrProvider provider, IntVector2 size, EyeType eyeType)
		{
			this.provider = provider;
			Size = size;
			TextureSwapChainDesc desc = new TextureSwapChainDesc {
				Type = TextureType.Texture2D,
				ArraySize = 1,
				Width = size.X,
				Height = size.Y,
				MipLevels = 1,
				Format = TextureFormat.R8G8B8A8_UNORM_SRGB,
				SampleCount = 1,
				StaticImage = 0,
			};
			var result = provider.Hmd.CreateTextureSwapChainGL(desc, out TextureChain);
			provider.WriteErrorDetails(result, "Unable to create texture swap chain");
			var length = 0;
			result = TextureChain.GetLength(out length);
			provider.WriteErrorDetails(result, "Unable to retrive texture swap chain length");
			textures = new OculusTexture[length];
			if (result >= Result.Success) {
				for (int i = 0; i < length; ++i) {
					uint chainTexId;
					result = TextureChain.GetBufferGL(i, out chainTexId);
					provider.WriteErrorDetails(result, "Unable to retrive OpenGL texture from chain");
					textures[i] = new OculusTexture(size.X, size.Y, chainTexId);
				}
			}
		}

		public void Dispose()
		{
			TextureChain.Dispose();
			foreach(var texture in textures) {
				texture.Dispose();
			}
		}
	}
}
#endif