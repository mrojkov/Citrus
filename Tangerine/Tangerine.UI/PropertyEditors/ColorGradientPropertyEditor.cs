using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class ColorGradientPropertyEditor : ExpandablePropertyEditor<ColorGradient>
	{
		private const float gradientPaneHeight = 35f;
		private readonly GradientControlWidget gradientControlWidget;
		private readonly ColorPickerPanel colorPanel;
		private Property<Color4> selectedColorProperty;
		private Property<float> selectedPositionProperty;
		private readonly EditBox colorEditor;
		private readonly NumericEditBox positionEditor;
		private readonly Widget positionPropertyContainer;
		private IDataflowProvider<string> currentColorString;
		private GradientControlPoint selectedControlPoint;
		private bool isColorFromPanel;
		public bool PropertyWasChanged { get; private set; }

		public ColorGradientPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			gradientControlWidget = new GradientControlWidget {
				MinMaxHeight = gradientPaneHeight,
				Height = gradientPaneHeight,
				LayoutCell = new LayoutCell(Alignment.LeftCenter),
				Padding = new Thickness { Right = 5f, Bottom = 5f}
			};
			var gradientProperty = CoalescedPropertyValue(new ColorGradient(Color4.White, Color4.Black)).DistinctUntilChanged();
			gradientControlWidget.Gradient = gradientProperty.GetValue();
			ContainerWidget.Tasks.Add(gradientProperty.Consume(g => {
				gradientControlWidget.Gradient = g;
			}));
			gradientControlWidget.SelectionChanged += SelectPoint;
			ContainerWidget.AddNode(gradientControlWidget);
			ContainerWidget.AddNode(CreatePipetteButton());
			ExpandableContent.Padding = new Thickness { Left = 25f, Right = 25f, Top = 5f };
			ExpandableContent.AddNode(positionPropertyContainer = new Widget {
				Layout = new HBoxLayout { Spacing = 10f },
				Nodes = {
					new ThemedSimpleText { Text = "Position", MinWidth = 150 },
					(positionEditor = EditorParams.NumericEditBoxFactory())
				},
				Padding = new Thickness(0, 3f)
			});
			ExpandableContent.AddNode(new Widget {
				Layout = new HBoxLayout { Spacing = 10f },
				Nodes = {
					new ThemedSimpleText { Text = "Color", MinWidth = 150 },
					(colorEditor = EditorParams.EditBoxFactory())
				},
				Padding = new Thickness(0, 3f)
			});
			positionEditor.Step = 0.01f;
			colorEditor.Submitted += SetColor;
			positionEditor.Submitted += SetPosition;
			gradientControlWidget.ControlPointCreated += point => {
				if (!InitializePropertyIfNecessary()) {
					foreach (var o in EditorParams.Objects) {
						Core.Operations.InsertListItem.Perform(point.Clone(), GetGradientProperty(o), 1);
					}
				}
			};
			gradientControlWidget.ControlPointRemoved += index => {
				if (!InitializePropertyIfNecessary()) {
					foreach (var o in EditorParams.Objects) {
						Core.Operations.RemoveListItem.Perform(index, GetGradientProperty(o));
					}
				}
			};
			gradientControlWidget.ControlPointMoved += (newPos, index) => {
				if (!InitializePropertyIfNecessary()) {
					foreach (var o in EditorParams.Objects) {
						Core.Operations.SetProperty.Perform(GetGradientProperty(o)[index], nameof(GradientControlPoint.Position), newPos);
					}
				}
			};
			colorPanel = new ColorPickerPanel();
			ExpandableContent.AddNode(colorPanel.Widget);
			colorPanel.Widget.Padding.Right = 12;
			colorPanel.DragStarted += () => EditorParams.History?.BeginTransaction();
			colorPanel.DragEnded += () => {
				EditorParams.History?.CommitTransaction();
				EditorParams.History?.EndTransaction();
			};
			colorPanel.Changed += () => {
				EditorParams.History?.RollbackTransaction();
				isColorFromPanel = true;
				InitializePropertyIfNecessary();
				Core.Operations.SetProperty.Perform(selectedControlPoint, nameof(GradientControlPoint.Color), colorPanel.Color);
				SetControlPointProperty(nameof(GradientControlPoint.Color), colorPanel.Color);
			};
			SelectPoint(gradientControlWidget.SelectedControlPoint, true);
		}

		protected override void ResetToDefault()
		{
			var gradientProperty = CoalescedPropertyValue(new ColorGradient(Color4.White, Color4.Black)).DistinctUntilChanged();
			var defaultValue = EditorParams.DefaultValueGetter();
			if (defaultValue != null) {
				DoTransaction(() => {
					foreach (var o in EditorParams.Objects) {
						EditorParams.PropertySetter(o, EditorParams.PropertyName, defaultValue);
					}
					Core.Operations.SetProperty.Perform(
						gradientControlWidget, nameof(GradientControlWidget.Gradient), gradientProperty.GetValue());
				});
			}
			gradientControlWidget.Tasks.Clear();
		}

		private ColorGradient GetGradientProperty(object o)
		{
			return (ColorGradient)o.GetType().GetProperty(EditorParams.PropertyName).GetValue(o);
		}

		private void SetControlPointProperty(string propertyName, object value)
		{
			foreach (var o in EditorParams.Objects) {
				var idx = gradientControlWidget.Gradient.IndexOf(selectedControlPoint);
				Core.Operations.SetProperty.Perform(GetGradientProperty(o)[idx], propertyName, value);
			}
		}

		private Node CreatePipetteButton()
		{
			var button = new ToolbarButton {
				Texture = IconPool.GetTexture("Tools.Pipette"),
			};
			button.Tasks.Add(UIProcessors.PickColorProcessor(
				button, v => SetControlPointProperty(nameof(GradientControlPoint.Color), v)));
			return button;
		}

		public void SetColor(string text)
		{
			if (Color4.TryParse(text, out var newColor)) {
				Document.Current.History.DoTransaction(() => {
					InitializePropertyIfNecessary();
					Core.Operations.SetProperty.Perform(selectedControlPoint, nameof(GradientControlPoint.Color), newColor);
					SetControlPointProperty(nameof(GradientControlPoint.Color), newColor);
				});
			} else {
				colorEditor.Text = currentColorString.GetValue();
			}
		}

		public void SetPosition(string text)
		{
			if (float.TryParse(text, out var newPosition)) {
				Document.Current.History.DoTransaction(() => {
					newPosition = Mathf.Clamp(newPosition, 0, 1);
					InitializePropertyIfNecessary();
					Core.Operations.SetProperty.Perform(selectedControlPoint, nameof(GradientControlPoint.Position),
						newPosition);
					SetControlPointProperty(nameof(GradientControlPoint.Position), newPosition);
				});
			}
			positionEditor.Text = selectedPositionProperty.GetValue().ToString();
		}

		private void SelectPoint(GradientControlPoint point, bool isBorderPoint)
		{
			selectedControlPoint = point;
			ContainerWidget.Tasks.Clear();
			colorEditor.Tasks.Clear();
			positionEditor.Tasks.Clear();
			selectedColorProperty = new Property<Color4>(selectedControlPoint, nameof(GradientControlPoint.Color));
			selectedPositionProperty = new Property<float>(selectedControlPoint, nameof(GradientControlPoint.Position));
			currentColorString = selectedColorProperty.DistinctUntilChanged().Select(i => i.ToString(Color4.StringPresentation.Dec));
			colorEditor.Tasks.Add(currentColorString.Consume(v => colorEditor.Text = v));
			colorPanel.Color = selectedControlPoint.Color;
			colorEditor.Tasks.Add(selectedColorProperty.DistinctUntilChanged().Consume(v => {
				if (!isColorFromPanel) {
					colorPanel.Color = v;
					isColorFromPanel = false;
				}
			}));
			positionEditor.Tasks.Add(selectedPositionProperty.DistinctUntilChanged().Consume(v => positionEditor.Text = v.ToString()));
			positionPropertyContainer.Visible = !isBorderPoint;
		}

		private bool InitializePropertyIfNecessary()
		{
			if (!PropertyWasChanged) {
				Core.Operations.SetProperty.Perform(this, nameof(PropertyWasChanged), true);
				foreach (var o in EditorParams.Objects) {
					Core.Operations.SetProperty.Perform(o, EditorParams.PropertyName,
						gradientControlWidget.Gradient.Clone());
				}
				return true;
			}
			return false;
		}
	}

	public class GradientControlWidget : Widget
	{
		private readonly Widget gradientPaneContainer;
		private readonly Widget createPointsPane;
		private readonly Image gradientPane;
		private readonly GradientMaterialComponent gradientMaterialComponent;
		private GradientControlPointWidget selectedControlPointWidget;
		public GradientControlPoint SelectedControlPoint { get; private set; }

		private ColorGradient gradient;
		public ColorGradient Gradient
		{
			get => gradient;
			set {
				if (value == null) {
					throw new ArgumentNullException();
				}
				if (gradient != value) {
					gradient = value;
					Rebuild();
				}
			}
		}

		public event Action<GradientControlPoint, bool> SelectionChanged;
		public event Action<GradientControlPoint> ControlPointCreated;
		public event Action<int> ControlPointRemoved;
		public event Action<float, int> ControlPointMoved;

		private void Rebuild()
		{
			gradientMaterialComponent.Gradient = gradient;
			gradientPaneContainer.Nodes.Clear();
			gradientPaneContainer.Nodes.Add(gradientPane);
			gradientPane.ExpandToContainerWithAnchors();
			InitializePoints(gradientPaneContainer);
		}

		public static ITexture PrepareChessTexture(Color4 color1, Color4 color2)
		{
			var chessTexture = new Texture2D();
			chessTexture.LoadImage(new[] { color1, color2, color2, color1 }, 2, 2);
			chessTexture.TextureParams = new TextureParams {
				WrapMode = TextureWrapMode.Repeat,
				MinMagFilter = TextureFilter.Nearest,
			};
			return chessTexture;
		}

		public GradientControlWidget()
		{
			gradientPane = new Image {
				PostPresenter = new WidgetBoundsPresenter(Color4.Black),
			};
			var chessTexture = PrepareChessTexture(Color4.White, Color4.Black);
			gradientPane.CompoundPresenter.Add(new DelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				Renderer.DrawSprite(chessTexture, Color4.White, Vector2.Zero, w.Size, Vector2.Zero, new Vector2(w.Size.X / w.Size.Y, 1));
			}));
			gradientMaterialComponent = new GradientMaterialComponent();
			gradientPane.Components.Add(gradientMaterialComponent);

			gradientPaneContainer = new Widget {
				LayoutCell = new LayoutCell(Alignment.LeftCenter, 1, 0.7f),
			};
			createPointsPane = new Widget {
				LayoutCell = new LayoutCell(Alignment.LeftTop, 1, 0.3f),
				PostPresenter = new WidgetFlatFillPresenter(Color4.Gray.Lighten(0.5f)),
				HitTestTarget = true,
			};
			var clickGesture = new ClickGesture();
			createPointsPane.Gestures.Add(clickGesture);
			createPointsPane.Tasks.Add(CreatePointsTask(clickGesture, createPointsPane));
			Layout = new VBoxLayout();
			Gradient = new ColorGradient(Color4.White, Color4.Black);
			Nodes.Add(gradientPaneContainer);
			Nodes.Add(createPointsPane);
			this.AddChangeWatcher(() => Gradient.Count, _ => {
				var controlPoint = Gradient.Contains(SelectedControlPoint) ?
					SelectedControlPoint : Gradient.Ordered().First();
				Rebuild();
				SelectPoint(controlPoint);
			});
		}

		private IEnumerator<object> CreatePointsTask(ClickGesture clickGesture, Widget pane)
		{
			while (true) {
				if (pane.IsMouseOverThisOrDescendant()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
					if (clickGesture.WasBegan()) {
						var pos = createPointsPane.LocalMousePosition().X / createPointsPane.Size.X;
						var point = new GradientControlPoint(Gradient.GetNearestPointTo(pos).Color, pos);
						Document.Current.History.DoTransaction(() => {
							Core.Operations.InsertListItem.Perform(point, gradient, 1);
							ControlPointCreated?.Invoke(point);
						});
						CreatePoint(point, false, gradientPaneContainer);
					}
				}
				yield return null;
			}
		}

		private void InitializePoints(Widget container)
		{
			var points = Gradient.Ordered().ToList();
			for (var i = 0; i < points.Count; i++) {
				var isBorderPoint = i == 0 || i == Gradient.Count - 1;
				CreatePoint(points[i], isBorderPoint, container);
			}
		}

		private void CreatePoint(GradientControlPoint controlPoint, bool isBorderPoint, Widget container)
		{
			var w = new GradientControlPointWidget(controlPoint, isBorderPoint) {
				Position = new Vector2(controlPoint.Position * container.Size.X, container.Size.Y),
				MinMaxHeight = 15f,
				MinMaxWidth = 10f,
				Size = new Vector2(10, 15f)
			};
			if (!isBorderPoint) {
				w.Gestures.Add(new ClickGesture(1, () => {
					var idx = gradient.IndexOf(w.ControlPoint);
					Document.Current.History.DoTransaction(() => {
						Core.Operations.RemoveListItem.Perform(w.ControlPoint, gradient);
						ControlPointRemoved?.Invoke(idx);
					});
					w.UnlinkAndDispose();
				}));
			}
			w.OnClick += () => SelectPoint(w);
			w.OnDrag += pos => OnControlPointDrag(pos, Gradient.IndexOf(controlPoint));
			if (!isBorderPoint) {
				container.Nodes.Insert(0, w);
			} else {
				container.AddNode(w);
			}
			SelectPoint(w);
		}

		private void OnControlPointDrag(float newPosition, int index)
		{
			ControlPointMoved?.Invoke(newPosition, index);
		}

		private void SelectPoint(GradientControlPoint point)
		{
			SelectPoint(gradientPaneContainer.Nodes.OfType<GradientControlPointWidget>()
				.First(cp => cp.ControlPoint == point));
		}

		private void SelectPoint(GradientControlPointWidget w)
		{
			SelectedControlPoint = w.ControlPoint;
			if (selectedControlPointWidget != null) {
				selectedControlPointWidget.Color = Color4.White;
			}
			selectedControlPointWidget = w;
			w.Color = Color4.Black;
			SelectionChanged?.Invoke(SelectedControlPoint, w.IsBorderPoint);
		}
	}

	public class GradientControlPointWidget : Widget
	{
		private readonly Vertex[] vertices = { new Vertex(), new Vertex(), new Vertex() };
		public GradientControlPoint ControlPoint { get; set; }
		private readonly DragGesture dragGesture;
		private readonly ITexture chessTexture;
		public event Action OnClick;
		public event Action<float> OnDrag;
		public bool IsBorderPoint { get; }
		public const float tipBodyRatio = 1f / 3f;

		public override void Update(float delta)
		{
			base.Update(delta);
			Position = new Vector2(ParentWidget.Size.X * ControlPoint.Position, ParentWidget.Size.Y);
		}

		public GradientControlPointWidget(GradientControlPoint controlPoint, bool isBorderPoint = false)
		{
			ControlPoint = controlPoint;
			Pivot = new Vector2(0.5f, 0);
			IsBorderPoint = isBorderPoint;
			CompoundPresenter.Add(new DelegatePresenter<Widget>(Render));
			HitTestTarget = true;
			Gestures.Add(dragGesture = new DragGesture());
			chessTexture = PrepareChessTexture(Color4.White, Color4.Black);
			Tasks.Add(DragTask());
		}

		public static ITexture PrepareChessTexture(Color4 color1, Color4 color2)
		{
			var chessTexture = new Texture2D();
			chessTexture.LoadImage(new[] { color1, color2}, 1, 2);
			chessTexture.TextureParams = new TextureParams {
				WrapMode = TextureWrapMode.Repeat,
				MinMagFilter = TextureFilter.Nearest,
			};
			return chessTexture;
		}

		private void Render(Widget w)
		{
			w.PrepareRendererState();
			Renderer.DrawRect(new Vector2(0, w.Size.Y * tipBodyRatio), w.Size, new Color4(ControlPoint.Color.R, ControlPoint.Color.G, ControlPoint.Color.B));
			var spriteColor = Color4.White.Transparentify(ControlPoint.Color.A / 255f);
			Renderer.DrawSprite(chessTexture, spriteColor,
				new Vector2(w.Size.X / 2, w.Size.Y * tipBodyRatio),
				new Vector2(w.Size.X / 2, w.Size.Y * (1 - tipBodyRatio)), Vector2.Zero, Vector2.One);
			Renderer.DrawRectOutline(new Vector2(0, w.Size.Y * tipBodyRatio), w.Size, Color4.Black);
			vertices[0].Pos = new Vector2(w.Size.X / 2, 0);
			vertices[0].Color = Color;
			vertices[1].Pos = new Vector2(w.Size.X, w.Size.Y * tipBodyRatio);
			vertices[1].Color = Color;
			vertices[2].Pos = new Vector2(0, w.Size.Y * tipBodyRatio);
			vertices[2].Color = Color;
			Renderer.DrawTriangleFan(vertices, 3);
			Renderer.DrawLine(vertices[0].Pos, vertices[1].Pos, Color4.Black);
			Renderer.DrawLine(vertices[1].Pos, vertices[2].Pos, Color4.Black);
			Renderer.DrawLine(vertices[2].Pos, vertices[0].Pos, Color4.Black);
		}

		private IEnumerator<object> DragTask()
		{
			while (true) {
				if (dragGesture.WasBegan()) {
					var prevPos = ParentWidget.LocalMousePosition();
					OnClick?.Invoke();
					if (!IsBorderPoint) {
						using (Document.Current.History.BeginTransaction()) {
							while (!dragGesture.WasEnded()) {
								Document.Current.History.RollbackTransaction();
								var mousePos = ParentWidget.LocalMousePosition();
								var delta = (mousePos - prevPos) / ParentWidget.Size;
								var newPosition = Mathf.Clamp(ControlPoint.Position + delta.X, 0, 1);
								Core.Operations.SetProperty.Perform(ControlPoint, nameof(GradientControlPoint.Position), newPosition);
								OnDrag?.Invoke(newPosition);
								var x = Mathf.Clamp(mousePos.X, 0, ParentWidget.Size.X);
								prevPos = new Vector2(x, mousePos.Y);
								Document.Current.History.CommitTransaction();
								yield return null;
							}
						}
					}
				}
				yield return null;
			}
		}

	}
}
