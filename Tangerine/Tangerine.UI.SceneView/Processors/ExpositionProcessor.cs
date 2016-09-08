using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class ExpositionComponent : IComponent
	{
		public bool InProgress;
	}

	public class ExpositionProcessor : Core.IProcessor
	{
		readonly WidgetInput input;

		public static readonly Key Key = KeyBindings.SceneViewKeys.SceneExposition;
		public static readonly Key MultiSelectKey = KeyBindings.SceneViewKeys.SceneExpositionMultiSelect;

		SceneView sceneView => SceneView.Instance;

		public ExpositionProcessor(WidgetInput input)
		{
			this.input = input;
		}

		public IEnumerator<object> Loop()
		{
			const float animationLength = 0.5f;
			while (true) {
				if (input.ConsumeKeyPress(Key) || input.ConsumeKeyPress(MultiSelectKey)) {
					sceneView.Components.Get<ExpositionComponent>().InProgress = true;
					using (var exposition = new Exposition(sceneView.RootWidget, input)) {
						float t = 0; 
						while (true) {
							if ((input.IsKeyPressed(Key) || input.IsKeyPressed(MultiSelectKey)) && !exposition.Closed()) {
								if (t < animationLength) {
									t += Task.Current.Delta;
									if (t >= animationLength) {
										exposition.RunItemAnimations();
									}
								}
							} else {
								t -= Task.Current.Delta * 3f;
								if (t < 0) {
									break;
								}
							}
							exposition.Morph(CalcMorphKoeff(t, animationLength));
							yield return null;
						}
					}
					sceneView.Components.Get<ExpositionComponent>().InProgress = false;
				}
				yield return null;
			}
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

			public Exposition(Widget root, WidgetInput input)
			{
				canvas = CreateCanvas(root);
				var cellSize = CalcCellSize(root.Size, GetWidgets().Count());
				items = GetWidgets().Select((w, i) => new Item(w, CreateItemFrame(i, canvas, cellSize), input)).ToList();
			}

			static Frame CreateItemFrame(int index, Widget canvas, Vector2 cellSize)
			{
				var rect = GetPlacementRect(index, cellSize, canvas.Size);
				var frame = new Frame { Position = rect.A, Size = cellSize };
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
				canvas.CompoundPresenter.Add(new WidgetFlatFillPresenter(Color4.Black));
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
				canvas.Opacity = morphKoeff * 0.75f;
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
				return Document.Current.Container.Nodes.OfType<Widget>();
			}

			class Item
			{
				readonly Widget originalWidget;
				readonly Widget exposedWidget;
				readonly Transform2 originalTransform;
				readonly Transform2 exposedTransform;
				readonly Vector2 OriginalPivot;
				readonly WidgetBoundsPresenter borderPresenter;
				readonly Frame frame;
				public bool Closed { get; private set; }

				public Item(Widget widget, Frame frame, WidgetInput input)
				{
					this.frame = frame;
					originalWidget = widget;
					exposedWidget = (Widget)widget.Clone();
					exposedWidget.Animations.Clear();
					OriginalPivot = widget.Pivot;
					originalTransform = widget.CalcTransformInSpaceOf(frame);
					exposedTransform = CalcExposedTransform(widget, frame);
					originalWidget.SetTangerineFlag(TangerineFlags.HiddenOnExposition, true);
					var label = new SimpleText { 
						Position = new Vector2(10, 10),
						Color = Colors.SceneView.Label,
						Text = (exposedWidget.Id ?? ""),
						OverflowMode = TextOverflowMode.Ignore
					};
					frame.HitTestTarget = true;
					var clickArea = new Widget { Size = frame.Size, Anchors = Anchors.LeftRightTopBottom, HitTestTarget = true };
					frame.AddNode(clickArea);
					frame.AddNode(label);
					frame.AddNode(exposedWidget);
					borderPresenter = new WidgetBoundsPresenter(Colors.SceneView.ExposedItemInactiveBorder, 1);
					frame.CompoundPresenter.Push(borderPresenter);
					frame.Tasks.AddLoop(() => {
						borderPresenter.Color = Document.Current.EnumerateSelectedNodes().Contains(widget) ? 
							Colors.SceneView.ExposedItemSelectedBorder :
							Colors.SceneView.ExposedItemInactiveBorder;
						if (clickArea.IsMouseOver()) {
							if (Task.Current.LifeTime % 0.5f < 0.25f) {
								borderPresenter.Color = Colors.SceneView.ExposedItemActiveBorder;
							}
							if (clickArea.Input.WasMousePressed()) {
								if (!input.IsKeyPressed(MultiSelectKey)) {
									Core.Operations.ClearRowSelection.Perform();
									Core.Operations.SelectNode.Perform(widget);
									Closed = true;
								} else {
									var isSelected = Document.Current.EnumerateSelectedNodes().Contains(widget);
									Core.Operations.SelectNode.Perform(widget, !isSelected);
								}
							}
						}
					});
				}

				public void RunAnimation()
				{
					exposedWidget.IsRunning = true;
				}

				Transform2 CalcExposedTransform(Widget widget, Frame frame)
				{
					var transform = new Transform2 { Scale = Vector2.One };
					var size = new Vector2(widget.Width.Abs(), widget.Height.Abs());
					if (size.X < float.Epsilon || size.Y < float.Epsilon) {
						return transform;
					}
					if (size.X > frame.Width || size.Y > frame.Height) {
						float scale = (size.X > size.Y) ?
							frame.Width / widget.Width :
							frame.Height / widget.Height;
						transform.Scale *= scale;
					}
					transform.Position = (frame.Size - widget.Size * transform.Scale) / 2;
					return transform;
				}

				public void Dispose()
				{
					originalWidget.SetTangerineFlag(TangerineFlags.HiddenOnExposition, false);
				}

				public void Morph(float morphKoeff)
				{
					var t = Transform2.Lerp(morphKoeff, originalTransform, exposedTransform);
					exposedWidget.Position = t.Position;
					exposedWidget.Scale = t.Scale;
					exposedWidget.Rotation = t.Rotation;
					exposedWidget.Pivot = Vector2.Lerp(morphKoeff, OriginalPivot, Vector2.Zero);
					frame.ClipChildren = morphKoeff >= 0.8f ? ClipMethod.ScissorTest : ClipMethod.None;
				}
			}
		}
	}
}