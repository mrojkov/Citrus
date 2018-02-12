using System;

namespace Lime
{
	public class WidgetMaterial : IMaterial
	{
		private static readonly WidgetMaterialList texturelessMaterials = new WidgetMaterialList();
		
		public readonly bool PremulAlpha;
		public readonly Blending Blending;
		public readonly ShaderProgram ShaderProgram;
		public readonly ITexture Texture1;
		public readonly ITexture Texture2;
		
		public static readonly IMaterial Diffuse = GetInstance(Blending.Alpha, ShaderId.Diffuse);

		public int PassCount { get; private set; }
		
		public static WidgetMaterial GetInstance(Blending blending, ShaderProgram shaderProgram, ITexture texture1 = null, ITexture texture2 = null, bool premulAlpha = false)
		{
			var atlas1 = texture1?.AtlasTexture;
			var atlas2 = texture2?.AtlasTexture;
			if (atlas1 != null) {
				return ((IWidgetMaterialListHolder)atlas1).WidgetMaterials.GetInstance(blending, shaderProgram, atlas1, atlas2, premulAlpha);
			} else {
				return texturelessMaterials.GetInstance(blending, shaderProgram, atlas1, atlas2, premulAlpha);
			}
		}

		public static WidgetMaterial GetInstance(Blending blending, ShaderId shader, ShaderProgram customShader = null, ITexture texture1 = null, ITexture texture2 = null, bool premulAlpha = false)
		{
			premulAlpha |= (blending == Blending.Burn || blending == Blending.Darken);
			var shaderProgram = shader == ShaderId.Custom ?
				customShader :
				ShaderPrograms.Instance.GetShaderProgram(shader, texture1, texture2, 
					premulAlpha ? ShaderOptions.PremultiplyAlpha : ShaderOptions.None);
			return GetInstance(blending, shaderProgram, texture1, texture2, premulAlpha);
		}

		internal WidgetMaterial(Blending blending, ShaderProgram shaderProgram, ITexture texture1, ITexture texture2, bool premulAlpha)
		{
			ShaderProgram = shaderProgram;
			Blending = blending;
			PassCount = Blending == Blending.Glow || Blending == Blending.Darken ? 2 : 1;
			Texture1 = texture1;
			Texture2 = texture2;
		}
		
		public IMaterial Clone() => (WidgetMaterial)MemberwiseClone();
		
		public void Apply(int pass)
		{
			PlatformRenderer.SetTexture(Texture1, 0);
			PlatformRenderer.SetTexture(Texture2, 1);
			if (PassCount == 2 && pass == 0) {
				PlatformRenderer.SetBlending(Blending.Alpha, PremulAlpha);
			} else {
				PlatformRenderer.SetBlending(Blending, PremulAlpha);
			}
			PlatformRenderer.SetShaderProgram(ShaderProgram);
		}
		
		public void Invalidate() { }
	}
	
	internal interface IWidgetMaterialListHolder
	{
		WidgetMaterialList WidgetMaterials { get; }
	}
	
	internal class WidgetMaterialList
	{
		private WidgetMaterial[] items = empty; 
		private static WidgetMaterial[] empty = new WidgetMaterial[0];
		
		public WidgetMaterial GetInstance(Blending blending, ShaderProgram shaderProgram, ITexture texture1, ITexture texture2, bool premulAlpha)
		{
			foreach (var i in items) {
				if (i.Blending == blending && i.ShaderProgram == shaderProgram && i.Texture1 == texture1 && i.Texture2 == texture2) {
					return i;
				}
			}
			var wm = new WidgetMaterial(blending, shaderProgram, texture1, texture2, premulAlpha);
			Array.Resize(ref items, items.Length + 1);
			items[items.Length - 1] = wm;
			return wm;
		}
	}
}
