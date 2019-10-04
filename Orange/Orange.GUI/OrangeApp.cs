using System;
using Lime;
using System.IO;

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
			LoadFont();
			UserInterface.Instance = Interface = new OrangeInterface();
			The.Workspace.Load();
			CreateActionMenuItems();
		}

		private static void CreateActionMenuItems()
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
