using System;

namespace AstralAssault;

public interface IKeyboardPressedEventListener
{
    void OnKeyboardPressedEvent(Object sender, KeyboardEventArgs e);
}