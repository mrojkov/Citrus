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
		static UnityEngine.Material diffuseMat3d;
		static UnityEngine.Material imageCombinerMat;
		static UnityEngine.Material silhuetteMat;
		static UnityEngine.Material silhuetteWith2TexturesMat;
		static UnityEngine.Material silhuetteInversedMat;
		public static bool ThreeDimensionalRendering;

		static MaterialFactory()
		{
			flatMat = new UnityEngine.Material(UnityEngine.Shader.Find("Flat"));
			diffuseMat = new UnityEngine.Material(UnityEngine.Shader.Find("Diffuse"));
			diffuseMat3d = new UnityEngine.Material(UnityEngine.Shader.Find("Diffuse3d"));
			imageCombinerMat = new UnityEngine.Material(UnityEngine.Shader.Find("ImageCombiner"));
			silhuetteMat = new UnityEngine.Material(UnityEngine.Shader.Find("Silhuette"));
			silhuetteWith2TexturesMat = new UnityEngine.Material(UnityEngine.Shader.Find("SilhuetteWith2Textures"));
			silhuetteInversedMat = new UnityEngine.Material(UnityEngine.Shader.Find("SilhuetteInversed"));
		}

		public static UnityEngine.Material GetMaterial(Blending blending, bool zTestMode, bool zWriteMode, ShaderId shaderId, ITexture texture1, ITexture texture2)
		{
			UnityEngine.Material mat;
			var texCount = texture1 != null ? (texture2 != null ? 2 : 1) : 0;
			switch (shaderId) {
			case ShaderId.Silhuette:
				mat = texCount == 2 ? silhuetteWith2TexturesMat : silhuetteMat;
				break;
			default:
				if (texCount == 1 && shaderId == ShaderId.InversedSilhuette) {
					mat = silhuetteInversedMat;
					break;
				} 
				mat = texCount == 2 ? imageCombinerMat : (texCount == 1 ? 
					(ThreeDimensionalRendering ? diffuseMat3d : diffuseMat) : flatMat);
				break;
			}
			if (texture1 != null) {
				mat.mainTexture = texture1.GetUnityTexture();
			}
			if (texture2 != null) {
				mat.SetTexture("SecondTex", texture2.GetUnityTexture());
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
			//mat.SetInt("ZWriteMode", zWriteMode ? 1 : 0);
			mat.SetInt ("ZTestMode", zTestMode ? (int)UnityEngine.Rendering.CompareFunction.LessEqual : (int)UnityEngine.Rendering.CompareFunction.Always);
			return mat;
		}
	}
}
#endif