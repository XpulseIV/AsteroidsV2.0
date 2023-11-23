using System;
using Microsoft.Xna.Framework;

namespace AstralAssault;

public struct VelocityProperty : IParticleProperty
{
    private readonly Single _angleRangeStart;
    private readonly Single _angleRangeEnd;

    private readonly Single _speedRangeStart;
    private readonly Single _speedRangeEnd;

    public Vector2 Velocity { get; }

    public Boolean IsRange { get; }

    public VelocityProperty(Single angleRangeStart, Single angleRangeEnd, Single speedRangeStart, Single speedRangeEnd)
    {
        this._angleRangeStart = angleRangeStart;
        this._angleRangeEnd = angleRangeEnd;

        this._speedRangeStart = speedRangeStart;
        this._speedRangeEnd = speedRangeEnd;

        this.Velocity = Vector2.Zero;

        this.IsRange = true;
    }

    public VelocityProperty(Single angle, Single speed)
    {
        this._angleRangeStart = angle;
        this._angleRangeEnd = angle;

        this._speedRangeStart = speed;
        this._speedRangeEnd = speed;
        
        Vector2 normal = new(
            (Single) Math.Cos(angle),
            (Single) Math.Sin(angle)
        );

        this.Velocity = normal * speed;

        this.IsRange = false;
    }

    public Vector2 GetVelocity()
    {
        if (!this.IsRange) return this.Velocity;

        Random rnd = new();
        Single multiplierAngle = rnd.NextSingle();
        Single multiplierSpeed = rnd.NextSingle();
        
        Single angle = this._angleRangeStart + (this._angleRangeEnd - this._angleRangeStart) * multiplierAngle;
        Single speed = this._speedRangeStart + (this._speedRangeEnd - this._speedRangeStart) * multiplierSpeed;
        
        Vector2 normal = new(
            (Single) Math.Cos(angle),
            (Single) Math.Sin(angle)
        );
        
        return normal * speed;
    }
}