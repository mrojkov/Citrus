namespace Lime
{
	internal abstract class PostProcessingAction
	{
		public PostProcessingRenderObject RenderObject { get; set; }

		public virtual Buffer TextureBuffer => null;
		public abstract bool Enabled { get; }
		public abstract void Do();

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
