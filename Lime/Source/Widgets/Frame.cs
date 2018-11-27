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
			var clipBy = ClipByWidget ?? this;
			var clipRegion = RenderChain.DefaultClipRegion;
			ro.ClipByLocalToWorld = clipBy.LocalToWorldTransform;
			ro.ClipBySize = clipBy.Size;
			if (renderTexture != null) {
				// TODO: Implement clipping, for now disable it.
				clipRegion = RenderChain.DefaultClipRegion;
				if (GetTangerineFlag(TangerineFlags.DisplayContent)) {
					ro.ScissorTest = true;
				}
			} else if (ClipChildren == ClipMethod.ScissorTest) {
				ro.ScissorTest = true;
				clipRegion = CalcGlobalBoundingRect();
			} else if (ClipChildren == ClipMethod.StencilTest) {
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
			public Matrix32 ClipByLocalToWorld;
			public Vector2 ClipBySize;
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
				}
				if (ScissorTest) {
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
						RenderState.StencilState |
						RenderState.CullMode |
						RenderState.Transform2);
					try {
						Renderer.ScissorState = ScissorState.ScissorDisabled;
						Renderer.StencilState = StencilState.Default;
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

			private WindowRect CalculateScissorRectangle(Vector2 size, WindowRect viewport, Matrix44 wvp)
			{
				var t1 = wvp.TransformVector(new Vector4(0, 0, 0, 1));
				var t2 = wvp.TransformVector(new Vector4(size.X, 0, 0, 1));
				var t3 = wvp.TransformVector(new Vector4(size.X, size.Y, 0, 1));
				var t4 = wvp.TransformVector(new Vector4(0, size.Y, 0, 1));

				if (Mathf.Abs(t1.W) < Mathf.ZeroTolerance ||
					Mathf.Abs(t2.W) < Mathf.ZeroTolerance ||
					Mathf.Abs(t3.W) < Mathf.ZeroTolerance ||
					Mathf.Abs(t4.W) < Mathf.ZeroTolerance
				) {
					return new WindowRect();
				}

				var v1 = (Vector2)(t1 / t1.W);
				var v2 = (Vector2)(t2 / t2.W);
				var v3 = (Vector2)(t3 / t3.W);
				var v4 = (Vector2)(t4 / t4.W);

				var min = v1;
				min = Vector2.Min(min, v2);
				min = Vector2.Min(min, v3);
				min = Vector2.Min(min, v4);

				var max = v1;
				max = Vector2.Max(max, v2);
				max = Vector2.Max(max, v3);
				max = Vector2.Max(max, v4);

				min = (Vector2)viewport.Origin + (min + Vector2.One) * (Vector2)viewport.Size * Vector2.Half;
				max = (Vector2)viewport.Origin + (max + Vector2.One) * (Vector2)viewport.Size * Vector2.Half;

				return new WindowRect {
					X = min.X.Round(),
					Y = min.Y.Round(),
					Width = (max - min).X.Round(),
					Height = (max - min).Y.Round()
				};
			}

			private void RenderWithScissorTest()
			{
				var rect = CalculateScissorRectangle(ClipBySize, Renderer.Viewport.Bounds, (Matrix44)(ClipByLocalToWorld * Renderer.Transform2) * Renderer.FixupWVP(Renderer.Projection));
				Renderer.PushState(RenderState.ScissorState);
				try {
					Renderer.SetScissorState(new ScissorState(rect), intersectWithCurrent: true);
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
