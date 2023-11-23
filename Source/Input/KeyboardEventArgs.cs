using System;
using Microsoft.Xna.Framework.Input;

namespace AstralAssault;

public class KeyboardEventArgs : EventArgs
{
    public Keys[] Keys { get; }
    public Single DeltaTime;

    public KeyboardEventArgs(Keys[] keys, Single deltaTime) {
        this.Keys = keys;
        this.DeltaTime = deltaTime;
    }
}