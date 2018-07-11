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
		StencilTest,
		NoRender,
	}

	[TangerineRegisterNode(CanBeRoot = true, Order = 0)]
	[AllowedChildrenTypes(typeof(Node))]
	public class Frame : Widget, IImageCombinerArg
	{
		private RenderTarget renderTarget;
		private ITexture renderTexture;

		[YuzuMember]
		[TangerineKeyframeColor(13)]
		public ClipMethod ClipChildren { get; set; }

		public Widget ClipByWidget { get; set; }

		[YuzuMember]
		[TangerineKeyframeColor(14)]
		public RenderTarget RenderTarget
		{
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
				// TODO: Implement clipping, for now disable it.
				renderChain.ClipRegion = RenderChain.DefaultClipRegion;
				RenderToTexture(renderTexture, renderChain);
				if (GetTangerineFlag(TangerineFlags.DisplayContent)) {
					RenderWithScissorTest();
				}
			} else if (ClipChildren == ClipMethod.ScissorTest) {
				RenderWithScissorTest();
			} else if (ClipChildren == ClipMethod.StencilTest) {
				RenderWithStencilTest();
			}
		}

		private RenderChain renderChain;

		private void RenderWithScissorTest()
		{
			var rect = CalculateScissorRectangle(ClipByWidget ?? this);
			var savedScissorState = Renderer.ScissorState;
			if (savedScissorState.Enable) {
				if (!IntersectRectangles(rect, savedScissorState.Bounds, out rect)) {
					return;
				}
			}
			Renderer.ScissorState = new ScissorState(rect);
			try {
				EnsureRenderChain();
				renderChain.ClipRegion = CalcGlobalBoundingRect();
				foreach (var node in Nodes) {
					node.RenderChainBuilder?.AddToRenderChain(renderChain);
				}
				renderChain.RenderAndClear();
			} finally {
				Renderer.ScissorState = savedScissorState;
			}
		}

		private void RenderWithStencilTest()
		{
			var savedStencilParams = Renderer.StencilState;
			try {
				// Draw mask into stencil buffer
				var sp = StencilState.Default;
				sp.Enable = true;
				sp.Comparison = CompareFunc.Always;
				sp.ReferenceValue = 1;
				sp.Pass = StencilOp.Replace;
				Renderer.StencilState = sp;
				Renderer.Clear(ClearOptions.StencilBuffer);
				Renderer.ColorWriteEnabled = ColorWriteMask.None;
				var widget = ClipByWidget ?? this;
				Renderer.Transform1 = widget.LocalToWorldTransform;
				Renderer.DrawRect(widget.ContentPosition, widget.ContentSize, Color4.White);
				Renderer.ColorWriteEnabled = ColorWriteMask.All;
				// Draw the frame content
				sp.Comparison = CompareFunc.Equal;
				Renderer.StencilState = sp;
				EnsureRenderChain();
				foreach (var node in Nodes) {
					node.RenderChainBuilder?.AddToRenderChain(renderChain);
				}
				renderChain.RenderAndClear();
			} finally {
				Renderer.StencilState = savedStencilParams;
			}
		}

		internal protected override bool PartialHitTest(ref HitTestArgs args)
		{
			switch (ClipChildren) {
				case ClipMethod.None:
					return base.PartialHitTest(ref args);
				case ClipMethod.ScissorTest:
				case ClipMethod.StencilTest:
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
			var aabb = widget.CalcAABBInViewportSpace(Renderer.Viewport.Bounds, Renderer.WorldViewProjection);
			return (WindowRect)aabb;
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (!GloballyVisible || ClipChildren == ClipMethod.NoRender || !ClipRegionTest(chain.ClipRegion)) {
				return;
			}
			if (renderTexture != null || ClipChildren == ClipMethod.ScissorTest || ClipChildren == ClipMethod.StencilTest) {
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
			var pathComponent = frame.Components.Get<AssetBundlePathComponent>();
			if (pathComponent == null) {
				frame.Components.Add(new AssetBundlePathComponent(path));
			} else {
				pathComponent.Path = path;
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
