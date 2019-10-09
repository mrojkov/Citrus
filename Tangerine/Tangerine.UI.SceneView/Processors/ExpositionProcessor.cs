using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class ExpositionProcessor : Core.ITaskProvider
	{
		public static readonly Key MainKey = Key.MapShortcut(Key.Tab);
		public static readonly Key MultiSelectKey = Key.MapShortcut(Modifiers.Control, Key.Tab);

		public IEnumerator<object> Task()
		{
			const float animationLength = 0.5f;
			while (true) {
				var sv = SceneView.Instance;
				if (sv.Input.ConsumeKeyPress(MainKey) || sv.Input.ConsumeKeyPress(MultiSelectKey)) {
					Audio.GloballyEnable = false;
					Document.Current.ExpositionMode = true;
					Document.Current.PreviewScene = true;
					using (var exposition = new Exposition(sv.Frame, sv.Input)) {
						float t = 0;
						float animationSpeed = CalcAnimationSpeed(exposition.ItemCount);
						while (true) {
							if ((sv.Input.IsKeyPressed(MainKey) || sv.Input.IsKeyPressed(MultiSelectKey)) && !exposition.Closed()) {
								if (t < animationLength) {
									t += Lime.Task.Current.Delta * animationSpeed;
									if (t >= animationLength) {
										exposition.RunItemAnimations();
									}
								}
							} else {
								t -= Lime.Task.Current.Delta * 3f * animationSpeed;
								if (t < 0) {
									break;
								}
							}
							exposition.Morph(CalcMorphKoeff(t, animationLength));
							yield return null;
						}
					}
					Audio.GloballyEnable = true;
					Document.Current.ExpositionMode = false;
					Document.Current.PreviewScene = false;
				}
				yield return Lime.Task.WaitForInput();
			}
		}

		static float CalcAnimationSpeed(int itemCount)
		{
			if (itemCount < 5) {
				return 4;
			} else if (itemCount < 20) {
				return 2;
			}
			return 1;
		}

		private static float CalcMorphKoeff(float time, float length)
		{
			return Mathf.Sin(time.Clamp(0, length) / length * Mathf.HalfPi);
		}

		class Exposition : IDisposable
		{
			const float spacing = 20;
			readonly Widget canvas;
			readonly List<Item> items;
			readonly WidgetFlatFillPresenter blackBackgroundPresenter;

			public int ItemCount => items.Count;

			public Exposition(Widget container, WidgetInput input)
			{
				blackBackgroundPresenter = new WidgetFlatFillPresenter(Color4.Transparent);
				canvas = CreateCanvas(container);
				var cellSize = CalcCellSize(container.Size, GetWidgets().Count());
				int itemCount = GetWidgets().Count();
				items = GetWidgets().Select((w, i) => new Item(w, CreateItemFrame(i, canvas, cellSize), input, showLabel: itemCount <= 50)).ToList();
			}

			static Frame CreateItemFrame(int index, Widget canvas, Vector2 cellSize)
			{
				var rect = GetPlacementRect(index, cellSize, canvas.Size);
				var frame = new Frame { Position = new Vector2(rect.A.X.Round(), rect.A.Y.Round()), Size = cellSize };
				canvas.AddNode(frame);
				return frame;
			}

			static Rectangle GetPlacementRect(int index, Vector2 cellSize, Vector2 canvasSize)
			{
				var stride = cellSize + Vector2.One * spacing;
				var cellsPerRow = (canvasSize.X / stride.X).Floor();
				var a = new Vector2(
					(index % cellsPerRow) * stride.X + spacing / 2,
					(index / cellsPerRow) * stride.Y + spacing / 2
				);
				return new Rectangle(a, a + cellSize);
			}

			Widget CreateCanvas(Widget root)
			{
				var canvas = new Widget { HitTestTarget = true, Anchors = Anchors.LeftRightTopBottom, Size = root.Size };
				canvas.CompoundPresenter.Add(blackBackgroundPresenter);
				root.Nodes.Push(canvas);
				return canvas;
			}

			public void Dispose()
			{
				foreach (var i in items) {
					i.Dispose();
				}
				canvas.Unlink();
			}

			public void Morph(float morphKoeff)
			{
				blackBackgroundPresenter.Color = Color4.Black.Transparentify(1 - morphKoeff);
				foreach (var i in items) {
					i.Morph(morphKoeff);
				}
				Window.Current.Invalidate();
			}

			public void RunItemAnimations()
			{
				foreach (var i in items) {
					i.RunAnimation();
				}
			}

			public bool Closed()
			{
				return items.Any(i => i.Closed);
			}

			static Vector2 CalcCellSize(Vector2 canvasSize, int itemCount)
			{
				if (itemCount == 1) {
					return canvasSize - spacing * Vector2.One;
				}
				var size = Vector2.Zero;
				for (float w = 20; w < canvasSize.X; w += 5) {
					float h = w * 0.75f;
					int numRows = (canvasSize.Y / h).Floor();
					int numCols = (canvasSize.X / w).Floor();
					if (numRows * numCols < itemCount) {
						break;
					}
					size = new Vector2(w, h);
				}
				return size - spacing * Vector2.One;
			}

			static IEnumerable<Widget> GetWidgets()
			{
				return Document.Current.Container.Nodes.OfType<Widget>().Where(n => !n.GetTangerineFlag(TangerineFlags.Hidden));
			}

			class Item
			{
				readonly Widget originalWidget;
				readonly Widget exposedWidget;
				readonly Transform2 originalTransform;
				readonly WidgetBoundsPresenter borderPresenter;
				readonly Frame frame;
				readonly SimpleText label;
				private double savedTime;
				public bool Closed { get; private set; }

				public Item(Widget widget, Frame frame, WidgetInput input, bool showLabel)
				{
					this.frame = frame;
					var totalTime = 0d;
					originalWidget = widget;
					exposedWidget = (Widget)widget.Clone();
					exposedWidget.Animations.Clear();
					originalTransform = widget.CalcTransitionToSpaceOf(frame).ToTransform2();
					originalWidget.SetTangerineFlag(TangerineFlags.HiddenOnExposition, true);
					frame.HitTestTarget = true;
					var clickArea = new Widget { Size = frame.Size, Anchors = Anchors.LeftRightTopBottom, HitTestTarget = true };
					frame.AddNode(clickArea);
					label = new ThemedSimpleText {
						Visible = showLabel,
						Position = new Vector2(3, 2),
						Color = ColorTheme.Current.SceneView.Label,
						Text = (exposedWidget.Id ?? ""),
						OverflowMode = TextOverflowMode.Ignore
					};
					frame.AddNode(label);
					frame.AddNode(exposedWidget);
					borderPresenter = new WidgetBoundsPresenter(ColorTheme.Current.SceneView.ExposedItemInactiveBorder, 1);
					frame.CompoundPresenter.Push(borderPresenter);
					int lastFrame = 0;
					foreach (var node in exposedWidget.Nodes) {
						foreach (var animator in node.Animators) {
							foreach (var key in animator.ReadonlyKeys) {
								lastFrame = Math.Max(lastFrame, key.Frame);
							}
						}
					}
					frame.Tasks.AddLoop(() => {
						borderPresenter.Color = Document.Current.SelectedNodes().Contains(widget) ?
							ColorTheme.Current.SceneView.ExposedItemSelectedBorder :
							ColorTheme.Current.SceneView.ExposedItemInactiveBorder;
						totalTime += Lime.Task.Current.Delta;
						if (clickArea.IsMouseOver()) {
							if (totalTime % 0.5f < 0.25f) {
								borderPresenter.Color = ColorTheme.Current.SceneView.ExposedItemActiveBorder;
							}
							label.Visible = true;
							if (clickArea.Input.WasMousePressed()) {
								if (!input.IsKeyPressed(MultiSelectKey)) {
									Document.Current.History.DoTransaction(() => {
										Core.Operations.ClearRowSelection.Perform();
										Core.Operations.SelectNode.Perform(widget);
									});
									Closed = true;
								} else {
									var isSelected = Document.Current.SelectedNodes().Contains(widget);
									Document.Current.History.DoTransaction(() => {
										Core.Operations.SelectNode.Perform(widget, !isSelected);
									});
								}
							}
						} else {
							label.Visible = showLabel;
						}
						if (exposedWidget.DefaultAnimation.Frame <= lastFrame) {
							savedTime = totalTime;
						}
						if (totalTime - savedTime > 1) {
							exposedWidget.DefaultAnimation.Frame = 0;
						}
					});
				}

				public void RunAnimation()
				{
					exposedWidget.DefaultAnimation.IsRunning = true;
				}

				Transform2 CalcExposedTransform(Widget widget, Frame frame)
				{
					var rect = CalcGlobalAABB(widget);
					var t = widget.LocalToWorldTransform.CalcInversed();
					rect.A *= t;
					rect.B *= t;
					var size = new Vector2(widget.Width.Abs(), widget.Height.Abs());
					var transform = new Transform2 { Scale = Vector2.One * Mathf.Min(size.X / rect.Width.Abs(), size.Y / rect.Height.Abs()) };
					if (size.X < float.Epsilon || size.Y < float.Epsilon) {
						return transform;
					}
					if (size.X > frame.Width || size.Y > frame.Height) {
						float scale = (size.X > size.Y) ?
							frame.Width / size.X :
							frame.Height / size.Y;
						transform.Scale *= scale;
					}
					transform.Translation.X = (frame.Size.X - (rect.Width - 2 * rect.AX.Abs()) * transform.Scale.X) / 2;
					transform.Translation.Y = (frame.Size.Y - (rect.Height - 2 * rect.AY.Abs()) * transform.Scale.Y) / 2;
					return transform;
				}

				private static Rectangle CalcGlobalAABB(Node node)
				{
					if (node is PointObject pointObject) {
						var parent = pointObject.Parent.AsWidget;
						var pos = pointObject.CalcPositionInSpaceOf(parent) * parent.LocalToWorldTransform;
						var offset = new Vector2(PointObjectsPresenter.CornerOffset);
						return new Rectangle(pos - offset, pos + offset);
					}
					if (node is Widget widget) {
						var transform = widget.LocalToWorldTransform;
						var result = new Quadrangle {
							V1 = Vector2.Zero * transform,
							V2 = new Vector2(widget.Width, 0) * transform,
							V3 = widget.Size * transform,
							V4 = new Vector2(0, widget.Height) * transform
						}.ToAABB();
						foreach (var childNode in widget.Nodes) {
							var aabb = CalcGlobalAABB(childNode);
							if (aabb.Width + aabb.Height <= Mathf.ZeroTolerance) {
								continue;
							}
							result = Rectangle.Bounds(result, aabb);
						}
						return result;
					}
					return new Rectangle(Vector2.Zero, Vector2.Zero);
				}

				public void Dispose()
				{
					originalWidget.SetTangerineFlag(TangerineFlags.HiddenOnExposition, false);
					// Dispose cloned object to preserve keyframes identity in the original node. See Animator.Dispose().
					exposedWidget.UnlinkAndDispose();
				}

				public void Morph(float morphKoeff)
				{
					var t = Transform2.Lerp(morphKoeff, originalTransform, CalcExposedTransform(exposedWidget, frame));
					exposedWidget.Position = t.Translation;
					exposedWidget.Scale = t.Scale;
					exposedWidget.Rotation = t.Rotation;
					exposedWidget.Pivot = Vector2.Zero;
					frame.ClipChildren = morphKoeff >= 0.8f ? ClipMethod.ScissorTest : ClipMethod.None;
				}
			}
		}
	}
}
