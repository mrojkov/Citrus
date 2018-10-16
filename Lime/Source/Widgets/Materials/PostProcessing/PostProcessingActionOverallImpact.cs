namespace Lime
{
	internal class PostProcessingActionOverallImpact : PostProcessingAction
	{
		public override bool EnabledCheck(PostProcessingRenderObject ro) => ro.OverallImpactEnabled && ro.IsNotDebugViewMode;

		public override void Do(PostProcessingRenderObject ro)
		{
			ro.RenderTexture(ro.SourceTextureBuffer.Texture, customUV1: ro.Size / (Vector2)ro.SourceTextureBuffer.Size);
			ro.Color = ro.OverallImpactColor;
		}
	}
}
