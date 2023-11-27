using System;
using System.Collections.Generic;
using AstralAssault.Source.Game;
using AstralAssault.Source.Game.GameState;
using AstralAssault.Source.Graphics;
using Microsoft.Xna.Framework;

namespace AstralAssault.Source.Entity.Entities
{
    public class Bullet : Entity
    {
        public Bullet(GameplayState gameState, Vector2 position, Single rotation, Single speed, BulletType bulletType)
            : base(gameState, position) {
            this.model = new List<Vector2> { new Vector2(0, 0) };
            this.Color = Palette.GetColor(Palette.Colors.Grey9);
            this.Bounds = new Rectangle((int)position.X, (int)position.Y, 1, 1);

            this.Velocity = new Vector2(
                (Single)Math.Cos(rotation),
                (Single)Math.Sin(rotation)
            ) * speed;

            this.OutOfBoundsBehavior = OutOfBounds.Destroy;

            this.ContactDamage = bulletType == BulletType.Light ? 4 : 8;

            this.IsFriendly = true;
            this.IsSolid = false;
        }

        public override void OnCollision(Entity other) {
            if (this.IsFriendly == other.IsFriendly) return;

            this.Destroy();
        }

        public override void Update(float deltaTime) {
            base.Update(deltaTime);

            if (this.Position.X is > Game1.GameWidth or < 0 || this.Position.Y is > Game1.GameHeight or < 0) {
                this.Destroy();
            }
        }
    }
}