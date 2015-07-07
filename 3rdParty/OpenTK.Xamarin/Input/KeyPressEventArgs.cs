#if MAC
using System;

namespace OpenTK
{
    /// <summary>
    /// Defines the event arguments for KeyPress events. Instances of this class are cached:
    /// KeyPressEventArgs should only be used inside the relevant event, unless manually cloned.
    /// </summary>
    public class KeyPressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a <see cref="System.Char"/> that defines the ASCII character that was typed.
        /// </summary>
        public char KeyChar { get; set; }
    }
}
#endif