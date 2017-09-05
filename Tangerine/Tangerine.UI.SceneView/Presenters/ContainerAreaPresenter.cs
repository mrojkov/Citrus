using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class ContainerAreaPresenter
	{
		private const float ChessCellSize = 50;
		private Color4 Color1 => Core.UserPreferences.Instance.Get<UserPreferences>().BackgroundColorA;
		private Color4 Color2 => Core.UserPreferences.Instance.Get<UserPreferences>().BackgroundColorB;
		private Color4 RootWidgetBackgroundColor => Core.UserPreferences.Instance.Get<UserPreferences>().RootWidgetOverlayColor;
		public ContainerAreaPresenter(SceneView sceneView)
		{
			var backgroundTexture = PrepareChessTexture(Color1, Color2);
			sceneView.Frame.AddChangeWatcher(
				() => Core.UserPreferences.Instance.Get<UserPreferences>().BackgroundColorA,
				(v) => backgroundTexture = PrepareChessTexture(v, Color2));
			sceneView.Frame.AddChangeWatcher(
				() => Core.UserPreferences.Instance.Get<UserPreferences>().BackgroundColorB,
				(v) => backgroundTexture = PrepareChessTexture(Color1, v));
			sceneView.Scene.CompoundPresenter.Push(new DelegatePresenter<Widget>(w => {
				var ctr = SceneView.Instance.Frame;
				if (ctr != null) {
					ctr.PrepareRendererState();
					if (Core.Document.Current.PreviewAnimation) {
						Renderer.DrawRect(Vector2.Zero, ctr.Size, Color4.Black);
					} else {
						if (Core.UserPreferences.Instance.Get<UserPreferences>().EnableChessBackground) {
							var ratio = ChessCellSize * sceneView.Scene.Scale;
							Renderer.DrawSprite(
								backgroundTexture,
								Color4.White,
								Vector2.Zero,
								ctr.Size,
								-sceneView.Scene.Position / ratio,
								 (ctr.Size - sceneView.Scene.Position) / ratio);
						} else {
							Renderer.DrawRect(Vector2.Zero, ctr.Size, Color1);
						}
						var root = Core.Document.Current.RootNode as Widget;
						Renderer.Transform1 = root.LocalToWorldTransform;
						Renderer.DrawRect(Vector2.Zero, root.Size, RootWidgetBackgroundColor);

					}
				}
			}));

			const float deviceHeight = 768;
			float[] deviceWidths = { 1366, 1152, 1024, 1579 };
			sceneView.Scene.CompoundPostPresenter.Push(
				new DelegatePresenter<Widget>(
					(w) => {
						var root = Core.Document.Current.RootNode as Widget;
						if (root != null && Core.UserPreferences.Instance.Get<UserPreferences>().ShowOverlays) {
							root.PrepareRendererState();
							var mtx = root.LocalToWorldTransform;
							var t1 = 1 / mtx.U.Length;
							Renderer.Transform1 = mtx;
							var rootCenter = root.Size * 0.5f;
							foreach (var width in deviceWidths) {
								SetAndRenderOverlay(width, deviceHeight, rootCenter, t1);
							}
						}
					}));

			sceneView.Scene.CompoundPostPresenter.Push(new DelegatePresenter<Widget>(w => {
				var frame = SceneView.Instance.Frame;
				if (frame != null && !Core.Document.Current.PreviewAnimation && Core.Document.Current.Container is Widget) {
					frame.PrepareRendererState();
					var c = ColorTheme.Current.SceneView.ContainerBorder;
					var mtx = frame.LocalToWorldTransform;
					var t1 = 1 / mtx.U.Length;
					var t2 = 1 / mtx.V.Length;
					Renderer.Transform1 = mtx;
					var rect = (Core.Document.Current.Container as Widget).CalcAABBInSpaceOf(SceneView.Instance.Frame);
					Renderer.DrawLine(new Vector2(0, rect.A.Y), new Vector2(frame.Size.X, rect.A.Y), c, t1);
					Renderer.DrawLine(new Vector2(0, rect.B.Y), new Vector2(frame.Size.X, rect.B.Y), c, t1);
					Renderer.DrawLine(new Vector2(rect.A.X, 0), new Vector2(rect.A.X, frame.Size.Y), c, t2);
					Renderer.DrawLine(new Vector2(rect.B.X, 0), new Vector2(rect.B.X, frame.Size.Y), c, t2);
				}
			}));
		}

		private static void SetAndRenderOverlay(float width, float height, Vector2 rootCenter, float thickness)
		{
			var a1 = new Vector2(width, height) * 0.5f + rootCenter;
			var b1 = new Vector2(width, height) * -0.5f + rootCenter;
			var a2 = new Vector2(height, width) * 0.5f + rootCenter;
			var b2 = new Vector2(height, width) * -0.5f + rootCenter;
			Renderer.DrawRectOutline(a1, b1, Color4.White, thickness);
			Renderer.DrawRectOutline(a2, b2, Color4.White, thickness);
		}

		private ITexture PrepareChessTexture(Color4 color1, Color4 color2)
		{
			var chessTexture = new Texture2D();
			chessTexture.LoadImage(new[] { color1, color2, color2, color1 }, 2, 2);
			chessTexture.TextureParams = new TextureParams {
				WrapMode = TextureWrapMode.Repeat,
				MinMagFilter = TextureFilter.Nearest,
			};
			return chessTexture;
		}
	}
}
