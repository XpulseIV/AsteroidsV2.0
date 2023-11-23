using System;

namespace AstralAssault;

public struct ColorChangeProperty : IParticleProperty
{
    public Palette.Colors[] Colors { get; }
    public Int32 TimeBetweenColorsMS { get; }

    public ColorChangeProperty(Palette.Colors[] colors, Int32 timeBetweenColorsMS)
    {
        this.Colors = colors;
        this.TimeBetweenColorsMS = timeBetweenColorsMS;
    }
}