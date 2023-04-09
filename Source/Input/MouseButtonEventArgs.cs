using System;
using static Asteroids2.Source.Input.InputEventSource;

namespace Asteroids2.Source.Input;

public class MouseButtonEventArgs : EventArgs
{
    public MouseButtons Button { get; }

    public MouseButtonEventArgs(MouseButtons button)
    {
        Button = button;
    }
}