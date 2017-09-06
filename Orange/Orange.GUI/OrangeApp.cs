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
			WidgetInput.AcceptMouseBeyondWidgetByDefault = false;
			Widget.DefaultWidgetSize = Vector2.Zero;
			LoadFont();
			UserInterface.Instance = Interface = new OrangeInterface();
			The.Workspace.Load();
			CreateMenuItems();
		}

		private static void CreateMenuItems()
		{
			The.MenuController.CreateAssemblyMenuItems();
		}

		private static void LoadFont()
		{
			var fontData = new EmbeddedResource("Orange.GUI.Resources.SegoeUIRegular.ttf", "Orange.GUI").GetResourceBytes();
			var font = new DynamicFont(fontData);
			FontPool.Instance.AddFont("Default", font);
		}
	}
}