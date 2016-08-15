using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using ProtoBuf;
using System.Collections.Generic;
using System.Collections.Specialized;
using Tangerine.UI;

namespace Tangerine
{
	public class PreferencesCommand : Command
	{
		public PreferencesCommand()
		{
			Text = "Preferences...";
			Shortcut = KeyBindings.Generic.PreferencesDialog;
		}

		public override void Execute()
		{
			new PreferencesDialog();
		}
	}

	public class TangerineApp
	{
		public static TangerineApp Instance { get; private set; }

		public readonly DockManager DockManager;
		public readonly DockManager.State DockManagerInitialState;

		public static void Initialize()
		{
			Instance = new TangerineApp();
		}

		public Menu PadsMenu { get; private set; }

		class Deserializer : Serialization.IDeserializer
		{
			public object Deserialize(System.IO.Stream stream, object value, Type type)
			{
				if (type == typeof(Frame)) {
					return new Orange.HotSceneImporter(stream).ParseNode(value as Node);
				} else if (type == typeof(Font)) {
					return new Orange.HotFontImporter().ParseFont(stream);
				} else {
					return new Serialization.ProtoBufDeserializer(Serialization.ProtoBufTypeModel).Deserialize(stream, value, type);
				}
			}
		}

		private TangerineApp()
		{
			Application.IsTangerine = true;
			Lime.Serialization.Deserializer = new Deserializer();

			Widget.DefaultWidgetSize = Vector2.Zero;
			CreateMainMenu();
			Theme.Current = new DesktopTheme();
			LoadFont();

			DockManager = new UI.DockManager(new Vector2(1024, 768), PadsMenu);
			Application.Exiting += () => Project.Current.Close();
			Application.Exited += () => {
				UserPreferences.Instance.DockState = DockManager.ExportState();
				UserPreferences.Instance.Save();
			};
			var timelinePanel = new UI.DockPanel("Timeline");
			var inspectorPanel = new UI.DockPanel("Inspector");
			var consolePanel = new UI.DockPanel("Console");
			var searchPanel = new UI.DockPanel("SearchPanel");
			DockManager.AddPanel(timelinePanel, UI.DockSite.Top, new Vector2(800, 300));
			DockManager.AddPanel(inspectorPanel, UI.DockSite.Left, new Vector2(300, 700));
			DockManager.AddPanel(searchPanel, UI.DockSite.Right, new Vector2(300, 700));
			DockManager.AddPanel(consolePanel, UI.DockSite.Right, new Vector2(300, 700));
			DockManagerInitialState = DockManager.ExportState();
			var documentViewContainer = InitializeDocumentArea(DockManager);

			UserPreferences.Initialize();
			DockManager.ImportState(UserPreferences.Instance.DockState);
			Document.Closing += doc => {
				var alert = new AlertDialog("Tangerine", $"Save the changes to document '{doc.Path}' before closing?", "Yes", "No", "Cancel");
				switch (alert.Show()) {
					case 0: return Document.CloseAction.SaveChanges;
					case 1: return Document.CloseAction.DiscardChanges;
					default: return Document.CloseAction.Cancel;
				}
			};
			Document.ViewsBuilder = () => {
				var doc = Document.Current;
				doc.Views.Add(new UI.Inspector.Inspector(inspectorPanel.ContentWidget));
				doc.Views.Add(new UI.Timeline.Timeline(timelinePanel.ContentWidget));
				doc.Views.Add(new UI.SceneView.SceneView(documentViewContainer));
				doc.Views.Add(new UI.Console(consolePanel.ContentWidget));
				doc.Views.Add(new UI.SearchPanel(searchPanel.ContentWidget));
				doc.History.Changed += () => CommonWindow.Current.Invalidate();
			};
			if (UserPreferences.Instance.RecentProjects.Count > 0) {
				new Project(UserPreferences.Instance.RecentProjects[0]).Open();
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
				new Command("Application", new Shortcut(Modifiers.Command, Key.Q)) {
					Submenu = new Menu {
						new PreferencesCommand(),
						Command.MenuSeparator, 
						new DelegateCommand("Quit", new Shortcut(Modifiers.Command, Key.Q), Application.Exit),
					}
				},
				new Command("File") {
					Submenu = new Menu {
						new OpenFileCommand(),
						new OpenProjectCommand(),
						Command.MenuSeparator,
						new CloseDocumentCommand(),
					}
				},
				new Command("Edit") {
					Submenu = new Menu {
						new KeySendingCommand("Undo", new Shortcut(Modifiers.Command, Key.Z), Key.Commands.Undo),
						new KeySendingCommand("Redo", new Shortcut(Modifiers.Command | Modifiers.Shift, Key.Z), Key.Commands.Redo),
						Command.MenuSeparator,
						new KeySendingCommand("Cut", new Shortcut(Modifiers.Command, Key.X), Key.Commands.Cut),
						new KeySendingCommand("Copy", new Shortcut(Modifiers.Command, Key.C), Key.Commands.Copy),
						new KeySendingCommand("Paste", new Shortcut(Modifiers.Command, Key.V), Key.Commands.Paste),
						new KeySendingCommand("Delete", Key.Delete, Key.Commands.Delete),
						Command.MenuSeparator,
						new KeySendingCommand("Select All", new Shortcut(Modifiers.Command, Key.A), Key.Commands.SelectAll),
					}
				},
				new Command("View") {
					Submenu = new Menu {
						new DefaultLayoutCommand(),
						new Command("Pads") {
							Submenu = (PadsMenu = new Menu())
						}
					}
				},
				new Command("Window") {
					Submenu = new Menu {
						new NextDocumentCommand(),
						new PreviousDocumentCommand(),
					}
				},
			};
		}

		static void LoadFont()
		{
			var fontData = new Tangerine.UI.EmbeddedResource("Tangerine.Resources.SegoeUIRegular.ttf", "Tangerine").GetResourceBytes();
			var font = new DynamicFont(fontData);
			FontPool.Instance.AddFont("Default", font);
		}
	}
}