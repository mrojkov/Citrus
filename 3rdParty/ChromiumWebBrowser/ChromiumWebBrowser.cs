using System;
using System.Collections.Generic;
using System.Linq;
using CefSharp;
using Lime;

namespace ChromiumWebBrowser
{
	public sealed class ChromiumWebBrowser : IWebBrowserImplementation
	{
		private ChromiumWebBrowserLogic browserLogic;
		private Texture2D texture = new Texture2D();
		private Texture2D popupTexture = new Texture2D();
		private int mouseWheelSpeed = 100;
		private CefEventFlags modifiers = CefEventFlags.None;
		private Widget widget;
		private Widget popupWidget;

		public ChromiumWebBrowser(Widget widget)
		{
			this.widget = widget;
			popupWidget = new Widget();
			HidePopupWidget();
			widget.AddNode(popupWidget);
			var browserSettings = new BrowserSettings {
				OffScreenTransparentBackground = false
			};
			browserLogic = new ChromiumWebBrowserLogic(browserSettings: browserSettings) {
				LifeSpanHandler = new LifeSpanHandler(),
			};
			browserLogic.NewScreenshot += LoadTexture;
			browserLogic.PopupOpen += OnPopupShow;
			browserLogic.PopupTransformed += OnPopupTransform;
		}

		private Input Input
		{
			get { return WidgetContext.Current.Window.Input; }
		}

		public Uri Url
		{
			get { return new Uri(browserLogic.Address); }
			set { browserLogic.Load(value.AbsoluteUri); }
		}

		public void Update(float delta)
		{
			UpdateModifiers();
			HandleMouse(widget);
			HandleKeyboard();
		}

		private void HidePopupWidget()
		{
			popupWidget.Position = Vector2.Zero;
			popupWidget.Size = Vector2.Zero;
		}

		private void OnPopupShow(object sender, PopupOpenArgs args)
		{
			if (args.Show == false) {
				HidePopupWidget();
			}
		}

		private void OnPopupTransform(object sender, PopupTransformArgs args)
		{
			popupWidget.Position = new Vector2(args.X, args.Y);
			popupWidget.Size = new Vector2(args.Width, args.Height);
		}

		private void LoadTexture(object sender, EventArgs eventArgs)
		{
			Application.InvokeOnMainThread(() => {
				if (browserLogic == null)
					return;
				var bitmapInfo = browserLogic.BitmapInfo;
				if (bitmapInfo == null)
					return;
				//lock (bitmapInfo.BitmapLock)
				{
					var bitmapPointer = bitmapInfo.BackBufferHandle;
					SwapRedAndBlue32(bitmapPointer, bitmapInfo.Width * bitmapInfo.Height);
					var targetTexture = browserLogic.BitmapInfo.IsPopup ? popupTexture : texture;
					targetTexture.LoadImage(bitmapPointer, bitmapInfo.Width, bitmapInfo.Height, false);
				}
			});
		}

		public void Dispose()
		{
			browserLogic.Dispose();
			browserLogic = null;
			texture.Dispose();
			texture = null;
			popupTexture.Dispose();
			popupTexture = null;
		}

		public void Render()
		{
			RenderTextureToWidget(widget, texture);
			RenderTextureToWidget(popupWidget, popupTexture);
		}

		private static void RenderTextureToWidget(Widget widget, ITexture texture)
		{
			Renderer.Blending = widget.GlobalBlending;
			Renderer.Shader = widget.GlobalShader;
			Renderer.Transform1 = widget.LocalToWorldTransform;
			Renderer.DrawSprite(texture, widget.GlobalColor, Vector2.Zero, widget.Size, Vector2.Zero, Vector2.One);
		}

		public void OnSizeChanged(Vector2 sizeDelta)
		{
			if (browserLogic != null) {
				browserLogic.Size = new Size((int)widget.Size.X, (int)widget.Size.Y);
			}
		}

		private void HandleMouse(Widget widget)
		{
			var position = Input.MousePosition - widget.GlobalPosition;
			var x = (int)position.X;
			var y = (int)position.Y;
			if (widget.IsMouseOver()) {
				browserLogic.SendMouseMove(x, y, false, modifiers);
			}
			else {
				browserLogic.SendMouseMove(-1, -1, true, modifiers);
			}
			HandleLeftMouseButton(x, y);
			HandleRightMouseButton(x, y);
			HandleMouseWheel(x, y);
		}

		private void HandleLeftMouseButton(int x, int y)
		{
			HandleMouseButton(x, y, 0, MouseButtonType.Left);
		}

		private void HandleRightMouseButton(int x, int y)
		{
			HandleMouseButton(x, y, 1, MouseButtonType.Right);
		}

		private void HandleMouseWheel(int x, int y)
		{
			HandleMouseButton(x, y, 2, MouseButtonType.Middle);
			if (Input.WasKeyPressed(Key.MouseWheelUp)) {
				browserLogic.SendMouseWheelEvent(x, y, 0, mouseWheelSpeed);
			}
			if (Input.WasKeyPressed(Key.MouseWheelDown)) {
				browserLogic.SendMouseWheelEvent(x, y, 0, -mouseWheelSpeed);
			}
		}

		private void HandleMouseButton(int x, int y, int limeButton, MouseButtonType buttonType)
		{
			if (Input.WasMousePressed(limeButton)) {
				browserLogic.SendMouseClick(x, y, buttonType, false, modifiers);
			}
			if (Input.WasMouseReleased(limeButton)) {
				browserLogic.SendMouseClick(x, y, buttonType, true, modifiers);
			}
		}

		private void HandleKeyboard()
		{
			HandleKeys();
			HandleTextInput();
		}

		private void HandleKeys()
		{
			var input = WidgetContext.Current.Window.Input;
			var keys = Enum.GetValues(typeof(Key))
				.Cast<int>()
				.Distinct()
				.Cast<Key>();
			foreach (var key in keys) {
				var cefKey = CefButtonKeyMap.GetButton(key);
				if (cefKey == null) {
					continue;
				}
				if (input.WasKeyPressed(key)) {
					browserLogic.SendKeyPress((int)CefMessage.KeyDown, (int)cefKey, modifiers);
					// OpenTK doesn't get character for Enter
					if (cefKey == CefKey.Return) {
						browserLogic.SendKeyPress((int)CefMessage.Char, '\r', modifiers);
					}
				}
				if (input.WasKeyReleased(key)) {
					browserLogic.SendKeyPress((int)CefMessage.KeyUp, (int)cefKey, modifiers);
				}
			}
		}

		private void HandleTextInput()
		{
			if (Input.TextInput == null) {
				return;
			}
			foreach (var character in Input.TextInput) {
				browserLogic.SendKeyPress((int)CefMessage.Char, character, modifiers);
			}
		}

		private void UpdateModifiers()
		{
			modifiers = CefEventFlags.None;

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

		private static void SwapRedAndBlue32(IntPtr data, int count)
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