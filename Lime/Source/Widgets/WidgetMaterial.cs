using System;
using System.Collections.Generic;

namespace Lime
{
	public class WidgetMaterial : IMaterial
	{
		private static Dictionary<int, WidgetMaterial> instanceCache = new Dictionary<int, WidgetMaterial>();

		private ShaderParams[] shaderParamsArray = new[] { Renderer.GlobalShaderParams };

		public readonly bool PremulAlpha;
		public readonly Blending Blending;
		public readonly ShaderProgram ShaderProgram;

		public static readonly IMaterial Diffuse = GetInstance(Blending.Alpha, ShaderId.Diffuse, 0);

		public int PassCount { get; private set; }

		public static WidgetMaterial GetInstance(Blending blending, ShaderId shader, int numTextures, bool premulAlpha = false)
		{
			lock (instanceCache) {
				var instanceKey = GetInstanceKey(blending, shader, numTextures, premulAlpha);
				WidgetMaterial instance;
				if (!instanceCache.TryGetValue(instanceKey, out instance)) {
					instance = new WidgetMaterial(blending, shader, numTextures, premulAlpha);
					instanceCache.Add(instanceKey, instance);
				}
				return instance;
			}
		}

		private static int GetInstanceKey(Blending blending, ShaderId shader, int numTextures, bool premulAlpha)
		{
			var premulAlphaFlag = premulAlpha ? 1 : 0;
			return (int)blending | ((int)shader << 8) | (numTextures << 16) | (premulAlphaFlag << 24);
		}

		private WidgetMaterial(Blending blending, ShaderId shader, int numTextures, bool premulAlpha)
		{
			PremulAlpha = premulAlpha || blending == Blending.Burn || blending == Blending.Darken;
			ShaderProgram = ShaderPrograms.Instance.GetShaderProgram(shader, numTextures,
				PremulAlpha ? ShaderOptions.PremultiplyAlpha : ShaderOptions.None);
			Blending = blending;
			PassCount = Blending == Blending.Glow || Blending == Blending.Darken ? 2 : 1;
		}

		public IMaterial Clone() => this;

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

	public class ColorfulTextMaterial : IMaterial
	{
		private static Dictionary<int, ColorfulTextMaterial> instanceCache = new Dictionary<int, ColorfulTextMaterial>();

		private ShaderParams[] shaderParamsArray;
		private ShaderParams shaderParams;
		private BlendState blendState;
		float colorIndex;
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

		public IMaterial Clone() => this;

		public void Invalidate()
		{
		}

		public void Apply(int pass)
		{
			PlatformRenderer.SetBlendState(blendState);
			PlatformRenderer.SetTextureLegacy(1, ShaderPrograms.ColorfulTextShaderProgram.GradientRampTexture);
			PlatformRenderer.SetShaderProgram(ShaderPrograms.ColorfulTextShaderProgram.GetInstance());
			PlatformRenderer.SetShaderParams(shaderParamsArray);
		}
	}
}
