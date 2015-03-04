#if WIN
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Lime
{
	public class WebBrowser : Widget
	{
		public Uri Url { get { return browser.Url; } set { browser.Url = value; } }

		private System.Windows.Forms.WebBrowser browser;
		private Form form;

		public WebBrowser() : base()
		{
			Application.Instance.Moved += CalcGeometry;
			var mainForm = Control.FromHandle(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
			form = new Form();
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