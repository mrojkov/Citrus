#if MAC
using System;
using System.Drawing;

namespace OpenTK.Input
{
    /// <summary>
    /// Defines the event data for <see cref="MouseDevice"/> events.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Do not cache instances of this type outside their event handler.
    /// If necessary, you can clone an instance using the 
    /// <see cref="MouseEventArgs(MouseEventArgs)"/> constructor.
    /// </para>
    /// </remarks>
    public class MouseEventArgs : EventArgs
    {
        #region Public Members

        /// <summary>
        /// Gets the X position of the mouse for the event.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets the Y position of the mouse for the event.
        /// </summary>
        public int Y { get; set; }


        /// <summary>
        /// Gets a System.Boolean representing the state of the mouse button for the event.
        /// </summary>
		public bool IsPressed { get; set; }

        #endregion
    }

    /// <summary>
    /// Defines the event data for <see cref="MouseDevice.Move"/> events.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Do not cache instances of this type outside their event handler.
    /// If necessary, you can clone an instance using the 
    /// <see cref="MouseMoveEventArgs(MouseMoveEventArgs)"/> constructor.
    /// </para>
    /// </remarks>
    public class MouseMoveEventArgs : MouseEventArgs
    {
        #region Public Members

        /// <summary>
        /// Gets the change in X position produced by this event.
        /// </summary>
        public int XDelta { get; set; }

        /// <summary>
        /// Gets the change in Y position produced by this event.
        /// </summary>
        public int YDelta { get; set; }

        #endregion
    }

    /// <summary>
    /// Defines the event data for <see cref="MouseDevice.ButtonDown"/> and <see cref="MouseDevice.ButtonUp"/> events.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Do not cache instances of this type outside their event handler.
    /// If necessary, you can clone an instance using the 
    /// <see cref="MouseButtonEventArgs(MouseButtonEventArgs)"/> constructor.
    /// </para>
    /// </remarks>
    public class MouseButtonEventArgs : MouseEventArgs
    {
        #region Public Members

        /// <summary>
        /// Gets the <see cref="MouseButton"/> that triggered this event.
        /// </summary>
        public MouseButton Button { get; set; }

        #endregion
    }

    /// <summary>
    /// Defines the event data for <see cref="MouseDevice.WheelChanged"/> events.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Do not cache instances of this type outside their event handler.
    /// If necessary, you can clone an instance using the 
    /// <see cref="MouseWheelEventArgs(MouseWheelEventArgs)"/> constructor.
    /// </para>
    /// </remarks>
    public class MouseWheelEventArgs : MouseEventArgs
    {
        /// <summary>
        /// Gets the change in value of the wheel for this event in integer units.
        /// To support high-precision mice, it is recommended to use <see cref="DeltaPrecise"/> instead.
        /// </summary>
        public int Delta { get; set; }
    }
}
#endif