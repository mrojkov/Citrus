using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class ContainerAreaPresenter
	{
		private const float ChessCellSize = 50;
		private Color4 Color1 => SceneUserPreferences.Instance.BackgroundColorA;
		private Color4 Color2 => SceneUserPreferences.Instance.BackgroundColorB;
		private Color4 RootWidgetBackgroundColor => SceneUserPreferences.Instance.RootWidgetOverlayColor;

		public ContainerAreaPresenter(SceneView sceneView)
		{
			var backgroundTexture = PrepareChessTexture(Color1, Color2);
			var playButtonTexture = new Texture2D();

			playButtonTexture.LoadImage(new Bitmap(new EmbeddedResource(Theme.Textures.PlayPath, "Tangerine").GetResourceStream()));
			sceneView.Frame.AddChangeWatcher(
				() => SceneUserPreferences.Instance.BackgroundColorA,
				(v) => backgroundTexture = PrepareChessTexture(v, Color2));
			sceneView.Frame.AddChangeWatcher(
				() => SceneUserPreferences.Instance.BackgroundColorB,
				(v) => backgroundTexture = PrepareChessTexture(Color1, v));
			sceneView.Scene.CompoundPresenter.Push(new DelegatePresenter<Widget>(w => {
				var ctr = SceneView.Instance.Frame;
				if (ctr != null) {
					ctr.PrepareRendererState();
					if (SceneUserPreferences.Instance.EnableChessBackground) {
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
					if (Document.Current.PreviewAnimation) {
						Renderer.DrawRect(
							Vector2.Zero,
							ctr.Size,
							SceneUserPreferences.Instance.AnimationPreviewBackground);
					} else {
						var root = Core.Document.Current.RootNode as Widget;
						Renderer.Transform1 = root.LocalToWorldTransform;
						Renderer.DrawRect(Vector2.Zero, root.Size, RootWidgetBackgroundColor);
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
			sceneView.Scene.CompoundPostPresenter.Push(
				new DelegatePresenter<Widget>(
					(w) => {
						if (Document.Current.PreviewAnimation) {
							var ctr = SceneView.Instance.Frame;
							if (ctr != null) {
								ctr.PrepareRendererState();
								Renderer.DrawSprite(
									playButtonTexture,
									Color4.White,
									new Vector2(10),
									new Vector2(35), Vector2.Zero, Vector2.One);
							}
						}
					}
			));
			var rc = new RenderChain();
			sceneView.Frame.CompoundPostPresenter.Push(
				new DelegatePresenter<Widget>(
					(w) => {
						if (!Document.Current.ExpositionMode) {
							foreach (var widget in Project.Current.Overlays.Values) {
								if (widget.Components.Get<NodeCommandComponent>()?.Command.Checked ?? false) {
									widget.Position = (Document.Current.RootNode.AsWidget.Position +
										(Document.Current.RootNode.AsWidget.Size - widget.Size) / 2) *
										Document.Current.RootNode.AsWidget.LocalToWorldTransform;
									widget.Scale = SceneView.Instance.Scene.Scale;
									widget.RenderChainBuilder.AddToRenderChain(rc);
								}
							}
							rc.RenderAndClear();
						}
						w.PrepareRendererState();
						var size = Document.Current.RootNode.AsWidget.Size / 2;
						foreach (var ruler in Project.Current.Rulers) {
							if (ruler.GetComponents().Get<CommandComponent>()?.Command.Checked ?? false) {
								DrawRuler(ruler, size, w);
							}
						}
						foreach (var ruler in Project.Current.DefaultRulers) {
							if (ruler.GetComponents().Get<CommandComponent>()?.Command.Checked ?? false) {
								DrawRuler(ruler, size, w);
							}
						}
					}));
		}

		private void DrawRuler(RulerData ruler, Vector2 size, Widget root)
		{
			var t = Document.Current.RootNode.AsWidget.CalcTransitionToSpaceOf(root);
			foreach (var line in ruler.Lines) {
				if (line.IsVertical) {
					var val = (new Vector2(line.Value + size.X, 0) * t).X;
					Renderer.DrawLine(new Vector2(val, 0), new Vector2(val, root.Size.Y), ColorTheme.Current.SceneView.Ruler);
				} else {
					var val = (new Vector2(0, line.Value + size.Y) * t).Y;
					Renderer.DrawLine(new Vector2(0, val), new Vector2(root.Size.X, val), ColorTheme.Current.SceneView.Ruler);
				}
			}
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
