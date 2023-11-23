using System;

namespace AstralAssault;

public interface IKeyboardEventListener
{
    void OnKeyboardEvent(Object sender, KeyboardEventArgs e);
}