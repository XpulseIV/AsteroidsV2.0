using System;

namespace AstralAssault.Source.Input
{
    public interface IKeyboardPressedEventListener
    {
        void OnKeyboardPressedEvent(Object sender, KeyboardEventArgs e);
    }
}