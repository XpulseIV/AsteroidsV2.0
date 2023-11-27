using System;
using Microsoft.Xna.Framework;

namespace AstralAssault.Source.Input
{
    public class MouseMoveEventArgs : EventArgs
    {
        public Point Position { get; }

        public MouseMoveEventArgs(Point position) {
            this.Position = position;
        }
    }
}