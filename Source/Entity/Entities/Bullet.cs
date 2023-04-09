using System.Collections.Generic;
using Asteroids2.Source.Entity.Components;
using Asteroids2.Source.Game;
using Asteroids2.Source.Game.GameState;
using Asteroids2.Source.Graphics;
using Asteroids2.Source.Utilities;
using AstralAssault;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Asteroids2.Source.Entity.Entities;

public class Bullet : Entity
{
    public Bullet(GameplayState gameState, Vector2 position, float rotation, float speed) : base(gameState, position)
    {
        Velocity = -Vector2.UnitY.RotateVector(rotation) * speed;

        model = new List<Vector2>() { new Vector2(0, 0) };

        Color = Palette.GetColor(Palette.Colors.Grey9);

        Collider = new Collider
        (
            this,
            3
        );
        GameState.CollisionSystem.AddCollider(Collider);

        OutOfBoundsBehavior = OutOfBounds.Destroy;

        ContactDamage = 5;
        IsFriendly = true;
        size = 1;
    }

    public override void OnCollision(Collider other)
    {
        if (IsFriendly == other.Parent.IsFriendly) return;

        Destroy();
    }

    public override void OnUpdate(UpdateEventArgs e)
    {
        base.OnUpdate(e);

        if (Position.X is > Game1.TargetWidth or < 0 ||
            Position.Y is > Game1.TargetHeight or < 0) Destroy();
    }
}