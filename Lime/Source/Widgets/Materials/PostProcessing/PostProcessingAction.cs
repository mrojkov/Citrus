namespace Lime
{
	internal abstract class PostProcessingAction
	{
		public virtual Buffer GetTextureBuffer(PostProcessingRenderObject ro) => null;
		public abstract bool EnabledCheck(PostProcessingRenderObject ro);
		public abstract void Do(PostProcessingRenderObject ro);

		internal class Buffer
		{
			private RenderTexture finalTexture;

			public bool IsDirty { get; set; } = true;

			public Size Size { get; }
			public RenderTexture Texture => finalTexture ?? (finalTexture = new RenderTexture(Size.Width, Size.Height));
			public bool WasApplied { get; set; }

			public Buffer(Size size)
			{
				Size = size;
			}

			public void MarkAsDirty() => IsDirty = true;
		}
	}
}
