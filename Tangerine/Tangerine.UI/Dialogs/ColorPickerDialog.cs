using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class ColorPickerDialog
	{
		bool result;
		readonly Window window;
		readonly WindowWidget rootWidget;
		readonly Spectrum spectrum;
		readonly Button okButton;
		readonly Button cancelButton;
		readonly ValueSlider valueSlider;
		readonly AlphaSlider alphaSlider;
		ColorHSVA colorHSVA;

		public Color4 Color => colorHSVA.ToRGBA();

		public ColorPickerDialog(Color4 color)
		{
			window = new Window(new WindowOptions {
				FixedSize = true,
				Title = "Colors",
				Visible = false,
				Style = WindowStyle.Dialog
			});
			colorHSVA = ColorHSVA.FromRGBA(color);
			var colorProperty = new Property<ColorHSVA>(() => colorHSVA, c => colorHSVA = c);
			spectrum = new Spectrum(colorProperty);
			valueSlider = new ValueSlider(colorProperty);
			alphaSlider = new AlphaSlider(colorProperty);
			rootWidget = new InvalidableWindowWidget(window) {
				LayoutBasedWindowSize = true,
				Padding = new Thickness(8),
				Layout = new VBoxLayout { Spacing = 8 },
				Nodes = {
					new Widget {
						LayoutCell = new LayoutCell(Alignment.Center),
						Layout = new StackLayout(),
						Nodes = {
							spectrum.Widget
						}
					},
					valueSlider.Widget,
					alphaSlider.Widget,
					new Widget {
						Layout = new HBoxLayout { Spacing = 8 },
						LayoutCell = new LayoutCell(Alignment.RightCenter) { StretchY = 0 },
						Nodes = {
							(okButton = new Button { Text = "Ok" }),
							(cancelButton = new Button { Text = "Cancel" }),
						}
					}
				}
			};
			okButton.Clicked += () => Close(true);
			cancelButton.Clicked += () => Close(false);
			rootWidget.FocusScope = new KeyboardFocusScope(rootWidget);
			okButton.SetFocus();
			rootWidget.Input.KeyPressed += (input, key) => {
				if (key == KeyBindings.CloseDialog) {
					input.ConsumeKey(key);
					Close(false);
				}
			};
		}

		void Close(bool result)
		{
			this.result = result;
			window.Close();
		}

		public bool Show()
		{
			window.ShowModal();
			return result;
		}

		class Spectrum
		{
			readonly Property<ColorHSVA> color;
			Vertex[] triangleFan;

			public const float Radius = 100;
			public readonly Widget Widget;

			public Spectrum(Property<ColorHSVA> color)
			{
				this.color = color;
				Widget = new Widget {
					HitTestTarget = true,
					MinMaxSize = Radius * 2 * Vector2.One,
					PostPresenter = new DelegatePresenter<Widget>(Render)
				};
				Widget.Tasks.Add(SelectTask());
			}

			void Render(Widget widget)
			{
				widget.PrepareRendererState();
				DrawSpectrumCircle();
				DrawCross();
			}

			void DrawCross()
			{
				var cursor = Radius * (color.Value.S * Vector2.CosSin(Mathf.DegToRad * color.Value.H) + Vector2.One);
				Renderer.DrawLine(cursor + new Vector2(-10, 0), cursor + new Vector2(10, 0), Color4.Black);
				Renderer.DrawLine(cursor + new Vector2(0, -10), cursor + new Vector2(0, 10), Color4.Black);
			}

			void DrawSpectrumCircle()
			{
				triangleFan = triangleFan ?? new Vertex[60];
				triangleFan[0] = new Vertex { Color = Color4.White.Darken(1 - color.Value.V), Pos = Vector2.One * Radius };
				for (int i = 0; i < triangleFan.Length - 1; i++) {
					float t = (float)i / (triangleFan.Length - 2);
					triangleFan[i + 1] = new Vertex {
						Color = new ColorHSVA(360 * t, 1, color.Value.V, 1).ToRGBA(),
						Pos = Radius * (Vector2.CosSin(t * Mathf.TwoPi) + Vector2.One)
					};
				}
				Renderer.DrawTriangleFan(null, triangleFan, triangleFan.Length);
			}

			IEnumerator<object> SelectTask()
			{
				while (true) {
					if (Widget.Input.WasMousePressed() && HitTest(Widget.Input.MousePosition)) {
						Widget.Input.CaptureMouse();
						while (Widget.Input.IsMousePressed()) {
							var c = color.Value;
							PositionToHueSaturation(Widget.Input.MousePosition - Widget.GlobalCenter, out c.H, out c.S);
							color.Value = c;
							Window.Current.Invalidate();
							yield return null;
						}
						Widget.Input.ReleaseMouse();
					}
					yield return null;
				}
			}

			static void PositionToHueSaturation(Vector2 pos, out float hue, out float saturation)
			{
				hue = Mathf.Atan2(pos) * 180 / Mathf.Pi;
				if (hue < 0) {
					hue += 360;
				}
				saturation = Mathf.Min(1, pos.Length / Radius);
			}

			bool HitTest(Vector2 pos)
			{
				return (pos - Widget.GlobalCenter).Length < Radius;
			}
		}

		class ValueSlider
		{
			public readonly Slider Widget;

			public ValueSlider(Property<ColorHSVA> color)
			{
				Widget = new Slider { RangeMin = 0, RangeMax = 1, Value = 1 - color.Value.V };
				Widget.Changed += () => {
					color.Value = new ColorHSVA(color.Value.H, color.Value.S, 1 - Widget.Value, color.Value.A);
				};
				var presenter = new BackgroundPresenter(color);
				Widget.CompoundPresenter.Insert(0, presenter);
			}

			class BackgroundPresenter : CustomPresenter
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
					Renderer.DrawHorizontalGradientRect(Vector2.Zero, widget.Size, GetGradient());
					Renderer.DrawRectOutline(Vector2.Zero, widget.Size, DesktopTheme.Colors.ControlBorder);
				}

				ColorGradient GetGradient()
				{
					return new ColorGradient(
						new ColorHSVA(color.Value.H, color.Value.S, 1, 1).ToRGBA(),
						new ColorHSVA(color.Value.H, color.Value.S, 0, 1).ToRGBA());
				}
			}
		}

		class AlphaSlider
		{
			public readonly Slider Widget;

			public AlphaSlider(Property<ColorHSVA> color)
			{
				Widget = new Slider { RangeMin = 0, RangeMax = 1, Value = 1 - color.Value.A };
				Widget.Changed += () => {
					color.Value = new ColorHSVA(color.Value.H, color.Value.S, color.Value.V, 1 - Widget.Value);
				};
				var presenter = new BackgroundPresenter(color);
				Widget.CompoundPresenter.Insert(0, presenter);
			}

			class BackgroundPresenter : CustomPresenter
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
					Renderer.DrawRect(Vector2.Zero, widget.Size, Color4.White);
					int numChecks = 20;
					var checkSize = new Vector2(widget.Width / numChecks, widget.Height / 2);
					for (int i = 0; i < numChecks; i++) {
						var checkPos = new Vector2(i * checkSize.X, (i % 2 == 0) ? 0 : checkSize.Y);
						Renderer.DrawRect(checkPos, checkPos + checkSize, Color4.Black);
					}
					Renderer.DrawRect(Vector2.Zero, widget.Size, color.Value.ToRGBA());
					Renderer.DrawRectOutline(Vector2.Zero, widget.Size, DesktopTheme.Colors.ControlBorder);
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