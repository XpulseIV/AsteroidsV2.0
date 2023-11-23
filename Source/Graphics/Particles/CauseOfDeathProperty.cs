using System;

namespace AstralAssault;

public struct CauseOfDeathProperty : IParticleProperty
{
    public enum CausesOfDeath
    {
        OutOfBounds,
        LifeSpan
    }

    public CausesOfDeath CauseOfDeath { get; }
    public Int32 LifeSpan { get; }

    public CauseOfDeathProperty(CausesOfDeath causeOfDeath, Int32 lifeSpan = 0)
    {
        this.CauseOfDeath = causeOfDeath;
        this.LifeSpan = lifeSpan;
    }
}