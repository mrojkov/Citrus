using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;

namespace Tangerine.UI.SceneView
{
	public class SceneView : IDocumentView
	{
		// Given panel.
		public readonly Widget Panel;
		// Widget which is a direct child of the panel.
		public readonly Widget Frame;
		// Widget having the same size as panel, used for intercepting mouse events above the canvas.
		public readonly Widget InputArea;
		public WidgetInput Input => InputArea.Input;
		// Container for the document root node.
		public readonly SceneWidget Scene;
		/// <summary>
		/// Gets the mouse position in the scene coordinates.
		/// </summary>
		public Vector2 MousePosition => Scene.Input.LocalMousePosition;

		public ComponentCollection<Component> Components = new ComponentCollection<Component>();

		public static SceneView Instance { get; private set; }

		public static void RegisterGlobalCommands()
		{
			var h = CommandHandlerList.Global;
			h.Connect(SceneViewCommands.PreviewAnimation, new PreviewAnimationHandler());
			h.Connect(SceneViewCommands.DragUp, () => DragNodes(new Vector2(0, -1)));
			h.Connect(SceneViewCommands.DragDown, () => DragNodes(new Vector2(0, 1)));
			h.Connect(SceneViewCommands.DragLeft, () => DragNodes(new Vector2(-1, 0)));
			h.Connect(SceneViewCommands.DragRight, () => DragNodes(new Vector2(1, 0)));
			h.Connect(SceneViewCommands.DragUpFast, () => DragNodes(new Vector2(0, -5)));
			h.Connect(SceneViewCommands.DragDownFast, () => DragNodes(new Vector2(0, 5)));
			h.Connect(SceneViewCommands.DragLeftFast, () => DragNodes(new Vector2(-5, 0)));
			h.Connect(SceneViewCommands.DragRightFast, () => DragNodes(new Vector2(5, 0)));
			h.Connect(SceneViewCommands.Duplicate, DuplicateNodes);
			h.Connect(SceneViewCommands.DisplayBones, new DisplayBones());
			h.Connect(SceneViewCommands.DisplayPivotsForAllWidgets, new DisplayPivotsForAllWidgets());
			h.Connect(SceneViewCommands.DisplayPivotsForInvisibleWidgets, new DisplayPivotsForInvisibleWidgets());
			h.Connect(SceneViewCommands.TieWidgetsWithBones, TieWidgetsWithBones);
			h.Connect(SceneViewCommands.UntieWidgetsFromBones, UntieWidgetsFromBones);
			h.Connect(SceneViewCommands.ToggleDisplayRuler, new DisplayRuler());
			h.Connect(SceneViewCommands.SaveCurrentRuler, new SaveRuler());
		}

		private class SaveRuler : DocumentCommandHandler
		{
			public override bool GetEnabled()
			{
				return SceneViewCommands.ToggleDisplayRuler.Checked && UI.SceneView.Ruler.Lines.Count > 0;
			}

			public override void Execute()
			{
				var ruler = Ruler.GetRulerData();
				ruler.Name = new SaveRulerDialog().Show();
				if (Project.Current.Rulers.Any(o => o.Name == ruler.Name)) {
					new AlertDialog("Ruler with exact name already exist").Show();
				} else {
					Project.Current.AddRuler(ruler);
					Ruler.Clear();
				}
			}
		}

		private class DisplayRuler : ToggleDisplayCommandHandler
		{
			public override void Execute()
			{
				Visible = Ruler.ToggleDisplay();
			}
		}

		private static void DuplicateNodes()
		{
			Document.Current.History.BeginTransaction();
			var text = Clipboard.Text;
			try {
				Copy.CopyToClipboard();
				Paste.Perform();
			} finally {
				Clipboard.Text = text;
				Document.Current.History.EndTransaction();
			}
		}

		private static void TieWidgetsWithBones()
		{
			var bones = Document.Current.SelectedNodes().Editable().OfType<Bone>().ToList();
			var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>().ToList();
			if (widgets.Count == 0) {
				return;
			}
			Document.Current.History.BeginTransaction();
			try
			{
				foreach (var widget in widgets)
				{
					if (widget is DistortionMesh) {
						var mesh = widget as DistortionMesh;
						foreach (PointObject point in mesh.Nodes) {
							Core.Operations.SetAnimableProperty.Perform(point, nameof(PointObject.SkinningWeights),
								CalcSkinningWeight(point.CalcPositionInSpaceOf(widget.ParentWidget), bones), CoreUserPreferences.Instance.AutoKeyframes);
						}
					} else {
						Core.Operations.SetAnimableProperty.Perform(widget, nameof(PointObject.SkinningWeights),
							CalcSkinningWeight(widget.Position, bones), CoreUserPreferences.Instance.AutoKeyframes);
					}
				}
				foreach (var bone in bones)
				{
					var entry = bone.Parent.AsWidget.BoneArray[bone.Index];
					Core.Operations.SetAnimableProperty.Perform(bone, nameof(Bone.RefPosition), entry.Joint, CoreUserPreferences.Instance.AutoKeyframes);
					Core.Operations.SetAnimableProperty.Perform(bone, nameof(Bone.RefLength), entry.Length, CoreUserPreferences.Instance.AutoKeyframes);
					Core.Operations.SetAnimableProperty.Perform(bone, nameof(Bone.RefRotation), entry.Rotation, CoreUserPreferences.Instance.AutoKeyframes);
				}
			} finally {
				Document.Current.History.EndTransaction();
			}
		}

		private static void UntieWidgetsFromBones()
		{
			var bones = Document.Current.SelectedNodes().Editable().OfType<Bone>().ToList();
			var widgets = Document.Current.SelectedNodes().Editable().OfType<Widget>().ToList();
			if (widgets.Count == 0) {
				return;
			}
			foreach (var widget in widgets) {
				if (widget is DistortionMesh) {
					var mesh = widget as DistortionMesh;
					foreach (PointObject point in mesh.Nodes) {
						UntieBones(point, nameof(PointObject.SkinningWeights), bones);
					}
				} else {
					UntieBones(widget, nameof(Widget.SkinningWeights), bones);
				}
			}
		}

		private static void UntieBones(object obj, string propName, List<Bone> bones)
		{
			var originSkinningWeights = (SkinningWeights)obj.GetType().GetProperty(propName).GetValue(obj);
			var indices = new List<int>();
			for (int i = 0; i < 4; i++) {
				if (bones.Any(b => b.Index == originSkinningWeights[i].Index)) {
					indices.Add(i);
				}
			}
			if (indices.Count != 0) {
				var skinningWeights = new SkinningWeights();
				for (int i = 0; i < 4; i++) {
					skinningWeights[i] = indices.Contains(i) ? new BoneWeight() : originSkinningWeights[i];
				}
				Core.Operations.SetProperty.Perform(obj, propName, skinningWeights);
			}
		}

		private static SkinningWeights CalcSkinningWeight(Vector2 position, List<Bone> bones)
		{
			var skinningWeights = new SkinningWeights();
			var i = 0;
			while (i < bones.Count && i < 4) {
				var bone = bones[i];
				skinningWeights[i] = new BoneWeight {
					Weight = bone.CalcWeightForPoint(position),
					Index = bone.Index
				};
				i++;
			}
			return skinningWeights;
		}


		private class DisplayBones : ToggleDisplayCommandHandler
		{
			public override void Execute()
			{
				var postPresenter = Instance.Frame.CompoundPostPresenter;
				if (Visible) {
					postPresenter.Remove(Bone3DPresenter.Presenter);
				} else {
					postPresenter.Add(Bone3DPresenter.Presenter);
				}
				CommonWindow.Current.Invalidate();
				base.Execute();
			}
		}

		private class DisplayPivotsForAllWidgets : DocumentCommandHandler
		{
			public override void Execute()
			{
				SceneUserPreferences.Instance.DisplayPivotsForAllWidgets = !SceneUserPreferences.Instance.DisplayPivotsForAllWidgets;
				CommonWindow.Current.Invalidate();
			}

			public override bool GetChecked()
			{
				return SceneUserPreferences.Instance.DisplayPivotsForAllWidgets;
			}
		}

		private class DisplayPivotsForInvisibleWidgets : DocumentCommandHandler
		{
			public override void Execute()
			{
				SceneUserPreferences.Instance.DisplayPivotsForInvisibleWidgets = !SceneUserPreferences.Instance.DisplayPivotsForInvisibleWidgets;
				CommonWindow.Current.Invalidate();
			}

			public override bool GetChecked()
			{
				return SceneUserPreferences.Instance.DisplayPivotsForInvisibleWidgets;
			}
		}

		static void DragNodes(Vector2 delta)
		{
			DragWidgets(delta);
			DragNodes3D(delta);
			DragSplinePoints3D(delta);
		}

		static void DragWidgets(Vector2 delta)
		{
			var containerWidget = Document.Current.Container as Widget;
			if (containerWidget != null) {
				var transform = containerWidget.CalcTransitionToSpaceOf(Instance.Scene).CalcInversed();
				var dragDelta = transform * delta - transform * Vector2.Zero;
				foreach (var widget in Document.Current.SelectedNodes().Editable().OfType<Widget>()) {
					Core.Operations.SetAnimableProperty.Perform(widget, nameof(Widget.Position), widget.Position + dragDelta, CoreUserPreferences.Instance.AutoKeyframes);
				}
			}
		}

		static void DragNodes3D(Vector2 delta)
		{
			foreach (var node3D in Document.Current.SelectedNodes().Editable().OfType<Node3D>()) {
				Core.Operations.SetAnimableProperty.Perform(node3D, nameof(Widget.Position), node3D.Position + (Vector3)delta / 100, CoreUserPreferences.Instance.AutoKeyframes);
			}
		}

		static void DragSplinePoints3D(Vector2 delta)
		{
			foreach (var point in Document.Current.SelectedNodes().Editable().OfType<SplinePoint3D>()) {
				Core.Operations.SetAnimableProperty.Perform(point, nameof(Widget.Position), point.Position + (Vector3)delta / 100, CoreUserPreferences.Instance.AutoKeyframes);
			}
		}

		public SceneView(Widget panelWidget)
		{
			this.Panel = panelWidget;
			InputArea = new Widget { HitTestTarget = true, Anchors = Anchors.LeftRightTopBottom };
			InputArea.FocusScope = new KeyboardFocusScope(InputArea);
			Scene = new SceneWidget {
				Nodes = { Document.Current.RootNode }
			};
			Frame = new Widget {
				Id = "SceneView",
				Nodes = { InputArea, Scene }
			};
			CreateComponents();
			CreateProcessors();
			CreatePresenters();
			panelWidget.Updated += HandleDropImage;
		}

		public void Attach()
		{
			Instance = this;
			Panel.AddNode(Frame);
			DockManager.Instance.FilesDropped += DropFiles;
		}

		public void Detach()
		{
			DockManager.Instance.FilesDropped -= DropFiles;
			Instance = null;
			Frame.Unlink();
		}

		/// <summary>
		/// Checks whether the mouse is over a control point within a given radius.
		/// </summary>
		public bool HitTestControlPoint(Vector2 controlPoint, float radius = 10)
		{
			return (controlPoint - MousePosition).Length < radius / Scene.Scale.X;
		}

		/// <summary>
		/// Checks whether the mouse is over a control point within a specific for resize radius.
		/// </summary>
		public bool HitTestResizeControlPoint(Vector2 controlPoint)
		{
			return HitTestControlPoint(controlPoint, 6);
		}

		void DropFiles(IEnumerable<string> files)
		{
			if (!InputArea.IsMouseOverThisOrDescendant()) {
				return;
			}
			if (!Window.Current.Active) {
				Window.Current.Activate();
				InputArea.SetFocus();
			}
			var widgetPos = MousePosition * Scene.CalcTransitionToSpaceOf(Document.Current.Container.AsWidget);
			Document.Current.History.BeginTransaction();
			try {
				List<string> pendingImages = new List<string>();
				foreach (var file in files) {
					try {
						string assetPath, assetType;
						if (Utils.ExtractAssetPathOrShowAlert(file, out assetPath, out assetType)) {
							var fileName = Path.GetFileNameWithoutExtension(assetPath);
							if (assetType == ".png") {
								pendingImages.Add(assetPath);
							} else if (assetType == ".ogg") {
								var node = Core.Operations.CreateNode.Perform(typeof(Audio));
								var sample = new SerializableSample(assetPath);
								Core.Operations.SetProperty.Perform(node, nameof(Audio.Sample), sample);
								Core.Operations.SetProperty.Perform(node, nameof(Node.Id), fileName);
								Core.Operations.SetProperty.Perform(node, nameof(Audio.Volume), 1);
								var key = new Keyframe<AudioAction> {
									Frame = Document.Current.AnimationFrame,
									Value = AudioAction.Play
								};
								Core.Operations.SetKeyframe.Perform(node, nameof(Audio.Action), Document.Current.AnimationId, key);
							} else if (assetType == ".tan" || assetType == ".model" || assetType == ".scene") {
								var scene = Node.CreateFromAssetBundle(assetPath);
								var node = Core.Operations.CreateNode.Perform(scene.GetType());
								Core.Operations.SetProperty.Perform(node, nameof(Widget.ContentsPath), assetPath);
								Core.Operations.SetProperty.Perform(node, nameof(Node.Id), fileName);
								if (scene is Widget) {
									Core.Operations.SetProperty.Perform(node, nameof(Widget.Position), widgetPos);
									Core.Operations.SetProperty.Perform(node, nameof(Widget.Pivot), Vector2.Half);
									Core.Operations.SetProperty.Perform(node, nameof(Widget.Size), ((Widget)scene).Size);
								}
								node.LoadExternalScenes();
							}
						}
					} catch (System.Exception e) {
						AlertDialog.Show(e.Message);
					}
				}

				if (pendingImages.Count > 0) {
					var menu = new Menu() {
						ImageDropCommands.AsImage,
						ImageDropCommands.AsDistortionMesh,
						ImageDropCommands.AsNineGrid,
						ImageDropCommands.AsParticleModifier,
					};
					ImageDropCommands.AssetPaths = pendingImages;
					menu.Popup();
				}
			} finally {
				Document.Current.History.EndTransaction();
			}
		}

		private static class ImageDropCommands
		{
			public static readonly ICommand AsImage = new Command("Create Image");
			public static readonly ICommand AsDistortionMesh = new Command("Create Distortion Mesh");
			public static readonly ICommand AsNineGrid = new Command("Create Nine Grid");
			public static readonly ICommand AsParticleModifier = new Command("Create Particle Modifier");

			public static readonly Dictionary<ICommand, Type> Commands = new Dictionary<ICommand, Type> {
				{ AsImage, typeof(Image) },
				{ AsDistortionMesh, typeof(DistortionMesh) },
				{ AsNineGrid, typeof(NineGrid) },
				{ AsParticleModifier, typeof(ParticleModifier) },
			};

			public static List<string> AssetPaths;
		}

		private void HandleDropImage(float dt)
		{
			foreach (var kv in ImageDropCommands.Commands) {
				if (kv.Key.WasIssued()) {
					kv.Key.Consume();
					Document.Current.History.BeginTransaction();
					try {
						foreach (string assetPath in ImageDropCommands.AssetPaths) {
							var node = Core.Operations.CreateNode.Perform(kv.Value);
							var widgetPos = MousePosition * Scene.CalcTransitionToSpaceOf(Document.Current.Container.AsWidget);
							var texture = new SerializableTexture(assetPath);
							SetProperty.Perform(node, nameof(Widget.Texture), texture);
							SetProperty.Perform(node, nameof(Widget.Position), widgetPos);
							SetProperty.Perform(node, nameof(Widget.Pivot), Vector2.Half);
							SetProperty.Perform(node, nameof(Widget.Size), (Vector2)texture.ImageSize);
							SetProperty.Perform(node, nameof(Widget.Id), Path.GetFileNameWithoutExtension(assetPath));
						}
					} finally {
						Document.Current.History.EndTransaction();
					}
				} else {
					kv.Key.Consume();
				}
			}
		}

		void CreateComponents() { }

		void CreateProcessors()
		{
			Frame.Tasks.Add(
				new ActivateOnMouseOverProcessor(),
				new CreateWidgetProcessor(),
				new CreateSplinePointProcessor(),
				new CreatePointObjectProcessor(),
				new CreateSplinePoint3DProcessor(),
				new CreateBoneProcessor(),
				new CreateNodeProcessor(),
				new ExpositionProcessor(),
				new MouseScrollProcessor(),
				new DragPivotProcessor(),
				new DragBoneProcessor(),
				new ChangeBoneRadiusProcessor(),
				new RotateBoneProcessor(),
				new DragWidgetsProcessor(),
				new DragPointObjectsProcessor(),
				new DragSplineTangentsProcessor(),
				new DragSplinePoint3DProcessor(),
				new ResizeWidgetsProcessor(),
				new RescalePointObjectSelectionProcessor(),
				new RotatePointObjectSelectionProcessor(),
				new RotateWidgetsProcessor(),
				new MouseSelectionProcessor(),
				new ShiftClickProcessor(),
				new PreviewAnimationProcessor()
			);
		}

		void CreatePresenters()
		{
			new ContainerAreaPresenter(this);
			new WidgetsPivotMarkPresenter(this);
			new SelectedWidgetsPresenter(this);
			new PointObjectsPresenter(this);
			new SplinePointPresenter(this);
			new TranslationGizmoPresenter(this);
			new BonePresenter(this);
			new BoneAsistantPresenter(this);
			new DistortionMeshPresenter(this);
			new FrameBorderPresenter(this);
		}

		public void CreateNode(Type nodeType)
		{
			Components.Add(new CreateNodeRequestComponent { NodeType = nodeType });
		}

		public void DuplicateSelectedNodes()
		{
			DuplicateNodes();
		}

		public class SceneWidget : Widget
		{
			public override void Update(float delta)
			{
				if (Document.Current.PreviewAnimation) {
					base.Update(delta);
				}
			}
		}
	}

	public class CreateNodeRequestComponent : Component
	{
		public Type NodeType { get; set; }

		public static bool Consume<T>(ComponentCollection<Component> components, out Type nodeType) where T: Node
		{
			var c = components.Get<CreateNodeRequestComponent>();
			if (c != null && (c.NodeType.IsSubclassOf(typeof(T)) || c.NodeType == typeof(T))) {
				components.Remove<CreateNodeRequestComponent>();
				nodeType = c.NodeType;
				return true;
			}
			nodeType = null;
			return false;
		}

		public static bool Consume<T>(ComponentCollection<Component> components) where T: Node
		{
			Type type;
			return Consume<T>(components, out type);
		}
	}
}
