#if WIN
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Lime
{
	class WinFormsWebBrowser: IWebBrowserImplementation
	{
		private System.Windows.Forms.WebBrowser browser;
		private Widget widget;

		public WinFormsWebBrowser(Widget widget)
		{
			this.widget = widget;
			browser = new System.Windows.Forms.WebBrowser {
				Parent = Form.ActiveForm,
				ScriptErrorsSuppressed = true
			};
		}

		public void Render()
		{
			if (browser == null) {
				return;
			}
			if (widget.GloballyVisible) {
				FitBrowserInWidget();
				browser.Show();
				browser.BringToFront();
			}
			else {
				browser.Hide();
			}
		}

		private void FitBrowserInWidget()
		{
			if (browser == null) {
				return;
			}
			var rectangle = (IntRectangle)widget.CalcAABBInSpaceOf(WidgetContext.Current.Root);
			browser.Left = (rectangle.Left * Window.Current.PixelScale).Round();
			browser.Top = (rectangle.Top * Window.Current.PixelScale).Round();
			browser.Width = (rectangle.Width * Window.Current.PixelScale).Round();
			browser.Height = (rectangle.Height * Window.Current.PixelScale).Round();
		}

		public void OnSizeChanged(Vector2 sizeDelta) { }

		public void Update(float delta) { }

		public void Dispose()
		{
			if (browser == null)
				return;
			Application.InvokeOnMainThread(browser.Dispose);
			browser = null;
		}

		public Uri Url {
			get { return browser.Url; }
			set { browser.Url = value; }
		}

		static WinFormsWebBrowser()
		{
			// By default embedded IE emulates version 7.0. We need to add ourselves to registry to enable more recent versiion.
			// http://msdn.microsoft.com/en-us/library/ee330720(v=vs.85).aspx

			var fileName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
			var ideNames = new[] { "devenv.exe", "xdesproc.exe" };
			if (ideNames.Any(s => string.Equals(fileName, s, StringComparison.InvariantCultureIgnoreCase)))
				return;
			SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", fileName, GetBrowserEmulationMode());
		}

		private static void SetBrowserFeatureControlKey(string feature, string appName, uint value)
		{
			using (var key = Registry.CurrentUser.CreateSubKey(
				@"Software\Microsoft\Internet Explorer\Main\FeatureControl\" + feature,
				RegistryKeyPermissionCheck.ReadWriteSubTree)
			) {
				key.SetValue(appName, value, RegistryValueKind.DWord);
			}
		}

		private static uint GetIEVersion()
		{
			using (var ieKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer",
				RegistryKeyPermissionCheck.ReadSubTree,
				System.Security.AccessControl.RegistryRights.QueryValues)
			) {
				var version = ieKey.GetValue("svcVersion") ?? ieKey.GetValue("Version") ?? "IE required";
				return uint.Parse(version.ToString().Split('.')[0]);
			}
		}

		private static uint GetBrowserEmulationMode()
		{
			switch (GetIEVersion()) {
				case 7:
					return 7000;
				case 8:
					return 8000;
				case 9:
					return 9000;
				case 10:
					return 10000;
				case 11:
					return 11001; // [sic]
				default:
					return 10000;
			}
		}
	}
}
#endif