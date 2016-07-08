using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using ProtoBuf;

namespace Tangerine
{
	[ProtoContract]
	public class UserPreferences
	{
		[ProtoMember(1)]
		public UI.DockManager.State DockState = new UI.DockManager.State();

		public bool Load()
		{
			if (System.IO.File.Exists(GetPath())) {
				try {
					Serialization.ReadObjectFromFile<UserPreferences>(GetPath(), this);
					return true;
				} catch (System.Exception e) {
					Debug.Write($"Failed to load the user preferences: {e}");
				}
			}
			return false;
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

	public class TangerineApp
	{
		public static TangerineApp Instance { get; private set; }

		public event Action<UserPreferences> Exiting;

		public static void Initialize()
		{
			Instance = new TangerineApp();
		}

		public Menu ViewMenu { get; private set; }

		private TangerineApp()
		{
			CreateMainMenu();
			Theme.Current = new DesktopTheme();
			LoadFont();
			var doc = new Document();
			doc.AddSomeNodes();
			Document.Current = doc;

			var dockManager = new UI.DockManager(new Vector2(1024, 768), ViewMenu);
			Exiting += p => p.DockState = dockManager.ExportState();
			dockManager.Closed += () => {
				var prefs = new UserPreferences();
				Exiting?.Invoke(prefs);
				prefs.Save();
			};
			var timelinePanel = new UI.DockPanel("Timeline");
			var inspectorPanel = new UI.DockPanel("Inspector");
			var browserPanel = new UI.DockPanel("Browser");
			dockManager.AddPanel(timelinePanel, UI.DockSite.Top, new Vector2(800, 300));
			dockManager.AddPanel(inspectorPanel, UI.DockSite.Left, new Vector2(400, 700));
			dockManager.AddPanel(browserPanel, UI.DockSite.Right, new Vector2(400, 700));

			var preferences = new UserPreferences();
			if (preferences.Load()) {
				dockManager.ImportState(preferences.DockState);
			}
			UI.Inspector.Inspector.Initialize(inspectorPanel.ContentWidget);
			UI.Timeline.Timeline.Initialize(timelinePanel.ContentWidget);

			doc.History.OnCommit += () => CommonWindow.Current.Invalidate();
			UI.Timeline.Timeline.Instance.RegisterDocument(doc);
		}

		void CreateMainMenu()
		{
			Application.MainMenu = new Menu {
				new Command {
					Text = "Application",
					Submenu = new Menu {
						new Command("Quit", Application.Exit) { Shortcut = new Shortcut(Modifiers.Command, Key.Q) },
					}
				},
				new Command {
					Text = "File",
					Submenu = new Menu {
						new Command { Text = "Open", Shortcut = new Shortcut(Modifiers.Command, Key.O) },
					}
				},
				new Command {
					Text = "Edit",
					Submenu = new Menu {
						new Command { Text = "Undo", Shortcut = new Shortcut(Modifiers.Command, Key.Z), Key = Key.Undo },
						new Command { Text = "Redo", Shortcut = new Shortcut(Modifiers.Command | Modifiers.Shift, Key.Z), Key = Key.Redo },
					}
				},
				new Command {
					Text = "View",
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