#if UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	internal static class MaterialFactory
	{
		static UnityEngine.Material flatMat;
		static UnityEngine.Material diffuseMat;
		static UnityEngine.Material imageCombinerMat;
		static UnityEngine.Material silhuetteMat;

		static MaterialFactory()
		{
			flatMat = new UnityEngine.Material(UnityEngine.Shader.Find("Flat"));
			diffuseMat = new UnityEngine.Material(UnityEngine.Shader.Find("Diffuse"));
			imageCombinerMat = new UnityEngine.Material(UnityEngine.Shader.Find("ImageCombiner"));
			silhuetteMat = new UnityEngine.Material(UnityEngine.Shader.Find("Silhuette"));
		}

		public static UnityEngine.Material GetMaterial(Blending blending, ShaderId shaderId, ITexture[] textures)
		{
			UnityEngine.Material mat;
			var texCount = textures[0] != null ? (textures[1] != null ? 2 : 1) : 0;
			switch (shaderId) {
			case ShaderId.Silhuette:
				mat = silhuetteMat;
				break;
			default:
				mat = texCount == 2 ? imageCombinerMat : (texCount == 1 ? diffuseMat : flatMat);
				break;
			}
			if (textures[0] != null) {
				mat.mainTexture = textures[0].GetUnityTexture();
			}
			if (textures[1] != null) {
				mat.SetTexture("SecondTex", textures[1].GetUnityTexture());
			}
			UnityEngine.Rendering.BlendMode srcMode, dstMode;
			switch (blending) {
			case Blending.Add:
			case Blending.Glow:
				srcMode = Renderer.PremultipliedAlphaMode ? 
					UnityEngine.Rendering.BlendMode.One : UnityEngine.Rendering.BlendMode.SrcAlpha;
				dstMode = UnityEngine.Rendering.BlendMode.One;
				break;
			case Blending.Burn:
			case Blending.Darken:
				srcMode = UnityEngine.Rendering.BlendMode.DstColor;
				dstMode = UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
				break;
			case Blending.Modulate:
				srcMode = UnityEngine.Rendering.BlendMode.DstColor;
				dstMode = UnityEngine.Rendering.BlendMode.Zero;
				break;
			case Blending.Inherited:
			case Blending.Alpha:
			default:
				srcMode = Renderer.PremultipliedAlphaMode ? 
					UnityEngine.Rendering.BlendMode.One : UnityEngine.Rendering.BlendMode.SrcAlpha;
				dstMode = UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
				break;
			}
			mat.SetInt("BlendSrcMode", (int)srcMode);
			mat.SetInt("BlendDstMode", (int)dstMode);
			return mat;
		}
	}
}
#endif