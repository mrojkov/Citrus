using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.UI.SceneView
{
	public class ZoomWidget : ThemedFrame
	{
		public const int FrameHeight = 24;

		private readonly Slider slider;
		private readonly Button zoomInButton;
		private readonly Button zoomOutButton;
		private readonly SimpleText zoomLabel;
		SceneView sv => SceneView.Instance;

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
					slider.Value = (slider.Value + slider.Step).Clamp(slider.RangeMin, slider.RangeMax);
					Zoom();
				},
				Texture = IconPool.GetTexture("SceneView.ZoomIn"),
			};
			zoomOutButton = new ToolbarButton {
				MinMaxSize = new Vector2(FrameHeight),
				Size = new Vector2(FrameHeight),
				LayoutCell = new LayoutCell(Alignment.RightCenter),
				Anchors = Anchors.Right,
				Clicked = () => {
					slider.Value = (slider.Value - slider.Step).Clamp(slider.RangeMin, slider.RangeMax);
					Zoom();
				},
				Texture = IconPool.GetTexture("SceneView.ZoomOut"),
			};

			zoomLabel = new ThemedSimpleText {
				LayoutCell = new LayoutCell(Alignment.RightCenter),
				Anchors = Anchors.Right
			};

			slider.Updating += delta => {
				int index = FindNearest(sv.Scene.Scale.X, 0, zoomTable.Count);
				slider.Value = index;
				zoomLabel.Text = String.Format("{0:P1}", zoomTable[index]);
			};
			slider.Changed += () => Zoom();
			AddNode(new Widget { LayoutCell = new LayoutCell(Alignment.LeftCenter, 1) });
			AddNode(zoomLabel);
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

		void Zoom()
		{
			var prevZoom = sv.Scene.Scale.X;
			var zoom = zoomTable[(int)(slider.Value).Clamp(0, zoomTable.Count - 1)];

			var p = (sv.Frame.Size / 2 - sv.Scene.Position) / sv.Scene.Scale.X;
			sv.Scene.Scale = zoom * Vector2.One;
			sv.Scene.Position -= p * (zoom - prevZoom);
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
