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
		private class HiddenForm : Form
		{
			protected override CreateParams CreateParams
			{
				get
				{
					var p = base.CreateParams;
					p.ExStyle |= 0x80; // WS_EX_TOOLWINDOW
					return p;
				}
			}
		}

		public Uri Url {
			get { return browser.Url; }
			set { browser.Url = value; }
		}

		private System.Windows.Forms.WebBrowser browser;
		private Form form;

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

		Widget widget;

		public WinFormsWebBrowser(Widget widget)
		{
			this.widget = widget;
			form = new HiddenForm {
				StartPosition = FormStartPosition.Manual,
				FormBorderStyle = FormBorderStyle.None,
				ShowInTaskbar = false
			};
			browser = new System.Windows.Forms.WebBrowser {
				Parent = form,
				ScriptErrorsSuppressed = true
			};
		}

		private IntRectangle CalculateAABBInWorldSpace(Widget widget)
		{
			var aabb = widget.CalcAABBInSpaceOf(WidgetContext.Current.Root);
			var viewport = Renderer.Viewport;
			var scale = new Vector2(viewport.Width, viewport.Height) / WidgetContext.Current.Root.Size;
			return new IntRectangle(
				(viewport.X + aabb.Left * scale.X).Round(),
				(viewport.Y + aabb.Top * scale.Y).Round(),
				(viewport.X + aabb.Right * scale.X).Round(),
				(viewport.Y + aabb.Bottom * scale.Y).Round()
			);
		}

		private void CalcGeometry(Widget widget)
		{
			if (form == null) {
				return;
			}
			var window = WidgetContext.Current.Window;
			var r = IntRectangle.Intersect(
				CalculateAABBInWorldSpace(widget),
				new IntRectangle(IntVector2.Zero, (IntVector2)window.ClientSize)
			).OffsetBy(window.ClientPosition);
			form.Left = r.Left;
			form.Top = r.Top;
			form.Width = r.Width;
			form.Height = r.Height;
			browser.SetBounds(0, 0, form.Width, form.Height);
		}

		public void Render()
		{
			if (form == null) {
				return;
			}
			if (widget.GloballyVisible) {
				CalcGeometry(widget);
				form.Show();
				form.BringToFront();
			}
			else {
				form.Hide();
			}
		}

		public void Update(float delta) { }

		public void OnSizeChanged(Vector2 sizeDelta)
		{
			CalcGeometry(widget);
		}

		public void Dispose()
		{
			if (browser != null) {
				Application.InvokeOnMainThread(browser.Dispose);
				browser = null;
			}
			if (form != null) {
				Application.InvokeOnMainThread(form.Dispose);
				form = null;
			}
		}
	}
}
#endif