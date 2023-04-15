#region
using System;
using System.Collections.Generic;
using Asteroids2.Source.Game;
using Asteroids2.Source.Game.GameState;
using Asteroids2.Source.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
#endregion

namespace Asteroids2.Source.Entity;

public class Entity
{
    protected readonly GameplayState GameState;

    private readonly long m_timeSpawned;
    protected Color Color;
    protected float ContactDamage;
    protected float HP;
    protected bool IsActor = false;

    public bool IsFriendly;

    private Texture2D m_healthBarTexture;
    protected float MaxHP;

    public List<Vector2> model;
    protected OutOfBounds OutOfBoundsBehavior = OutOfBounds.Wrap;
    public Vector2 Position;
    protected float Rotation;

    protected int size;
    public Vector2 Velocity;

    public const float MaxSpeed = 100;

    public Rectangle Bounds;

    public bool IsSolid;
    public int Mass;

    protected Entity(GameplayState gameState, Vector2 position)
    {
        GameState = gameState;
        Position = position;
        m_timeSpawned = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }

    public long TimeSinceSpawned
    {
        get => DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - m_timeSpawned;
    }

    public virtual void OnUpdate(UpdateEventArgs e)
    {
        Bounds.X = (int)(Position.X - Bounds.Width / 2f);
        Bounds.Y = (int)(Position.Y - Bounds.Height / 2f);

        if (IsActor && (HP <= 0))
        {
            OnDeath();

            return;
        }

        Position += Velocity * e.DeltaTime;

        switch (OutOfBoundsBehavior)
        {
        case OutOfBounds.DoNothing:
        {
            break;
        }

        case OutOfBounds.Destroy:
        {
            if (Position.X is < 0 or > Game1.TargetWidth ||
                Position.Y is < 0 or > Game1.TargetHeight) Destroy();

            break;
        }

        case OutOfBounds.Wrap:
        {
            Position.X = Position.X switch
            {
                < 0 => Game1.TargetWidth,
                > Game1.TargetWidth => 0,
                var _ => Position.X
            };

            Position.Y = Position.Y switch
            {
                < 0 => Game1.TargetHeight,
                > Game1.TargetHeight => 0,
                var _ => Position.Y
            };

            break;
        }

        default:
        {
            throw new ArgumentOutOfRangeException();
        }
        }
    }

    public virtual void Draw()
    {
        GameState.Root.PixelRenderer.DrawWireFrameModel(model, Position.X, Position.Y, Rotation, size, Color);

        if (IsActor) DrawHealthBar();
    }

    public virtual void Destroy()
    {
        GameState.Entities.Remove(this);
    }

    protected virtual void OnDeath()
    {
        Destroy();
    }

    private void DrawHealthBar()
    {
        const int width = 20;
        const int height = 3;

        int filled = (int)Math.Ceiling(HP / MaxHP * width);

        int x = (int)Position.X - width / 2;
        int y = (int)Position.Y - 20;

        Color outlineColor = Palette.GetColor(Palette.Colors.Black);
        Color emptyColor = Palette.GetColor(Palette.Colors.Red6);
        Color filledColor = Palette.GetColor(Palette.Colors.Green7);

        GameState.Root.PixelRenderer.DrawRect(x - 1, y - 1, width + 1, height + 1, outlineColor); // Outline
        GameState.Root.PixelRenderer.FillRect(x, y, width, height, emptyColor); // Empty
        GameState.Root.PixelRenderer.FillRect(x, y, filled, height, filledColor); // Filled
    }

    protected enum OutOfBounds { DoNothing, Wrap, Destroy }

    public virtual void OnCollision(Entity other)
    {
        if (!IsActor || (other.IsFriendly == IsFriendly)) return;

        HP = Math.Max(0, HP - other.ContactDamage);
    }
}