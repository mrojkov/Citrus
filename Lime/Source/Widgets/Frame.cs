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

	public class KeyEventArgs : EventArgs 
	{
		public bool Consumed;
	}

	[ProtoContract]
	public class Frame : Widget, IImageCombinerArg
	{
		RenderTarget renderTarget;
		SerializableTexture renderTexture;

		public event EventHandler<UpdateEventArgs> BeforeUpdate;
		public event EventHandler<UpdateEventArgs> AfterUpdate;
		public event EventHandler<EventArgs> BeforeLateUpdate;
		public event EventHandler<EventArgs> AfterLateUpdate;

		[ProtoMember(1)]
		public RenderTarget RenderTarget {
			get { return renderTarget; }
			set {
				renderTarget = value;
				renderedToTexture = value != RenderTarget.None;
				switch(value) {
				case RenderTarget.A:
					renderTexture = new SerializableTexture("#a");
					break;
				case RenderTarget.B:
					renderTexture = new SerializableTexture("#b");
					break;
				case RenderTarget.C:
					renderTexture = new SerializableTexture("#c");
					break;
				case RenderTarget.D:
					renderTexture = new SerializableTexture("#d");
					break;
				default:
					renderTexture = null;
					break;
				}
			}
		}

		void IImageCombinerArg.BypassRendering() {}

		ITexture IImageCombinerArg.GetTexture()
		{
			return renderTexture;
		}

		public override void LateUpdate(int delta)
		{
			if (BeforeLateUpdate != null)
				BeforeLateUpdate(this, new UpdateEventArgs {Delta = delta});
			base.LateUpdate(delta);
			if (AfterLateUpdate != null)
				AfterLateUpdate(this, new UpdateEventArgs {Delta = delta});
		}

		public override void Update(int delta)
		{
			if (BeforeUpdate != null)
				BeforeUpdate(this, new UpdateEventArgs {Delta = delta});
			base.Update(delta);
			if (AfterUpdate != null)
				AfterUpdate(this, new UpdateEventArgs {Delta = delta});
		}

		public override void Render()
		{
			if (renderTexture != null) {
				if (Size.X > 0 && Size.Y > 0) {
					renderTexture.SetAsRenderTarget();
					Viewport vp = Renderer.Viewport;
					Renderer.Viewport = new Viewport { X = 0, Y = 0, Width = renderTexture.ImageSize.Width, Height = renderTexture.ImageSize.Height };
					Renderer.PushProjectionMatrix();
					Renderer.SetOrthogonalProjection(0, Size.Y, Size.X, 0);
					var chain = new RenderChain();
					base.AddToRenderChain(chain);
					chain.RenderAndClear();
					renderTexture.RestoreRenderTarget();
					Renderer.Viewport = vp;
					Renderer.PopProjectionMatrix();
					Renderer.SetOrthogonalProjection(0, 0, 1024, 768);
				}
			}
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (worldShown) {
				if (renderTexture != null)
					chain.Add(this);
				else
					base.AddToRenderChain(chain);
			}
		}
	}
}
