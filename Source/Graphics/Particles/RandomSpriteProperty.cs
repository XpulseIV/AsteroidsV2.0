using System;

namespace AstralAssault.Source.Graphics.Particles
{
    public struct RandomSpriteProperty : IParticleProperty
    {
        private readonly Random _rnd = new();
        private readonly Int32 _rangeStart;
        private readonly Int32 _rangeEnd;
        public Int32 SpriteIndex => this._rnd.Next(this._rangeStart, this._rangeEnd + 1);

        public RandomSpriteProperty(Int32 rangeStart, Int32 rangeEnd) {
            this._rangeStart = rangeStart;
            this._rangeEnd = rangeEnd;
        }
    }
}