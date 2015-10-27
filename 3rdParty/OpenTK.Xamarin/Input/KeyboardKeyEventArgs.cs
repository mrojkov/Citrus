#if MAC
using System;
using System.Collections.Generic;
using System.Text;
using AppKit;

namespace OpenTK.Input
{
    /// <summary>
    /// Defines the event data for <see cref="KeyboardDevice"/> events.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Do not cache instances of this type outside their event handler.
    /// If necessary, you can clone a KeyboardEventArgs instance using the 
    /// <see cref="KeyboardKeyEventArgs(KeyboardKeyEventArgs)"/> constructor.
    /// </para>
    /// </remarks>
    public class KeyboardKeyEventArgs : EventArgs
    {
        #region Public Members

        /// <summary>
        /// Gets the <see cref="Key"/> that generated this event.
        /// </summary>
        public Key Key { get; set; }
		public NSEventModifierMask Modifiers { get; set; }
    
        #endregion
    }
}
#endif