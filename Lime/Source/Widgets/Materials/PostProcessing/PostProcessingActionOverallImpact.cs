namespace Lime
{
	internal class PostProcessingActionOverallImpact : PostProcessingAction
	{
		public override bool Enabled => RenderObject.OverallImpactEnabled && RenderObject.IsNotDebugViewMode;

		public override void Do()
		{
			RenderObject.RenderTexture(RenderObject.SourceTextureBuffer.Texture, customUV1: RenderObject.Size / (Vector2)RenderObject.SourceTextureBuffer.Size);
			RenderObject.Color = RenderObject.OverallImpactColor;
		}
	}
}
