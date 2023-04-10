#region
using System;
using System.Collections.Generic;
using Asteroids2.Source.Entity.Components;
using Asteroids2.Source.Game;
using Asteroids2.Source.Game.GameState;
using Asteroids2.Source.Graphics;
using Asteroids2.Source.Utilities;
using Microsoft.Xna.Framework;
#endregion

namespace Asteroids2.Source.Entity.Entities;

public class Asteroid : Entity
{
    public enum Sizes { Smallest, Small, Medium }
    private readonly float m_rotSpeed;
    private readonly Sizes m_size;
    private bool m_hasExploded;

    public Asteroid(
        GameplayState gameState,
        Vector2 position,
        float direction,
        Sizes size)
        : base(gameState, position)
    {
        m_size = size;

        this.size = ((int)m_size + 1) * 4;

        const int verts = 20;

        model = new List<Vector2>();

        Color = Palette.GetColor(Palette.Colors.Red6);

        for (int i = 0; i < verts; i++)
        {
            float noise = (float)new Random().NextDouble() * 0.4f + 0.8f;
            model.Add
            (
                new Vector2
                (
                    noise * MathF.Sin(i / (float)verts * 6.28318f),
                    noise * MathF.Cos(i / (float)verts * 6.28318f)
                )
            );
        }

        Random rnd = new Random();
        m_rotSpeed = rnd.Next(5, 20) / 10F;
        int speed = rnd.Next(30, 100);

        Velocity = Vector2.UnitX.RotateVector(direction) * speed;

        float mass;

        switch (size)
        {
        case Sizes.Smallest:
            MaxHP = 12;
            HP = MaxHP;
            ContactDamage = 5;
            mass = 6;

            break;

        case Sizes.Small:
            MaxHP = 24;
            HP = MaxHP;
            ContactDamage = 7;
            mass = 12;

            break;

        case Sizes.Medium:
            MaxHP = 36;
            HP = MaxHP;
            ContactDamage = 12;
            mass = 18;

            break;

        default:
            throw new ArgumentOutOfRangeException();
        }

        Collider = new Collider
        (
            this,
            ((int)m_size + 1) * 4,
            true,
            mass
        );
        GameState.CollisionSystem.AddCollider(Collider);

        OutOfBoundsBehavior = OutOfBounds.Wrap;

        IsActor = true;
    }

    protected override void OnDeath()
    {
        if (!m_hasExploded && ((m_size - 1) >= 0))
        {
            Random rnd = new Random();
            int amount = rnd.Next(1, 4);

            Vector2 playerPosition = GameState.Player.Position;
            float angleToPlayer = MathF.Atan2(Position.Y - playerPosition.Y, Position.X - playerPosition.X);

            for (int i = 0; i < amount; i++)
            {
                angleToPlayer += (float)rnd.NextDouble() * MathF.PI / 1 - MathF.PI / 2;

                GameState.Entities.Add
                (
                    new Asteroid(GameState, Position, angleToPlayer, m_size - 1)
                );
            }
        }

        m_hasExploded = true;

        base.OnDeath();
    }

    public override void OnUpdate(UpdateEventArgs e)
    {
        base.OnUpdate(e);

        Rotation += m_rotSpeed * e.DeltaTime;

        if (Rotation > Math.PI) Rotation = -MathF.PI;
    }
}