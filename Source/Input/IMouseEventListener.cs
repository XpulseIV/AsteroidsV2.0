using System;

namespace AstralAssault;

public interface IMouseEventListener
{
    void OnMouseButtonEvent(Object sender, MouseButtonEventArgs e);
    void OnMouseMoveEvent(Object sender, MouseMoveEventArgs e);
}