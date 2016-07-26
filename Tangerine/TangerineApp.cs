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
	[ProtoContract]
	public class UserPreferences
	{
		[ProtoMember(1)]
		public UI.DockManager.State DockState = new UI.DockManager.State();

		[ProtoMember(2)]
		public readonly List<string> RecentProjects = new List<string>();

		public void Load()
		{
			if (System.IO.File.Exists(GetPath())) {
				try {
					Serialization.ReadObjectFromFile<UserPreferences>(GetPath(), this);
				} catch (System.Exception e) {
					Debug.Write($"Failed to load the user preferences: {e}");
				}
			}
		}

		public void Save()
		{
			Serialization.WriteObjectToFile(GetPath(), this);
		}

		public static string GetPath()
		{
			return System.IO.Path.Combine(Lime.Environment.GetDataDirectory("Tangerine"), "UserPreferences");
		}
	}

	public class OpenProjectCommand : Command
	{
		public OpenProjectCommand()
		{
			Text = "Open Project...";
			Shortcut = KeyBindings.Generic.OpenProject;
		}

		public override void Execute()
		{
			var dlg = new FileDialog { AllowedFileTypes = new string[] { "citproj" }, Mode = FileDialogMode.Open };
			if (dlg.RunModal()) {
				if (Project.Current == null || Project.Current.Close()) {
					Project.SetCurrent(new Project(dlg.FileName));
					var prefs = TangerineApp.Instance.Preferences;
					prefs.RecentProjects.Remove(dlg.FileName);
					prefs.RecentProjects.Insert(0, dlg.FileName);
					prefs.Save();
				}
			}
		}
	}

	public class OpenFileCommand : Command
	{
		public OpenFileCommand()
		{
			Text = "Open File...";
			Shortcut = KeyBindings.Generic.OpenFile;
		}

		public override void Execute()
		{
			var dlg = new FileDialog { AllowedFileTypes = new string[] { "scene" }, Mode = FileDialogMode.Open };
			if (dlg.RunModal()) {
				Project.Current.OpenDocument(dlg.FileName);
			}
		}

		public override void Refresh()
		{
			Enabled = Project.Current != null;
		}
	}

	public class PreferencesCommand : Command
	{
		public PreferencesCommand()
		{
			Text = "Preferences...";
			Shortcut = KeyBindings.Generic.PreferencesDialog;
		}

		public override void Execute()
		{
			new PreferencesDialog(TangerineApp.Instance.Preferences);
		}
	}

	public class TangerineApp
	{
		public static TangerineApp Instance { get; private set; }

		public UserPreferences Preferences = new UserPreferences();

		public static void Initialize()
		{
			Instance = new TangerineApp();
		}

		public Menu ViewMenu { get; private set; }

		private TangerineApp()
		{
			Widget.DefaultWidgetSize = Vector2.Zero;
			CreateMainMenu();
			Theme.Current = new DesktopTheme();
			LoadFont();

			var dockManager = new UI.DockManager(new Vector2(1024, 768), ViewMenu);
			dockManager.Closing += () => Project.Current == null || Project.Current.Close();
			dockManager.Closed += () => {
				Preferences.DockState = dockManager.ExportState();
				Preferences.Save();
			};
			var timelinePanel = new UI.DockPanel("Timeline");
			var inspectorPanel = new UI.DockPanel("Inspector");
			var consolePanel = new UI.DockPanel("Console");
			dockManager.AddPanel(timelinePanel, UI.DockSite.Top, new Vector2(800, 300));
			dockManager.AddPanel(inspectorPanel, UI.DockSite.Left, new Vector2(400, 700));
			dockManager.AddPanel(consolePanel, UI.DockSite.Right, new Vector2(400, 700));
			var documentViewContainer = InitializeDocumentArea(dockManager);

			Preferences = new UserPreferences();
			Preferences.Load();
			dockManager.ImportState(Preferences.DockState);
			// new PreferencesDialog(Preferences);

			Document.ViewsBuilder = () => {
				var doc = Document.Current;
				doc.Views.Add(new UI.Inspector.Inspector(inspectorPanel.ContentWidget));
				doc.Views.Add(new UI.Timeline.Timeline(timelinePanel.ContentWidget));
				doc.Views.Add(new UI.SceneView.SceneView(documentViewContainer));
				doc.Views.Add(new UI.Console(consolePanel.ContentWidget));
				doc.History.Changed += () => CommonWindow.Current.Invalidate();
			};
			if (Preferences.RecentProjects.Count > 0) {
				Project.SetCurrent(new Project(Preferences.RecentProjects[0]));
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
				var documentsChanged = new EventflowProvider<NotifyCollectionChangedEventArgs>(Project.Current.Documents, "CollectionChanged");
				tabBar.Updated += documentsChanged.Consume(_ => RebuildTabs(tabBar)).Execute;
				var currentProjectChanged = new Property<Project>(() => Project.Current).DistinctUntilChanged();
				tabBar.Updated += currentProjectChanged.Consume(_ => RebuildTabs(tabBar)).Execute;
			}

			private void RebuildTabs(TabBar tabBar)
			{
				tabBar.Nodes.Clear();
				foreach (var doc in Project.Current.Documents) {
					var tab = new Tab { Closable = true };
					var currentDocumentChanged = new Property<bool>(() => Document.Current == doc).DistinctUntilChanged().Where(i => i);
					var documentModified = new Property<bool>(() => doc.IsModified).DistinctUntilChanged();
					tab.Updated += currentDocumentChanged.Consume(_ => tabBar.ActivateTab(tab)).Execute;
					tab.Updated += documentModified.Consume(_ => RefreshTabText(doc, tab)).Execute;
					tab.Clicked += doc.MakeCurrent;
					tab.Closing += () => doc.Close();
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
						new DelegateCommand("Quit", new Shortcut(Modifiers.Command, Key.Q), Application.Exit),
					}
				},
				new Command("File") {
					Submenu = new Menu {
						new OpenFileCommand(),
						new OpenProjectCommand()
					}
				},
				new Command("Edit") {
					Submenu = new Menu {
						new KeySendingCommand("Undo", new Shortcut(Modifiers.Command, Key.Z), Key.Undo),
						new KeySendingCommand("Redo", new Shortcut(Modifiers.Command | Modifiers.Shift, Key.Z), Key.Redo),
						new KeySendingCommand("Select All", new Shortcut(Modifiers.Command, Key.A), Key.SelectAll),
					}
				},
				new Command("View") {
					Submenu = (ViewMenu = new Menu { })
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