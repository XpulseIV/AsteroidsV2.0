using System;

namespace AstralAssault.Source.Graphics.Particles
{
    public struct SpriteChangeProperty : IParticleProperty
    {
        public Int32 StartIndex { get; }
        public Int32 EndIndex { get; }
        public Int32 TimeBetweenChangesMS { get; }

        public SpriteChangeProperty(Int32 startIndex, Int32 endIndex, Int32 timeBetweenChangesMS) {
            this.StartIndex = startIndex;
            this.EndIndex = endIndex;
            this.TimeBetweenChangesMS = timeBetweenChangesMS;
        }
    }
}