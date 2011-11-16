using System;
using Lime;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public enum RenderTarget
	{
		[ProtoEnum]
		None,
		[ProtoEnum]
		A,
		[ProtoEnum]
		B,
		[ProtoEnum]
		C,
		[ProtoEnum]
		D,
	}
	
	public class UpdateEventArgs : EventArgs 
	{
		public int Delta;
	}

    [ProtoContract]
    [ProtoInclude(101, typeof(Button))]
	public class Frame : Widget, IImageCombinerArg
	{
		RenderTarget renderTarget;
		PersistentTexture renderTexture;

		[ProtoMember(1)]
		public RenderTarget RenderTarget {
			get { return renderTarget; }
			set {
				renderTarget = value;
				renderedToTexture = value != RenderTarget.None;
				switch (value) {
				case RenderTarget.A:
					renderTexture = new PersistentTexture ("#a");
					break;
				case RenderTarget.B:
					renderTexture = new PersistentTexture ("#b");
					break;
				case RenderTarget.C:
					renderTexture = new PersistentTexture ("#c");
					break;
				case RenderTarget.D:
					renderTexture = new PersistentTexture ("#d");
					break;
				default:
					renderTexture = null;
					break;
				}
			}
		}

		public event EventHandler<EventArgs> BeforeRendering;
		public event EventHandler<EventArgs> AfterRendering;
		public event EventHandler<UpdateEventArgs> BeforeUpdate;
		public event EventHandler<UpdateEventArgs> AfterUpdate;

		void IImageCombinerArg.BypassRendering ()
		{
		}

		ITexture IImageCombinerArg.GetTexture ()
		{
			return renderTexture;
		}

		public override void Update (int delta)
		{
			if (BeforeUpdate != null)
				BeforeUpdate (this, new UpdateEventArgs {Delta = delta});
			
			base.Update (delta);
			
			if (AfterUpdate != null)
				AfterUpdate (this, new UpdateEventArgs {Delta = delta});
		}

		public override void Render ()
		{
			if (BeforeRendering != null)
				BeforeRendering (this, new EventArgs ());
			if (renderTexture != null) {
				if (Size.X > 0 && Size.Y > 0) {
					renderTexture.SetAsRenderTarget ();
					Viewport vp = Renderer.Instance.Viewport;
					Renderer.Instance.Viewport = new Viewport { X = 0, Y = 0, Width = renderTexture.ImageSize.Width, Height = renderTexture.ImageSize.Height };
					Renderer.Instance.PushProjectionMatrix ();
					Renderer.Instance.SetOrthogonalProjection (0, Size.Y, Size.X, 0);
					base.Render ();
					renderTexture.RestoreRenderTarget ();
					Renderer.Instance.Viewport = vp;
					Renderer.Instance.PopProjectionMatrix ();
					Renderer.Instance.SetOrthogonalProjection (0, 0, 1024, 768);
				}
			} else
				base.Render ();
			if (AfterRendering != null)
				AfterRendering (this, new EventArgs ());
		}
	}
}
