using Lime;
using System.Collections.Generic;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class ZoomWidget : ThemedFrame
	{
		public const int FrameHeight = 24;

		private readonly Slider slider;
		private readonly Button zoomInButton;
		private readonly Button zoomOutButton;
		private readonly ThemedEditBox zoomEditor;
		SceneView sv => SceneView.Instance;
		private float currentSliderValue => zoomTable[(int)(slider.Value).Clamp(0, zoomTable.Count - 1)];

		public ZoomWidget()
		{
			MinMaxHeight = FrameHeight;
			LayoutCell = new LayoutCell(new Alignment { X = HAlignment.Center, Y = VAlignment.Bottom }, 1, 0);
			Layout = new HBoxLayout { Spacing = 8 };
			Padding = new Thickness(10, 0);
			Anchors = Anchors.LeftRight | Anchors.Bottom;
			HitTestTarget = true;

			slider = new ThemedSlider {
				MinMaxSize = new Vector2(100, 2),
				Size = new Vector2(100, 2),
				Y = FrameHeight / 2,
				LayoutCell = new LayoutCell(Alignment.RightCenter, 1),
				Anchors = Anchors.Right,
				RangeMin = 0,
				RangeMax = zoomTable.Count - 1,
				Step = 1
			};
			slider.CompoundPresenter.Add(new SliderCenterPresenter(FindNearest(1f, 0, zoomTable.Count), zoomTable.Count));

			zoomInButton = new ToolbarButton {
				MinMaxSize = new Vector2(FrameHeight),
				Size = new Vector2(FrameHeight),
				LayoutCell = new LayoutCell(Alignment.RightCenter),
				Anchors = Anchors.Right,
				Clicked = () => {
					if (currentSliderValue <= sv.Scene.Scale.X) {
						slider.Value = (slider.Value + slider.Step).Clamp(slider.RangeMin, slider.RangeMax);
					}
					Zoom(currentSliderValue);
				},
				Texture = IconPool.GetTexture("SceneView.ZoomIn"),
			};
			zoomOutButton = new ToolbarButton {
				MinMaxSize = new Vector2(FrameHeight),
				Size = new Vector2(FrameHeight),
				LayoutCell = new LayoutCell(Alignment.RightCenter),
				Anchors = Anchors.Right,
				Clicked = () => {
					if (currentSliderValue >= sv.Scene.Scale.X) {
						slider.Value = (slider.Value - slider.Step).Clamp(slider.RangeMin, slider.RangeMax);
					}
					Zoom(currentSliderValue);
				},
				Texture = IconPool.GetTexture("SceneView.ZoomOut"),
			};

			zoomEditor = new ThemedEditBox {
				LayoutCell = new LayoutCell(Alignment.RightCenter),
				Anchors = Anchors.Right,
				MinMaxWidth = 50
			};
			zoomEditor.Submitted += value => {
				bool success = float.TryParse(value.TrimEnd('%'), out float zoom);
				if (success) {
					Zoom(zoom / 100);
				}
			};

			this.AddChangeWatcher(() => sv.Scene.Scale.X, value => {
				int index = FindNearest(value, 0, zoomTable.Count);
				slider.Value = index;
				zoomEditor.Text = (value * 100f).ToString() + "%";
			});
			slider.Changed += () => Zoom(currentSliderValue);
			AddNode(new Widget { LayoutCell = new LayoutCell(Alignment.LeftCenter, 1) });
			AddNode(zoomEditor);
			AddNode(zoomOutButton);
			AddNode(slider);
			AddNode(zoomInButton);
		}

		readonly List<float> zoomTable = new List<float> {
			0.001f, 0.0025f, 0.005f, 0.01f, 0.025f, 0.05f, 0.10f,
			0.15f, 0.25f, 0.5f, 0.75f, 1f, 1.5f, 2f, 3f,
			4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f,
			12f, 13f, 14f, 15f, 16f
		};

		void Zoom(float newZoom)
		{
			var prevZoom = sv.Scene.Scale.X;
			var p = (sv.Frame.Size / 2 - sv.Scene.Position) / sv.Scene.Scale.X;
			sv.Scene.Scale = newZoom * Vector2.One;
			sv.Scene.Position -= p * (newZoom - prevZoom);
		}

		private int FindNearest(float x, int left, int right)
		{
			if (right - left == 1) {
				return left;
			}
			var idx = left + (right - left) / 2;
			return x < zoomTable[idx] ? FindNearest(x, left, idx) : FindNearest(x, idx, right);
		}

		class SliderCenterPresenter : CustomPresenter
		{
			private readonly int middleIndex;
			private readonly int partsCount;

			public SliderCenterPresenter(int middleIndex, int parts)
			{
				this.middleIndex = middleIndex;
				partsCount = parts;
			}

			public override void Render(Node node)
			{
				var widget = node.AsWidget;
				widget.PrepareRendererState();
				float xPos = (widget.Width / partsCount) * middleIndex;
				Renderer.DrawRect(new Vector2(xPos - 1f, -5), new Vector2(xPos + 1f, 7), Theme.Colors.ControlBorder);
			}
		}
	}
}
