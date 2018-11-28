using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lime
{
	internal class SDFRenderActionOutline : SDFRenderAction
	{
		public override bool EnabledCheck(SDFRenderObject ro) => ro.OutlineEnabled;

		public override void Do(SDFRenderObject ro)
		{
			ro.OutlineMaterial.Thickness = ro.Thickness;
			ro.OutlineMaterial.Softness = ro.OutlineSoftness;
			ro.OutlineMaterial.Color = ro.OutlineColor;
			ro.RenderSpriteList(ro.OutlineMaterialProvider);
		}
	}
}
