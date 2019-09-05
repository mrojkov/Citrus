using System;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	public enum TextureBlending
	{
		None,
		Multiply,
		CutOut
	}

	[YuzuCopyable]
	public class WidgetMaterial : IMaterial
	{
		private static Dictionary<int, WidgetMaterial> instanceCache = new Dictionary<int, WidgetMaterial>();

		private ShaderParams[] shaderParamsArray = new[] { Renderer.GlobalShaderParams };

		public readonly bool PremulAlpha;
		public readonly Blending Blending;
		public readonly ShaderProgram ShaderProgram;

		public static readonly IMaterial Diffuse = GetInstance(Blending.Alpha, ShaderId.Diffuse, 0);

		public string Id { get; set; }
		public int PassCount { get; private set; }

		public static WidgetMaterial GetInstance(Blending blending, ShaderId shader, int numTextures, TextureBlending textureBlending = TextureBlending.Multiply, bool premulAlpha = false)
		{
			lock (instanceCache) {
				var instanceKey = GetInstanceKey(blending, shader, numTextures, textureBlending, premulAlpha);
				WidgetMaterial instance;
				if (!instanceCache.TryGetValue(instanceKey, out instance)) {
					instance = new WidgetMaterial(blending, shader, numTextures, textureBlending, premulAlpha);
					instanceCache.Add(instanceKey, instance);
				}
				return instance;
			}
		}

		private static int GetInstanceKey(Blending blending, ShaderId shader, int numTextures, TextureBlending textureBlending, bool premulAlpha)
		{
			var premulAlphaFlag = premulAlpha ? 1 : 0;
			return (int)blending | ((int)shader << 8) | (numTextures << 16) | (premulAlphaFlag << 24) | ((int)textureBlending << 25);
		}

		private WidgetMaterial(Blending blending, ShaderId shader, int numTextures, TextureBlending textureBlending, bool premulAlpha)
		{
			PremulAlpha = premulAlpha || blending == Blending.Burn || blending == Blending.Darken;
			var options = PremulAlpha ? ShaderOptions.PremultiplyAlpha : ShaderOptions.None;
			if (textureBlending == TextureBlending.CutOut) {
				options |= ShaderOptions.CutOutTextureBlending;
			}
			ShaderProgram = ShaderPrograms.Instance.GetShaderProgram(shader, numTextures, options);
			Blending = blending;
			PassCount = Blending == Blending.Glow || Blending == Blending.Darken ? 2 : 1;
		}

		public void Apply(int pass)
		{
			if (PassCount == 2 && pass == 0) {
				PlatformRenderer.SetBlendState(Blending.Alpha.GetBlendState(PremulAlpha));
			} else {
				PlatformRenderer.SetBlendState(Blending.GetBlendState(PremulAlpha));
			}
			PlatformRenderer.SetShaderParams(shaderParamsArray);
			PlatformRenderer.SetShaderProgram(ShaderProgram);
		}

		public void Invalidate() { }

		public static int GetNumTextures(ITexture texture1, ITexture texture2)
		{
			return texture1 != null ? texture2 != null ? 2 : 1 : 0;
		}
	}

	[YuzuCopyable]
	public class ColorfulTextMaterial : IMaterial
	{
		private static Dictionary<int, ColorfulTextMaterial> instanceCache = new Dictionary<int, ColorfulTextMaterial>();

		private ShaderParams[] shaderParamsArray;
		private ShaderParams shaderParams;
		private BlendState blendState;
		float colorIndex;
		public string Id { get; set; }
		public int PassCount => 1;

		private ColorfulTextMaterial(Blending blending, int styleIndex)
		{
			colorIndex = ShaderPrograms.ColorfulTextShaderProgram.StyleIndexToColorIndex(styleIndex);
			blendState = blending.GetBlendState();
			shaderParams = new ShaderParams();
			shaderParams.Set(shaderParams.GetParamKey<float>("colorIndex"), colorIndex);
			shaderParamsArray = new[] { Renderer.GlobalShaderParams, shaderParams };
		}

		public static ColorfulTextMaterial GetInstance(Blending blending, int styleIndex)
		{
			lock (instanceCache) {
				var instanceKey = GetInstanceKey(blending, styleIndex);
				ColorfulTextMaterial instance;
				if (!instanceCache.TryGetValue(instanceKey, out instance)) {
					instance = new ColorfulTextMaterial(blending, styleIndex);
					instanceCache.Add(instanceKey, instance);
				}
				return instance;
			}
		}

		private static int GetInstanceKey(Blending blending, int styleIndex)
		{
			return (int)blending | (styleIndex << 8);
		}

		public void Invalidate()
		{
		}

		public void Apply(int pass)
		{
			var texture = ShaderPrograms.ColorfulTextShaderProgram.GradientRampTexture;
			if (texture == null || texture.IsStubTexture) {
				var warningText = "GradientMap texture doesnt exist at './Data/Fonts/GradientMap.png' If you want to use it, create it and dont forget cooking rule file to disable texture atlas for it.";
#if DEBUG
				throw new Lime.Exception(warningText);
#else
				Console.WriteLine(warningText);
#endif
			}
			PlatformRenderer.SetBlendState(blendState);
			PlatformRenderer.SetTextureLegacy(1, ShaderPrograms.ColorfulTextShaderProgram.GradientRampTexture);
			PlatformRenderer.SetShaderProgram(ShaderPrograms.ColorfulTextShaderProgram.GetInstance());
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}
	}
}
