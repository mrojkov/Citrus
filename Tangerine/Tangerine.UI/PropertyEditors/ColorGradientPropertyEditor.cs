using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class ColorGradientPropertyEditor : ExpandablePropertyEditor<ColorGradient>
	{
		private readonly TransactionalGradientControlWidget gradientControlWidget;
		private readonly ColorPickerPanel colorPanel;
		private IDataflowProvider<Color4> selectedPointColorProperty;
		private IDataflowProvider<float> selectedPointPositionProperty;
		private IDataflowProvider<string> currentColorString;
		private readonly EditBox colorEditor;
		private readonly NumericEditBox positionEditor;
		private GradientControlPoint selectedControlPoint => gradientControlWidget.SelectedControlPoint;

		public ColorGradientPropertyEditor(IPropertyEditorParams editorParams) : base(editorParams)
		{
			if (editorParams.Objects.Count() > 1) {
				ExpandButton.Visible = false;
				EditorContainer.AddNode(new ThemedSimpleText {
					Text = "Unable to edit multiple gradients",
					VAlignment = VAlignment.Center,
					LayoutCell = new LayoutCell(Alignment.Center, stretchX: 0),
				});
				return;
			}
			gradientControlWidget = new TransactionalGradientControlWidget(EditorParams.History) {
				MinMaxHeight = 35f,
				Height = 35f,
				LayoutCell = new LayoutCell(Alignment.LeftCenter),
				Padding = new Thickness { Right = 5f, Bottom = 5f}
			};
			var gradientProperty = CoalescedPropertyValue(new ColorGradient(Color4.White, Color4.Black)).DistinctUntilChanged();
			gradientControlWidget.Gradient = gradientProperty.GetValue().Value;
			ContainerWidget.AddChangeWatcher(gradientProperty, g => gradientControlWidget.Gradient = g.Value);
			gradientControlWidget.SelectionChanged += SelectPoint;
			EditorContainer.AddNode(gradientControlWidget);
			EditorContainer.AddNode(CreatePipetteButton());
			ExpandableContent.Padding = new Thickness { Left = 25f, Right = 25f, Top = 5f };
			ExpandableContent.AddNode(new Widget {
				Layout = new HBoxLayout { Spacing = 10f },
				Nodes = {
					new ThemedSimpleText { Text = nameof(GradientControlPoint.Position), MinWidth = 150 },
					(positionEditor = EditorParams.NumericEditBoxFactory())
				},
				Padding = new Thickness(0, 3f)
			});
			ExpandableContent.AddNode(new Widget {
				Layout = new HBoxLayout { Spacing = 10f },
				Nodes = {
					new ThemedSimpleText { Text = nameof(GradientControlPoint.Color), MinWidth = 150 },
					(colorEditor = EditorParams.EditBoxFactory())
				},
				Padding = new Thickness(0, 3f)
			});
			positionEditor.Step = 0.005f;
			colorEditor.Submitted += SetColor;
			positionEditor.Submitted += SetPosition;
			colorPanel = new ColorPickerPanel();
			ExpandableContent.AddNode(colorPanel.Widget);
			var padding = colorPanel.Widget.Padding;
			padding.Right = 12;
			colorPanel.Widget.Padding = padding;
			colorPanel.DragStarted += () => EditorParams.History?.BeginTransaction();
			gradientControlWidget.DragStarted += () => EditorParams.History?.BeginTransaction();
			gradientControlWidget.DragEnded += () => {
				EditorParams.History?.CommitTransaction();
				EditorParams.History?.EndTransaction();
			};
			colorPanel.DragEnded += () => {
				EditorParams.History?.CommitTransaction();
				EditorParams.History?.EndTransaction();
			};
			colorPanel.Changed += () => {
				EditorParams.History?.RollbackTransaction();
				Core.Operations.SetProperty.Perform(selectedControlPoint, nameof(GradientControlPoint.Color), colorPanel.Color);
			};
			SelectPoint(selectedControlPoint);
		}

		private void SetControlPointProperty(string propertyName, object value)
		{
			DoTransaction(() => Core.Operations.SetProperty.Perform(selectedControlPoint, propertyName, value));
		}

		private Node CreatePipetteButton()
		{
			var button = new ToolbarButton {
				Texture = IconPool.GetTexture("Tools.Pipette"),
			};
			button.Tasks.Add(Color4PropertyEditor.PickColorProcessor(
				button, v => {
					v.A = selectedControlPoint.Color.A;
					SetControlPointProperty(nameof(GradientControlPoint.Color), v);
				}));
			return button;
		}

		public void SetColor(string text)
		{
			if (Color4.TryParse(text, out var newColor)) {
				SetControlPointProperty(nameof(GradientControlPoint.Color), newColor);
			} else {
				colorEditor.Text = currentColorString.GetValue();
			}
		}

		public void SetPosition(string text)
		{
			if (float.TryParse(text, out var newPosition)) {
				SetControlPointProperty(nameof(GradientControlPoint.Position), Mathf.Clamp(newPosition, 0, 1));
			}
			positionEditor.Text = selectedPointPositionProperty.GetValue().ToString("0.###");
		}

		private void SelectPoint(GradientControlPoint point)
		{
			colorEditor.Tasks.Clear();
			positionEditor.Tasks.Clear();
			selectedPointColorProperty = new Property<Color4>(point, nameof(GradientControlPoint.Color));
			selectedPointPositionProperty = new Property<float>(point, nameof(GradientControlPoint.Position));
			currentColorString = selectedPointColorProperty.DistinctUntilChanged().Select(i => i.ToString(Color4.StringPresentation.Dec));
			colorEditor.Tasks.Add(currentColorString.Consume(v => colorEditor.Text = v));
			colorPanel.Color = selectedPointColorProperty.GetValue();
			colorEditor.Tasks.Add(selectedPointColorProperty.DistinctUntilChanged().Consume(v => {
				if (colorPanel.Color != v) {
					colorPanel.Color = v;
				}
			}));
			positionEditor.Tasks.Add(selectedPointPositionProperty.DistinctUntilChanged().Consume(v => positionEditor.Text = v.ToString()));
		}
	}

	public class TransactionalGradientControlWidget : Widget
	{
		private readonly ITransactionalHistory history;
		private readonly Widget gradientPaneContainer;
		private readonly Widget createPointsPane;
		private readonly Image gradientPane;
		private readonly GradientComponent gradientComponent;
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

		public event Action DragStarted;
		public event Action DragEnded;
		public event Action<GradientControlPoint> SelectionChanged;
		public event Action<GradientControlPoint, int> ControlPointCreated;
		public event Action<int> ControlPointRemoved;
		public event Action<float, int> ControlPointPositionChanged;

		private void Rebuild()
		{
			gradientComponent.Gradient = gradient;
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

		public TransactionalGradientControlWidget(ITransactionalHistory history)
		{
			this.history = history;
			gradientPane = new Image {
				PostPresenter = new WidgetBoundsPresenter(Color4.Black),
			};
			var chessTexture = PrepareChessTexture(Color4.White, Color4.Black);
			gradientPane.CompoundPresenter.Add(new SyncDelegatePresenter<Widget>(w => {
				w.PrepareRendererState();
				Renderer.DrawSprite(chessTexture, Color4.White, Vector2.Zero, w.Size, Vector2.Zero, new Vector2(w.Size.X / w.Size.Y, 1));
			}));
			gradientComponent = new GradientComponent();
			gradientPane.Components.Add(gradientComponent);
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
						AddPoint(point);
						CreatePoint(point, Gradient, gradientPaneContainer);
					}
				}
				yield return null;
			}
		}

		private void AddPoint(GradientControlPoint point)
		{
			using (history.BeginTransaction()) {
				Core.Operations.InsertIntoList.Perform(gradient, 1, point);
				ControlPointCreated?.Invoke(point, 1);
				history.CommitTransaction();
			}
		}

		private void RemovePoint(GradientControlPoint controlPoint, int idx)
		{
			using (history.BeginTransaction()) {
				Core.Operations.RemoveFromList.Perform(gradient, gradient.IndexOf(controlPoint));
				ControlPointRemoved?.Invoke(idx);
				history.CommitTransaction();
			};
		}

		private void InitializePoints(Widget container)
		{
			foreach (var t in Gradient.Ordered()) {
				CreatePoint(t, Gradient, container);
			}
		}

		private void CreatePoint(GradientControlPoint controlPoint, ColorGradient gradient, Widget container)
		{
			var w = new GradientControlPointWidget(controlPoint) {
				Position = new Vector2(controlPoint.Position * container.Size.X, container.Size.Y),
				MinMaxHeight = 15f,
				MinMaxWidth = 10f,
				Size = new Vector2(10, 15f)
			};
			w.Gestures.Add(new ClickGesture(1, () => {
				if (gradient.Count > 1) {
					var idx = gradient.IndexOf(w.ControlPoint);
					RemovePoint(controlPoint, idx);
					w.UnlinkAndDispose();
				}
			}));
			var dragGesture = new DragGesture();
			w.Gestures.Add(dragGesture);
			w.Tasks.Add(DragTask(w, controlPoint, dragGesture));
			container.Nodes.Insert(0, w);
			SelectPoint(w);
		}

		private IEnumerator<object> DragTask(GradientControlPointWidget w, GradientControlPoint point, DragGesture dragGesture)
		{
			while (true) {
				if (dragGesture.WasBegan()) {
					var prevPos = LocalMousePosition();
					SelectPoint(w);
					DragStarted?.Invoke();
					while (!dragGesture.WasEnded()) {
						var mousePos = LocalMousePosition();
						var delta = (mousePos - prevPos) / Size;
						var newPosition = Mathf.Clamp(point.Position + delta.X, 0, 1);
						Core.Operations.SetProperty.Perform(point, nameof(GradientControlPoint.Position), newPosition);
						ControlPointPositionChanged?.Invoke(newPosition, Gradient.IndexOf(point));
						var x = Mathf.Clamp(mousePos.X, 0, Size.X);
						prevPos = new Vector2(x, mousePos.Y);
						yield return null;
					}
					DragEnded?.Invoke();
				}
				yield return null;
			}
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
			SelectionChanged?.Invoke(SelectedControlPoint);
		}
	}

	public class GradientControlPointWidget : Widget
	{
		private readonly Vertex[] vertices = { new Vertex(), new Vertex(), new Vertex() };
		public GradientControlPoint ControlPoint { get; set; }
		private readonly ITexture chessTexture;
		public const float tipBodyRatio = 1f / 3f;

		public override void Update(float delta)
		{
			base.Update(delta);
			Position = new Vector2(ParentWidget.Size.X * ControlPoint.Position, ParentWidget.Size.Y);
		}

		public GradientControlPointWidget(GradientControlPoint controlPoint)
		{
			ControlPoint = controlPoint;
			Pivot = new Vector2(0.5f, 0);
			CompoundPresenter.Add(new SyncDelegatePresenter<Widget>(Render));
			HitTestTarget = true;
			chessTexture = PrepareChessTexture(Color4.White, Color4.Black);
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
	}
}
