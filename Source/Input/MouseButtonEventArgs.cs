using System;
using static AstralAssault.Source.Input.InputEventSource;

namespace AstralAssault.Source.Input
{
    public class MouseButtonEventArgs : EventArgs
    {
        public MouseButtons Button { get; }

        public MouseButtonEventArgs(MouseButtons button) {
            this.Button = button;
        }
    }
}