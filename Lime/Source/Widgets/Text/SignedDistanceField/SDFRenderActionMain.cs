using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lime
{
	internal class SDFRenderActionMain : SDFRenderAction
	{
		public override bool EnabledCheck(SDFRenderObject ro) => true;
		public override SDFRenderAction.Buffer GetTextureBuffer(SDFRenderObject ro) => ro.SDFBuffer;

		public override void Do(SDFRenderObject ro)
		{
			ro.SDFMaterial.Softness = ro.Softness;
			ro.SDFMaterial.Dilate = ro.Dilate;
			ro.SDFMaterial.Color = ro.Color;
			ro.RenderSpriteList(ro.SDFMaterialProvider);
		}

		internal new class Buffer : SDFRenderAction.Buffer
		{
			private float softness;

			public Buffer(Size size) : base(size) { }

			public bool EqualRenderParameters(SDFRenderObject ro) =>
				!IsDirty &&
				softness == ro.Softness;

			public void SetRenderParameters(SDFRenderObject ro)
			{
				IsDirty = false;
				softness = ro.Softness;
			}
		}
	}
}
