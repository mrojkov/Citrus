
namespace Lime
{
	internal abstract class SDFRenderAction
	{
		public virtual Buffer GetTextureBuffer(SDFRenderObject ro) => null;
		public abstract bool EnabledCheck(SDFRenderObject ro);
		public abstract void Do(SDFRenderObject ro);

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
