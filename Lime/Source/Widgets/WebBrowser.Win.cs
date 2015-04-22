#if WIN
using Microsoft.Win32;
using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Lime
{
	public class WebBrowser : Widget
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

		public Uri Url { get { return browser.Url; } set { browser.Url = value; } }

		private System.Windows.Forms.WebBrowser browser;
		private Form form;

		private static void SetBrowserFeatureControlKey(string feature, string appName, UInt32 value)
		{
			using (var key = Registry.CurrentUser.CreateSubKey(
				@"Software\Microsoft\Internet Explorer\Main\FeatureControl\" + feature,
				RegistryKeyPermissionCheck.ReadWriteSubTree)
			) {
				key.SetValue(appName, value, RegistryValueKind.DWord);
			}
		}

		private static UInt32 GetIEVersion()
		{
			using (var ieKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer",
				RegistryKeyPermissionCheck.ReadSubTree,
				System.Security.AccessControl.RegistryRights.QueryValues)
			) {
				var version = ieKey.GetValue("svcVersion") ?? ieKey.GetValue("Version") ?? "IE required";
				return UInt32.Parse(version.ToString().Split('.')[0]);
			}
		}

		private static UInt32 GetBrowserEmulationMode()
		{
			switch (GetIEVersion()) {
				case 7: return 7000;
				case 8: return 8000;
				case 9: return 9000;
				case 10: return 10000;
				case 11: return 11001; // [sic]
				default: return 10000;
			}
		}

		static WebBrowser()
		{
			// By default embedded IE emulates version 7.0. We need to add ourselves to registry to enable more recent versiion.
			// http://msdn.microsoft.com/en-us/library/ee330720(v=vs.85).aspx

			var fileName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
			var ideNames = new[] { "devenv.exe", "xdesproc.exe" };
			if (ideNames.Any(s => String.Equals(fileName, s, StringComparison.InvariantCultureIgnoreCase)))
				return;
			SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", fileName, GetBrowserEmulationMode());
		}

		public WebBrowser() : base()
		{
			Application.Instance.Moved += CalcGeometry;
			var mainForm = Control.FromHandle(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
			form = new HiddenForm();
			form.StartPosition = FormStartPosition.Manual;
			form.FormBorderStyle = FormBorderStyle.None;
			form.ShowInTaskbar = false;
			browser = new System.Windows.Forms.WebBrowser();
			browser.Parent = form;
			browser.ScriptErrorsSuppressed = true;
		}

		public WebBrowser(Widget parentWidget)
			: this()
		{
			AddToWidget(parentWidget);
		}

		private IntRectangle CalculateAABBInWorldSpace(Widget widget)
		{
			var aabb = widget.CalcAABBInSpaceOf(World.Instance);
			var viewport = Renderer.Viewport;
			var scale = new Vector2(viewport.Width, viewport.Height) / World.Instance.Size;
			return new IntRectangle(
				(viewport.X + aabb.Left * scale.X).Round(),
				(viewport.Y + aabb.Top * scale.Y).Round(),
				(viewport.X + aabb.Right * scale.X).Round(),
				(viewport.Y + aabb.Bottom * scale.Y).Round()
			);
		}

		private IntRectangle SysToIntRect(System.Drawing.Rectangle r)
		{
			return new IntRectangle(r.Left, r.Top, r.Right, r.Bottom);
		}

		private void CalcGeometry()
		{
			if (form == null)
				return;
			var r = IntRectangle.Intersect(
				CalculateAABBInWorldSpace(this),
				SysToIntRect(GameView.Instance.ClientRectangle)
			).OffsetBy(new IntVector2(GameView.Instance.X, GameView.Instance.Y));
			form.Left = r.Left;
			form.Top = r.Top;
			form.Width = r.Width;
			form.Height = r.Height;
			browser.SetBounds(0, 0, form.Width, form.Height);
		}

		public override void Render()
		{
			if (form == null)
				return;
			if (GloballyVisible) {
				CalcGeometry();
				form.Show();
				form.BringToFront();
			}
			else {
				form.Hide();
			}
		}

		public void AddToWidget(Widget parentWidget)
		{
			parentWidget.AddNode(this);
			Anchors = Anchors.LeftRightTopBottom;
			Size = parentWidget.Size;
		}

		public override void Dispose()
		{
			if (browser != null)
				browser.Dispose();
			if (form != null)
				form.Dispose();
		}
	}
}
#endif