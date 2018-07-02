using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.UI;
using Tangerine.UI.Docking;

namespace Tangerine
{
	public class TangerineApp
	{
		public static TangerineApp Instance { get; private set; }
		public readonly Dictionary<string, Toolbar> Toolbars = new Dictionary<string, Toolbar>();
		public readonly DockManager.State DockManagerInitialState;

		public static void Initialize(string[] args)
		{
			Instance = new TangerineApp(args);
		}

		private TangerineApp(string[] args)
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
#if WIN
			TangerineSingleInstanceKeeper.Initialize(args);
			TangerineSingleInstanceKeeper.AnotherInstanceArgsRecieved += OpenDocumentsFromArgs;
			Application.Exited += () => {
				TangerineSingleInstanceKeeper.Instance.ReleaseInstance();
			};
#endif
			SetColorTheme(AppUserPreferences.Instance.Theme);

			LoadFont();

			DockManager.Initialize(new Vector2(1024, 768), TangerineMenu.PadsMenu);
			TangerineMenu.Create();
			var mainWidget = DockManager.Instance.MainWindowWidget;
			mainWidget.Components.Add(new RequestedDockingComponent());
			mainWidget.CompoundPostPresenter.Add(new DockingPresenter());
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

			var timelinePanel = new Panel("Timeline");
			var inspectorPanel = new Panel("Inspector");
			var searchPanel = new Panel("Search");
			var filesystemPanel = new Panel("Filesystem");
			var consolePanel = new Panel("Console");
			var documentPanel = new Panel(DockManager.DocumentAreaId, undockable: false);
			new UI.Console(consolePanel);

			var dockManager = DockManager.Instance;
			var root = dockManager.Model.WindowPlacements.First();
			var documentPlacement = dockManager.AppendPanelTo(documentPanel, root);
			dockManager.AddPanel(timelinePanel, documentPlacement, DockSite.Top, 0.4f);
			dockManager.AddPanel(inspectorPanel, documentPlacement, DockSite.Left, 0.3f);
			dockManager.AddPanel(searchPanel, documentPlacement, DockSite.Right, 0.3f);
			dockManager.AddPanel(filesystemPanel, documentPlacement, DockSite.Right, 0.3f);
			dockManager.AddPanel(consolePanel, documentPlacement, DockSite.Bottom, 0.3f);
			DockManagerInitialState = dockManager.ExportState();
			var documentViewContainer = InitializeDocumentArea(dockManager);
			documentPanel.ContentWidget.Nodes.Add(dockManager.DocumentArea);
			dockManager.ImportState(AppUserPreferences.Instance.DockState);
			dockManager.Refresh();
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
			new BackupsManager().Activate(Project.Tasks);
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
			dockManager.UnhandledExceptionOccurred += e => {
				AlertDialog.Show(e.Message + "\n" + e.StackTrace);
				if (Document.Current != null) {
					while (Document.Current.History.IsTransactionActive) {
						Document.Current.History.EndTransaction();
					}
					var closeConfirmation = Document.CloseConfirmation;
					try {
						Document.CloseConfirmation = doc => {
							var alert = new AlertDialog($"Save the changes to document '{doc.Path}' before closing?", "Yes", "No");
							switch (alert.Show()) {
								case 0: return Document.CloseAction.SaveChanges;
								default: return Document.CloseAction.DiscardChanges;
							}
						};
						var path = Document.Current.Path;
						Project.Current.CloseDocument(Document.Current);
						Project.Current.OpenDocument(path);
					} finally {
						Document.CloseConfirmation = closeConfirmation;
					}
				}
			};
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
				OpenDocumentsFromArgs(args);
			}
			WidgetContext.Current.Root.AddChangeWatcher(() => Project.Current, project => TangerineMenu.OnProjectChanged(project));

			WidgetContext.Current.Root.AddChangeWatcher(() => ProjectUserPreferences.Instance.RecentDocuments.Count == 0 ?
				null : ProjectUserPreferences.Instance.RecentDocuments[0], document => TangerineMenu.RebuildRecentDocumentsMenu());

			WidgetContext.Current.Root.AddChangeWatcher(() => AppUserPreferences.Instance.RecentProjects.Count == 0 ?
				null : AppUserPreferences.Instance.RecentProjects[0], document => TangerineMenu.RebuildRecentProjectsMenu());

			new UI.FilesystemView.FilesystemPane(filesystemPanel);
			RegisterGlobalCommands();
			
			HotkeyRegistry.InitCommands(typeof(GenericCommands), "Generic Commands");
			HotkeyRegistry.InitCommands(typeof(TimelineCommands), "Timeline Commands");
			HotkeyRegistry.InitCommands(typeof(InspectorCommands), "Inspector Commands");
			HotkeyRegistry.InitCommands(typeof(SceneViewCommands), "Scene View Commands");
			HotkeyRegistry.InitCommands(typeof(Tools), "Tools");
			HotkeyRegistry.InitCommands(typeof(FilesystemCommands), "Filesystem Commands");
			HotkeyRegistry.InitCommands(typeof(OrangeCommands), "Orange Commands");
			HotkeyRegistry.InitCommands(Command.Editing, "Editing", "Editing");
			if (File.Exists(HotkeyRegistry.Filepath)) {
				HotkeyRegistry.Load();
			} else {
				HotkeyRegistry.Save();
			}
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
							Document.Clicked = doc;
							var menu = new Menu {
								GenericCommands.CloseDocument,
								GenericCommands.CloseAllTabs,
								GenericCommands.CloseAllTabsExceptThis,
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
			var fontData = new EmbeddedResource("Tangerine.Resources.SegoeUI.ttf", "Tangerine").GetResourceBytes();
			var font = new DynamicFont(fontData);
			// Workaround. DynamicFont incorrectly applies fontHeight when rasterizing the font,
			// so the visual font height for the same fontHeight will be different for different ttf files.
			// This workaround returns the magic number for the specific current SegoeUINormal font.
			font.SetFontHeightResolver(fontHeight => (int) Math.Round(fontHeight * 21f / 16f));
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
			h.Connect(GenericCommands.SaveCurrent, new CurrentFileSave());
			h.Connect(GenericCommands.Save, new ClickedFileSave());
			h.Connect(GenericCommands.SaveAs, new FileSaveAs());
			h.Connect(GenericCommands.SaveAll, new AllFilesSave());
			h.Connect(GenericCommands.Revert, new FileRevert());
			h.Connect(GenericCommands.UpgradeDocumentFormat, new UpgradeDocumentFormat());
			h.Connect(GenericCommands.CloseCurrentDocument, new CurrentFileClose());
			h.Connect(GenericCommands.CloseDocument, new ClickedFileClose());
			h.Connect(GenericCommands.CloseAllTabs, new AllFilesClose());
			h.Connect(GenericCommands.CloseAllTabsExceptThis, new AllFilesCloseExceptThis());
			h.Connect(GenericCommands.CloseAllTabsExceptCurrent, new AllFilesCloseExceptCurrent());
			h.Connect(GenericCommands.Quit, Application.Exit);
			h.Connect(GenericCommands.PreferencesDialog, () => new PreferencesDialog());
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
			h.Connect(Command.Copy, new DocumentDelegateCommandHandler(Core.Operations.Copy.CopyToClipboard, IsCopyPasteAllowedForSelection));
			h.Connect(Command.Cut, new DocumentDelegateCommandHandler(Core.Operations.Cut.Perform, IsCopyPasteAllowedForSelection));
			h.Connect(Command.Paste, new DocumentDelegateCommandHandler(Paste, Document.HasCurrent));
			h.Connect(Command.Delete, new DocumentDelegateCommandHandler(Core.Operations.Delete.Perform, IsCopyPasteAllowedForSelection));
			h.Connect(Command.SelectAll, new DocumentDelegateCommandHandler(() => {
				foreach (var row in Document.Current.Rows) {
					Core.Operations.SelectRow.Perform(row, true);
				}
			}, () => Document.Current?.Rows.Count > 0));
			h.Connect(Command.Undo, () => Document.Current.History.Undo(), () => Document.Current?.History.CanUndo() ?? false);
			h.Connect(Command.Redo, () => Document.Current.History.Redo(), () => Document.Current?.History.CanRedo() ?? false);
			h.Connect(OrangeCommands.Run, new DocumentDelegateCommandHandler(() => WidgetContext.Current.Root.Tasks.Add(OrangeTask)));
			h.Connect(OrangeCommands.OptionsDialog, new DocumentDelegateCommandHandler(() => new OrangePluginOptionsDialog()));
			h.Connect(OrangeCommands.CookGameAssets, new DocumentDelegateCommandHandler(() => WidgetContext.Current.Root.Tasks.Add(CookingTask)));
			h.Connect(SceneViewCommands.SnapWidgetBorderToRuler, new SnapWidgetBorderCommandHandler());
			h.Connect(SceneViewCommands.SnapWidgetPivotToRuler, new SnapWidgetPivotCommandHandler());
			h.Connect(SceneViewCommands.SnapRulerLinesToWidgets, new SnapRulerLinesToWidgetCommandHandler());
			h.Connect(SceneViewCommands.ClearActiveRuler, new DocumentDelegateCommandHandler(ClearActiveRuler));
			h.Connect(SceneViewCommands.ManageRulers, new ManageRulers());
		}

		private static bool IsCopyPasteAllowedForSelection()
		{
			return Document.Current?.TopLevelSelectedRows().Any(row => row.IsCopyPasteAllowed()) ?? false;
		}

		private void ClearActiveRuler()
		{
			if (new AlertDialog("Are you sure you want to clear active ruler?", "Yes", "No").Show() == 0) {
				ProjectUserPreferences.Instance.ActiveRuler.Lines.Clear();
			}
		}

		private class SnapWidgetPivotCommandHandler : DocumentCommandHandler
		{
			public override bool GetChecked() => UI.SceneView.SceneUserPreferences.Instance.SnapWidgetPivotToRuler;
			public override void ExecuteTransaction()
			{
				var prefs = UI.SceneView.SceneUserPreferences.Instance;
				prefs.SnapWidgetPivotToRuler = !prefs.SnapWidgetPivotToRuler;
			}
		}

		private class SnapWidgetBorderCommandHandler : DocumentCommandHandler
		{
			public override bool GetChecked() => UI.SceneView.SceneUserPreferences.Instance.SnapWidgetBorderToRuler;
			public override void ExecuteTransaction()
			{
				var prefs = UI.SceneView.SceneUserPreferences.Instance;
				prefs.SnapWidgetBorderToRuler = !prefs.SnapWidgetBorderToRuler;
			}
		}

		private class SnapRulerLinesToWidgetCommandHandler : DocumentCommandHandler
		{
			public override bool GetChecked() => UI.SceneView.SceneUserPreferences.Instance.SnapRulerLinesToWidgets;
			public override void ExecuteTransaction()
			{
				var prefs = UI.SceneView.SceneUserPreferences.Instance;
				prefs.SnapRulerLinesToWidgets = !prefs.SnapRulerLinesToWidgets;
			}
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

		private IEnumerator<object> CookingTask()
		{
			Tangerine.UI.Console.Instance.Show();
			Orange.The.Workspace?.AssetFiles?.Rescan();
			yield return Task.ExecuteAsync(() => {
				try {
					Orange.AssetCooker.CookForActivePlatform();
				}
				catch (System.Exception e) {
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
				Document.Current.History.RollbackTransaction();
				AlertDialog.Show(e.Message);
			}
		}

		static void OpenDocumentsFromArgs(string[] args)
		{
			foreach (var arg in args) {
				if (File.Exists(arg)) {
					Project.Current?.OpenDocument(arg, pathIsGlobal: true);
				}
			}
		}
	}
}
