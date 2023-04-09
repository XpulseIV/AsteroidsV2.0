using System;
using Microsoft.Xna.Framework;

namespace Asteroids2.Source.Input;

public class MouseMoveEventArgs : EventArgs
{
    public Point Position { get; }

    public MouseMoveEventArgs(Point position)
    {
        Position = position;
    }
}