using Lime;

namespace Orange
{
	public class OrangeApp
	{
		public static OrangeApp Instance { get; private set; }
		private OrangeInterface Interface { get; }

		public static void Initialize()
		{
			Instance = new OrangeApp();
		}

		private OrangeApp()
		{
			WindowOptions.DefaultRefreshRate = 60;
			WidgetInput.AcceptMouseBeyondWidgetByDefault = false;
			Widget.DefaultWidgetSize = Vector2.Zero;
			LoadFont();
			SetupTheme();
			CreateMenuItems();
			UserInterface.Instance = Interface = new OrangeInterface();
			The.Workspace.Load();
		}

		private static void CreateMenuItems()
		{
			The.MenuController.CreateAssemblyMenuItems();
		}

		private static void SetupTheme()
		{
			Theme.Current = new DesktopTheme();
			DesktopTheme.Colors = DesktopTheme.ColorTheme.CreateLightTheme();
		}

		private static void LoadFont()
		{
			var fontData = new EmbeddedResource("Orange.GUI.Resources.SegoeUIRegular.ttf", "Orange.GUI").GetResourceBytes();
			var font = new DynamicFont(fontData);
			FontPool.Instance.AddFont("Default", font);
		}
	}
}