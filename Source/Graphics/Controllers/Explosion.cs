using System;
using Microsoft.Xna.Framework;

namespace AstralAssault;

public readonly struct Explosion
{
    private readonly Int64 _timeSpawned;
    public Point Position { get; }
    public Int64 TimeSinceSpawned => DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - this._timeSpawned;

    public Explosion(Point position)
    {
        this._timeSpawned = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        this.Position = position;
    }
}