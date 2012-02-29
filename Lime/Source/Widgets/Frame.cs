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

		public float AnimationSpeed = 1.0f;

		// In dialog mode frame acts like a modal dialog, all controls behind the dialog are frozen.
		// If dialog is being shown or hidden then all controls on dialog are frozen either.
		public bool DialogMode;

		void IImageCombinerArg.BypassRendering() {}

		ITexture IImageCombinerArg.GetTexture()
		{
			return renderTexture;
		}

		void UpdateForDialogMode(int delta)
		{
			if (worldShown && Input.MouseVisible) {
				if (RootFrame.Instance.ActiveWidget != null && !RootFrame.Instance.ActiveWidget.ChildOf(this)) {
					// Discard active widget if it's not a child of the topmost dialog.
					RootFrame.Instance.ActiveWidget = null;
				}
			}
			if (worldShown && RootFrame.Instance.ActiveTextWidget != null && !RootFrame.Instance.ActiveTextWidget.ChildOf(this)) {
				// Discard active text widget if it's not a child of the topmost dialog.
				RootFrame.Instance.ActiveTextWidget = null;
			}
			if (!Playing) {
				base.Update(delta);
			}
			if (worldShown) {
				// Cosume all input events and drive mouse out of the screen.
				Input.ConsumeAllKeyEvents(true);
				Input.MouseVisible = false;
				Input.TextInput = null;
			}
			if (Playing) {
				base.Update(delta);
			}
		}

		public override void LateUpdate(int delta)
		{
			if (AnimationSpeed != 1.0f) {
				delta = MultiplyDeltaByAnimationSpeed(delta);
			}
			if (BeforeLateUpdate != null)
				BeforeLateUpdate(this, new UpdateEventArgs {Delta = delta});
			base.LateUpdate(delta);
			if (AfterLateUpdate != null)
				AfterLateUpdate(this, new UpdateEventArgs {Delta = delta});
		}

		public override void Update(int delta)
		{
			if (AnimationSpeed != 1.0f) {
				delta = MultiplyDeltaByAnimationSpeed(delta);
			}
			if (BeforeUpdate != null)
				BeforeUpdate(this, new UpdateEventArgs {Delta = delta});
			if (DialogMode) {
				UpdateForDialogMode(delta);
			} else {
				base.Update(delta);
			}
			if (AfterUpdate != null)
				AfterUpdate(this, new UpdateEventArgs {Delta = delta});
		}

		int MultiplyDeltaByAnimationSpeed(int delta)
		{
			if (AnimationSpeed < 0 || AnimationSpeed > 1) {
				throw new Lime.Exception("AnimationSpeed out of range [0..1]");
			}
			delta = (int)(delta * AnimationSpeed);
			return delta;
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
					foreach (Node node in Nodes.AsArray) {
						node.AddToRenderChain(chain);
					}
					chain.RenderAndClear();
					renderTexture.RestoreRenderTarget();
					Renderer.Viewport = vp;
					Renderer.PopProjectionMatrix();
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
