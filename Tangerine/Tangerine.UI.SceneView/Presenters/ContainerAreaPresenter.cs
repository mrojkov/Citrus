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

			playButtonTexture.LoadImage(new Bitmap(new ThemedconResource("SceneView.Play", "Tangerine").GetResourceStream()));
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
								var pos = Vector2.Zero;
								if (ProjectUserPreferences.Instance.RulerVisible) {
									pos += Vector2.One * RulersWidget.RulerHeight;
								}
								Renderer.DrawSprite(
									playButtonTexture,
									Color4.White,
									pos,
									(Vector2)playButtonTexture.ImageSize,
									Vector2.Zero,
									Vector2.One
								);
							}
						}
					}
			));
			var renderChain = new RenderChain();
			sceneView.Frame.CompoundPostPresenter.Push(
				new DelegatePresenter<Widget>(
					(w) => {
						if (!Document.Current.ExpositionMode) {
							foreach (var pair in Project.Current.Overlays) {
								var widget = pair.Value;
								if (ProjectUserPreferences.Instance.DisplayedOverlays.Contains(pair.Key)) {
									if (widget.Components.Get<NodeCommandComponent>()?.Command.Checked ?? false) {
										widget.Position = (Document.Current.RootNode.AsWidget.Position +
											(Document.Current.RootNode.AsWidget.Size - widget.Size) / 2) *
											Document.Current.RootNode.AsWidget.LocalToWorldTransform;
										widget.Scale = SceneView.Instance.Scene.Scale;
										widget.RenderChainBuilder.AddToRenderChain(renderChain);
									}
								}
							}
							renderChain.RenderAndClear();
							w.PrepareRendererState();
							foreach (var ruler in ProjectUserPreferences.Instance.Rulers) {
								if (ProjectUserPreferences.Instance.DisplayedRulers.Contains(ruler.Name)) {
									DrawRuler(ruler, w);
								}
							}
							foreach (var ruler in ProjectUserPreferences.Instance.DefaultRulers) {
								if (ProjectUserPreferences.Instance.DisplayedRulers.Contains(ruler.Name)) {
									DrawRuler(ruler, w);
								}
							}
						}
					}));
			sceneView.Scene.CompoundPostPresenter.Push(
				new DelegatePresenter<Widget>(
					(w) => {
						if (!Document.Current.ResolutionPreview.Enabled) {
							return;
						}
						var ctr = SceneView.Instance.Frame;
						var rootNode = Document.Current.RootNode as Widget;
						if (ctr == null || rootNode == null) {
							return;
						}
						ctr.PrepareRendererState();
						var aabb = rootNode.CalcAABBInSpaceOf(ctr);
						var rectangles = new[] {
							new Rectangle(Vector2.Zero, new Vector2(aabb.Left, ctr.Height)).Normalized,
							new Rectangle(new Vector2(aabb.Left, 0), new Vector2(aabb.Right, aabb.Top)).Normalized,
							new Rectangle(new Vector2(aabb.Right, 0), ctr.Size).Normalized,
							new Rectangle(new Vector2(aabb.Left, aabb.Bottom), new Vector2(aabb.Right, ctr.Height)).Normalized
						};
						foreach (var rectangle in rectangles) {
							Renderer.DrawRect(rectangle.A, rectangle.B, ColorTheme.Current.SceneView.ResolutionPreviewOuterSpace);
						}
						const float FontHeight = 16;
						var resolutionDescription = Document.Current.ResolutionPreview.Preset.GetDescription(Document.Current.ResolutionPreview.IsPortrait);
						Renderer.DrawTextLine(aabb.Left, aabb.Top - FontHeight - 2, resolutionDescription, FontHeight, ColorTheme.Current.SceneView.ResolutionPreviewText, 0);
					}
			));
		}

		private void DrawRuler(Ruler ruler, Widget root)
		{
			var t = Document.Current.RootNode.AsWidget.CalcTransitionToSpaceOf(root);
			var size = Document.Current.RootNode.AsWidget.Size / 2;
			foreach (var line in ruler.Lines) {
				if (line.RulerOrientation == RulerOrientation.Vertical) {
					var val = (new Vector2(line.Value + (ruler.AnchorToRoot ? size.X : 0), 0) * t).X;
					Renderer.DrawLine(new Vector2(val, 0), new Vector2(val, root.Size.Y), ColorTheme.Current.SceneView.Ruler);
				} else {
					var val = (new Vector2(0, line.Value + (ruler.AnchorToRoot ? size.Y : 0)) * t).Y;
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
