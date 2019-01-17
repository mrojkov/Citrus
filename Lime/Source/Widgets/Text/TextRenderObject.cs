using System;
using System.Collections.Generic;
using Lime;

namespace Lime
{
	public enum TextRenderingMode
	{
		Common,
		OnePassWithoutOutline,
		OnePassWithOutline,
		TwoPasses
	}

	internal class TextRenderObject : WidgetRenderObject
	{
		public SpriteList SpriteList;
		public Color4 Color;
		public int GradientMapIndex;
		public TextRenderingMode RenderMode;

		public override void Render()
		{
			Renderer.Transform1 = LocalToWorldTransform;
			if (GradientMapIndex < 0 || RenderMode == TextRenderingMode.Common) {
				SpriteList.Render(Color, Blending, Shader);
			} else {
				if (RenderMode == TextRenderingMode.OnePassWithOutline || RenderMode == TextRenderingMode.TwoPasses) {
					ColorfulMaterialProvider.Instance.Init(Blending, GradientMapIndex);
					SpriteList.Render(Color, ColorfulMaterialProvider.Instance);
				}

				if (RenderMode == TextRenderingMode.OnePassWithoutOutline || RenderMode == TextRenderingMode.TwoPasses) {
					ColorfulMaterialProvider.Instance.Init(
						Blending, ShaderPrograms.ColorfulTextShaderProgram.GradientMapTextureSize - GradientMapIndex - 1);
					SpriteList.Render(Color, ColorfulMaterialProvider.Instance);
				}
			}
		}

		protected override void OnRelease()
		{
			SpriteList = null;
		}


		private class ColorfulMaterialProvider : Sprite.IMaterialProvider
		{
			public static readonly ColorfulMaterialProvider Instance = new ColorfulMaterialProvider();

			private IMaterial material;

			public void Init(Blending blending, int gradientMapIndex)
			{
				material = ColorfulTextMaterial.GetInstance(blending, gradientMapIndex);
			}

			public IMaterial GetMaterial(int tag) => material;

			public Sprite ProcessSprite(Sprite s) => s;
		}
	}
}
