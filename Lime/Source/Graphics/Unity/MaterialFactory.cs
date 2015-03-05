#if UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	internal static class MaterialFactory
	{
		static UnityEngine.Shader diffuseShader;
		static UnityEngine.Shader imageCombinerShader;
		static UnityEngine.Shader silhuetteShader;

		static MaterialFactory()
		{
			diffuseShader = UnityEngine.Shader.Find("Diffuse");
			imageCombinerShader = UnityEngine.Shader.Find("ImageCombiner");
			silhuetteShader = UnityEngine.Shader.Find("Silhuette");
			if (diffuseShader == null || imageCombinerShader == null || silhuetteShader == null) {
				throw new Lime.Exception("One of standard shaders not found");
			}
		}

		public static UnityEngine.Material CreateMaterial(Blending blending, ShaderId shaderId, ITexture[] textures)
		{
			UnityEngine.Material mat;
			var texCount = textures[1] != null ? 2 : 1;
			UnityEngine.Shader shader;
			switch (shaderId) {
			case ShaderId.Silhuette:
				shader = silhuetteShader;
				break;
			default:
				shader = texCount == 2 ? imageCombinerShader : diffuseShader;
				break;
			}
			mat = new UnityEngine.Material(shader);
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