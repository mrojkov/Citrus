#if MAC || MONOMAC
#region License
//
// The Open Toolkit Library License
//
// Copyright (c) 2006 - 2010 the Open Toolkit library.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
#endregion

//  Created by Erik Ylvisaker on 3/17/08.

using System;
using System.Collections.Generic;
using System.Text;

namespace Lime.Platform
{
    static class MacKeyMap
    {
        public static Key GetKey(MacKeyCode code)
        {
            // comments indicate members of the Key enum that are missing
            switch (code)
            {
                case MacKeyCode.A:
                    return Key.A;
                case MacKeyCode.OptionAlt:
                    return Key.Alt;
                // RAlt
                case MacKeyCode.B:
                    return Key.B;
                case MacKeyCode.Backslash:
                    return Key.BackSlash;
                case MacKeyCode.Backspace:
                    return Key.BackSpace;
                case MacKeyCode.BracketLeft:
                    return Key.LBracket;
                case MacKeyCode.BracketRight:
                    return Key.RBracket;
                case MacKeyCode.C:
                    return Key.C;
                // Capslock
                // Clear
                case MacKeyCode.Comma:
                    return Key.Comma;
                case MacKeyCode.Control:
                    return Key.Control;
                // RControl
                case MacKeyCode.D:
                    return Key.D;
                case MacKeyCode.Del:
                    return Key.Delete;
                case MacKeyCode.Down:
                    return Key.Down;
                case MacKeyCode.E:
                    return Key.E;
                case MacKeyCode.End:
                    return Key.End;
                case MacKeyCode.Enter:
                    return Key.Enter;
                case MacKeyCode.Return:
                    return Key.Enter;
                case MacKeyCode.Esc:
                    return Key.Escape;
                case MacKeyCode.F:
                    return Key.F;
                case MacKeyCode.F1:
                    return Key.F1;
                case MacKeyCode.F2:
                    return Key.F2;
                case MacKeyCode.F3:
                    return Key.F3;
                case MacKeyCode.F4:
                    return Key.F4;
                case MacKeyCode.F5:
                    return Key.F5;
                case MacKeyCode.F6:
                    return Key.F6;
                case MacKeyCode.F7:
                    return Key.F7;
                case MacKeyCode.F8:
                    return Key.F8;
                case MacKeyCode.F9:
                    return Key.F9;
                case MacKeyCode.F10:
                    return Key.F10;
                case MacKeyCode.F11:
                    return Key.F11;
                case MacKeyCode.F12:
                    return Key.F12;
                case MacKeyCode.G:
                    return Key.G;
                case MacKeyCode.H:
                    return Key.H;
                case MacKeyCode.Home:
                    return Key.Home;
                case MacKeyCode.I:
                    return Key.I;
                case MacKeyCode.Insert:
                    return Key.Insert;
                case MacKeyCode.J:
                    return Key.J;
                case MacKeyCode.K:
                    return Key.K;
                case MacKeyCode.L:
                    return Key.L;
                case MacKeyCode.Left:
                    return Key.Left;
                case MacKeyCode.M:
                    return Key.M;
                //Key.MaxKeys 
                case MacKeyCode.Minus:
                    return Key.Minus;
                case MacKeyCode.N:
                    return Key.N;
                case MacKeyCode.Key_0:
                    return Key.Number0;
                case MacKeyCode.Key_1:
                    return Key.Number1;
                case MacKeyCode.Key_2:
                    return Key.Number2;
                case MacKeyCode.Key_3:
                    return Key.Number3;
                case MacKeyCode.Key_4:
                    return Key.Number4;
                case MacKeyCode.Key_5:
                    return Key.Number5;
                case MacKeyCode.Key_6:
                    return Key.Number6;
                case MacKeyCode.Key_7:
                    return Key.Number7;
                case MacKeyCode.Key_8:
                    return Key.Number8;
                case MacKeyCode.Key_9:
                    return Key.Number9;
                // Numlock
                case MacKeyCode.O:
                    return Key.O;
                case MacKeyCode.P:
                    return Key.P;
                case MacKeyCode.Pagedown:
                    return Key.PageDown;
                case MacKeyCode.Pageup:
                    return Key.PageUp;
                // Pause
                case MacKeyCode.Period:
                    return Key.Period;
                case MacKeyCode.Equals:
                    return Key.EqualsSign;
                // PrintScreen
                case MacKeyCode.Q:
                    return Key.Q;
                case MacKeyCode.Quote:
                    return Key.Quote;
                case MacKeyCode.R:
                    return Key.R;
                case MacKeyCode.Right:
                    return Key.Right;
                case MacKeyCode.S:
                    return Key.S;
                // ScrollLock
                case MacKeyCode.Semicolon:
                    return Key.Semicolon;
                case MacKeyCode.Shift:
                    return Key.Shift;
                //Key.RShift 
                case MacKeyCode.Slash:
                    return Key.Slash;
                // Key.Sleep
                case MacKeyCode.Space:
                    return Key.Space;
                case MacKeyCode.T:
                    return Key.T;
                case MacKeyCode.Tab:
                    return Key.Tab;
                case MacKeyCode.Tilde:
                    return Key.Tilde;
                case MacKeyCode.U:
                    return Key.U;
                case MacKeyCode.Up:
                    return Key.Up;
                case MacKeyCode.V:
                    return Key.V;
                case MacKeyCode.W:
                    return Key.W;
                case MacKeyCode.X:
                    return Key.X;
                case MacKeyCode.Y:
                    return Key.Y;
                case MacKeyCode.Z:
                    return Key.Z;

                default:
                    return Key.Unknown;
            }
        }
    }
}
#endif