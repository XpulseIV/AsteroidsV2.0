#region
using System;
using static Asteroids2.Source.Input.InputEventSource;
#endregion

namespace Asteroids2.Source.Input;

public class MouseButtonEventArgs : EventArgs
{
    public MouseButtonEventArgs(MouseButtons button)
    {
        Button = button;
    }

    public MouseButtons Button { get; }
}