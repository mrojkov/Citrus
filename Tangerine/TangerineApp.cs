using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using Yuzu;
using System.Collections.Generic;
using System.Collections.Specialized;
using Tangerine.UI;

namespace Tangerine
{
	public class TangerineApp
	{
		public static TangerineApp Instance { get; private set; }
		public readonly Menu PadsMenu;
		public readonly Dictionary<string, Toolbar> Toolbars = new Dictionary<string, Toolbar>();
		public readonly DockManager.State DockManagerInitialState;

		public static void Initialize()
		{
			Instance = new TangerineApp();
		}

		private TangerineApp()
		{
			WindowOptions.DefaultRefreshRate = 30;
			WidgetInput.AcceptMouseBeyondWidgetByDefault = false;
			Application.IsTangerine = true;
			Lime.Serialization.DeserializerBuilders.Insert(0, DeserializeHotStudioAssets);
			Widget.DefaultWidgetSize = Vector2.Zero;
			Theme.Current = new DesktopTheme();
			LoadFont();

			PadsMenu = new Submenu("Pads");
			DockManager.Initialize(new Vector2(1024, 768), PadsMenu);
			DockManager.Instance.DockPanelAdded += panel => AddGlobalProcessors(panel.RootWidget);
			AddGlobalProcessors(DockManager.Instance.DocumentArea);
			CreateMainMenu();

			Application.Exiting += () => Project.Current.Close();
			Application.Exited += () => {
				UserPreferences.Instance.DockState = DockManager.Instance.ExportState();
				UserPreferences.Instance.Save();
			};
			var timelinePanel = new DockPanel("Timeline");
			var inspectorPanel = new DockPanel("Inspector");
			var consolePanel = new DockPanel("Console");
			var searchPanel = new DockPanel("SearchPanel");

			var dockManager = DockManager.Instance;
			dockManager.AddPanel(timelinePanel, DockSite.Top, new Vector2(800, 300));
			dockManager.AddPanel(inspectorPanel, DockSite.Left, new Vector2(300, 700));
			dockManager.AddPanel(searchPanel, DockSite.Right, new Vector2(300, 700));
			dockManager.AddPanel(consolePanel, DockSite.Right, new Vector2(300, 700));
			DockManagerInitialState = dockManager.ExportState();
			var documentViewContainer = InitializeDocumentArea(dockManager);

			UserPreferences.Initialize();
			dockManager.ImportState(UserPreferences.Instance.DockState);
			Document.Closing += doc => {
				var alert = new AlertDialog("Tangerine", $"Save the changes to document '{doc.Path}' before closing?", "Yes", "No", "Cancel");
				switch (alert.Show()) {
					case 0: return Document.CloseAction.SaveChanges;
					case 1: return Document.CloseAction.DiscardChanges;
					default: return Document.CloseAction.Cancel;
				}
			};
			Toolbars.Add("Create", new Toolbar(dockManager.ToolbarArea));
			Toolbars.Add("Tools", new Toolbar(dockManager.ToolbarArea));
			foreach (var c in Application.MainMenu.FindCommand("Create").Submenu) {
				Toolbars["Create"].Add(c);
			}
			CreateToolsToolbar();
			Document.AttachingViews += doc => {
				if (doc.Views.Count == 0) {
					doc.Views.AddRange(new IDocumentView [] {
						new UI.Inspector.Inspector(inspectorPanel.ContentWidget),
						new UI.Timeline.Timeline(timelinePanel.ContentWidget),
						new UI.SceneView.SceneView(documentViewContainer),
						new UI.Console(consolePanel.ContentWidget),
						new UI.SearchPanel(searchPanel.ContentWidget),
					});
					doc.History.Changed += InvalidateWindows;
				}
				RefreshExternalContent(doc.RootNode);
				InvalidateWindows();
			};
			var proj = UserPreferences.Instance.RecentProjects.FirstOrDefault();
			if (proj != null) {
				new Project(proj).Open();
			}
		}

		void CreateToolsToolbar()
		{
			var tb = Toolbars["Tools"];
			tb.Add(CenterHorizontally.Instance);
			tb.Add(CenterVertically.Instance);
			tb.Add(AlignCentersHorizontally.Instance);
			tb.Add(AlignCentersVertically.Instance);
		}

		Yuzu.AbstractDeserializer DeserializeHotStudioAssets(string path, System.IO.Stream stream)
		{
			if (path.EndsWith(".scene", StringComparison.CurrentCultureIgnoreCase)) {
				return new Orange.HotSceneDeserializer(stream); 
			} else if (path.EndsWith(".fnt", StringComparison.CurrentCultureIgnoreCase)) {
				return new Orange.HotFontDeserializer(stream); 
			}
			return null;
		}

		private static void InvalidateWindows()
		{
			foreach (var window in Application.Windows) {
				window.Invalidate();
			}
		}

		private void AddGlobalProcessors(Widget panel)
		{
			panel.LateTasks.Add(new BuildRowsProcessor());
			panel.LateTasks.Add(new UI.Timeline.GlobalKeyboardShortcutsProcessor(panel.Input));
			panel.LateTasks.Add(new UI.SceneView.PreviewAnimationProcessor(panel.Input));
		}

		private static void RefreshExternalContent(Node node)
		{
			if (node.ContentsPath != null) {
				var path = System.IO.Path.ChangeExtension(node.ContentsPath, Document.SceneFileExtension);
				var doc = Project.Current.Documents.FirstOrDefault(i => i.Path == path);
				if (doc != null && doc.IsModified) {
					node.Nodes.Clear();
					node.Markers.Clear();
					var content = doc.RootNode.Clone();
					RefreshExternalContent(content);
					if (content.AsWidget != null && node.AsWidget != null) {
						content.AsWidget.Size = node.AsWidget.Size;
					}
					node.Markers.AddRange(content.Markers);
					var nodes = content.Nodes.ToList();
					content.Nodes.Clear();
					node.Nodes.AddRange(nodes);
				}
			} else {
				foreach (var child in node.Nodes) {
					RefreshExternalContent(child);
				}
			}
		}

		static Frame InitializeDocumentArea(UI.DockManager dockManager)
		{
			var tabBar = new TabBar { LayoutCell = new LayoutCell { StretchY = 0 } };
			var documentViewContainer = new Frame {
				ClipChildren = ClipMethod.ScissorTest,
				Layout = new StackLayout()
			};
			new DocumentTabsProcessor(tabBar);
			var docArea = dockManager.DocumentArea;
			docArea.Layout = new VBoxLayout();
			docArea.AddNode(tabBar);
			docArea.AddNode(documentViewContainer);
			docArea.FocusScope = new KeyboardFocusScope(docArea);
			docArea.LateTasks.Add(new UI.Timeline.GlobalKeyboardShortcutsProcessor(docArea.Input));
			return documentViewContainer;
		}

		class DocumentTabsProcessor
		{
			public DocumentTabsProcessor(TabBar tabBar)
			{
				RebuildTabs(tabBar);
				var documentsCollectionChanged = new Property<int>(() => Project.Current.Documents.Version).DistinctUntilChanged();
				var currentProjectChanged = new Property<Project>(() => Project.Current).DistinctUntilChanged();
				tabBar.Tasks.Add(
					documentsCollectionChanged.Consume(_ => RebuildTabs(tabBar)),
					currentProjectChanged.Consume(_ => RebuildTabs(tabBar))
				);
			}

			private void RebuildTabs(TabBar tabBar)
			{
				tabBar.Nodes.Clear();
				foreach (var doc in Project.Current.Documents) {
					var tab = new Tab { Closable = true };
					var currentDocumentChanged = new Property<bool>(() => Document.Current == doc).DistinctUntilChanged().Where(i => i);
					var documentModified = new Property<bool>(() => doc.IsModified).DistinctUntilChanged();
					tab.Tasks.Add(
						currentDocumentChanged.Consume(_ => tabBar.ActivateTab(tab)),
						documentModified.Consume(_ => RefreshTabText(doc, tab))
					);
					tab.Clicked += doc.MakeCurrent;
					tab.Closing += () => Project.Current.CloseDocument(doc);
					tabBar.AddNode(tab);
				}
				tabBar.AddNode(new Widget { LayoutCell = new LayoutCell { StretchX = 0 }});
			}

			void RefreshTabText(Document doc, Tab tab)
			{
				tab.Text = System.IO.Path.GetFileName(System.IO.Path.ChangeExtension(doc.Path, null));
				if (doc.IsModified) {
					tab.Text += '*';
				}
			}
		}

		void CreateMainMenu()
		{
			Application.MainMenu = new Menu {
				new Submenu("Application") {
					new PreferencesCommand(),
					Command.MenuSeparator, 
					#if MAC
					new DelegateCommand("Quit", new Shortcut(Modifiers.Command, Key.Q), Application.Exit),
					#else
					new DelegateCommand("Quit", new Shortcut(Modifiers.Alt, Key.F4), Application.Exit),
					#endif
				},
				new Submenu("Create") {
				},
				new Submenu("File") {
					new OpenFileCommand(),
					new OpenProjectCommand(),
					Command.MenuSeparator,
					new CloseDocumentCommand(),
				},
				new Submenu("Edit") {
					new KeySendingCommand("Undo", new Shortcut(Modifiers.Command, Key.Z), Key.Commands.Undo),
					new KeySendingCommand("Redo", new Shortcut(Modifiers.Command | Modifiers.Shift, Key.Z), Key.Commands.Redo),
					Command.MenuSeparator,
					new KeySendingCommand("Cut", new Shortcut(Modifiers.Command, Key.X), Key.Commands.Cut),
					new KeySendingCommand("Copy", new Shortcut(Modifiers.Command, Key.C), Key.Commands.Copy),
					new KeySendingCommand("Paste", new Shortcut(Modifiers.Command, Key.V), Key.Commands.Paste),
					new KeySendingCommand("Delete", Key.Delete, Key.Commands.Delete),
					Command.MenuSeparator,
					new KeySendingCommand("Select All", new Shortcut(Modifiers.Command, Key.A), Key.Commands.SelectAll),
				},
				new Submenu("View") {
					new DefaultLayoutCommand(),
					(Submenu)PadsMenu
				},
				new Submenu("Window") {
					new NextDocumentCommand(),
					new PreviousDocumentCommand(),
				},
			};
			Application.MainMenu.FindCommand("Create").Submenu.AddRange(
				new List<Type> {
					typeof(Frame),
					typeof(Image),
					typeof(Movie),
					typeof(Bone),
					typeof(SplineGear),
					typeof(ParticleEmitter),
					typeof(ParticlesMagnet),
					typeof(EmitterShapePoint),
					typeof(ParticlesMagnet),
					typeof(ParticleModifier),
					typeof(SimpleText),
					typeof(RichText),
					typeof(TextStyle),
					typeof(NineGrid),
					typeof(DistortionMesh),
					typeof(Spline),
					typeof(SplinePoint),
					typeof(ImageCombiner)
				}.Select(i => new CreateNodeCommand(i)));
		}

		static void LoadFont()
		{
			var fontData = new Tangerine.UI.EmbeddedResource("Tangerine.Resources.SegoeUIRegular.ttf", "Tangerine").GetResourceBytes();
			var font = new DynamicFont(fontData);
			FontPool.Instance.AddFont("Default", font);
		}
	}
}