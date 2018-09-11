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
	[TangerineAllowedChildrenTypes(typeof(Node))]
	[TangerineVisualHintGroup("/All/Nodes/Containers")]
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

		[Obsolete("Use Node.CreateFromAssetBundle instead", false)]
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

		private WindowRect CalculateScissorRectangle(Widget widget)
		{
			var root = (WindowWidget)WidgetContext.Current.Root;
			var aabb = widget.CalcAABBInViewportSpace(root.GetViewport(), root.GetProjection());
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
			} else {
				AddSelfAndChildrenToRenderChain(chain, Layer);
			}
		}

		public static Frame CreateSubframe(string path)
		{
			var frame = (Frame)CreateFromAssetBundle(path).Nodes[0];
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

		private RenderChain renderChain;

		private void EnsureRenderChain()
		{
			renderChain = renderChain ?? new RenderChain();
		}

		protected internal override Lime.RenderObject GetRenderObject()
		{
			if (renderTexture == null && ClipChildren != ClipMethod.ScissorTest && ClipChildren != ClipMethod.StencilTest) {
				return null;
			}
			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.CaptureRenderState(this);
			ro.RenderTexture = renderTexture;
			ro.FrameSize = Size;
			ro.ScissorTest = ro.StencilTest = false;
			var clipRegion = RenderChain.DefaultClipRegion;
			if (renderTexture != null) {
				if (GetTangerineFlag(TangerineFlags.DisplayContent)) {
					throw new NotImplementedException();
				}
				// TODO: Implement clipping, for now disable it.
				clipRegion = RenderChain.DefaultClipRegion;
			} else if (ClipChildren == ClipMethod.ScissorTest) {
				ro.ScissorTest = true;
				ro.ScissorRect = CalculateScissorRectangle(ClipByWidget ?? this);
				clipRegion = CalcGlobalBoundingRect();
			} else if (ClipChildren == ClipMethod.StencilTest) {
				var clipBy = ClipByWidget ?? this;
				ro.StencilTest = true;
				ro.StencilRect = new Rectangle(clipBy.ContentPosition, clipBy.ContentPosition + clipBy.ContentSize);
				clipRegion = RenderChain.DefaultClipRegion;
			}
			EnsureRenderChain();
			renderChain.Clear();
			renderChain.ClipRegion = clipRegion;
			foreach (var node in Nodes) {
				node.RenderChainBuilder?.AddToRenderChain(renderChain);
			}
			renderChain.GetRenderObjects(ro.Objects);
			return ro;
		}

		private class RenderObject : WidgetRenderObject
		{
			public RenderObjectList Objects = new RenderObjectList();
			public ITexture RenderTexture;
			public Vector2 FrameSize;
			public bool ScissorTest;
			public WindowRect ScissorRect;
			public bool StencilTest;
			public Rectangle StencilRect;

			protected override void OnRelease()
			{
				Objects.Clear();
				RenderTexture = null;
			}

			public override void Render()
			{
				if (RenderTexture != null) {
					RenderToTexture();
				} else if (ScissorTest) {
					RenderWithScissorTest();
				} else if (StencilTest) {
					RenderWithStencilTest();
				}
			}

			private void RenderToTexture()
			{
				if (FrameSize.X > 0 && FrameSize.Y > 0) {
					RenderTexture.SetAsRenderTarget();
					Renderer.PushState(
						RenderState.Viewport |
						RenderState.World |
						RenderState.View |
						RenderState.Projection |
						RenderState.DepthState |
						RenderState.ScissorState |
						RenderState.CullMode |
						RenderState.Transform2);
					try {
						Renderer.ScissorState = ScissorState.ScissorDisabled;
						Renderer.Viewport = new Viewport(0, 0, RenderTexture.ImageSize.Width, RenderTexture.ImageSize.Height);
						Renderer.Clear(new Color4(0, 0, 0, 0));
						Renderer.World = Renderer.View = Matrix44.Identity;
						Renderer.SetOrthogonalProjection(0, 0, FrameSize.X, FrameSize.Y);
						Renderer.DepthState = DepthState.DepthDisabled;
						Renderer.CullMode = CullMode.None;
						Renderer.Transform2 = LocalToWorldTransform.CalcInversed();
						Objects.Render();
					} finally {
						RenderTexture.RestoreRenderTarget();
						Renderer.PopState();
					}
				}
			}

			private void RenderWithScissorTest()
			{
				Renderer.PushState(RenderState.ScissorState);
				try {
					Renderer.SetScissorState(new ScissorState(ScissorRect), intersectWithCurrent: true);
					Objects.Render();
				} finally {
					Renderer.PopState();
				}
			}

			private void RenderWithStencilTest()
			{
				Renderer.PushState(RenderState.StencilState);
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
					Renderer.Transform1 = LocalToWorldTransform;
					Renderer.DrawRect(StencilRect.A, StencilRect.Size, Color4.White);
					Renderer.ColorWriteEnabled = ColorWriteMask.All;
					// Draw the frame content
					sp.Comparison = CompareFunc.Equal;
					Renderer.StencilState = sp;
					Objects.Render();
				} finally {
					Renderer.PopState();
				}
			}
		}
	}
}
