using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.SceneView
{
	public class ContainerAreaPresenter
	{
		private const float ChessCellSize = 50;

		public ContainerAreaPresenter(SceneView sceneView)
		{
			var backgroundTexture = PrepareChessTexture(Color4.Gray, Color4.Gray.Darken(0.15f));
			sceneView.Scene.CompoundPresenter.Push(new DelegatePresenter<Widget>(w => {
				var ctr = SceneView.Instance.Frame;
				if (ctr != null) {
					ctr.PrepareRendererState();
					if (Core.Document.Current.PreviewAnimation) {
						Renderer.DrawRect(Vector2.Zero, ctr.Size, Color4.Black);
					} else {
						var ratio = ChessCellSize * sceneView.Scene.Scale;
						Renderer.DrawSprite(
							backgroundTexture,
							Color4.White,
							Vector2.Zero,
							ctr.Size,
							-sceneView.Scene.Position / ratio,
							 (ctr.Size - sceneView.Scene.Position) / ratio);
						var root = Core.Document.Current.RootNode as Widget;
						Renderer.Transform1 = root.LocalToWorldTransform;
						Renderer.DrawRect(Vector2.Zero, root.Size, Color4.White.Transparentify(0.8f));
					}
				}
			}));

			const float deviceHeight = 768;
			const float deviceWidth1 = 1366;
			const float deviceWidth2 = 1152;
			const float deviceWidth3 = 1024;
			sceneView.Scene.CompoundPostPresenter.Push(
				new DelegatePresenter<Widget>(
					(w) => {
						var root = Core.Document.Current.RootNode as Widget;
						if (root != null && Core.UserPreferences.Instance.Get<UserPreferences>().ShowOverlays) {
							root.PrepareRendererState();
							var mtx = root.LocalToWorldTransform;
							var t1 = 1 / mtx.U.Length;
							var t2 = 1 / mtx.V.Length;
							Renderer.Transform1 = mtx;
							var rootCenter = root.Size * 0.5f;
							var a1 = new Vector2(deviceWidth1, deviceHeight) * 0.5f + rootCenter;
							var b1 = new Vector2(deviceWidth1, deviceHeight) * -0.5f + rootCenter;
							var a2 = new Vector2(deviceHeight, deviceWidth1) * 0.5f + rootCenter;
							var b2 = new Vector2(deviceHeight, deviceWidth1) * -0.5f + rootCenter;
							var a3 = new Vector2(deviceWidth2, deviceHeight) * 0.5f + rootCenter;
							var b3 = new Vector2(deviceWidth2, deviceHeight) * -0.5f + rootCenter;
							var a4 = new Vector2(deviceHeight, deviceWidth2) * 0.5f + rootCenter;
							var b4 = new Vector2(deviceHeight, deviceWidth2) * -0.5f + rootCenter;
							var a5 = new Vector2(deviceWidth3, deviceHeight) * 0.5f + rootCenter;
							var b5 = new Vector2(deviceWidth3, deviceHeight) * -0.5f + rootCenter;
							var a6 = new Vector2(deviceHeight, deviceWidth3) * 0.5f + rootCenter;
							var b6 = new Vector2(deviceHeight, deviceWidth3) * -0.5f + rootCenter;
							Renderer.DrawRectOutline(a1, b1, Color4.White, t1);
							Renderer.DrawRectOutline(a2, b2, Color4.White, t1);
							Renderer.DrawRectOutline(a3, b3, Color4.White, t1);
							Renderer.DrawRectOutline(a4, b4, Color4.White, t1);
							Renderer.DrawRectOutline(a5, b5, Color4.White, t1);
							Renderer.DrawRectOutline(a6, b6, Color4.White, t1);
						}
					}));

			sceneView.Scene.CompoundPostPresenter.Push(new DelegatePresenter<Widget>(w => {
				var frame = SceneView.Instance.Frame;
				if (frame != null && !Core.Document.Current.PreviewAnimation) {
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

		private ITexture PrepareChessTexture(Color4 color1, Color4 color2)
		{
			var chessTexture = new Texture2D();
			chessTexture.LoadImage(new[] { color1, color2, color2, color1 }, 2, 2);
			chessTexture.WrapModeV = chessTexture.WrapModeU = TextureWrapMode.Repeat;
			chessTexture.MinFilter = chessTexture.MagFilter = TextureFilter.Nearest;
			return chessTexture;
		}
	}
}
