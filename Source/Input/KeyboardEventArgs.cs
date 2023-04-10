#region
using System;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Asteroids2.Source.Input;

public class KeyboardEventArgs : EventArgs
{
    public KeyboardEventArgs(Keys[] keys)
    {
        Keys = keys;
    }

    public Keys[] Keys { get; }
}