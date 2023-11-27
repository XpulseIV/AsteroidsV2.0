using System;

namespace AstralAssault.Source.Input
{
    public interface IKeyboardEventListener
    {
        void OnKeyboardEvent(Object sender, KeyboardEventArgs e);
    }
}