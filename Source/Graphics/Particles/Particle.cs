using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace AstralAssault;

public class Particle
{
    private Vector2 _startingPosition;
    private Vector2 _velocity;
    public EffectContainer EffectContainer = new();

    public Int32 TextureIndex { get; set; }
    public Int64 TimeSpawned { get; private set; }
    public Boolean IsActive { get; private set; }
    public Vector2 Position => this._startingPosition + this._velocity * (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - this.TimeSpawned);

    public Particle(Int32 textureIndex, Vector2 startingPosition, Vector2 velocity, Int64 timeSpawned)
    {
        this.TextureIndex = textureIndex;
        this._startingPosition = startingPosition;
        this._velocity = velocity;
        this.TimeSpawned = timeSpawned;
        this.IsActive = true;
    }

    public void Set(Int32 textureIndex, Vector2 startingPosition, Vector2 velocity, Int64 timeSpawned)
    {
        this.TextureIndex = textureIndex;
        this._startingPosition = startingPosition;
        this._velocity = velocity;
        this.TimeSpawned = timeSpawned;
        this.IsActive = true;
    }

    public void Deactivate()
    {
        this.IsActive = false;
    }
}