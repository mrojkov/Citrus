﻿using System;
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
				readonly Transform2 exposedTransform;
				readonly WidgetBoundsPresenter borderPresenter;
				readonly Frame frame;
				readonly SimpleText label;
				public bool Closed { get; private set; }

				public Item(Widget widget, Frame frame, WidgetInput input, bool showLabel)
				{
					this.frame = frame;
					var totalTime = 0d;
					originalWidget = widget;
					exposedWidget = (Widget)widget.Clone();
					exposedWidget.Animations.Clear();
					originalTransform = widget.CalcTransitionToSpaceOf(frame).ToTransform2();
					exposedTransform = CalcExposedTransform(widget, frame);
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
					label.FontHeight *= 0.75f;
					frame.AddNode(label);
					frame.AddNode(exposedWidget);
					borderPresenter = new WidgetBoundsPresenter(ColorTheme.Current.SceneView.ExposedItemInactiveBorder, 1);
					frame.CompoundPresenter.Push(borderPresenter);
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
					transform.Translation = (frame.Size - widget.Size * transform.Scale) / 2;
					return transform;
				}

				public void Dispose()
				{
					originalWidget.SetTangerineFlag(TangerineFlags.HiddenOnExposition, false);
					// Dispose cloned object to preserve keyframes identity in the original node. See Animator.Dispose().
					exposedWidget.UnlinkAndDispose();
				}

				public void Morph(float morphKoeff)
				{
					var t = Transform2.Lerp(morphKoeff, originalTransform, exposedTransform);
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