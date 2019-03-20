using System;
using Lime;

namespace Orange
{
	class WarnAboutNPOTTextures : CookStage
	{
		public WarnAboutNPOTTextures() : base()
		{

		}

		protected override void SetExtensions()
		{
			Extensions = new string[] { };
		}

		public override void Action()
		{
			foreach (var path in AssetCooker.AssetBundle.EnumerateFiles()) {
				if ((AssetCooker.AssetBundle.GetAttributes(path) & AssetAttributes.NonPowerOf2Texture) != 0) {
					Console.WriteLine("Warning: non-power of two texture: {0}", path);
				}
			}
		}
	}
}
