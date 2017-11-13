using System;
using System.Collections.Generic;
using Yuzu;

namespace Lime
{
	public enum RenderTarget
	{
		None,
		A,
		B,
		C,
		D,
		E,
		F,
		G
	}

	public enum ClipMethod
	{
		None,
		ScissorTest,
		NoRender,
	}

	[AllowedChildrenTypes(typeof(Node))]
	public class Frame : Widget, IImageCombinerArg
	{
		public ClipMethod ClipChildren { get; set; }

		public Widget ClipByWidget { get; set; }

		RenderTarget renderTarget;
		ITexture renderTexture;

		[YuzuMember]
		public RenderTarget RenderTarget {
			get { return renderTarget; }
			set { SetRenderTarget(value); }
		}

		public Frame() { }

		public Frame(Vector2 position)
		{
			Presenter = DefaultPresenter.Instance;
			this.Position = position;
		}

		public Frame(string path) : this()
		{
			CreateFromAssetBundle(path, this);
		}

		private void SetRenderTarget(RenderTarget value)
		{
			renderTarget = value;
			renderTexture = CreateRenderTargetTexture(value);
			PropagateDirtyFlags();
		}

		internal protected override bool IsRenderedToTexture() => renderTarget != RenderTarget.None;

		public override void Render()
		{
			if (renderTexture != null) {
				EnsureRenderChain();
				renderChain.ClipRegion = new Rectangle(0, 0, Width, Height);
				RenderToTexture(renderTexture, renderChain);
				if (GetTangerineFlag(TangerineFlags.DisplayContent)) {
					RenderWithScissorTest();
				}
			} else if (ClipChildren == ClipMethod.ScissorTest) {
				RenderWithScissorTest();
			}
		}

		private RenderChain renderChain;

		private void RenderWithScissorTest()
		{
			var savedScissorTest = Renderer.ScissorTestEnabled;
			var savedScissorRect = Renderer.ScissorRectangle;
			var rect = CalculateScissorRectangle(ClipByWidget ?? this);
			if (savedScissorTest) {
				if (!IntersectRectangles(rect, savedScissorRect, out rect)) {
					return;
				}
			}
			Renderer.ScissorTestEnabled = true;
			Renderer.ScissorRectangle = rect;
			try {
				EnsureRenderChain();
				renderChain.ClipRegion = GlobalBoundingRect;
				foreach (var node in Nodes) {
					node.RenderChainBuilder?.AddToRenderChain(renderChain);
				}
				renderChain.RenderAndClear();
			} finally {
				Renderer.ScissorTestEnabled = savedScissorTest;
				Renderer.ScissorRectangle = savedScissorRect;
			}
		}

		internal protected override bool PartialHitTest(ref HitTestArgs args)
		{
			switch (ClipChildren) {
				case ClipMethod.None:
					return base.PartialHitTest(ref args);
				case ClipMethod.ScissorTest:
					if (!(ClipByWidget ?? this).BoundingRectHitTest(args.Point)) {
						return false;
					}
					EnsureRenderChain();
					try {
						for (var node = FirstChild; node != null; node = node.NextSibling) {
							node.RenderChainBuilder?.AddToRenderChain(renderChain);
						}
						if (renderChain.HitTest(ref args)) {
							return true;
						}
						return base.PartialHitTest(ref args);
					} finally {
						renderChain.Clear();
					}
				case ClipMethod.NoRender:
					return false;
				default:
					throw new InvalidOperationException();
			}
		}

		private void EnsureRenderChain()
		{
			renderChain = renderChain ?? new RenderChain();
		}

		private bool IntersectRectangles(WindowRect a, WindowRect b, out WindowRect r)
		{
			r = (WindowRect)IntRectangle.Intersect((IntRectangle)a, (IntRectangle)b);
			return r.Width > 0 && r.Height > 0;
		}

		private WindowRect CalculateScissorRectangle(Widget widget)
		{
			var aabb = widget.CalcAABBInViewportSpace(Renderer.Viewport, Renderer.WorldViewProjection);
			return (WindowRect)aabb;
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (!GloballyVisible || ClipChildren == ClipMethod.NoRender) {
				return;
			}
			if (renderTexture != null || ClipChildren == ClipMethod.ScissorTest) {
				AddSelfToRenderChain(chain, Layer);
				if (GetTangerineFlag(TangerineFlags.DisplayContent) && ClipChildren != ClipMethod.ScissorTest) {
					AddSelfAndChildrenToRenderChain(chain, Layer);
				}
			} else if (Layer == 0) {
				// 90% calls should go here.
				if (PostPresenter != null) {
					chain.Add(this, PostPresenter);
				}
				for (var node = FirstChild; node != null; node = node.NextSibling) {
					node.RenderChainBuilder?.AddToRenderChain(chain);
				}
				if (Presenter != null) {
					chain.Add(this, Presenter);
				}
			} else {
				AddSelfAndChildrenToRenderChain(chain, Layer);
			}
		}

		public static Frame CreateSubframe(string path)
		{
			var frame = (Frame)(new Frame(path).Nodes[0]);
			frame.Unlink();
			if (!Application.IsTangerine) {
				frame.Tag = path;
			}
			return frame;
		}

		private static ITexture CreateRenderTargetTexture(RenderTarget value)
		{
			switch (value) {
				case RenderTarget.A:
					return new SerializableTexture("#a");
				case RenderTarget.B:
					return new SerializableTexture("#b");
				case RenderTarget.C:
					return new SerializableTexture("#c");
				case RenderTarget.D:
					return new SerializableTexture("#d");
				case RenderTarget.E:
					return new SerializableTexture("#e");
				case RenderTarget.F:
					return new SerializableTexture("#f");
				case RenderTarget.G:
					return new SerializableTexture("#g");
				default:
					return null;
			}
		}

		#region IImageCombinerArg

		void IImageCombinerArg.SkipRender() { }

		ITexture IImageCombinerArg.GetTexture()
		{
			return renderTexture;
		}

		Matrix32 IImageCombinerArg.UVTransform { get { return Matrix32.Identity; } }

		#endregion
	}
}
