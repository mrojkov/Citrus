using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine
{
	public class TangerineApp
	{
		public static TangerineApp Instance { get; private set; }
		public readonly Dictionary<string, Toolbar> Toolbars = new Dictionary<string, Toolbar>();
		public readonly DockManager.State DockManagerInitialState;

		private ModalOperationDialog cookingOfModifiedAssetsDialog;

		public static void Initialize()
		{
			Instance = new TangerineApp();
		}

		private TangerineApp()
		{
			Orange.UserInterface.Instance = new OrangeInterface();
			WidgetInput.AcceptMouseBeyondWidgetByDefault = false;
			Application.IsTangerine = true;
			Serialization.DeserializerBuilders.Insert(0, DeserializeHotStudioAssets);

			if (!UserPreferences.Initialize()) {
				UserPreferences.Instance.Clear();
				UserPreferences.Instance.Add(new AppUserPreferences());
				UserPreferences.Instance.Add(new UI.SceneView.SceneUserPreferences());
				UserPreferences.Instance.Add(new UI.Timeline.TimelineUserPreferences());
				UserPreferences.Instance.Add(new UI.FilesystemView.FilesystemUserPreferences());
				UserPreferences.Instance.Add(new CoreUserPreferences());
			}
			SetColorTheme(AppUserPreferences.Instance.Theme);

			LoadFont();
			DockManager.Initialize(new Vector2(1024, 768), TangerineMenu.PadsMenu);
			TangerineMenu.Create();
			var mainWidget = DockManager.Instance.MainWindowWidget;
			mainWidget.Window.AllowDropFiles = true;
			mainWidget.AddChangeWatcher(() => Project.Current, _ => {
				SetupMainWindowTitle(mainWidget);
				TangerineMenu.RebuildCreateImportedTypeMenu();
			});

			Application.Exiting += () => Project.Current.Close();
			Application.Exited += () => {
				AppUserPreferences.Instance.DockState = DockManager.Instance.ExportState();
				Core.UserPreferences.Instance.Save();
			};

			var timelinePanel = new DockPanel("Timeline");
			var inspectorPanel = new DockPanel("Inspector");
			var searchPanel = new DockPanel("Search");
			var filesystemPanel = new DockPanel("Filesystem");
			var consolePanel = new DockPanel("Console");
			new UI.Console(consolePanel);

			var dockManager = DockManager.Instance;
			dockManager.AddPanel(timelinePanel, DockSite.Top, new Vector2(800, 300));
			dockManager.AddPanel(inspectorPanel, DockSite.Left, new Vector2(300, 700));
			dockManager.AddPanel(searchPanel, DockSite.Right, new Vector2(300, 700));
			dockManager.AddPanel(filesystemPanel, DockSite.Right, new Vector2(300, 700));
			dockManager.AddPanel(consolePanel, DockSite.Bottom, new Vector2(800, 200));
			DockManagerInitialState = dockManager.ExportState();
			var documentViewContainer = InitializeDocumentArea(dockManager);

			dockManager.ImportState(AppUserPreferences.Instance.DockState);
			Document.CloseConfirmation += doc => {
				var alert = new AlertDialog($"Save the changes to document '{doc.Path}' before closing?", "Yes", "No", "Cancel");
				switch (alert.Show()) {
					case 0: return Document.CloseAction.SaveChanges;
					case 1: return Document.CloseAction.DiscardChanges;
					case -1:
					default: return Document.CloseAction.Cancel;
				}
			};
			Project.DocumentReloadConfirmation += doc => {
				var alert = new AlertDialog($"The file '{doc.Path}' has been changed outside of Tangerine.\nDo you want to keep your changes, or reload the file from disk?", "Keep", "Reload");
				var res = alert.Show();
				return res == 0 || res == -1 ? false : true;
			};

			Project.TempFileLoadConfirmation += path => {
				var alert = new AlertDialog($"Do you want to load autosaved version of '{path}'?", "Yes", "No");
				return alert.Show() == 0;
			};

			Project.CookingOfModifiedAssetsStarted += () => {
				cookingOfModifiedAssetsDialog = new ModalOperationDialog(() => Project.CookingOfModifiedAssetsStatus, "Cooking of modified assets");
				cookingOfModifiedAssetsDialog.Show();
			};
			Project.CookingOfModifiedAssetsEnded += () => {
				cookingOfModifiedAssetsDialog.Close();
				cookingOfModifiedAssetsDialog = null;
			};
			Project.OpenFileOutsideProjectAttempt += (string filePath) => {
				var path = filePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				path[0] += '\\';
				string projectFilePath = null;
				for (int i = path.Length - 2; i >= 0; i--) {
					var ppp = Path.Combine(path.Take(i + 1).ToArray());
					var projectFileCandidates = Directory.GetFiles(ppp, "*.citproj", SearchOption.TopDirectoryOnly);
					if (projectFileCandidates.Length > 0) {
						projectFilePath = projectFileCandidates[0];
						break;
					}
				}
				if (projectFilePath != null) {
					var alert = new AlertDialog($"You're trying to open a document outside the project directory. Change the current project to '{Path.GetFileName(projectFilePath)}'?", "Yes", "No");
					if (alert.Show() == 0) {
						FileOpenProject.Execute(projectFilePath);
						Project.Current.OpenDocument(filePath, true);
						return;
					}
				}
				AlertDialog.Show("Can't open a document outside the project directory");
			};
			Project.Tasks = dockManager.MainWindowWidget.Tasks;
			Project.Tasks.Add(new AutosaveProcessor(() => AppUserPreferences.Instance.AutosaveDelay));
			Document.NodeDecorators.AddFor<Spline>(n => n.CompoundPostPresenter.Add(new UI.SceneView.SplinePresenter()));
			Document.NodeDecorators.AddFor<Viewport3D>(n => n.CompoundPostPresenter.Add(new UI.SceneView.Spline3DPresenter()));
			Document.NodeDecorators.AddFor<Widget>(n => {
				if (n.AsWidget.SkinningWeights == null) {
					n.AsWidget.SkinningWeights = new SkinningWeights();
				}
			});
			Document.NodeDecorators.AddFor<PointObject>(n => {
				if ((n as PointObject).SkinningWeights == null) {
					(n as PointObject).SkinningWeights = new SkinningWeights();
				}
			});

			Document.NodeDecorators.AddFor<Node>(n => n.SetTangerineFlag(TangerineFlags.SceneNode, true));

			dockManager.MainWindowWidget.Updated += delta => Document.Current?.History.NextBatch();
			DocumentHistory.Processors.AddRange(new IOperationProcessor[] {
				new Core.Operations.SelectRow.Processor(),
				new Core.Operations.SetProperty.Processor(),
				new Core.Operations.RemoveKeyframe.Processor(),
				new Core.Operations.SetKeyframe.Processor(),
				new Core.Operations.InsertFolderItem.Processor(),
				new Core.Operations.UnlinkFolderItem.Processor(),
				new Core.Operations.MoveNodes.Processor(),
				new Core.Operations.SetMarker.Processor(),
				new Core.Operations.DeleteMarker.Processor(),
				new Core.Operations.DistortionMeshProcessor(),
				new Core.Operations.SyncFolderDescriptorsProcessor(),
				new Core.Operations.TimelineHorizontalShift.Processor(),
				new UI.Timeline.Operations.SelectGridSpan.Processor(),
				new UI.Timeline.Operations.DeselectGridSpan.Processor(),
				new UI.Timeline.Operations.ClearGridSelection.Processor(),
				new UI.Timeline.Operations.ShiftGridSelection.Processor(),
				new UI.Timeline.Operations.SetCurrentColumn.Processor(),
				new UI.Timeline.Operations.SelectCurveKey.Processor(),
				new TriggersValidatorOnSetProperty(),
				new TriggersValidatorOnSetKeyframe(),
				new UpdateNodesAndApplyAnimatorsProcessor(),
				new RowsSynchronizer(),
			});
			DocumentHistory.Processors.AddRange(UI.Timeline.Timeline.GetOperationProcessors());

			Toolbars.Add("Create", new Toolbar(dockManager.ToolbarArea));
			Toolbars.Add("Tools", new Toolbar(dockManager.ToolbarArea));
			foreach (var c in Application.MainMenu.FindCommand("Create").Menu) {
				Toolbars["Create"].Add(c);
			}
			CreateToolsToolbar();
			Document.AttachingViews += doc => {
				if (doc.Views.Count == 0) {
					doc.Views.AddRange(new IDocumentView[] {
						new UI.Inspector.Inspector(inspectorPanel.ContentWidget),
						new UI.Timeline.Timeline(timelinePanel),
						new UI.SceneView.SceneView(documentViewContainer),
						new UI.SearchPanel(searchPanel.ContentWidget),
					});
				}
			};
			var proj = AppUserPreferences.Instance.RecentProjects.FirstOrDefault();
			if (proj != null) {
				new Project(proj).Open();
			}
			WidgetContext.Current.Root.AddChangeWatcher(() => Project.Current, project => TangerineMenu.OnProjectChanged(project));
			new UI.FilesystemView.FilesystemPane(filesystemPanel);
			RegisterGlobalCommands();
		}

		void SetupMainWindowTitle(WindowWidget windowWidget)
		{
			var title = "Tangerine";
			if (Project.Current != Project.Null) {
				var citProjName = System.IO.Path.GetFileNameWithoutExtension(Project.Current.CitprojPath);
				title = $"{citProjName} - Tangerine";
			}
			windowWidget.Window.Title = title;
		}

		void SetColorTheme(ColorThemeEnum theme)
		{
			Theme.Colors = theme == ColorThemeEnum.Light ? Theme.ColorTheme.CreateLightTheme() : Theme.ColorTheme.CreateDarkTheme();
			ColorTheme.Current = theme == ColorThemeEnum.Light ? ColorTheme.CreateLightTheme() : ColorTheme.CreateDarkTheme();
		}

		class UpdateNodesAndApplyAnimatorsProcessor : SymmetricOperationProcessor
		{
			public override void Process(IOperation op)
			{
				var doc = Document.Current;
				doc.Container.AnimationFrame = doc.Container.AnimationFrame;
				doc.RootNode.Update(0);
			}
		}

		void CreateToolsToolbar()
		{
			var tb = Toolbars["Tools"];
			tb.Add(Tools.AlignLeft);
			tb.Add(Tools.AlignTop);
			tb.Add(Tools.AlignRight);
			tb.Add(Tools.AlignBottom);
			tb.Add(Tools.AlignCentersHorizontally);
			tb.Add(Tools.AlignCentersVertically);
			tb.Add(Tools.CenterHorizontally);
			tb.Add(Tools.CenterVertically);
			tb.Add(Tools.RestoreOriginalSize);
			tb.Add(Tools.ResetScale);
			tb.Add(Tools.ResetRotation);
			tb.Add(Tools.FitToContainer);
			tb.Add(Tools.FitToContent);
			tb.Add(Tools.FlipX);
			tb.Add(Tools.FlipY);
		}

		Yuzu.AbstractDeserializer DeserializeHotStudioAssets(string path, System.IO.Stream stream)
		{
			if (path.EndsWith(".scene", StringComparison.CurrentCultureIgnoreCase)) {
				return new HotSceneDeserializer(stream);
			} else if (path.EndsWith(".fnt", StringComparison.CurrentCultureIgnoreCase)) {
				return new HotFontDeserializer(stream);
			}
			return null;
		}

		static Frame InitializeDocumentArea(DockManager dockManager)
		{
			var tabBar = new ThemedTabBar { LayoutCell = new LayoutCell { StretchY = 0 } };
			var documentViewContainer = new Frame {
				ClipChildren = ClipMethod.ScissorTest,
				Layout = new StackLayout(),
				HitTestTarget = true
			};
			new DocumentTabsProcessor(tabBar);
			var docArea = dockManager.DocumentArea;
			docArea.Layout = new VBoxLayout();
			docArea.AddNode(tabBar);
			docArea.AddNode(documentViewContainer);
			docArea.FocusScope = new KeyboardFocusScope(docArea);
			return documentViewContainer;
		}

		class DocumentTabsProcessor
		{
			private TabBar tabBar;
			public DocumentTabsProcessor(TabBar tabBar)
			{
				tabBar.HitTestTarget = true;
				this.tabBar = tabBar;
				DockManager.Instance.FilesDropped += DropFiles;
				RebuildTabs(tabBar);
				tabBar.AddChangeWatcher(() => Project.Current.Documents.Version, _ => RebuildTabs(tabBar));
				tabBar.AddChangeWatcher(() => Project.Current, _ => RebuildTabs(tabBar));
			}

			private void DropFiles(IEnumerable<string> obj)
			{
				if (tabBar.IsMouseOverThisOrDescendant()) {
					foreach (var path in obj) {
						if (path.EndsWith(".scene") || path.EndsWith(".tan")) {
							Project.Current.OpenDocument(path, true);
						}
					}
				}
			}

			private void RebuildTabs(TabBar tabBar)
			{
				tabBar.Nodes.Clear();
				foreach (var doc in Project.Current.Documents) {
					var tab = new ThemedTab { Closable = true };
					var currentDocumentChanged = new Property<bool>(() => Document.Current == doc).DistinctUntilChanged().Where(i => i);
					tab.Tasks.Add(currentDocumentChanged.Consume(_ => tabBar.ActivateTab(tab)));
					tab.AddChangeWatcher(() => doc.Path, _ => RefreshTabText(doc, tab));
					tab.AddChangeWatcher(() => doc.IsModified, _ => RefreshTabText(doc, tab));
					tab.Clicked += doc.MakeCurrent;
					tab.Closing += () => Project.Current.CloseDocument(doc);
					tab.Updated += (dt) => {
						if (tab.Input.WasKeyReleased(Key.Mouse1)) {
							var menu = new Menu {
								GenericCommands.CloseDocument,
								GenericCommands.Save,
								FilesystemCommands.NavigateTo,
								FilesystemCommands.OpenInSystemFileManager,
							};
							var path = Path.Combine(Project.Current.AssetsDirectory, doc.Path);
							FilesystemCommands.NavigateTo.UserData = path;
							FilesystemCommands.OpenInSystemFileManager.UserData = path;
							menu.Popup();
						}
					};

					DragGesture dragGesture = new DragGesture();
					tab.Gestures.Add(dragGesture);
					dragGesture.Changed += () => {
						int index = -1;
						foreach (Tab tabEl in tabBar.Nodes.OfType<Tab>()) {
							index++;
							if (tabEl == tab) {
								continue;
							}

							Vector2 localMousePosition = tabEl.LocalMousePosition();

							if (!(localMousePosition.X >= 0 && localMousePosition.Y >= 0 &&
								localMousePosition.X < tabEl.Width && localMousePosition.Y < tabEl.Height)) {
								continue;
							}

							Project.Current.ReorderDocument(doc, index);

							int previousIndex = tabBar.Nodes.IndexOf(tab);
							int toIndex = tabBar.Nodes.IndexOf(tabEl);

							if (previousIndex < 0 || toIndex < 0) {
								RebuildTabs(tabBar);
								break;
							}

							tabBar.Nodes.Remove(tab);
							tabBar.Nodes.Insert(toIndex, tab);
							break;
						}
					};
					tabBar.AddNode(tab);
				}
				tabBar.AddNode(new Widget { LayoutCell = new LayoutCell { StretchX = 0 } });
			}

			void RefreshTabText(Document doc, Tab tab)
			{
				tab.Text = System.IO.Path.GetFileName(System.IO.Path.ChangeExtension(doc.Path, null));
				if (doc.IsModified) {
					tab.Text += '*';
				}
			}
		}

		static void LoadFont()
		{
			var fontData = new EmbeddedResource("Tangerine.Resources.SegoeUIRegular.ttf", "Tangerine").GetResourceBytes();
			var font = new DynamicFont(fontData);
			FontPool.Instance.AddFont("Default", font);
		}

		void RegisterGlobalCommands()
		{
			UI.Inspector.Inspector.RegisterGlobalCommands();
			UI.Timeline.CommandBindings.Bind();
			UI.SceneView.SceneView.RegisterGlobalCommands();

			var h = CommandHandlerList.Global;
			h.Connect(GenericCommands.New, new FileNew());
			h.Connect(GenericCommands.Open, new FileOpen());
			h.Connect(GenericCommands.OpenProject, new FileOpenProject());
			h.Connect(GenericCommands.Save, new FileSave());
			h.Connect(GenericCommands.SaveAs, new FileSaveAs());
			h.Connect(GenericCommands.Revert, new FileRevert());
			h.Connect(GenericCommands.UpgradeDocumentFormat, new UpgradeDocumentFormat());
			h.Connect(GenericCommands.CloseDocument, new FileClose());
			h.Connect(GenericCommands.Quit, Application.Exit);
			h.Connect(GenericCommands.PreferencesDialog, () => new PreferencesDialog());
			h.Connect(SceneViewCommands.DeleteRulers, new DeleteRulers());
			h.Connect(GenericCommands.Group, new GroupNodes());
			h.Connect(GenericCommands.Ungroup, new UngroupNodes());
			h.Connect(GenericCommands.InsertTimelineColumn, new InsertTimelineColumn());
			h.Connect(GenericCommands.RemoveTimelineColumn, new RemoveTimelineColumn());
			h.Connect(GenericCommands.NextDocument, new SetNextDocument());
			h.Connect(GenericCommands.PreviousDocument, new SetPreviousDocument());
			h.Connect(GenericCommands.DefaultLayout, new ViewDefaultLayout());
			h.Connect(GenericCommands.GroupContentsToMorphableMeshes, new GroupContentsToMorphableMeshes());
			h.Connect(GenericCommands.ExportScene, new ExportScene());
			h.Connect(GenericCommands.UpsampleAnimationTwice, new UpsampleAnimationTwice());
			h.Connect(Tools.AlignLeft, new AlignLeft());
			h.Connect(Tools.AlignRight, new AlignRight());
			h.Connect(Tools.AlignTop, new AlignTop());
			h.Connect(Tools.AlignBottom, new AlignBottom());
			h.Connect(Tools.CenterHorizontally, new CenterHorizontally());
			h.Connect(Tools.CenterVertically, new CenterVertically());
			h.Connect(Tools.AlignCentersHorizontally, new AlignCentersHorizontally());
			h.Connect(Tools.AlignCentersVertically, new AlignCentersVertically());
			h.Connect(Tools.RestoreOriginalSize, new RestoreOriginalSize());
			h.Connect(Tools.ResetScale, new ResetScale());
			h.Connect(Tools.ResetRotation, new ResetRotation());
			h.Connect(Tools.FitToContainer, new FitToContainer());
			h.Connect(Tools.FitToContent, new FitToContent());
			h.Connect(Tools.FlipX, new FlipX());
			h.Connect(Tools.FlipY, new FlipY());
			h.Connect(Command.Copy, Core.Operations.Copy.CopyToClipboard, () => Document.Current?.SelectedRows().Any() ?? false);
			h.Connect(Command.Cut, Core.Operations.Cut.Perform, () => Document.Current?.SelectedRows().Any() ?? false);
			h.Connect(Command.Paste, Paste, Document.HasCurrent);
			h.Connect(Command.Delete, Core.Operations.Delete.Perform, () => Document.Current?.SelectedRows().Any() ?? false);
			h.Connect(Command.SelectAll, () => {
				foreach (var row in Document.Current.Rows) {
					Core.Operations.SelectRow.Perform(row, true);
				}
			}, () => Document.Current?.Rows.Count > 0);
			h.Connect(Command.Undo, () => Document.Current.History.Undo(), () => Document.Current?.History.CanUndo() ?? false);
			h.Connect(Command.Redo, () => Document.Current.History.Redo(), () => Document.Current?.History.CanRedo() ?? false);
			h.Connect(OrangeCommands.Run, () => WidgetContext.Current.Root.Tasks.Add(OrangeTask));
			h.Connect(OrangeCommands.OptionsDialog, () => new OrangePluginOptionsDialog());
			h.Connect(SceneViewCommands.SnapWidgetBorderToRuler, new ToggleDisplayCommandHandler());
			h.Connect(SceneViewCommands.SnapWidgetPivotToRuler, new ToggleDisplayCommandHandler());
			h.Connect(SceneViewCommands.DeleteRulers, new DeleteRulers());
		}

		private IEnumerator<object> OrangeTask()
		{
			Tangerine.UI.Console.Instance.Show();
			Orange.The.Workspace?.AssetFiles?.Rescan();
			yield return Task.ExecuteAsync(() => {
				try {
					Orange.Actions.BuildAndRunAction();
				} catch (System.Exception e) {
					System.Console.WriteLine(e);
				}
			});
			System.Console.WriteLine("Done.");
		}

		static void Paste()
		{
			try {
				Core.Operations.Paste.Perform();
			} catch (InvalidOperationException e) {
				AlertDialog.Show(e.Message);
			}
		}
	}
}
