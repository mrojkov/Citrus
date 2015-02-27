#if UNITY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	internal static class MaterialFactory
	{
		static UnityEngine.Shader defaultShader;
		static UnityEngine.Shader additiveShader;

		static MaterialFactory()
		{
			defaultShader = UnityEngine.Shader.Find("AlphaBlending");
			additiveShader = UnityEngine.Shader.Find("AdditiveBlending");
			if (defaultShader == null || additiveShader == null) {
				throw new Lime.Exception("One of standard shaders not found");
			}
		}

		public static UnityEngine.Material CreateMaterial(Blending blending, ITexture[] textures)
		{
			UnityEngine.Material mat;
			if (blending == Blending.Add) {
				mat = new UnityEngine.Material(additiveShader);
			} else {
				mat = new UnityEngine.Material(defaultShader);
			}
			if (textures[0] != null) {
				mat.mainTexture = textures[0].GetUnityTexture ();
			}
			if (textures[1] != null) {
				mat.SetTexture("SecondTex", textures[1].GetUnityTexture());
			}
			return mat;
		}
	}

}
#endif