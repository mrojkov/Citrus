#if WIN
using System;
using System.Collections.Generic;
using System.Linq;
using CefSharp;
using Lime.Chromium;

namespace Lime
{
	public class WebBrowser : Image
	{
		private ChromiumWebBrowser browser;
		private Texture2D texture = new Texture2D();
		private int mouseWheelSpeed = 100;
		private CefEventFlags modifiers = CefEventFlags.None;

		public Uri Url
		{
			get { return new Uri(browser.Address); }
			set { browser.Load(value.AbsoluteUri); }
		}

		public WebBrowser()
		{
			Texture = texture;
			var browserSettings = new BrowserSettings {
				OffScreenTransparentBackground = false
			};
			browser = new ChromiumWebBrowser(browserSettings: browserSettings) {
				LifeSpanHandler = new LifeSpanHandler()
			};
			browser.NewScreenshot += LoadTexture;
			Tasks.Add(HandleInput());
		}

		public WebBrowser(Widget parentWidget)
			: this()
		{
			AddToWidget(parentWidget);
		}

		private void LoadTexture(object sender, EventArgs eventArgs)
		{
			var bitmapInfo = browser.BitmapInfo;
			if (bitmapInfo == null)
				return;
			Application.InvokeOnMainThread(() => {
				var bitmapPointer = bitmapInfo.BackBufferHandle;
				SwapRedAndBlue32(bitmapPointer, browser.Size.Width * browser.Size.Height);
				texture.LoadImage(bitmapPointer, browser.Size.Width, browser.Size.Height, false);
			});
		}

		public override void Dispose()
		{
			base.Dispose();
			browser.Dispose();
			texture.Dispose();
		}

		protected override void OnSizeChanged(Vector2 sizeDelta)
		{
			if (browser != null) {
				browser.Size = new Size((int) Size.X, (int) Size.Y);
			}
		}

		public void AddToWidget(Widget parentWidget)
		{
			AddToNode(parentWidget);
			Anchors = Anchors.LeftRightTopBottom;
			CenterOnParent();
			Size = parentWidget.Size;
		}

		private IEnumerator<object> HandleInput()
		{
			while (true) {
				UpdateModifiers();
				HandleMouse();
				HandleKeyboard();
				yield return null;
			}
		}

		private void HandleMouse()
		{
			var position = Input.MousePosition - GlobalPosition;
			var x = (int)position.X;
			var y = (int)position.Y;
			if (IsMouseOver()) {
				browser.SendMouseMove(x, y, false, modifiers);
			}
			else {
				browser.SendMouseMove(-1, -1, true, modifiers);
			}
			HandleLeftMouseButton(x, y);
			HandleRightMouseButton(x, y);
			HandleMouseWheel(x, y);
		}

		private void HandleLeftMouseButton(int x, int y)
		{
			HandleMouseButton(x, y, 0, 0);
		}

		private void HandleRightMouseButton(int x, int y)
		{
			HandleMouseButton(x, y, 1, 2);
		}

		private void HandleMouseWheel(int x, int y)
		{
			HandleMouseButton(x, y, 2, 1);
			if (Input.WasKeyPressed(Key.MouseWheelUp)) {
				browser.SendMouseWheelEvent(x, y, 0, mouseWheelSpeed);
			}
			if (Input.WasKeyPressed(Key.MouseWheelDown)) {
				browser.SendMouseWheelEvent(x, y, 0, -mouseWheelSpeed);
			}
		}

		private void HandleMouseButton(int x, int y, int limeButton, int chromiumButon)
		{
			if (Input.WasMousePressed(limeButton)) {
				browser.SendMouseClick(x, y, chromiumButon, false, modifiers);
			}
			if (Input.WasMouseReleased(limeButton)) {
				browser.SendMouseClick(x, y, chromiumButon, true, modifiers);
			}
		}

		private void HandleKeyboard()
		{
			HandleKeys();
			HandleTextInput();
		}

		private void HandleKeys()
		{
			var keys = Enum.GetValues(typeof(Key))
				.Cast<int>()
				.Distinct()
				.Cast<Key>();
			foreach (var key in keys) {
				var cefKey = CefButtonKeyMap.GetButton(key);
				if (cefKey == null) {
					continue;
				}
				if (Input.WasKeyPressed(key)) {
					browser.SendKeyPress((int)CefMessage.KeyDown, (int)cefKey, modifiers);
					// OpenTK doesn't get character for Enter
					if (cefKey == CefKey.Return) {
						browser.SendKeyPress((int)CefMessage.Char, '\r', modifiers);
					}
				}
				if (Input.WasKeyReleased(key)) {
					browser.SendKeyPress((int)CefMessage.KeyUp, (int)cefKey, modifiers);
				}
			}
		}

		private void HandleTextInput()
		{
			if (Input.TextInput == null) {
				return;
			}
			foreach (var character in Input.TextInput) {
				browser.SendKeyPress((int)CefMessage.Char, character, modifiers);
			}
		}

		private void UpdateModifiers()
		{
			modifiers = 0;

			if (Input.IsMousePressed(0))
			{
				modifiers |= CefEventFlags.LeftMouseButton;
			}
			if (Input.IsMousePressed(2))
			{
				modifiers |= CefEventFlags.MiddleMouseButton;
			}
			if (Input.IsMousePressed(1))
			{
				modifiers |= CefEventFlags.RightMouseButton;
			}

			if (Input.IsKeyPressed(Key.LControl))
			{
				modifiers |= CefEventFlags.ControlDown | CefEventFlags.IsLeft;
			}

			if (Input.IsKeyPressed(Key.RControl))
			{
				modifiers |= CefEventFlags.ControlDown | CefEventFlags.IsRight;
			}

			if (Input.IsKeyPressed(Key.LShift))
			{
				modifiers |= CefEventFlags.ShiftDown | CefEventFlags.IsLeft;
			}

			if (Input.IsKeyPressed(Key.RShift))
			{
				modifiers |= CefEventFlags.ShiftDown | CefEventFlags.IsRight;
			}

			if (Input.IsKeyPressed(Key.LAlt))
			{
				modifiers |= CefEventFlags.AltDown | CefEventFlags.IsLeft;
			}

			if (Input.IsKeyPressed(Key.RAlt))
			{
				modifiers |= CefEventFlags.AltDown | CefEventFlags.IsRight;
			}
		}

		private void SwapRedAndBlue32(IntPtr data, int count)
		{
			unsafe {
				var p = (uint*) data;
				while (count-- > 0) {
					// ABGR -> ARGB
					var pixel = *p;
					*p++ = (pixel & 0xFF00FF00U) | (pixel & 0x000000FFU) << 16 | (pixel & 0x00FF0000U) >> 16;
				}
			}
		}
	}
}
#endif