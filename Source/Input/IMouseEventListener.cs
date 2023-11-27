using System;

namespace AstralAssault.Source.Input
{
    public interface IMouseEventListener
    {
        void OnMouseButtonEvent(Object sender, MouseButtonEventArgs e);
        void OnMouseMoveEvent(Object sender, MouseMoveEventArgs e);
    }
}