using System;

namespace AstralAssault;

public struct HighlightEffect : IDrawTaskEffect
{
    public Single Alpha { get; private set; }

    public HighlightEffect(Single alpha)
    {
        this.Alpha = alpha;
    }

    public void SetAlpha(Single alpha)
    {
        this.Alpha = alpha;
    }
}