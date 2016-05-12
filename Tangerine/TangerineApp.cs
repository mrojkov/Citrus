using System;
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

		public void Load()
		{
			if (System.IO.File.Exists(GetPath())) {
				try {
					Lime.Serialization.ReadObjectFromFile<UserPreferences>(GetPath(), this);
				} catch (System.Exception e) {
					Debug.Write($"Failed to load the user preferences: {e}");
				}
			}
		}

		public void Save()
		{
			Lime.Serialization.WriteObjectToFile(GetPath(), this);
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

		private TangerineApp()
		{
			Theme.Current = new Lime.DesktopTheme();
			LoadFont();
			var doc = new Document();
			doc.AddSomeNodes();
			Document.Current = doc;

			var dockManager = new UI.DockManager();
			Exiting += p => p.DockState = dockManager.ExportState();
			dockManager.Closed += Exit;

			var timelinePanel = new UI.DockPanel("Timeline");
			var inspectorPanel = new UI.DockPanel("Inspector");
			var browserPanel = new UI.DockPanel("Browser");
			dockManager.AddPanel(timelinePanel, UI.DockSite.Top, new Vector2(0.3f));
			dockManager.AddPanel(inspectorPanel, UI.DockSite.Left, new Vector2(0.2f));
			dockManager.AddPanel(browserPanel, UI.DockSite.Right, new Vector2(0.1f));

			var prefs = new UserPreferences();
			prefs.Load();
			dockManager.ImportState(prefs.DockState);

			UI.Inspector.Inspector.Initialize(inspectorPanel.ContentWidget);
			UI.Timeline.Timeline.Initialize(timelinePanel.ContentWidget);

			doc.History.OnCommit += () => Window.Current.Invalidate();
			UI.Timeline.Timeline.Instance.RegisterDocument(doc);
		}

		public void Exit()
		{
			var prefs = new UserPreferences();
			Exiting?.Invoke(prefs);
			prefs.Save();
		}

		static void LoadFont()
		{
			var fontData = new Tangerine.UI.EmbeddedResource("Tangerine.Resources.SegoeUIRegular.ttf", "Tangerine").GetResourceBytes();
			var font = new DynamicFont(fontData);
			FontPool.Instance.AddFont("Default", font);
		}
	}
}