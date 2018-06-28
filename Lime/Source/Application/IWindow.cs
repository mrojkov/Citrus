using System;
using System.Collections.Generic;

namespace Lime
{
	/// <summary>
	/// Delegate used by <see cref="IWindow.Resized"/> event.
	/// </summary>
	/// <param name="deviceRotated">Defines whether resize is triggered by device rotation or not.</param>
	public delegate void ResizeDelegate(bool deviceRotated);

	/// <summary>
	/// Delegate used by <see cref="IWindow.Closing"/> event.
	/// </summary>
	/// <param name="reason">Defines the reason of window closing.</param>
	/// <returns>Whether to cancel closing or not.</returns>
	public delegate bool ClosingDelegate(CloseReason reason);

	/// <summary>
	/// Delegate used by <see cref="IWindow.Updating"/> event.
	/// </summary>
	/// <param name="delta">Time delta since last update.</param>
	public delegate void UpdatingDelegate(float delta);

	/// <summary>
	/// Delegate used by <see cref="IWindow.VisibleChanging"/> event.
	/// </summary>
	/// <param name="value">New value of <see cref="IWindow.Visible"/>.</param>
	/// <param name="modal">Indicates whether the window is showing modal.</param>
	public delegate void VisibleChangingDelegate(bool value, bool modal);

	/// <summary>
	/// Enumerates available window states.
	/// </summary>
	public enum WindowState
	{
		/// <summary>
		/// The window is in its normal state.
		/// </summary>
		Normal = 0,
		/// <summary>
		/// The window is minimized to the taskbar (also known as 'iconified').
		/// </summary>
		Minimized,
		/// <summary>
		/// The window covers the whole working area, which includes the desktop but not the taskbar and/or panels.
		/// </summary>
		Maximized,
		/// <summary>
		/// The window covers the whole screen, including all taskbars and/or panels.
		/// </summary>
		Fullscreen
	}

	/// <summary>
	/// Defines the reason for the window closing.
	/// </summary>
	public enum CloseReason
	{
		/// <summary>
		/// The cause if the closure was not defined or could not be determined.
		/// </summary>
		Unknown,
		/// <summary>
		/// The user has closed the window from UI.
		/// </summary>
		UserClosing,
		/// <summary>
		/// A window is closing because main window is closing.
		/// </summary>
		MainWindowClosing
	}

	/// <summary>
	/// Defines the interface for a window.
	/// </summary>
	public interface IWindow
	{
		/// <summary>
		/// Indicates whether the window is active.
		/// For PC, Mac it means that the window has the input focus.
		/// On mobile platforms it indicates that the application is on screen and running.
		/// </summary>
		bool Active { get; }

		/// <summary>
		/// Gets or sets the title of the window.
		/// </summary>
		string Title { get; set; }

		/// <summary>
		/// Gets or sets the window state.
		/// </summary>
		WindowState State { get; set; }

		/// <summary>
		/// Gets or sets the window behavior to be resizable or not.
		/// </summary>
		bool FixedSize { get; set; }

		/// <summary>
		/// Gets or sets the fullscreen window state.
		/// </summary>
		bool Fullscreen { get; set; }

		/// <summary>
		/// Gets or sets an upper-left corner of the client area on the desktop.
		/// </summary>
		Vector2 ClientPosition { get; set; }

		/// <summary>
		/// Gets or sets a client size of this window.
		/// </summary>
		Vector2 ClientSize { get; set; }

		/// <summary>
		/// Gets or sets a position of this window on the desktop.
		/// </summary>
		Vector2 DecoratedPosition { get; set; }

		/// <summary>
		/// Gets or sets an decorated size (including title and border) of this window.
		/// </summary>
		Vector2 DecoratedSize { get; set; }

		// Gets or sets the minimum size to which the window (including title and border) can be sized.
		Vector2 MinimumDecoratedSize { get; set; }

		// Gets or sets the maximum size to which the window (including title and border) can be sized.
		Vector2 MaximumDecoratedSize { get; set; }

		/// <summary>
		/// Gets or sets a value indicates whether the window is displayed.
		/// </summary>
		bool Visible { get; set; }

		/// <summary>
		/// Gets <see cref="Lime.WindowInput"/> for this window.
		/// </summary>
		WindowInput Input { get; }

		/// <summary>
		/// Gets current FPS for the window.
		/// </summary>
		float FPS { get; }

		/// <summary>
		/// Milliseconds since last update.
		/// </summary>
		float UnclampedDelta { get; }

		/// <summary>
		/// Gets the display device containing the largest portion of this window on desktops
		/// or the default screen on devices.
		/// </summary>
		IDisplay Display { get; }

		[Obsolete("Use FPS property instead", true)]
		float CalcFPS();

		/// <summary>
		/// Centers the game window on the current display.
		/// </summary>
		void Center();

		/// <summary>
		/// Activates this window.
		/// </summary>
		void Activate();

		/// <summary>
		/// Gets or sets the cursor for this window.
		/// </summary>
		MouseCursor Cursor { get; set; }

		/// <summary>
		/// Closes this window.
		/// </summary>
		void Close();

		/// <summary>
		/// Occurs when the <see cref="Active"/> property of the window becomes true.
		/// </summary>
		event Action Activated;

		/// <summary>
		/// Occurs when the <see cref="Active"/> property of the window becomes false.
		/// </summary>
		event Action Deactivated;

		/// <summary>
		/// Occurs when the window is about to close.
		/// Returns false to cancel closing.
		/// </summary>
		event ClosingDelegate Closing;

		/// <summary>
		/// Occurs when the window is about to show or hide.
		/// </summary>
		event VisibleChangingDelegate VisibleChanging;

		/// <summary>
		/// Occurs after the window has closed.
		/// </summary>
		event Action Closed;

		/// <summary>
		/// Occurs whenever the window is moved.
		/// </summary>
		event Action Moved;

		/// <summary>
		/// Occurs whenever the window is resized or a device orientation has changed.
		/// </summary>
		event ResizeDelegate Resized;

		/// <summary>
		/// Occurs when it is time to update a frame.
		/// </summary>
		event UpdatingDelegate Updating;

		/// <summary>
		/// Occurs when it is time to render a frame.
		/// </summary>
		event Action Rendering;

		/// <summary>
		/// Sets a flag indicating whether the current frame should be rendered.
		/// </summary>
		void Invalidate();

#if iOS
		GameController UIViewController { get; }
#elif MAC
		Platform.NSGameView NSGameView { get; }
#elif WIN
		System.Windows.Forms.Form Form { get; }
#endif
		/// <summary>
		/// Gets the scale factor which translates virtual units to the physical pixels.
		/// </summary>
		float PixelScale { get; }

		IContext Context { get; set; }

		object Tag { get; set; }

		/// <summary>
		/// You can use this method to display a modal dialog box in your application.
		/// When this method is called, the code following it is not executed until after the dialog box is closed.
		/// The window must be hidden before calling this method.
		/// </summary>
		void ShowModal();

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:Lime.IWindow"/> allow files drop.
		/// </summary>
		bool AllowDropFiles { get; set; }

		/// <summary>
		/// Occurs when files are dropped.
		/// </summary>
		event Action<IEnumerable<string>> FilesDropped;

		/// <summary>
		/// Initiate dragging of files supported by native controls
		/// </summary>
		/// <param name="filenames">list of paths to files</param>
		void DragFiles(string[] filenames);

		/// <summary>
		/// Occurs when unhandled exception on update.
		/// </summary>
		event Action<System.Exception> UnhandledExceptionOnUpdate;

		/// <summary>
		/// Indicates is this window in rendering phase.
		/// </summary>
		bool IsRenderingPhase { get; }

		/// <summary>
		/// Executes an action at the rendering phase.
		/// </summary>
		/// <param name="action"></param>
		void InvokeOnRendering(Action action);

		bool VSync { get; set; }

		/// <summary>
		/// Converts local window coordinates into desktop coordinates.
		/// </summary>
		Vector2 LocalToDesktop(Vector2 localPosition);

		/// <summary>
		/// Converts desktop coordinates into local window coordinates.
		/// </summary>
		Vector2 DesktopToLocal(Vector2 desktopPosition);
	}
}
