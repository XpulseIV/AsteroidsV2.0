﻿namespace Asteroids2.Source.Input;

public interface IMouseEventListener
{
    void OnMouseButtonEvent(object sender, MouseButtonEventArgs e);
    void OnMouseMoveEvent(object sender, MouseMoveEventArgs e);
}