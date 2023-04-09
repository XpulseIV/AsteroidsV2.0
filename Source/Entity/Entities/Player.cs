﻿using System;
using System.Collections.Generic;
using System.Linq;
using Asteroids2.Source.Entity.Components;
using Asteroids2.Source.Game;
using Asteroids2.Source.Game.GameState;
using Asteroids2.Source.Graphics;
using Asteroids2.Source.Input;
using Asteroids2.Source.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Asteroids2.Source.Entity.Entities;

public class Player : Entity, IInputEventListener
{
    private long m_lastTimeFired;
    private float m_delta;

    private const float MoveSpeed = 200;
    private const float RotaionSpeed = 4.5f;
    private const float MaxSpeed = 100;
    private const float Friction = 30;
    private const float BulletSpeed = 250;
    private const int ShootSpeed = 150;

    private static readonly List<Vector2> ShipModel = new List<Vector2>
    {
        new Vector2(0.0f, -5.0f),
        new Vector2(-2.5f, 2.5f),
        new Vector2(2.5f, 2.5f)
    };

    public Player(GameplayState gameState, Vector2 position) : base(gameState, position)
    {
        Position = position;
        Rotation = 0;

        StartListening();

        Collider = new Collider
        (
            this,
            6,
            true,
            5
        );
        GameState.CollisionSystem.AddCollider(Collider);

        OutOfBoundsBehavior = OutOfBounds.Wrap;

        IsActor = true;
        MaxHP = 50;
        HP = MaxHP;
        IsFriendly = true;
    }

    public override List<DrawTask> GetDrawTasks()
    {
        List<DrawTask> drawTasks = new List<DrawTask>();

        drawTasks.AddRange(base.GetDrawTasks());

        GameState.PixelRenderer.DrawWireFrameModel(ShipModel, Position.X, Position.Y, Rotation, 1, Color.Aqua);

        return drawTasks;
    }

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

        if ((m_lastTimeFired + ShootSpeed) > timeNow) return;

        GameState.Entities.Add(new Bullet(GameState, Position, Rotation, BulletSpeed));

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

    public void OnKeyboardEvent(object sender, KeyboardEventArgs e)
    {
        int xAxis = 0;
        int yAxis = 0;

        if (e.Keys.Contains(Keys.Left))
        {
            xAxis = -1;
        }
        else if (e.Keys.Contains(Keys.Right))
        {
            xAxis = 1;
        }


        if (e.Keys.Contains(Keys.Up))
        {
            yAxis = 1;
        }
        else if (e.Keys.Contains(Keys.Down))
        {
            yAxis = -1;
        }


        if (e.Keys.Contains(Keys.Space))
        {
            HandleFiring();
        }

        HandleMovement(xAxis, yAxis);
    }

    public void OnMouseMoveEvent(object sender, MouseMoveEventArgs e) { }

    public void OnMouseButtonEvent(object sender, MouseButtonEventArgs e) { }

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