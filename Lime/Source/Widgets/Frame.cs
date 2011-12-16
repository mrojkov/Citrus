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
    [ProtoInclude(101, typeof(Button))]
	[ProtoInclude(102, typeof(Slider))]
	public class Frame : Widget, IImageCombinerArg
	{
		RenderTarget renderTarget;
		SerializableTexture renderTexture;

		[ProtoMember(1)]
		public RenderTarget RenderTarget {
			get { return renderTarget; }
			set {
				renderTarget = value;
				renderedToTexture = value != RenderTarget.None;
				switch (value) {
				case RenderTarget.A:
					renderTexture = new SerializableTexture ("#a");
					break;
				case RenderTarget.B:
					renderTexture = new SerializableTexture ("#b");
					break;
				case RenderTarget.C:
					renderTexture = new SerializableTexture ("#c");
					break;
				case RenderTarget.D:
					renderTexture = new SerializableTexture ("#d");
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
		public event EventHandler<EventArgs> BeforeUpdateGUI;
		public event EventHandler<EventArgs> AfterUpdateGUI;
		public event EventHandler<EventArgs> MouseDown;
		public event EventHandler<EventArgs> MouseUp;

		void IImageCombinerArg.BypassRendering ()
		{
		}

		ITexture IImageCombinerArg.GetTexture ()
		{
			return renderTexture;
		}

		public override void UpdateGUI ()
		{
			if (BeforeUpdateGUI != null)
				BeforeUpdateGUI (this, null);
			if (MouseDown != null && Input.GetKeyDown (Key.Mouse0)) {
				if (HitTest (Input.MousePosition)) {
					var args = new KeyEventArgs { Consumed = true };
					MouseDown (this, args);
					Input.ConsumeKeyEvent (Key.Mouse0, args.Consumed);
				}
			}
			if (MouseUp != null && Input.GetKeyUp (Key.Mouse0)) {
				if (HitTest (Input.MousePosition)) {
					var args = new KeyEventArgs { Consumed = true };
					MouseUp (this, new EventArgs ());
					Input.ConsumeKeyEvent (Key.Mouse0, args.Consumed);
				}
			}
			base.UpdateGUI ();
			if (AfterUpdateGUI != null)
				AfterUpdateGUI (this, null);
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
				BeforeRendering (this, null);
			if (renderTexture != null) {
				if (Size.X > 0 && Size.Y > 0) {
					renderTexture.SetAsRenderTarget ();
					Viewport vp = Renderer.Viewport;
					Renderer.Viewport = new Viewport { X = 0, Y = 0, Width = renderTexture.ImageSize.Width, Height = renderTexture.ImageSize.Height };
					Renderer.PushProjectionMatrix ();
					Renderer.SetOrthogonalProjection (0, Size.Y, Size.X, 0);
					base.Render ();
					renderTexture.RestoreRenderTarget ();
					Renderer.Viewport = vp;
					Renderer.PopProjectionMatrix ();
					Renderer.SetOrthogonalProjection (0, 0, 1024, 768);
				}
			} else {
				base.Render ();
			}
			if (AfterRendering != null)
				AfterRendering (this, null);
		}
	}
}
