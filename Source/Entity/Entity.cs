using System;
using System.Collections.Generic;
using Asteroids2.Source.Entity.Components;
using Asteroids2.Source.Game;
using Asteroids2.Source.Game.GameState;
using Asteroids2.Source.Graphics;
using Asteroids2.Source.Utilities;
using AstralAssault;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace Asteroids2.Source.Entity;

public class Entity
{
    public Vector2 Position;
    public Vector2 Velocity;
    protected float Rotation;
    protected Collider Collider;
    protected readonly GameplayState GameState;
    protected OutOfBounds OutOfBoundsBehavior = OutOfBounds.Wrap;
    protected bool IsActor = false;
    protected float MaxHP;
    protected float HP;
    protected float ContactDamage;

    protected int size;
    protected Color Color;

    public bool IsFriendly;

    private readonly long m_timeSpawned;

    public List<Vector2> model;

    public long TimeSinceSpawned
    {
        get => DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - m_timeSpawned;
    }

    private Texture2D m_healthBarTexture;

    protected enum OutOfBounds { DoNothing, Wrap, Destroy }

    protected Entity(GameplayState gameState, Vector2 position)
    {
        GameState = gameState;
        Position = position;
        m_timeSpawned = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }

    public virtual void OnUpdate(UpdateEventArgs e)
    {
        Collider.SetPosition(Position);

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

    public virtual void OnCollision(Collider other)
    {
        if (!IsActor || (other.Parent.IsFriendly == IsFriendly)) return;

        HP = Math.Max(0, HP - other.Parent.ContactDamage);
    }

    public virtual void Draw()
    {
        GameState.Root.PixelRenderer.DrawWireFrameModel(model, Position.X, Position.Y, Rotation, size, Color);

        if (IsActor) DrawHealthBar();
    }

    public virtual void Destroy()
    {
        GameState.Entities.Remove(this);
        GameState.CollisionSystem.RemoveCollider(Collider);
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

        GameState.Root.PixelRenderer.DrawRect(x - 1, y - 1, width + 2, height + 2, outlineColor); // Outline
        GameState.Root.PixelRenderer.DrawFilledRect(x, y, width, height, emptyColor); // Empty
        GameState.Root.PixelRenderer.DrawFilledRect(x, y, filled, height, filledColor); // Filled
    }
}