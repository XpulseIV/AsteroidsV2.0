using System;
using Microsoft.Xna.Framework.Input;

namespace Asteroids2.Source.Input;

public class KeyboardEventArgs : EventArgs
{
    public Keys[] Keys { get; }

    public KeyboardEventArgs(Keys[] keys)
    {
        Keys = keys;
    }
}