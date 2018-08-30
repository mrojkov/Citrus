using System;
using Lime;

namespace Tangerine.Core
{
	public static class WidgetExtensions
	{
		public static void PrepareRendererState(this Widget widget)
		{
			Renderer.Transform1 = widget.LocalToWorldTransform;
			Renderer.Blending = widget.GlobalBlending;
			Renderer.Shader = widget.GlobalShader;
		}

		public static void RenderToTexture(this Widget widget, ITexture texture, RenderChain renderChain, bool clearRenderTarget = true)
		{
			if (widget.Width > 0 && widget.Height > 0) {
				texture.SetAsRenderTarget();
				Renderer.PushState(
					RenderState.ScissorState |
					RenderState.View |
					RenderState.World |
					RenderState.View |
					RenderState.Projection |
					RenderState.DepthState |
					RenderState.CullMode |
					RenderState.Transform2);
				Renderer.ScissorState = ScissorState.ScissorDisabled;
				Renderer.Viewport = new Viewport(0, 0, texture.ImageSize.Width, texture.ImageSize.Height);
				if (clearRenderTarget) {
					Renderer.Clear(new Color4(0, 0, 0, 0));
				}
				Renderer.World = Matrix44.Identity;
				Renderer.View = Matrix44.Identity;
				Renderer.SetOrthogonalProjection(0, 0, widget.Width, widget.Height);
				Renderer.DepthState = DepthState.DepthDisabled;
				Renderer.CullMode = CullMode.None;
				Renderer.Transform2 = widget.LocalToWorldTransform.CalcInversed();
				try {
					for (var node = widget.FirstChild; node != null; node = node.NextSibling) {
						node.RenderChainBuilder?.AddToRenderChain(renderChain);
					}
				} finally {
					renderChain.Clear();
				}
				Renderer.PopState();
				texture.RestoreRenderTarget();
			}
		}

		public static Bitmap ToBitmap(this Widget widget)
		{
			var pixelScale = Window.Current.PixelScale;
			var scaledWidth = (int)(widget.Width * pixelScale);
			var scaledHeight = (int)(widget.Height * pixelScale);
			var savedScale = widget.Scale;
			var savedPosition = widget.Position;
			var savedPivot = widget.Pivot;

			try {
				widget.Scale = Vector2.One;
				widget.Position = Vector2.Zero;
				widget.Pivot = Vector2.Zero;

				using (var texture = new RenderTexture(scaledWidth, scaledHeight)) {
					var renderChain = new RenderChain();
					widget.RenderChainBuilder?.AddToRenderChain(renderChain);
					widget.RenderToTexture(texture, renderChain);
					return new Bitmap(texture.GetPixels(), scaledWidth, scaledHeight);
				}
			} finally {
				widget.Scale = savedScale;
				widget.Position = savedPosition;
				widget.Pivot = savedPivot;
			}
		}

		public static void AddChangeWatcher<T>(this Widget widget, Func<T> getter, Action<T> action)
		{
			widget.Tasks.Add(new Property<T>(getter).DistinctUntilChanged().Consume(action));
		}

		public static void AddChangeWatcher<T>(this Widget widget, IDataflowProvider<T> provider, Action<T> action)
		{
			widget.Tasks.Add(provider.DistinctUntilChanged().Consume(action));
		}

		public static void AddChangeLateWatcher<T>(this Widget widget, Func<T> getter, Action<T> action)
		{
			widget.LateTasks.Add(new Property<T>(getter).DistinctUntilChanged().Consume(action));
		}

		public static void AddChangeLateWatcher<T>(this Widget widget, IDataflowProvider<T> provider, Action<T> action)
		{
			widget.LateTasks.Add(provider.DistinctUntilChanged().Consume(action));
		}

		public static void AddChangeWatcher<T>(this Widget widget, Property<T> prop, Action<T> action)
		{
			widget.Tasks.Add(new Property<T>(prop.Getter).DistinctUntilChanged().Consume(action));
		}

		public static void AddTransactionClickHandler(this Button button, Action clicked)
		{
			button.Clicked += () => {
				var history = Document.Current.History;
				using (history.BeginTransaction()) {
					clicked();
					history.CommitTransaction();
				}
			};
		}

		public static float Left(this Widget widget) => widget.X;
		public static float Right(this Widget widget) => widget.X + widget.Width;
		public static float Top(this Widget widget) => widget.Y;
		public static float Bottom(this Widget widget) => widget.Y + widget.Height;
	}
}
