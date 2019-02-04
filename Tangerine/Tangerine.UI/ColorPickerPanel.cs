using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class ColorPickerPanel
	{
		readonly TriangleColorWheel colorWheel;
		readonly AlphaSlider alphaSlider;
		public readonly Widget Widget;
		ColorHSVA colorHSVA;

		public event Action DragStarted;
		public event Action DragEnded;
		public event Action Changed;

		public Color4 Color
		{
			get { return colorHSVA.ToRGBA(); }
			set { colorHSVA = ColorHSVA.FromRGBA(value); }
		}

		public ColorPickerPanel()
		{
			var colorProperty = new Property<ColorHSVA>(() => colorHSVA, c => colorHSVA = c);
			colorWheel = new TriangleColorWheel(colorProperty);
			colorWheel.DragStarted += () => DragStarted?.Invoke();
			colorWheel.Changed += () => Changed?.Invoke();
			colorWheel.DragEnded += () => DragEnded?.Invoke();
			alphaSlider = new AlphaSlider(colorProperty);
			SetupSliderDragHandlers(alphaSlider.Widget);
			Widget = new Widget {
				Padding = new Thickness(8),
				Layout = new VBoxLayout { Spacing = 8 },
				Nodes = {
					new Widget {
						LayoutCell = new LayoutCell(Alignment.Center),
						Layout = new StackLayout(),
						Nodes = {
							colorWheel.Widget
						}
					},
					alphaSlider.Widget,
				}
			};
			Widget.FocusScope = new KeyboardFocusScope(Widget);
		}

		void SetupSliderDragHandlers(Slider slider)
		{
			slider.DragStarted += () => DragStarted?.Invoke();
			slider.DragEnded += () => DragEnded?.Invoke();
			slider.Changed += () => Changed?.Invoke();
		}

		class TriangleColorWheel
		{
			private readonly Property<ColorHSVA> color;

			public const float InnerRadius = 100;
			public const float OuterRadius = 120;
			public const float Margin = 1.05f;
			public readonly Widget Widget;

			public event Action DragStarted;
			public event Action DragEnded;
			public event Action Changed;

			private const float CenterX = OuterRadius;
			private const float CenterY = OuterRadius;
			private const float CursorRadius = (OuterRadius - InnerRadius * Margin) / 2;

			private bool wasHueChanged = true;

			public TriangleColorWheel(Property<ColorHSVA> color)
			{
				this.color = color;
				Widget = new Widget {
					HitTestTarget = true,
					MinMaxSize = OuterRadius * 2 * Vector2.One,
					PostPresenter = new SyncDelegatePresenter<Widget>(Render)
				};
				Widget.Tasks.Add(SelectTask());
				Widget.AddChangeWatcher(() => color.Value.H, _ => wasHueChanged = true);
			}

			void Render(Widget widget)
			{
				widget.PrepareRendererState();
				DrawControl();
				DrawTriangleCursor();
				DrawWheelCursor();
			}

			void DrawTriangleCursor()
			{
				var cursor = new Vector2(
					CenterX - InnerRadius * (1 - 3 * color.Value.S * color.Value.V) / 2,
					CenterY + InnerRadius * Mathf.Sqrt(3) *
					(color.Value.S * color.Value.V - 2 * color.Value.V + 1) / 2
				);
				Renderer.DrawCircle(cursor, CursorRadius, 20, Color4.Black);
			}

			void DrawWheelCursor()
			{
				var cursor =
					Vector2.CosSin(color.Value.H * Mathf.DegToRad) *
					(InnerRadius * Margin + OuterRadius) / 2 +
					Vector2.One * new Vector2(CenterX, CenterY);
				Renderer.DrawCircle(cursor, CursorRadius, 20, Color4.Black);
			}

			private Texture2D texture = new Texture2D();
			private Color4[] image;

			void DrawControl()
			{
				int size = (int)Math.Floor(OuterRadius * 2);
				if (wasHueChanged) {
					if (image == null) {
						image = new Color4[size * size];
					}
					for (int y = 0; y < size; ++y) {
						for (int x = 0; x < size; ++x) {
							var pick = Pick(size - x - 1, y);
							if (pick.Area == Area.Outside) {
								image[y * size + x] = Color4.Transparent;
							} else if (pick.Area == Area.Wheel) {
								image[y * size + x] = new ColorHSVA(pick.H.Value, 1, 1).ToRGBA();
							} else {
								image[y * size + x] = new ColorHSVA(color.Value.H, pick.S.Value, pick.V.Value).ToRGBA();
							}
						}
					}
					texture.LoadImage(image, size, size);
					wasHueChanged = false;
				}
				Renderer.DrawSprite(texture, Color4.White, Vector2.Zero, Vector2.One * size, new Vector2(1, 0), new Vector2(0, 1));
			}

			enum Area
			{
				Outside,
				Wheel,
				Triangle
			}

			struct Result
			{
				public Area Area { get; set; }
				public float? H { get; set; }
				public float? S { get; set; }
				public float? V { get; set; }
			}

			private void ShiftedCoordinates(float x, float y, out float nx, out float ny)
			{
				nx = x - CenterX;
				ny = y - CenterY;
			}

			private static Result PositionToHue(float nx, float ny)
			{
				float angle = Mathf.Atan2(ny, nx);
				if (angle < 0) {
					angle += Mathf.TwoPi;
				}
				return new Result { Area = Area.Wheel, H = angle / Mathf.DegToRad };
			}

			private static Result PositionToSV(float nx, float ny, bool ignoreBounds = false)
			{
				float sqrt3 = Mathf.Sqrt(3);
				float x1 = -ny / InnerRadius;
				float y1 = -nx / InnerRadius;
				if (
					!ignoreBounds && (
					0 * x1 + 2 * y1 > 1 ||
					sqrt3 * x1 + (-1) * y1 > 1 ||
					-sqrt3 * x1 + (-1) * y1 > 1)
				) {
					return new Result { Area = Area.Outside };
				}
				else {
					var sat = (1 - 2 * y1) / (sqrt3 * x1 - y1 + 2);
					var val = (sqrt3 * x1 - y1 + 2) / 3;
					return new Result { Area = Area.Triangle, S = sat, V = val };
				}
			}

			private Result Pick(float x, float y)
			{
				float nx, ny;
				ShiftedCoordinates(x, y, out nx, out ny);
				float centerDistance = Mathf.Sqrt(nx * nx + ny * ny);
				if (centerDistance > OuterRadius) {
					return new Result { Area = Area.Outside };
				}
				else if (centerDistance > InnerRadius * Margin) {
					return PositionToHue(nx, ny);
				}
				else {
					return PositionToSV(nx, ny);
				}
			}

			IEnumerator<object> SelectTask()
			{
				while (true) {
					if (Widget.GloballyEnabled && Widget.Input.WasMousePressed()) {
						var pick = Pick(
							Widget.Input.MousePosition.X - Widget.GlobalPosition.X,
							Widget.Input.MousePosition.Y - Widget.GlobalPosition.Y);
						if (pick.Area != Area.Outside) {
							DragStarted?.Invoke();
							while (Widget.Input.IsMousePressed()) {
								float nx, ny;
								ShiftedCoordinates(
									Widget.Input.MousePosition.X - Widget.GlobalPosition.X,
									Widget.Input.MousePosition.Y - Widget.GlobalPosition.Y,
									out nx, out ny);
								if (pick.Area == Area.Triangle) {
									var newPick = PositionToSV(nx, ny, ignoreBounds: true);
									color.Value = new ColorHSVA {
										H = color.Value.H,
										S = Mathf.Min(Mathf.Max(newPick.S.Value, 0), 1),
										V = Mathf.Min(Mathf.Max(newPick.V.Value, 0), 1),
										A = color.Value.A
									};
								}
								else {
									var newPick = PositionToHue(nx, ny);
									color.Value = new ColorHSVA {
										H = Mathf.Min(Mathf.Max(newPick.H.Value, 0), 360),
										S = color.Value.S,
										V = color.Value.V,
										A = color.Value.A
									};
								}
								Window.Current.Invalidate();
								Changed?.Invoke();
								yield return null;
							}
							DragEnded?.Invoke();
						}
					}
					yield return null;
				}
			}
		}

		class AlphaSlider
		{
			public readonly Slider Widget;

			public AlphaSlider(Property<ColorHSVA> color)
			{
				Widget = new ThemedSlider { RangeMin = 0, RangeMax = 1 };
				Widget.Changed += () => {
					color.Value = new ColorHSVA(color.Value.H, color.Value.S, color.Value.V, 1 - Widget.Value);
				};
				Widget.Updating += delta => Widget.Value = 1 - color.Value.A;
				var presenter = new BackgroundPresenter(color);
				Widget.CompoundPresenter.Insert(0, presenter);
			}

			class BackgroundPresenter : SyncCustomPresenter
			{
				readonly Property<ColorHSVA> color;

				public BackgroundPresenter(Property<ColorHSVA> color)
				{
					this.color = color;
				}

				public override void Render(Node node)
				{
					var widget = node.AsWidget;
					widget.PrepareRendererState();
					RendererWrapper.Current.DrawRect(Vector2.Zero, widget.Size, Color4.White);
					int numChecks = 20;
					var checkSize = new Vector2(widget.Width / numChecks, widget.Height / 2);
					for (int i = 0; i < numChecks; i++) {
						var checkPos = new Vector2(i * checkSize.X, (i % 2 == 0) ? 0 : checkSize.Y);
						Renderer.DrawRect(checkPos, checkPos + checkSize, Color4.Black);
					}
					RendererWrapper.Current.DrawRect(Vector2.Zero, widget.Size, color.Value.ToRGBA());
					RendererWrapper.Current.DrawRectOutline(Vector2.Zero, widget.Size, Theme.Colors.ControlBorder);
				}
			}
		}

		struct ColorHSVA
		{
			public float H;
			public float S;
			public float V;
			public float A;

			public ColorHSVA(float hue, float saturation, float value, float alpha = 1)
			{
				H = hue;
				S = saturation;
				V = value;
				A = alpha;
			}

			public static ColorHSVA FromRGBA(Color4 rgb)
			{
				var c = new ColorHSVA();
				int max = Math.Max(rgb.R, Math.Max(rgb.G, rgb.B));
				int min = Math.Min(rgb.R, Math.Min(rgb.G, rgb.B));
				c.H = GetHue(rgb);
				c.S = (max == 0) ? 0 : 1f - (1f * min / max);
				c.V = max / 255f;
				c.A = rgb.A / 255f;
				return c;
			}

			public Color4 ToRGBA()
			{
				var a = (byte)(A * 255);
				int hi = Convert.ToInt32(Math.Floor(H / 60)) % 6;
				float f = H / 60f - (float)Math.Floor(H / 60);
				byte v = Convert.ToByte(V * 255);
				byte p = Convert.ToByte(v * (1 - S));
				byte q = Convert.ToByte(v * (1 - f * S));
				byte t = Convert.ToByte(v * (1 - (1 - f) * S));
				switch (hi) {
					case 0:
						return new Color4(v, t, p, a);
					case 1:
						return new Color4(q, v, p, a);
					case 2:
						return new Color4(p, v, t, a);
					case 3:
						return new Color4(p, q, v, a);
					case 4:
						return new Color4(t, p, v, a);
					default:
						return new Color4(v, p, q, a);
				}
			}

			private static float GetHue(Color4 rgb)
			{
				int r = rgb.R;
				int g = rgb.G;
				int b = rgb.B;
				byte minval = (byte)Math.Min(r, Math.Min(g, b));
				byte maxval = (byte)Math.Max(r, Math.Max(g, b));
				if (maxval == minval) {
					return 0.0f;
				}
				float diff = (float)(maxval - minval);
				float rnorm = (maxval - r) / diff;
				float gnorm = (maxval - g) / diff;
				float bnorm = (maxval - b) / diff;
				float hue = 0.0f;
				if (r == maxval)
					hue = 60.0f * (6.0f + bnorm - gnorm);
				if (g == maxval)
					hue = 60.0f * (2.0f + rnorm - bnorm);
				if (b  == maxval)
					hue = 60.0f * (4.0f + gnorm - rnorm);
				if (hue >= 360.0f)
					hue = hue - 360.0f;
				return hue;
			}
		}
	}
}
