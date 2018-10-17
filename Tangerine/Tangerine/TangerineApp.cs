using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Lime;
using Tangerine.Core;
using Tangerine.UI;
using Tangerine.UI.SceneView;
using Tangerine.UI.Docking;
using Tangerine.UI.Timeline.Processors;
using Tangerine.UI.Timeline;

namespace Tangerine
{
	public class TangerineApp
	{
		public static TangerineApp Instance { get; private set; }
		public ToolbarView Toolbar { get; private set; }
		public readonly DockManager.State DockManagerInitialState;

		public static void Initialize(string[] args)
		{
			Instance = new TangerineApp(args);
		}

		private TangerineApp(string[] args)
		{
			Orange.UserInterface.Instance = new OrangeInterface();
			WidgetInput.AcceptMouseBeyondWidgetByDefault = false;
			TangerineYuzu.Instance.Value.DeserializerBuilders.Insert(0, DeserializeHotStudioAssets);
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
			switch (AppUserPreferences.Instance.ColorThemeKind) {
				case ColorTheme.ColorThemeKind.Light:
					SetColorTheme(ColorTheme.CreateLightTheme(), Theme.ColorTheme.CreateLightTheme());
					break;
				case ColorTheme.ColorThemeKind.Dark:
					SetColorTheme(ColorTheme.CreateDarkTheme(), Theme.ColorTheme.CreateDarkTheme());
					break;
				case ColorTheme.ColorThemeKind.Custom: {
					bool isDark = AppUserPreferences.Instance.ColorTheme.IsDark;
					ColorTheme theme = null;
					var flags =
						BindingFlags.Public |
						BindingFlags.GetProperty |
						BindingFlags.SetProperty |
						BindingFlags.Instance;
					foreach (var category in typeof(ColorTheme).GetProperties(flags)) {
						if (category.Name == "Basic") {
							continue;
						}
						var categoryValue = category.GetValue(AppUserPreferences.Instance.ColorTheme);
						if (categoryValue == null) {
							if (theme == null) {
								theme = isDark ? ColorTheme.CreateDarkTheme() : ColorTheme.CreateLightTheme();
							}
							category.SetValue(AppUserPreferences.Instance.ColorTheme, category.GetValue(theme));
							category.SetValue(theme, null);
						}
					}
					SetColorTheme(AppUserPreferences.Instance.ColorTheme, AppUserPreferences.Instance.LimeColorTheme);
					break;
				}

			}
			Application.InvalidateWindows();

			LoadFont();

			DockManager.Initialize(new Vector2(1024, 768));
			TangerineMenu.Create();
			var mainWidget = DockManager.Instance.MainWindowWidget;
			mainWidget.Components.Add(new RequestedDockingComponent());
			mainWidget.CompoundPostPresenter.Add(new DockingPresenter());
			mainWidget.Window.AllowDropFiles = true;
			mainWidget.AddChangeWatcher(() => Project.Current, _ => {
				SetupMainWindowTitle(mainWidget);
				TangerineMenu.RebuildCreateImportedTypeMenu();
			});
			mainWidget.AddChangeWatcher(() => CoreUserPreferences.Instance.AnimationMode, _ => Document.ForceAnimationUpdate());
			mainWidget.AddChangeWatcher(() => Document.Current?.Container, _ => Document.ForceAnimationUpdate());

			Application.Exiting += () => Project.Current.Close();
			Application.Exited += () => {
				AppUserPreferences.Instance.DockState = DockManager.Instance.ExportState();
				SceneUserPreferences.Instance.VisualHintsRegistry = VisualHintsRegistry.Instance;
				Core.UserPreferences.Instance.Save();
			};

			var timelinePanel = new Panel("Timeline");
			var inspectorPanel = new Panel("Inspector");
			var searchPanel = new Panel("Hierarchy");
			var filesystemPanel = new Panel("Filesystem");
			var consolePanel = new Panel("Console");
			var backupHistoryPanel = new Panel("Backups");
			var documentPanel = new Panel(DockManager.DocumentAreaId, undockable: false);
			var visualHintsPanel = new Panel("Visual Hints");
			var dockManager = DockManager.Instance;
			new UI.Console(consolePanel);
			var root = dockManager.Model.WindowPlacements.First();
			var documentPlacement = dockManager.AppendPanelTo(documentPanel, root);
			var commandHandlerList = CommandHandlerList.Global;
			var commandsDictionary = new Dictionary<string, Command> {
				{ timelinePanel.Id, new Command(timelinePanel.Title) },
				{ inspectorPanel.Id, new Command(inspectorPanel.Title) },
				{ searchPanel.Id, new Command(searchPanel.Title) },
				{ filesystemPanel.Id, new Command(filesystemPanel.Title) },
				{ consolePanel.Id, new Command(consolePanel.Title) },
				{ backupHistoryPanel.Id, new Command(backupHistoryPanel.Title) },
				{ visualHintsPanel.Id, new Command(visualHintsPanel.Title) },
			};
			foreach (var pair in commandsDictionary) {
				commandHandlerList.Connect(pair.Value, new PanelCommandHandler(pair.Key));
				TangerineMenu.PadsMenu.Add(pair.Value);
			}
			dockManager.AddPanel(timelinePanel, documentPlacement, DockSite.Top, 0.3f);
			dockManager.AddPanel(inspectorPanel, documentPlacement, DockSite.Left);
			var filesystemPlacement = dockManager.AddPanel(filesystemPanel, documentPlacement, DockSite.Right, 0.3f);
			dockManager.AddPanel(searchPanel, filesystemPlacement, DockSite.Fill);
			dockManager.AddPanel(backupHistoryPanel, filesystemPlacement, DockSite.Fill);
			dockManager.AddPanel(consolePanel, filesystemPlacement, DockSite.Bottom, 0.3f);
			dockManager.AddPanel(visualHintsPanel, documentPlacement, DockSite.Right, 0.3f).Hidden = true;
			DockManagerInitialState = dockManager.ExportState();
			var documentViewContainer = InitializeDocumentArea(dockManager);
			documentPanel.ContentWidget.Nodes.Add(dockManager.DocumentArea);
			dockManager.ImportState(AppUserPreferences.Instance.DockState);
			dockManager.ResolveAndRefresh();
			Document.CloseConfirmation += doc => {
				var alert = new AlertDialog($"Save the changes to document '{doc.Path}' before closing?", "Yes", "No", "Cancel");
				switch (alert.Show()) {
					case 0: return Document.CloseAction.SaveChanges;
					case 1: return Document.CloseAction.DiscardChanges;
					case -1:
					default: return Document.CloseAction.Cancel;
				}
			};
			Project.HandleMissingDocuments += missingDocuments => {
				while (missingDocuments.Any()) {
					var nextDocument = missingDocuments.First();
					bool loaded = nextDocument.Loaded;
					string path = nextDocument.Path;
					if (loaded) {
						Document.SetCurrent(nextDocument);
						path = nextDocument.FullPath;
					} else {
						if (Project.Current.GetFullPath(nextDocument.Path, out string fullPath)) {
							path = fullPath;
						}
					}
					path = path.Replace('\\', '/');
					var choices = loaded ? new [] { "Save", "Save All", "Discard", "Discard All" } : new [] { "Locate", "Discard", "Discard All" };
					var alert = new AlertDialog($"Document {path} has been moved or deleted.", choices);
					var r = alert.Show();
					if (loaded && r == 0) {
						// Save
						Directory.CreateDirectory(Path.GetDirectoryName(nextDocument.FullPath));
						nextDocument.Save();
					} else if (loaded && r == 1) {
						// Save All
						while (missingDocuments.Any()) {
							var d = missingDocuments.First();
							Directory.CreateDirectory(Path.GetDirectoryName(d.FullPath));
							d.Save();
						}
					} else if (loaded && r == 2 || !loaded && r == 1) {
						// Discard
						Project.Current.CloseDocument(nextDocument);
					} else if (loaded && r == 3 || !loaded && r == 2) {
						// Discard All
						while (missingDocuments.Any()) {
							Project.Current.CloseDocument(missingDocuments.First());
						}
					} else if (!loaded && r == 0) {
						// Locate
						var dialog = new Lime.FileDialog {
							AllowsMultipleSelection = false,
							AllowedFileTypes = Document.AllowedFileTypes,
							InitialDirectory = Project.Current.AssetsDirectory,
							InitialFileName = Path.GetFileName(path),
							Mode = FileDialogMode.Open,
						};
						if (dialog.RunModal()) {
							var newPath = dialog.FileName;
							newPath = Project.Current.GetLocalDocumentPath(newPath, System.IO.Path.IsPathRooted(newPath) &&
								!System.IO.Path.GetPathRoot(newPath).Equals(System.IO.Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal));
							if (!string.IsNullOrEmpty(newPath) && !Project.Current.Documents.Any(d => d.Path == newPath)) {
								nextDocument.Relocate(newPath);
							} else {
								alert = new AlertDialog($"Invalid document path: {newPath}");
								alert.Show();
							}
						}
					}
				}
			};
			Project.DocumentReloadConfirmation += doc => {
				if (doc.IsModified) {
					var modifiedAlert = new AlertDialog($"{doc.Path}\n\nThis file has been modified by another program and has unsaved changes.\nDo you want to reload it from disk? ", "Yes", "No");
					var res = modifiedAlert.Show();
					if (res == 1 || res == -1) {
						doc.History.ExternalModification();
						return false;
					}
					return true;
				}
				if (CoreUserPreferences.Instance.ReloadModifiedFiles) {
					return true;
				}
				var alert = new AlertDialog($"{doc.Path}\n\nThis file has been modified by another program.\nDo you want to reload it from disk? ", "Yes, always", "Yes", "No");
				var r = alert.Show();
				if (r == 0) {
					CoreUserPreferences.Instance.ReloadModifiedFiles = true;
					return true;
				}
				if (r == 2) {
					doc.History.ExternalModification();
					return false;
				}
				return true;
			};

			Project.TempFileLoadConfirmation += path => {
				var alert = new AlertDialog($"Do you want to load autosaved version of '{path}'?", "Yes", "No");
				return alert.Show() == 0;
			};

			Project.OpenFileOutsideProjectAttempt += (string filePath) => {
				var projectFilePath = SearhForCitproj(filePath);
				if (projectFilePath != null && Project.Current.CitprojPath != projectFilePath) {
					var alert = new AlertDialog($"You're trying to open a document outside the project directory. Change the current project to '{Path.GetFileName(projectFilePath)}'?", "Yes", "No");
					if (alert.Show() == 0) {
						if (FileOpenProject.Execute(projectFilePath)) {
							Project.Current.OpenDocument(filePath, true);
						}
						return;
					}
				}
				else if (projectFilePath == null) {
					AlertDialog.Show("Can't open a document outside the project directory");
				}
			};
			Project.Tasks = dockManager.MainWindowWidget.Tasks;
			Project.Tasks.Add(new AutosaveProcessor(() => AppUserPreferences.Instance.AutosaveDelay));
			BackupManager.Instance.Activate(Project.Tasks);
			Document.NodeDecorators.AddFor<Spline>(n => n.CompoundPostPresenter.Add(new UI.SceneView.SplinePresenter()));
			Document.NodeDecorators.AddFor<Viewport3D>(n => n.CompoundPostPresenter.Add(new UI.SceneView.Spline3DPresenter()));
			Document.NodeDecorators.AddFor<Viewport3D>(n => n.CompoundPostPresenter.Add(new UI.SceneView.Animation3DPathPresenter()));
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

			if (SceneUserPreferences.Instance.VisualHintsRegistry != null) {
				VisualHintsRegistry.Instance = SceneUserPreferences.Instance.VisualHintsRegistry;
			}
			VisualHintsRegistry.Instance.RegisterDefaultHints();

			Document.NodeDecorators.AddFor<Node>(n => n.SetTangerineFlag(TangerineFlags.SceneNode, true));
			dockManager.UnhandledExceptionOccurred += e => {
				AlertDialog.Show(e.Message + "\n" + e.StackTrace);
				var doc = Document.Current;
				if (doc != null) {
					while (doc.History.IsTransactionActive) {
						doc.History.EndTransaction();
					}
					var closeConfirmation = Document.CloseConfirmation;
					try {
						Document.CloseConfirmation = d => {
							var alert = new AlertDialog($"Save the changes to document '{d.Path}' before closing?", "Yes", "No");
							switch (alert.Show()) {
								case 0: return Document.CloseAction.SaveChanges;
								default: return Document.CloseAction.DiscardChanges;
							}
						};
						var fullPath = doc.FullPath;

						if (!File.Exists(fullPath)) {
							Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
							doc.Save();
						}
						var path = doc.Path;
						Project.Current.CloseDocument(doc);
						Project.Current.OpenDocument(path);
					} finally {
						Document.CloseConfirmation = closeConfirmation;
					}
				}
			};

			Document.NodeDecorators.AddFor<ParticleEmitter>(n => n.CompoundPostPresenter.Add(new UI.SceneView.ParticleEmitterPresenter()));
			DocumentHistory.AddOperationProcessorTypes(new[] {
				typeof(Core.Operations.TimelineHorizontalShift.Processor),
				typeof(Core.Operations.RemoveKeyframeRange.Processor),
				typeof(Core.Operations.SelectRow.Processor),
				typeof(Core.Operations.SetProperty.Processor),
				typeof(Core.Operations.SetIndexedProperty.Processor),
				typeof(Core.Operations.RemoveKeyframe.Processor),
				typeof(Core.Operations.SetKeyframe.Processor),
				typeof(Core.Operations.InsertFolderItem.Processor),
				typeof(Core.Operations.InsertIntoList.Processor),
				typeof(Core.Operations.RemoveFromList.Processor),
				typeof(Core.Operations.UnlinkFolderItem.Processor),
				typeof(Core.Operations.MoveNodes.Processor),
				typeof(Core.Operations.SetMarker.Processor),
				typeof(Core.Operations.DeleteMarker.Processor),
				typeof(Core.Operations.SetComponent.Processor),
				typeof(Core.Operations.DeleteComponent.Processor),
				typeof(Core.Operations.DistortionMeshProcessor),
				typeof(Core.Operations.SyncFolderDescriptorsProcessor),
				typeof(UI.SceneView.ResolutionPreviewOperation.Processor),
				typeof(UI.Timeline.Operations.SelectGridSpan.Processor),
				typeof(UI.Timeline.Operations.DeselectGridSpan.Processor),
				typeof(UI.Timeline.Operations.ClearGridSelection.Processor),
				typeof(UI.Timeline.Operations.ShiftGridSelection.Processor),
				typeof(UI.Timeline.Operations.SetCurrentColumn.Processor),
				typeof(UI.Timeline.Operations.SelectCurveKey.Processor),
				typeof(TriggersValidatorOnSetProperty),
				typeof(TriggersValidatorOnSetKeyframe),
				typeof(UpdateNodesAndApplyAnimatorsProcessor),
				typeof(RowsSynchronizer),
				typeof(Core.Operations.ReplaceContents.Processor)
			});
			DocumentHistory.AddOperationProcessorTypes(UI.Timeline.Timeline.GetOperationProcessorTypes());

			RegisterCommands();
			InitializeHotkeys();

			AppUserPreferences.Instance.ToolbarModel.RefreshAfterLoad();
			Toolbar = new ToolbarView(dockManager.ToolbarArea, AppUserPreferences.Instance.ToolbarModel);
			RefreshCreateNodeCommands();
			Document.AttachingViews += doc => {
				if (doc.Views.Count == 0) {
					doc.Views.AddRange(new IDocumentView[] {
						new UI.Inspector.Inspector(inspectorPanel.ContentWidget),
						new UI.Timeline.Timeline(timelinePanel),
						new UI.SceneView.SceneView(documentViewContainer),
						new Panels.HierarchyPanel(searchPanel.ContentWidget),
						new Panels.BackupHistoryPanel(backupHistoryPanel.ContentWidget),
						// Use VisualHintsPanel sigleton because we need preserve its state between documents.
						VisualHintsPanel.Instance ?? VisualHintsPanel.Initialize(visualHintsPanel)
				});
					UI.SceneView.SceneView.ShowNodeDecorationsPanelButton.Clicked = () => dockManager.TogglePanel(visualHintsPanel);
				}
			};
			var proj = AppUserPreferences.Instance.RecentProjects.FirstOrDefault();
			if (proj != null) {
				try {
					new Project(proj).Open();
				} catch {
					AlertDialog.Show($"Cannot open project '{proj}'. It may be deleted or be otherwise unavailable.");
				}
			}
			OpenDocumentsFromArgs(args);
			WidgetContext.Current.Root.AddChangeWatcher(() => Project.Current, project => TangerineMenu.OnProjectChanged(project));

			WidgetContext.Current.Root.AddChangeWatcher(() => ProjectUserPreferences.Instance.RecentDocuments.Count == 0 ?
				null : ProjectUserPreferences.Instance.RecentDocuments[0], document => TangerineMenu.RebuildRecentDocumentsMenu());

			WidgetContext.Current.Root.AddChangeWatcher(() => AppUserPreferences.Instance.RecentProjects.Count == 0 ?
				null : AppUserPreferences.Instance.RecentProjects[0], document => TangerineMenu.RebuildRecentProjectsMenu());

			new UI.FilesystemView.FilesystemPane(filesystemPanel);
			RegisterGlobalCommands();

			Documentation.Init();
			DocumentationComponent.Clicked = page => Documentation.ShowHelp(page);
		}

		private void OpenDocumentsFromArgs(string[] args)
		{
			foreach (var arg in args) {
				if (Path.GetExtension(arg) == ".citproj") {
					FileOpenProject.Execute(arg);
				} else {
					Project.Current.OpenDocument(arg, pathIsAbsolute: true);
				}
			}
		}

		private string SearhForCitproj(string filePath)
		{
			var path = filePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			path[0] += Path.DirectorySeparatorChar;
			for (int i = path.Length - 2; i >= 0; i--) {
				var ppp = Path.Combine(path.Take(i + 1).ToArray());
				var projectFileCandidates = Directory.GetFiles(ppp, "*.citproj", SearchOption.TopDirectoryOnly);
				if (projectFileCandidates.Length > 0) {
					return projectFileCandidates[0];
				}
			}
			return null;
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

		void SetColorTheme(ColorTheme theme, Theme.ColorTheme limeTheme)
		{
			AppUserPreferences.Instance.LimeColorTheme = Theme.Colors = limeTheme.Clone();
			AppUserPreferences.Instance.ColorTheme = ColorTheme.Current = theme.Clone();
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

		public void RefreshCreateNodeCommands()
		{
			Toolbar.Rebuild();
			HotkeyRegistry.InitDefaultShortcuts();
			HotkeyRegistry.UpdateProfiles();
			UI.SceneView.VisualHintsPanel.Refresh();
		}

		void RegisterCommands()
		{
			RegisterCommands(typeof(TimelineCommands));
			RegisterCommands(typeof(InspectorCommands));
			RegisterCommands(typeof(GenericCommands));
			RegisterCommands(typeof(SceneViewCommands));
			RegisterCommands(typeof(Tools));
			RegisterCommands(typeof(OrangeCommands));
			RegisterCommands(typeof(FilesystemCommands));
			CommandRegistry.Register(Command.Undo, "GenericCommands", "Undo");
			CommandRegistry.Register(Command.Redo, "GenericCommands", "Redo");
		}

		void RegisterCommands(Type type)
		{
			foreach (var field in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)) {
				var fieldType = field.FieldType;
				if (!(fieldType == typeof(ICommand) || fieldType.IsSubclassOf(typeof(ICommand)))) {
					continue;
				}
				CommandRegistry.Register((ICommand)field.GetValue(null), type.Name, field.Name);
			}
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
				if (tabBar.IsMouseOverThisOrDescendant() || Document.Current == null) {
					foreach (var path in obj) {
						if (path.EndsWith(".scene") || path.EndsWith(".tan")) {
							try {
								Project.Current.OpenDocument(path, true);
							} catch (System.InvalidOperationException e) {
								AlertDialog.Show(e.Message);
							}
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
					tab.AddChangeWatcher(() => doc.DisplayName, _ => tab.Text = doc.DisplayName);
					tab.Clicked += doc.MakeCurrent;
					tab.Closing += () => Project.Current.CloseDocument(doc);
					tab.Updated += (dt) => {
						if (tab.Input.WasKeyReleased(Key.Mouse1)) {
							DocumentTabContextMenu.Create(doc);
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
		}

		public static void LoadFont()
		{
			var fontData = new EmbeddedResource("Tangerine.Resources.SegoeUI.ttf", "Tangerine").GetResourceBytes();
			var font = new DynamicFont(fontData);
			// Workaround. DynamicFont incorrectly applies fontHeight when rasterizing the font,
			// so the visual font height for the same fontHeight will be different for different ttf files.
			// This workaround returns the magic number for the specific current SegoeUINormal font.
			font.SetFontHeightResolver(fontHeight => (int) Math.Round(fontHeight * 21f / 16f));
			FontPool.Instance.AddFont(FontPool.DefaultFontName, font);
		}

		void RegisterGlobalCommands()
		{
			UI.Inspector.Inspector.RegisterGlobalCommands();
			UI.Timeline.CommandBindings.Bind();
			UI.SceneView.SceneView.RegisterGlobalCommands();

			var h = CommandHandlerList.Global;
			h.Connect(GenericCommands.NewProject, new ProjectNew());
			h.Connect(GenericCommands.NewScene, new FileNew());
			h.Connect(GenericCommands.NewTan, new FileNew(DocumentFormat.Tan));
			h.Connect(GenericCommands.Open, new FileOpen());
			h.Connect(GenericCommands.OpenProject, new FileOpenProject());
			h.Connect(GenericCommands.Save, new FileSave());
			h.Connect(GenericCommands.CloseProject, new FileCloseProject());
			h.Connect(GenericCommands.SaveAs, new FileSaveAs());
			h.Connect(GenericCommands.SaveAll, new FileSaveAll());
			h.Connect(GenericCommands.Revert, new FileRevert());
			h.Connect(GenericCommands.UpgradeDocumentFormat, new UpgradeDocumentFormat());
			h.Connect(GenericCommands.Close, new FileClose());
			h.Connect(GenericCommands.CloseAll, new FileCloseAll());
			h.Connect(GenericCommands.CloseAllButCurrent, new FileCloseAllButCurrent());
			h.Connect(GenericCommands.Quit, Application.Exit);
			h.Connect(GenericCommands.PreferencesDialog, () => new PreferencesDialog());
			h.Connect(GenericCommands.Group, new GroupNodes());
			h.Connect(GenericCommands.Ungroup, new UngroupNodes());
			h.Connect(GenericCommands.InsertTimelineColumn, new InsertTimelineColumn());
			h.Connect(GenericCommands.RemoveTimelineColumn, new RemoveTimelineColumn());
			h.Connect(GenericCommands.NextDocument, new SetNextDocument());
			h.Connect(GenericCommands.PreviousDocument, new SetPreviousDocument());
			h.Connect(GenericCommands.DefaultLayout, new ViewDefaultLayout());
			h.Connect(GenericCommands.SaveLayout, new SaveLayout());
			h.Connect(GenericCommands.LoadLayout, new LoadLayout());
			h.Connect(GenericCommands.GroupContentsToMorphableMeshes, new GroupContentsToMorphableMeshes());
			h.Connect(GenericCommands.ExportScene, new ExportScene());
			h.Connect(GenericCommands.UpsampleAnimationTwice, new UpsampleAnimationTwice());
			h.Connect(GenericCommands.ViewHelp, () => Documentation.ShowHelp(Documentation.StartPageName));
			h.Connect(GenericCommands.HelpMode, () => Documentation.IsHelpModeOn = !Documentation.IsHelpModeOn);
			h.Connect(GenericCommands.ViewChangelog, () => Documentation.ShowHelp(Documentation.ChangelogPageName));
			h.Connect(GenericCommands.ConvertToButton, new ConvertToButton());
			h.Connect(Tools.AlignLeft, new AlignLeft());
			h.Connect(Tools.AlignRight, new AlignRight());
			h.Connect(Tools.AlignTop, new AlignTop());
			h.Connect(Tools.AlignBottom, new AlignBottom());
			h.Connect(Tools.CenterHorizontally, new CenterHorizontally());
			h.Connect(Tools.CenterVertically, new CenterVertically());
			h.Connect(Tools.DistributeLeft, new DistributeLeft());
			h.Connect(Tools.DistributeHorizontally, new DistributeCenterHorizontally());
			h.Connect(Tools.DistributeRight, new DistributeRight());
			h.Connect(Tools.DistributeTop, new DistributeTop());
			h.Connect(Tools.DistributeVertically, new DistributeCenterVertically());
			h.Connect(Tools.DistributeBottom, new DistributeBottom());
			h.Connect(Tools.CenterVertically, new DistributeCenterVertically());
			h.Connect(Tools.AlignCentersHorizontally, new AlignCentersHorizontally());
			h.Connect(Tools.AlignCentersVertically, new AlignCentersVertically());
			h.Connect(Tools.AlignTo, new AlignAndDistributeToHandler(Tools.AlignTo));
			h.Connect(Tools.CenterAlignTo, new CenterToHandler(Tools.CenterAlignTo));
			h.Connect(Tools.RestoreOriginalSize, new RestoreOriginalSize());
			h.Connect(Tools.ResetScale, new ResetScale());
			h.Connect(Tools.ResetRotation, new ResetRotation());
			h.Connect(Tools.FitToContainer, new FitToContainer());
			h.Connect(Tools.FitToContent, new FitToContent());
			h.Connect(Tools.FlipX, new FlipX());
			h.Connect(Tools.FlipY, new FlipY());
			h.Connect(Tools.CenterView, new CenterView());
			h.Connect(Command.Copy, Core.Operations.Copy.CopyToClipboard, IsCopyPasteAllowedForSelection);
			h.Connect(Command.Cut, new DocumentDelegateCommandHandler(Core.Operations.Cut.Perform, IsCopyPasteAllowedForSelection));
			h.Connect(Command.Paste, new DocumentDelegateCommandHandler(() => Paste(), Document.HasCurrent));
			h.Connect(Command.Delete, new DocumentDelegateCommandHandler(Core.Operations.Delete.Perform, IsCopyPasteAllowedForSelection));
			h.Connect(Command.SelectAll, new DocumentDelegateCommandHandler(() => {
				foreach (var row in Document.Current.Rows) {
					Core.Operations.SelectRow.Perform(row, true);
				}
			}, () => Document.Current?.Rows.Count > 0));
			h.Connect(Command.Undo, () => Document.Current.History.Undo(), () => Document.Current?.History.CanUndo() ?? false);
			h.Connect(Command.Redo, () => Document.Current.History.Redo(), () => Document.Current?.History.CanRedo() ?? false);
			h.Connect(SceneViewCommands.PasteAtOldPosition, new DocumentDelegateCommandHandler(() => Paste(pasteAtMouse: false), Document.HasCurrent));
			h.Connect(SceneViewCommands.SnapWidgetBorderToRuler, new SnapWidgetBorderCommandHandler());
			h.Connect(SceneViewCommands.SnapWidgetPivotToRuler, new SnapWidgetPivotCommandHandler());
			h.Connect(SceneViewCommands.SnapRulerLinesToWidgets, new SnapRulerLinesToWidgetCommandHandler());
			h.Connect(SceneViewCommands.ClearActiveRuler, new DocumentDelegateCommandHandler(ClearActiveRuler));
			h.Connect(SceneViewCommands.ManageRulers, new ManageRulers());
			h.Connect(SceneViewCommands.GeneratePreview, new GeneratePreview());
			h.Connect(TimelineCommands.CutKeyframes, UI.Timeline.Operations.CutKeyframes.Perform);
			h.Connect(TimelineCommands.CopyKeyframes, UI.Timeline.Operations.CopyKeyframes.Perform);
			h.Connect(TimelineCommands.PasteKeyframes, UI.Timeline.Operations.PasteKeyframes.Perform);
			h.Connect(TimelineCommands.ReverseKeyframes, UI.Timeline.Operations.ReverseKeyframes.Perform);
			h.Connect(TimelineCommands.CreatePositionKeyframe, UI.Timeline.Operations.TogglePositionKeyframe.Perform);
			h.Connect(TimelineCommands.CreateRotationKeyframe, UI.Timeline.Operations.ToggleRotationKeyframe.Perform);
			h.Connect(TimelineCommands.CreateScaleKeyframe, UI.Timeline.Operations.ToggleScaleKeyframe.Perform);
			h.Connect(TimelineCommands.CenterTimelineOnCurrentColumn, UI.Timeline.Operations.CenterTimelineOnCurrentColumn.Perform);
			h.Connect(TimelineCommands.ShowModel3DAttachmentDialog, () => {
				var model = (Model3D)TimelineCommands.ShowModel3DAttachmentDialog.UserData;
				TimelineCommands.ShowModel3DAttachmentDialog.UserData = null;
				AttachmentDialog.ShowFor(model);
			});
			h.Connect(OrangeCommands.RunConfig, new OrangeCommandHandler(() => new OrangePluginOptionsDialog()));
			h.Connect(SceneViewCommands.ToggleDisplayRuler, new DisplayRuler());
			h.Connect(SceneViewCommands.SaveCurrentRuler, new SaveRuler());
			h.Connect(TimelineCommands.NumericMove, () => new NumericMoveDialog());
			h.Connect(TimelineCommands.NumericScale, () => new NumericScaleDialog());
		}

		private void InitializeHotkeys()
		{
			string dir = HotkeyRegistry.ProfilesDirectory;
			Directory.CreateDirectory(dir);
			HotkeyRegistry.InitDefaultShortcuts();
			var defaultProfile = HotkeyRegistry.CreateProfile(HotkeyRegistry.DefaultProfileName);
			if (File.Exists(defaultProfile.Filepath)) {
				defaultProfile.Load();
			}
			else {
				defaultProfile.Save();
			}
			HotkeyRegistry.Profiles.Add(defaultProfile);
			foreach (string file in Directory.EnumerateFiles(dir)) {
				string name = Path.GetFileName(file);
				if (name == HotkeyRegistry.DefaultProfileName) {
					continue;
				}
				var profile = HotkeyRegistry.CreateProfile(name);
				profile.Load();
				HotkeyRegistry.Profiles.Add(profile);
			}
			var currentProfile = HotkeyRegistry.Profiles.FirstOrDefault(i => i.Name == AppUserPreferences.Instance.CurrentHotkeyProfile);
			if (currentProfile != null) {
				HotkeyRegistry.CurrentProfile = currentProfile;
			}
			else {
				HotkeyRegistry.CurrentProfile = defaultProfile;
			}
		}

		private static bool IsCopyPasteAllowedForSelection()
		{
			return (Document.Current?.InspectRootNode ?? false) || (Document.Current?.TopLevelSelectedRows().Any(row => row.IsCopyPasteAllowed()) ?? false);
		}

		private void ClearActiveRuler()
		{
			if (new AlertDialog("Are you sure you want to clear active ruler?", "Yes", "No").Show() == 0) {
				ProjectUserPreferences.Instance.ActiveRuler.Lines.Clear();
			}
		}

		static void Paste(bool pasteAtMouse = true)
		{
			try {
				Core.Operations.Paste.Perform(
					pasteAtMouse:
						UI.SceneView.SceneView.Instance.InputArea.IsMouseOverThisOrDescendant() &&
						pasteAtMouse &&
						!CoreUserPreferences.Instance.DontPasteAtMouse
				);
			} catch (InvalidOperationException e) {
				Document.Current.History.RollbackTransaction();
				AlertDialog.Show(e.Message);
			}
		}
	}
}
