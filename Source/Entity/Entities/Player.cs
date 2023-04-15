#region
using System;
using System.Collections.Generic;
using System.Linq;
using Asteroids2.Source.Game;
using Asteroids2.Source.Game.GameState;
using Asteroids2.Source.Graphics;
using Asteroids2.Source.Input;
using Asteroids2.Source.Upgrades.BaseClasses;
using Asteroids2.Source.Upgrades.Cannons;
using Asteroids2.Source.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Asteroids2.Source.Entity.Entities;

public class Player : Entity, IInputEventListener
{
    private const float MoveSpeed = 200;
    private const float RotaionSpeed = 4.5f;
    private const float Friction = 30;
    private const float BulletSpeed = 250;

    private readonly CannonBase m_cannon;
    private float m_delta;
    private long m_lastTimeFired;

    public int m_money;

    public Player(GameplayState gameState, Vector2 position) : base(gameState, position)
    {
        Position = position;
        Rotation = 0;
        size = 1;

        m_money = 0;

        model = new List<Vector2>
        {
            new Vector2(0.0f, -5.0f),
            new Vector2(-2.5f, 2.5f),
            new Vector2(2.5f, 2.5f)
        };

        Bounds = new Rectangle((int)position.X, (int)position.Y, 6, 6);

        Color = Palette.GetColor(Palette.Colors.Blue8);

        m_cannon = new Mk3Cannon();

        StartListening();

        OutOfBoundsBehavior = OutOfBounds.Wrap;

        IsActor = true;
        MaxHP = 5000;
        HP = MaxHP;
        IsFriendly = true;
        IsSolid = true;

        Mass = 7;
    }

    public void OnKeyboardEvent(object sender, KeyboardEventArgs e)
    {
        int xAxis = 0;
        int yAxis = 0;

        if (e.Keys.Contains(Keys.Left)) xAxis = -1;
        else if (e.Keys.Contains(Keys.Right)) xAxis = 1;


        if (e.Keys.Contains(Keys.Up)) yAxis = 1;
        else if (e.Keys.Contains(Keys.Down)) yAxis = -1;

        if (e.Keys.Contains(Keys.Space)) HandleFiring();

        if (e.Keys.Contains(Keys.G))
        {
            Random rnd = new();

            int side = rnd.Next(0, 4);

            int x = side switch
            {
                0 => 0,
                1 => Game1.TargetWidth,
                2 => rnd.Next(0, Game1.TargetWidth),
                3 => rnd.Next(0, Game1.TargetWidth),
                _ => throw new ArgumentOutOfRangeException()
            };

            int y = side switch
            {
                0 => rnd.Next(0, Game1.TargetHeight),
                1 => rnd.Next(0, Game1.TargetHeight),
                2 => 0,
                3 => Game1.TargetHeight,
                _ => throw new ArgumentOutOfRangeException()
            };

            Vector2 position = new(x, y);
            Asteroid.Sizes size = (Asteroid.Sizes)rnd.Next(0, 3);

            GameState.Entities.Add(new Asteroid(GameState, position, 0, size));
        }

        HandleMovement(xAxis, yAxis);
    }

    public void OnMouseMoveEvent(object sender, MouseMoveEventArgs e) { }

    public void OnMouseButtonEvent(object sender, MouseButtonEventArgs e) { }

    private void StartListening()
    {
        InputEventSource.KeyboardEvent += OnKeyboardEvent;
        InputEventSource.MouseMoveEvent += OnMouseMoveEvent;
        InputEventSource.MouseButtonEvent += OnMouseButtonEvent;
    }

    private void StopListening()
    {
        InputEventSource.KeyboardEvent -= OnKeyboardEvent;
        InputEventSource.MouseMoveEvent -= OnMouseMoveEvent;
        InputEventSource.MouseButtonEvent -= OnMouseButtonEvent;
    }

    private void HandleMovement(int xAxis, int yAxis)
    {
        Rotation += RotaionSpeed * xAxis * m_delta;

        // acceleration and deceleration
        Vector2 forward = -Vector2.UnitY.RotateVector(Rotation) *
            MoveSpeed * m_delta;

        // apply velocity vectors
        Velocity = new Vector2
        (
            Math.Clamp(Velocity.X + forward.X * yAxis, -MaxSpeed, MaxSpeed),
            Math.Clamp(Velocity.Y + forward.Y * yAxis, -MaxSpeed, MaxSpeed)
        );

        if (Velocity.Length() > MaxSpeed)
        {
            Velocity.Normalize();
            Velocity *= MaxSpeed;
        }
        else if (Velocity.Length() < -MaxSpeed)
        {
            Velocity.Normalize();
            Velocity *= -MaxSpeed;
        }
    }

    private void HandleFiring()
    {
        long timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        if ((m_lastTimeFired + m_cannon.ShootSpeed) > timeNow) return;

        var positiveRotMat = Matrix.CreateRotationZ(Rotation + 15.0f);

        var cannonPos = Position + Vector2.UnitY.RotateVector(Rotation);

        GameState.Entities.Add(new Bullet(GameState, cannonPos, Rotation, BulletSpeed));

        var idk = new Mk3Cannon();

        if (m_cannon.GetType() == idk.GetType())
        {
            GameState.Entities.Add(new Bullet(GameState, cannonPos + Vector2.Transform(Vector2.UnitX, -positiveRotMat), Rotation, BulletSpeed));
            GameState.Entities.Add(new Bullet(GameState, cannonPos + Vector2.Transform(Vector2.UnitX, positiveRotMat), Rotation, BulletSpeed));

            GameState.Entities.Add(new Bullet(GameState, cannonPos - Vector2.Transform(Vector2.UnitX, -positiveRotMat), Rotation, BulletSpeed));
            GameState.Entities.Add(new Bullet(GameState, cannonPos - Vector2.Transform(Vector2.UnitX, positiveRotMat), Rotation, BulletSpeed));
        }

        m_lastTimeFired = timeNow;
    }

    public override void Destroy()
    {
        StopListening();

        base.Destroy();
    }

    protected override void OnDeath()
    {
        Game1 root = GameState.Root;

        root.GameStateMachine.ChangeState(new GameOverState(root));

        base.OnDeath();
    }

    public override void OnUpdate(UpdateEventArgs e)
    {
        base.OnUpdate(e);

        m_delta = e.DeltaTime;

        // apply friction
        float sign = Math.Sign(Velocity.Length());

        if (sign != 0)
        {
            float direction = (float)Math.Atan2(Velocity.Y, Velocity.X);

            Velocity -=
                Vector2.UnitX.RotateVector(direction) * Friction * m_delta * sign;
        }
    }
}